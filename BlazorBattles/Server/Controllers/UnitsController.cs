using BlazorBattles.Server.Data;
using BlazorBattles.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBattles.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly DataContext _context;

        public UnitsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnits()
        {
            var units = await _context.Units.ToListAsync();
            return Ok(_context.Units);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUnit(int id)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
            return Ok(unit);
        }

        [HttpPost]
        public async Task<IActionResult> AddUnit(Unit unit)
        {
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUnit), new { Id = unit.Id }, unit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnit(int id, Unit unit)
        {
            var dbUnit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
            if(dbUnit == null)
                return NotFound("Unit with given Id doesn't exist.");

            dbUnit.Title = unit.Title;
            dbUnit.Attack = unit.Attack;
            dbUnit.Defense = unit.Defense;
            dbUnit.BananaCost = unit.BananaCost;
            dbUnit.HitPoints = unit.HitPoints;
            
            await _context.SaveChangesAsync();

            return Ok(dbUnit);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var dbUnit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
            if (dbUnit == null)
                return NotFound("Unit with given Id doesn't exist.");

            _context.Units.Remove(dbUnit);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
