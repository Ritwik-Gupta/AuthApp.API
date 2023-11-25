using AuthApp.API.Domain;
using AuthApp.API.DTO;
using AuthApp.API.Helpers;
using AuthApp.API.MapperService;
using AuthApp.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AuthApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AuthAppDbContext _context;
        private readonly PasswordHasher _hasher;
        public UsersController(AuthAppDbContext dbContext, PasswordHasher hasher)
        {
            _context = dbContext;
            _hasher = hasher;
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> SaveUser([FromBody] UserDTO user)
        {
            if (user != null)
            {
                var mappedUser = UserMapper.MapFromDTO(user);

                if(_context.Users.Any(x => x.Username == mappedUser.Username))
                {
                    return BadRequest(new { Message = "Username already taken" });
                }

                //hash password before saving
                mappedUser.Password = _hasher.HashPasword(mappedUser.Password, out byte[] hash);
                mappedUser.SecretSalt = new UserSecret
                {
                    secretSalt = Convert.ToBase64String(hash)
                };
                await _context.Users.AddAsync(mappedUser);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Used added successfully"});
            }
            return BadRequest(new {Message = "Invalid request"});
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userCreds)
        {
            if(userCreds != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == userCreds.Username.ToLower());

                if(user != null)
                {
                    var userSecret = await _context.UsersSecrets.FirstOrDefaultAsync(x => x.UserId == user.Id);

                    if(_hasher.VerifyPassword(userCreds.Password, user.Password, Convert.FromBase64String(userSecret.secretSalt)))
                    {
                        return Ok(new { Message = "User logged in successfully" });
                    }
                }
                return BadRequest(new { Message = "Invalid username or password" });
            }
            return BadRequest(new { Message = "Invalid request" });
        }
    }
}
