using AuthApp.API.Domain;
using AuthApp.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AuthAppDbContext _context;
        public UsersController(AuthAppDbContext dbContext)
        {
            _context = dbContext;
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> SaveUser([FromBody] User user)
        {
            if (user != null)
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
    }
}
