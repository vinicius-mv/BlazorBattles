using BlazorBattles.Server.Data;
using BlazorBattles.Server.Services;
using BlazorBattles.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBattles.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserUnitsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;

        public UserUnitsController(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        [HttpPost("revive")]
        public async Task<IActionResult> ReviveArmy()
        {
            var user = await _utilityService.GetUserAsync();
            var userUnits = await _context.UserUnits
                .Where(uu => uu.UserId == user.Id)
                .Include(uu =>  uu.Unit)
                .ToArrayAsync();

            int bananaCost = 1000;

            if(user.Bananas < bananaCost)
            {
                return BadRequest("Not enough bananas! You need 1000 bananas to revive your army");
            }
            bool armyAlreadyAlive = true;

            var rng = new Random();

            foreach (var userUnit in userUnits)
            {
                if(userUnit.HitPoints <= 0)
                {
                    armyAlreadyAlive = false;
                    //userUnit.HitPoints = rng.Next(0, userUnit.Unit.HitPoints);
                    userUnit.HitPoints = 100;
                }
            }

            if(armyAlreadyAlive)
                return Ok("Your army is already alive.");

            user.Bananas -= bananaCost;

            await _context.SaveChangesAsync();

            return Ok("Army revived!");
        }

        [HttpPost]
        public async Task<IActionResult> BuildUserUnit([FromBody] int unitId)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId);
            var user = await _utilityService.GetUserAsync();

            if (user.Bananas < unit.BananaCost)
                return BadRequest("Not enough bananas.");

            user.Bananas -= unit.BananaCost;

            var newUserUnit = new UserUnit
            {
                UnitId = unit.Id,
                UserId = user.Id,
                HitPoints = unit.HitPoints
            };

            _context.UserUnits.Add(newUserUnit);
            await _context.SaveChangesAsync();

            return Ok(newUserUnit);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserUnits()
        {
            var user = await _utilityService.GetUserAsync();
            var userUnits = await _context.UserUnits.Where(unit => unit.UserId == user.Id).ToListAsync();

            var response = userUnits.Select(
                unit => new UserUnitResponse
                {
                    UnitId = unit.UnitId,
                    HitPoints = unit.HitPoints
                });

            return Ok(response);
        }
    }
}
