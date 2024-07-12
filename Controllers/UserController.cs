using Microsoft.AspNetCore.Mvc;
using webapi_todolist.Models;
using webapi_todolist.Data;
using webapi_todolist.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
namespace webapi_todolist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;


        public UserController(ApplicationDbContext context, JwtService jwtService, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Vérifiez si l'utilisateur existe déjà
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        errors = new Dictionary<string, string[]>
                        {
                            { "username", new[] { "Ce nom d'utilisateur est déjà utilisé." } }
                        }
                    });
                }

                // Ajoutez l'utilisateur à la base de données
                user.RefreshToken = _jwtService.GenerateRefreshToken(); // Génère un refresh token
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Durée de validité du refresh token

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Générez un jeton JWT pour l'utilisateur nouvellement inscrit
                var token = _jwtService.GenerateToken(user);

                return Ok(new { Token = token });
            }

            return BadRequest(ModelState);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username && u.Password == user.Password);

            if (existingUser != null)
            {
                // Générez un jeton JWT pour l'utilisateur authentifié
                var token = _jwtService.GenerateToken(existingUser);
                var refreshToken = _jwtService.GenerateRefreshToken();
                _jwtService.SaveRefreshToken(user, refreshToken);

                return Ok(new { Token = token });
            }

            return Unauthorized("Nom d'utilisateur ou mot de passe incorrect.");
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshRequest.RefreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Génération du nouveau token
            var newToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            _jwtService.SaveRefreshToken(user, newRefreshToken);

            return Ok(new { Token = newToken, RefreshToken = newRefreshToken });
        }

        public class RefreshRequest
        {
            public string RefreshToken { get; set; }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (GetUser == null)
            {
                return NotFound();
            }
            return user;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            user.Id = 0;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUser", new { id = user.Id }, user);

        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            _context.Entry(user).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();

        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
