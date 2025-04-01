using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace ServerForTest.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(string username);
    }

    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        public TokenService(IConfiguration config)
        {
            _secretKey = config["Jwt:Key"];
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured properly.");
            }
        }




        public string GenerateJwtToken(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username  cannot be null or empty.");
            }


            var salt = GenerateSalt();
            var combinedKey = _secretKey + salt;


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(combinedKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            Console.WriteLine("Сгенерованний токен: " + jwt);

            return jwt;
        }

        private string GenerateSalt()
        {
            var random = new Random();
            byte[] saltBytes = new byte[16];
            random.NextBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}
