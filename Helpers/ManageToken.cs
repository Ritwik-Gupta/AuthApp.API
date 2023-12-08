using AuthApp.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace AuthApp.API.Helpers
{
    public class ManageToken
    {
        public static string GenerateJSONWebToken(User userInfo)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtSecrets:SecretKey")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(configuration.GetValue<string>("JwtSecrets:Issuer"),
              configuration.GetValue<string>("JwtSecrets:Issuer"),
              claims: new[] { new Claim(ClaimTypes.Role, userInfo.RoleId.ToString()) },
              expires: DateTime.Now.AddMinutes(2), //token valid for 60 minutes after login
              signingCredentials: credentials);

            //Console.WriteLine(token.ValidTo.ToShortTimeString());

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateRefreshToken()
        {
            //generate a random string 
            var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var resultToken = new string(
               Enumerable.Repeat(allChar, 8)
               .Select(token => token[random.Next(token.Length)]).ToArray());

            string authToken = resultToken.ToString();

            return authToken;
        }

    }
}
