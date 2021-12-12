using BlazorBattles.Server.Data;
using BlazorBattles.Server.Services;
using BlazorBattles.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBattles.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BattlesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;

        public BattlesController(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        [HttpPost]
        public async Task<IActionResult> StartBattle([FromBody] int opponentId)
        {
            var currentUser = await _utilityService.GetUserAsync();

            if (opponentId == currentUser.Id)
                return BadRequest("Invalid opponent");

            var opponentUser = await _context.Users.FindAsync(opponentId);

            if (opponentUser == null || opponentUser.IsDeleted)
            {
                return NotFound("Opponent not available");
            }

            var result = await Fight(currentUser, opponentUser);

            return Ok(result);
        }

        private async Task<BattleResult> Fight(User currentUser, User opponentUser)
        {
            var userArmy = await _context.UserUnits
                .Where(u => u.UserId == currentUser.Id && u.HitPoints > 0)
                .Include(u => u.Unit)
                .ToListAsync();

            var opponentArmy = await _context.UserUnits
                .Where(u => u.UserId == opponentUser.Id && u.HitPoints > 0)
                .Include(u => u.Unit)
                .ToListAsync();

            int currentUserDamageSum = 0;
            int opponentUserDamageSum = 0;
            int currentRound = 0;
            BattleResult result = new BattleResult();

            while (userArmy.Count > 0 && opponentArmy.Count > 0)
            {
                currentRound++;

                if (currentRound % 2 == 1)
                    currentUserDamageSum += FightRound(currentUser, opponentUser, userArmy, opponentArmy, result);
                else
                    opponentUserDamageSum += FightRound(opponentUser, currentUser, opponentArmy, userArmy, result);
            }

            result.IsVictory = opponentArmy.Count == 0;
            result.RoundsFought = currentRound;

            if (result.RoundsFought > 0)
                await FinishFight(currentUser, opponentUser, result, currentUserDamageSum, opponentUserDamageSum);

            return result;
        }

        private int FightRound(User attacker, User defender, List<UserUnit> attackerArmy, List<UserUnit> defenderArmy, BattleResult result)
        {
            var rng = new Random();

            var randomAttackerIndex = rng.Next(attackerArmy.Count);
            var randomAttacker = attackerArmy[randomAttackerIndex];

            var randomDefenderIndex = rng.Next(defenderArmy.Count);
            var randomDefender = defenderArmy[randomDefenderIndex];

            var damage = rng.Next(randomAttacker.Unit.Attack) - rng.Next(randomDefender.Unit.Defense);

            if (damage < 0) damage = 0;

            if (damage >= randomDefender.HitPoints) damage = randomDefender.HitPoints;

            randomDefender.HitPoints -= damage;
            result.Log.Add(
                $"{attacker.Username}'s {randomAttacker.Unit.Title} attacks " +
                $"{defender.Username}'s {randomDefender.Unit.Title} with {damage} damage.");

            if (randomDefender.HitPoints <= 0)
            {
                defenderArmy.Remove(randomDefender);
                result.Log.Add(
               $"{attacker.Username}'s {randomAttacker.Unit.Title} kills " +
               $"{defender.Username}'s {randomDefender.Unit.Title}!!!");
            }

            return damage;
        }

        private async Task FinishFight(User attacker, User opponent, BattleResult result, int attackerDamageSum, int opponentDamageSum)
        {
            result.AttackerDamageSum = attackerDamageSum;
            result.OpponentDamageSum = opponentDamageSum;

            attacker.Battles++;
            opponent.Battles++;

            if (result.IsVictory)
            {
                attacker.Victories++;
                opponent.Defeats++;

                attacker.Bananas += opponentDamageSum;
                opponent.Bananas += attackerDamageSum * 10;
            }
            else
            {
                attacker.Defeats++;
                opponent.Victories++;

                attacker.Bananas += opponentDamageSum * 10;
                opponent.Bananas += attackerDamageSum;
            }

            StoreBattleHistory(attacker, opponent, result);

            await _context.SaveChangesAsync();
        }

        private void StoreBattleHistory(User attacker, User opponent, BattleResult result)
        {
            var battle = new Battle()
            {
                Attacker = attacker,
                Opponent = opponent,
                RoundsFought = result.RoundsFought,
                WinnerDamage = result.IsVictory ? result.AttackerDamageSum : result.OpponentDamageSum,
                Winner = result.IsVictory ? attacker : opponent,
            };

            _context.Battles.Add(battle);
        }
    }
}
