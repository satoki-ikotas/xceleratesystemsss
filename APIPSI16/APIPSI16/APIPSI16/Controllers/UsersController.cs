using APIPSI16.Data;
using APIPSI16.Models;
using APIPSI16.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIPSI16.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly xcleratesystemslinks_SampleDBContext _context;

        public UsersController(xcleratesystemslinks_SampleDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.UserId) return BadRequest();

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(u => u.UserId == id);
        }

        // GET: api/Users/byNationality/{id}
        // Returns users filtered by nationality (no test/sample data)
        [HttpGet("byNationality/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetByNationality(int id)
        {
            var users = await _context.Users
                .Where(u => u.Nationality == id)
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Nationality = u.Nationality,
                    JobPreference = u.JobPreference
                    // intentionally do NOT include password/hash
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/byJobRole/{id}
        // Returns users filtered by job preference/role
        [HttpGet("byJobRole/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetByJobRole(int id)
        {
            var users = await _context.Users
                .Where(u => u.JobPreference == id)
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Nationality = u.Nationality,
                    JobPreference = u.JobPreference
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}