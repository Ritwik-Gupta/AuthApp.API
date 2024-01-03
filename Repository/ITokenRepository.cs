namespace AuthApp.API.Repository
{
    public interface ITokenRepository
    {
        Task UpdateRefreshToken(int userId, string refreshToken);
    }
}
