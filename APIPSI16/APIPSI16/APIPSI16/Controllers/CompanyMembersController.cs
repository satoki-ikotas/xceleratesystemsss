using APIPSI16.Data;
using APIPSI16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIPSI16.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyMembersController : ControllerBase
    {
        private readonly xcleratesystemslinks_SampleDBContext _context;

        public CompanyMembersController(xcleratesystemslinks_SampleDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: api/CompanyMembers
        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            var members = await _context.CompanyMembers
                .Include(cm => cm.Company) // Include related company
                .Include(cm => cm.User)   // Include related user
                .ToListAsync();
            return Ok(members);
        }

        // GET: api/CompanyMembers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(int id)
        {
            var member = await _context.CompanyMembers
                .Include(cm => cm.Company)
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(m => m.CompanyMemberId == id);

            if (member == null) return NotFound();

            return Ok(member);
        }

        // POST: api/CompanyMembers
        [HttpPost]
        public async Task<IActionResult> AddMember([FromBody] CompanyMember member)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.CompanyMembers.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMember), new { id = member.CompanyMemberId }, member);
        }

        // DELETE: api/CompanyMembers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveMember(int id)
        {
            var member = await _context.CompanyMembers.FindAsync(id);
            if (member == null) return NotFound();

            _context.CompanyMembers.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}