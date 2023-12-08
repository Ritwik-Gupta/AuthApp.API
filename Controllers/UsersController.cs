using AuthApp.API.Domain;
using AuthApp.API.DTO;
using AuthApp.API.Helpers;
using AuthApp.API.MapperService;
using AuthApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Xml;

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
                var mappedUser = UserMapper.MapUserFromDTO(user);

                if(_context.Users.Any(x => x.Username == mappedUser.Username))
                {
                    return BadRequest("Username already taken");
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
            return BadRequest("Invalid request");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userCreds)
        {
            if(userCreds != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == userCreds.Username.ToLower() && x.IsDeleted == 0);

                if(user != null)
                {
                    var userSecret = await _context.UsersSecrets.FirstOrDefaultAsync(x => x.UserId == user.Id);

                    if(_hasher.VerifyPassword(userCreds.Password, user.Password, Convert.FromBase64String(userSecret.secretSalt)))
                    {
                        var token = ManageToken.GenerateJSONWebToken(user);
                        var refreshToken = ManageToken.GenerateRefreshToken();

                        //update the refresh token in the users table
                        user.Token = refreshToken;
                        await _context.SaveChangesAsync();

                        return Ok(new
                        {
                            Message = "User logged in successfully",
                            Token = token,
                            RefreshToken = refreshToken,
                            Id = user.Id,
                            Username = user.Username,
                            Role = user.RoleId.ToString()
                        });
                    }
                }
                return BadRequest("Invalid username or password");
            }
            return BadRequest("Invalid request");
        }

        [Authorize(Roles = "User,Admin,Visitor")]
        [HttpGet("get-users")]
        public async Task<IActionResult> AllUsers()
        {
            var users = await _context.Users.Where(x => x.IsDeleted == 0).Select(x => new
                {
                    id = x.Id,
                    fname = x.FirstName,
                    lname = x.LastName,
                    email = x.Email,
                    username = x.Username,
                    role = x.RoleId.ToString()
                }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Roles = "User,Admin,Visitor")]
        [HttpGet("get-user-detail")]
        public async Task<IActionResult> GetProfileDetailsforEdit(int userId)
        {
            if(userId > 0)
            {
                var user = await  _context.Users.Where(x => x.Id == userId && x.IsDeleted == 0).Select(x => new
                {
                    id = x.Id,
                    fname = x.FirstName,
                    lname = x.LastName,
                    email = x.Email,
                    username = x.Username,
                    role = x.RoleId.ToString()
                }).FirstOrDefaultAsync();
                return Ok(user);
            }
            return BadRequest();
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserViewDTO userObj)
        {
            if(userObj != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userObj.id && x.IsDeleted == 0);

                if (user == null)
                    return BadRequest("User doesnt exist");

                user.FirstName = userObj.fname;
                user.LastName = userObj.lname;
                user.Email = userObj.email;
                //user.Username = userObj.username;
                user.RoleId = RoleMapper.MapRoleFromDTO(userObj.role);
      
                await _context.SaveChangesAsync();
                return Ok(new { Message = "User details updated successfully" });
            }
            return NotFound("User doesnt exist");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-other-userdetail")]
        public async Task<IActionResult> GetOtherUserProfileDetailsforEdit(int userId)
        {
            if (userId > 0)
            {
                var user = await _context.Users.Where(x => x.Id == userId && x.IsDeleted == 0).Select(x => new
                {
                    id = x.Id,
                    fname = x.FirstName,
                    lname = x.LastName,
                    email = x.Email,
                    username = x.Username,
                    role = x.RoleId.ToString()
                }).FirstOrDefaultAsync();
                return Ok(user);
            }
            return BadRequest("Invalid request");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("delete-other-user")]
        public async Task<IActionResult> DeleteOtherUser(int userId)
        {
            if(userId > 0)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsDeleted == 0);

                if(user != null)
                {
                    user.IsDeleted = 1;
                    await _context.SaveChangesAsync();

                    return Ok(new { Message = "User deleted successfully" });
                }
                return BadRequest("User doesn't exist");
            }
            return BadRequest("Invalid request");
        }


        [HttpGet("try-refresh-token")]
        public async Task<IActionResult> TryAuthenticateWithRefreshToken(int userId)
        {
            const string HeaderKeyName = "refreshToken";

            Request.Headers.TryGetValue(HeaderKeyName, out StringValues refreshToken);

            if(refreshToken[0] != string.Empty)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Token == refreshToken[0] && x.IsDeleted == 0);

                if(user != null)
                {
                    //check if refresh token is correct
                    if(user.Token == refreshToken[0])
                    {
                        var token = ManageToken.GenerateJSONWebToken(user);
                        var refreshTokenNew = ManageToken.GenerateRefreshToken();

                        //update the refresh token in the users table
                        user.Token = refreshTokenNew;
                        await _context.SaveChangesAsync();

                        return Ok(new {
                            Message = "Session refreshed successfully",
                            Token = token,
                            RefreshToken = refreshToken,
                        });
                    }
                    return Forbid("Invalid Token");
                }
                return NotFound("Invalid Request");
            }
            return BadRequest("User doesnt exist");
        }
    }
}
