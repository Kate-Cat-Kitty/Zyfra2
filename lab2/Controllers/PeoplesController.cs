using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System;

namespace lab2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeoplesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PeoplesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/peoples
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.Select(u => u.Username).ToListAsync();
            return Ok(users);
        }

        // POST /api/peoples/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Пользователь уже существует.");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = PasswordHasher.HashPassword(request.PasswordHash),
                Status = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Пользователь зарегистрирован.");
        }

        // POST /api/peoples/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || user.PasswordHash != PasswordHasher.HashPassword(request.PasswordHash))
                return Unauthorized("Неверный логин или пароль.");

            if (user.Status == 1)
                return Conflict("Пользователь уже вошел в систему.");

            var session = new Session
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = user.Id
            };

            user.Status = 1;

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { SessionId = session.SessionId, user.Username });
        }

        // POST /api/peoples/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionId == request.SessionId);

            if (session == null)
                return NotFound("Сессия не найдена.");

            session.User.Status = 0;

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok($"Пользователь {session.User.Username} вышел из системы.");
        }

        // GET /api/peoples/session
        [HttpGet("session")]
        public async Task<IActionResult> GetUserSession([FromQuery] string username, [FromQuery] string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.PasswordHash != PasswordHasher.HashPassword(password))
                return Unauthorized("Неверный логин или пароль.");

            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (session == null)
                return NotFound("Активной сессии нет.");

            return Ok(new { SessionId = session.SessionId });
        }
    }
}
