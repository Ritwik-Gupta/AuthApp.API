using AuthApp.API.Domain;
using AuthApp.API.MapperService;
using AuthApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.API.Repository.Users
{
    public class UsersRepository: IUsersRepository
    {
        private readonly AuthAppDbContext _context;
        public UsersRepository(AuthAppDbContext context)
        {
            this._context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users.Where(_ => _.IsDeleted == 0).ToListAsync();
        }

        public async Task AddUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _context.Users.FirstAsync(_ => _.Id == userId && _.IsDeleted == 0);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstAsync(user => user.Username.ToLower() == username.ToLower() && user.IsDeleted == 0);
        }

        public async Task UpdateUser(User userObj)
        {
            User user = await _context.Users.FirstAsync(_ => _.Id == userObj.Id && userObj.IsDeleted == 0);

            if (user != null)
            {
                user.FirstName = userObj.FirstName;
                user.LastName = userObj.LastName;
                user.Email = userObj.Email;
                user.RoleId = userObj.RoleId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveUser(int userId)
        {
            User user = await _context.Users.FirstAsync(_ => _.Id == userId);

            if (user != null)
                user.IsDeleted = 1;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUsernameAlreadyTaken(string username)
        {
            return await _context.Users.AnyAsync(_ => _.Username == username);
        }

        public async Task<UserSecret> GetUserSerets(int userId)
        {
            return await _context.UsersSecrets.FirstAsync(_ => _.Id == userId);
        }

        public async Task UpdateRefreshToken(int userId, string refreshToken)
        {
            var user = await _context.Users.FirstAsync(x => x.Id == userId);

            if (user != null)
                user.Token = refreshToken;

            await _context.SaveChangesAsync();
        }

        public void Dispose() => _context.Dispose();
    }
}