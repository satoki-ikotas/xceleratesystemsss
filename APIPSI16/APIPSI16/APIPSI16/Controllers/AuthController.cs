using APIPSI16.Data;
using APIPSI16.Models;
using APIPSI16.Services; // ITokenService
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // for IPasswordHasher
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace APIPSI16.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly xcleratesystemslinks_SampleDBContext _db;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            xcleratesystemslinks_SampleDBContext db,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthController> logger)   // injected
        {
            _db = db;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }


        // DTOs
        public class LoginRequest
        {
            public string Username { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        public class RegisterRequest
        {
            public string Name { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string? PhoneNumber { get; set; }
            public int? Nationality { get; set; }
            public int? JobPreference { get; set; }
            public string? ProfileBio { get; set; }
            public DateOnly? DoB { get; set; }
            public int? Role { get; set; } = 1; // default to regular user
            public string Password { get; set; } = default!;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("username and password required");

            var normalized = req.Username.Trim().ToLowerInvariant();

            // Normalize comparisons to lowercase so email/name case doesn't cause misses.
            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    (u.Email != null && u.Email.ToLower() == normalized) ||
                    (u.Name != null && u.Name.ToLower() == normalized));

            if (user == null)
            {
                _logger.LogInformation("Login failed: user not found for '{Username}'", req.Username);
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                _logger.LogWarning("Login failed: user {UserId} has empty password hash", user.UserId);
                return Unauthorized();
            }

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            _logger.LogDebug("Password verification result for user {UserId}: {Result}", user.UserId, verify.ToString());

            if (verify == PasswordVerificationResult.Failed)
            {
                // wrong password
                return Unauthorized();
            }

            // If rehash recommended, persist new hash (still considered successful login)
            if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, req.Password);
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Password rehashed for user {UserId}", user.UserId);
            }

            var claims = new[]
            {
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Name ?? user.Email ?? string.Empty),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserId.ToString()),
    };

            var token = _tokenService.CreateToken(user.UserId.ToString(), claims);
            var expires = _tokenService.GetLastExpiry();

            return Ok(new { token, expiresAt = expires });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            // Basic validation (extend with proper ModelState & data annotations as needed)
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and password are required.");

            // Prevent duplicate email
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return Conflict("Email already in use.");

            // Create user entity (do NOT store plaintext password)
            var user = new User
            {
                Name = req.Name,
                Email = req.Email,
                PhoneNumber = req.PhoneNumber,
                Nationality = req.Nationality,
                JobPreference = req.JobPreference,
                ProfileBio = req.ProfileBio,
                DoB = req.DoB,
                Role = req.Role
            };

            // Hash the password server-side
            user.PasswordHash = _passwordHasher.HashPassword(user, req.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Return minimal result (do not return the PasswordHash)
            return CreatedAtAction(nameof(Register), new { id = user.UserId }, new { id = user.UserId, email = user.Email });
        }

        [HttpPost("login-debug-verify")]
        public async Task<IActionResult> LoginDebugVerify([FromBody] LoginRequest req)
        {
            if (req == null) return BadRequest("Request body required.");
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Username and password required.");

            try
            {
                var user = await _db.Users.SingleOrDefaultAsync(u =>
                    u.Name == req.Username || u.Email == req.Username);

                if (user == null)
                {
                    _logger.LogInformation("DebugVerify: user not found: {Username}", req.Username);
                    return Unauthorized(new { error = "Invalid credentials" });
                }

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("DebugVerify: user {Id} has no PasswordHash.", user.UserId);
                    return Unauthorized(new { error = "Invalid credentials" });
                }

                var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
                _logger.LogInformation("DebugVerify: Verify result for user {Id}: {Result}", user.UserId, verify);

                return Ok(new { verified = (verify != PasswordVerificationResult.Failed), result = verify.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DebugVerify: unhandled exception");
                throw; // keep default behavior for now so you get full stacktrace in console
            }
        }

        [HttpPost("login-debug-token")]
        public IActionResult LoginDebugToken([FromBody] object _ = null)
        {
            try
            {
                // Create a minimal claims set for testing
                var claims = new[]
                {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "debug"),
            new System.Security.Claims.Claim("userid", "9999")
        };

                var token = _tokenService.CreateToken("9999", claims);
                var expires = _tokenService.GetLastExpiry();

                // Return the actual token so you can copy it into Swagger's Authorize dialog
                return Ok(new { token, expiresAt = expires });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DebugToken: token creation failed");
                return Problem(detail: ex.ToString(), title: "Token generation failed", statusCode: 500);
            }
        }
    }
}