using AuthApp.API.Models;

namespace AuthApp.API.Repository.Users
{
    public interface IUsersRepository: IDisposable
    {
        Task<IEnumerable<User>> GetAllUsers();

        Task AddUser(User user);

        Task<User> GetUserById(int id);

        Task UpdateUser(User user);

        Task RemoveUser(int id);

        Task<User> GetUserByUsername(string username);

        Task<bool> IsUsernameAlreadyTaken(string username);

        Task<UserSecret> GetUserSerets(int userId);

        Task UpdateRefreshToken(int userId, string refreshToken);
    }
}