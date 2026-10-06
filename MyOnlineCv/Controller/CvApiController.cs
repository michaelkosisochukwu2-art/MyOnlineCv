using Microsoft.AspNetCore.Mvc;
using MyOnlineCv.Data;
using Microsoft.EntityFrameworkCore;
using MyOnlineCv.Models;

namespace MyOnlineCv.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CvApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CvApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= EDUCATION =================
        [HttpGet("education")]
        public async Task<IActionResult> GetEducation()
        {
            var items = await _context.EducationItems.ToListAsync();
            return Ok(items);
        }

        [HttpPost("education")]
        public async Task<IActionResult> AddEducation([FromBody] EducationItem item)
        {
            if (item == null) return BadRequest("Invalid education entry.");

            _context.EducationItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEducation), new { id = item.Id }, item);
        }

        // ================= EXPERIENCE =================
        [HttpGet("experience")]
        public async Task<IActionResult> GetExperience()
        {
            var items = await _context.ExperienceItems.ToListAsync();
            return Ok(items);
        }

        [HttpPost("experience")]
        public async Task<IActionResult> AddExperience([FromBody] ExperienceItem item)
        {
            if (item == null) return BadRequest("Invalid experience entry.");

            _context.ExperienceItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetExperience), new { id = item.Id }, item);
        }

        // ================= SKILLS =================
        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            var items = await _context.SkillItems.ToListAsync();
            return Ok(items);
        }

        [HttpPost("skills")]
        public async Task<IActionResult> AddSkill([FromBody] SkillItem item)
        {
            if (item == null) return BadRequest("Invalid skill entry.");

            _context.SkillItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSkills), new { id = item.Id }, item);
        }
        // POST: api/CvApi/calculate-tax
        [HttpPost("calculate-tax")]
        public async Task<IActionResult> CalculateTax([FromBody] TaxCalculationRequest request)
        {
            if (request == null || request.GrossAnnualIncome <= 0)
                return BadRequest("Gross annual income must be greater than zero.");

            decimal gross = request.GrossAnnualIncome;

            // Relief Allowance Calculation (CRA)
            decimal cra = gross > 200000
                ? Math.Max(200000, gross * 0.01m) + (gross * 0.20m)
                : gross * 0.20m;

            decimal taxableIncome = Math.Max(0, gross - cra);

            // Progressive Nigerian PITA Rates
            decimal tax = 0m;
            decimal remaining = taxableIncome;

            if (remaining > 0)
            {
                decimal band1 = Math.Min(remaining, 300000m);
                tax += band1 * 0.07m;
                remaining -= band1;
            }
            if (remaining > 0)
            {
                decimal band2 = Math.Min(remaining, 300000m);
                tax += band2 * 0.11m;
                remaining -= band2;
            }
            if (remaining > 0)
            {
                decimal band3 = Math.Min(remaining, 500000m);
                tax += band3 * 0.15m;
                remaining -= band3;
            }
            if (remaining > 0)
            {
                decimal band4 = Math.Min(remaining, 500000m);
                tax += band4 * 0.19m;
                remaining -= band4;
            }
            if (remaining > 0)
            {
                decimal band5 = Math.Min(remaining, 1600000m);
                tax += band5 * 0.21m;
                remaining -= band5;
            }
            if (remaining > 0)
            {
                tax += remaining * 0.24m;
            }

            // Populate your TaxCalculationResult class and persist to DB
            var result = new TaxCalculationResult
            {
                GrossIncome = gross,
                TaxableIncome = taxableIncome,
                EstimatedTax = tax,
                CalculatedAt = DateTime.UtcNow
            };

            _context.TaxCalculationResults.Add(result);
            await _context.SaveChangesAsync();

            return Ok(result);
        }

        // GET: api/CvApi/tax-history
        [HttpGet("tax-history")]
        public async Task<IActionResult> GetTaxHistory()
        {
            var history = await _context.TaxCalculationResults
                .OrderByDescending(t => t.CalculatedAt)
                .ToListAsync();

            return Ok(history);
        }
    }
}