using APIPSI16.Data;
using APIPSI16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIPSI16.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatUsersController : ControllerBase
    {
        private readonly xcleratesystemslinks_SampleDBContext _context;

        public ChatUsersController(xcleratesystemslinks_SampleDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: api/ChatUsers
        [HttpGet]
        public async Task<IActionResult> GetChatUsers()
        {
            var chatUsers = await _context.ChatUsers
                .Include(cu => cu.Chat) // Include the associated chat
                .Include(cu => cu.User) // Include the associated user
                .ToListAsync();

            return Ok(chatUsers);
        }

        // GET: api/ChatUsers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatUser(int id)
        {
            var chatUser = await _context.ChatUsers
                .Include(cu => cu.Chat)
                .Include(cu => cu.User)
                .FirstOrDefaultAsync(cu => cu.ChatUserId == id);

            if (chatUser == null) return NotFound();

            return Ok(chatUser);
        }

        // POST: api/ChatUsers
        [HttpPost]
        public async Task<IActionResult> AddUserToChat([FromBody] ChatUser chatUser)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Add(chatUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChatUser), new { id = chatUser.ChatUserId }, chatUser);
        }

        // DELETE: api/ChatUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveUserFromChat(int id)
        {
            var chatUser = await _context.ChatUsers.FindAsync(id);
            if (chatUser == null) return NotFound();

            _context.ChatUsers.Remove(chatUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}