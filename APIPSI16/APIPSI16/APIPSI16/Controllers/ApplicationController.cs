using APIPSI16.Data;
using APIPSI16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace APIPSI16.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly xcleratesystemslinks_SampleDBContext _context;

        public ApplicationController(xcleratesystemslinks_SampleDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: api/Application
        [HttpGet]
        public async Task<IActionResult> GetApplications()
        {
            var applications = await _context.Applications
                .Include(a => a.Opportunity)
                .Include(a => a.User)
                .ToListAsync();
            return Ok(applications); // API response
        }

        // GET: api/Application/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplication(int id)
        {
            var application = await _context.Applications
                .Include(a => a.Opportunity)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (application == null) return NotFound();

            return Ok(application);
        }

        // POST: api/Application
        [HttpPost]
        public async Task<IActionResult> CreateApplication([FromBody] Application application)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
        }

        // PUT: api/Application/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] Application application)
        {
            if (id != application.Id) return BadRequest();

            _context.Entry(application).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationExists(id)) return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Application/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApplicationExists(int id)
        {
            return _context.Applications.Any(e => e.Id == id);
        }
    }
}

