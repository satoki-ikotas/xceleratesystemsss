using APIPSI16.Data;
using APIPSI16.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIPSI16.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly xcleratesystemslinks_SampleDBContext _context;

        public ChatController(xcleratesystemslinks_SampleDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: api/Chat
        [HttpGet]
        public async Task<IActionResult> GetChats()
        {
            var chats = await _context.Chats
                .Include(c => c.ChatUsers) // Include related participants
                .ToListAsync();
            return Ok(chats);
        }

        // GET: api/Chat/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChat(int id)
        {
            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .ThenInclude(cu => cu.User) // Include User information of each participant
                .FirstOrDefaultAsync(c => c.ChatId == id);

            if (chat == null) return NotFound();

            return Ok(chat);
        }

        // POST: api/Chat
        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] Chat chat)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChat), new { id = chat.ChatId }, chat);
        }

        // DELETE: api/Chat/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat == null) return NotFound();

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}