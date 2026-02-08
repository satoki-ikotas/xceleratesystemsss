using APIPSI16.Data;
using APIPSI16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIPSI16.Controllers
{
    public class CompaniesControllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class CompaniesController : ControllerBase
        {
            private readonly xcleratesystemslinks_SampleDBContext _context;

            public CompaniesController(xcleratesystemslinks_SampleDBContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            // GET: api/Companies
            [HttpGet]
            public async Task<IActionResult> GetCompanies()
            {
                return Ok(await _context.Companies.ToListAsync());
            }

            // GET: api/Companies/5
            [HttpGet("{id}")]
            public async Task<IActionResult> GetCompany(int id)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null) return NotFound();
                return Ok(company);
            }

            // POST: api/Companies
            [HttpPost]
            public async Task<IActionResult> CreateCompany([FromBody] Company company)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCompany), new { id = company.CompanyId }, company);
            }

            // PUT: api/Companies/5
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateCompany(int id, [FromBody] Company company)
            {
                if (id != company.CompanyId) return BadRequest();

                _context.Entry(company).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(id)) return NotFound();
                    throw;
                }

                return NoContent();
            }

            // DELETE: api/Companies/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteCompany(int id)
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null) return NotFound();

                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            private bool CompanyExists(int id)
            {
                return _context.Companies.Any(c => c.CompanyId == id);
            }
        }
    }
}
