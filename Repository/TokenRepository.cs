using AuthApp.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.API.Repository
{
    public class TokenRepository: ITokenRepository
    {
        private readonly AuthAppDbContext _context;

        public TokenRepository(AuthAppDbContext context)
        {
            this._context = context;
        }

        public async Task UpdateRefreshToken(int userId, string refreshToken)
        {
            var user = await _context.Users.FirstAsync(_ => _.Id == userId);
            user.Token = refreshToken;

            await _context.SaveChangesAsync();
        }
    }
}
