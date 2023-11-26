using AuthApp.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
