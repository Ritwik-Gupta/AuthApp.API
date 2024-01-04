using AuthApp.API.Domain;
using AuthApp.API.DTO;
using AuthApp.API.Helpers;
using AuthApp.API.MapperService;
using AuthApp.API.Models;
using AuthApp.API.Repository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;


namespace AuthApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PasswordHasher _hasher;
        private readonly IUsersRepository _usersRepo;

        public UsersController(PasswordHasher hasher, IUsersRepository usersRepo)
        {
            _hasher = hasher;
            _usersRepo = usersRepo;
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> SaveUser([FromBody] UserDTO user)
        {
            if (user != null)
            {
                var mappedUser = UserMapper.MapFromUserDTO(user);

                if(await _usersRepo.IsUsernameAlreadyTaken(user.Username))
                    return BadRequest("Username already taken");

                //hash password before saving
                mappedUser.Password = _hasher.HashPasword(mappedUser.Password, out byte[] hash);
                mappedUser.SecretSalt = new UserSecret
                {
                    secretSalt = Convert.ToBase64String(hash)
                };
                await _usersRepo.AddUser(mappedUser);

                return Ok(new { Message = "Used added successfully"});
            }
            return BadRequest("Invalid request");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO userCreds)
        {
            if(userCreds != null)
            {
                var user = await _usersRepo.GetUserByUsername(userCreds.Username);

                if(user != null)
                {
                    var userSecret = await _usersRepo.GetUserSerets(user.Id);

                    if(_hasher.VerifyPassword(userCreds.Password, user.Password, Convert.FromBase64String(userSecret.secretSalt)))
                    {
                        var token = ManageToken.GenerateJSONWebToken(user);
                        var refreshToken = ManageToken.GenerateRefreshToken();

                        //update the refresh token in the users table
                        await _usersRepo.UpdateRefreshToken(user.Id, refreshToken);

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
            var users = await _usersRepo.GetAllUsers();
            var mappedUsers = users.AsQueryable().Select(_ => UserMapper.MapToUserViewDTO(_));

            return Ok(mappedUsers);
        }

        [Authorize(Roles = "User,Admin,Visitor")]
        [HttpGet("get-user-detail")]
        public async Task<IActionResult> GetProfileDetailsforEdit(int userId)
        {
            if(userId > 0)
            { 
                var user = await _usersRepo.GetUserById(userId);
                var mappedUser = MapperService.UserMapper.MapToUserViewDTO(user);
                return Ok(mappedUser);
            }
            return BadRequest();
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserViewDTO userObj)
        {
            if(userObj != null)
            {
                //var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userObj.id && x.IsDeleted == 0);
                var user = await _usersRepo.GetUserById(userObj.Id);

                if (user == null)
                    return BadRequest("User doesnt exist");

                var mappedUser = UserMapper.MapFromUserViewDTO(userObj);

                await _usersRepo.UpdateUser(mappedUser);
      
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
                var user = await _usersRepo.GetUserById(userId);
                var mappedUser = UserMapper.MapToUserViewDTO(user);

                return Ok(mappedUser);
            }
            return BadRequest("Invalid request");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("delete-other-user")]
        public async Task<IActionResult> DeleteOtherUser(int userId)
        {
            if(userId > 0)
            {
                await _usersRepo.RemoveUser(userId);
                return Ok("User deleted successfully");
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
                //var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsDeleted == 0);
                var user = await _usersRepo.GetUserById(userId);

                if(user != null)
                {
                    //check if refresh token is correct
                    if(user.Token == refreshToken[0])
                    {
                        var token = ManageToken.GenerateJSONWebToken(user);
                        var refreshTokenNew = ManageToken.GenerateRefreshToken();

                        //update the refresh token in the users table
                        await _usersRepo.UpdateRefreshToken(user.Id, refreshTokenNew);

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
