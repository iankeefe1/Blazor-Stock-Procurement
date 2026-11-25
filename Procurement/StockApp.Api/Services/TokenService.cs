using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StockApp.Data.Entities;

namespace StockApp.Api.Services
{
    public class TokenService(IOptions<JwtOptions> options)
    {
        private readonly JwtOptions _o = options.Value;

        public (string Token, DateTime ExpiresAt) Create(AppUser user, IEnumerable<string> roles)
        {
            var expires = DateTime.UtcNow.AddMinutes(_o.ExpiryMinutes);

            var claims = new List<Claim>
            {
                new("sub", user.Id),
                new("name", user.UserName!),
                new("full_name", user.FullName),
                new("jti", Guid.NewGuid().ToString()),
            };
            claims.AddRange(roles.Select(r => new Claim("role", r)));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _o.Issuer,
                Audience = _o.Audience,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.Key)),
                    SecurityAlgorithms.HmacSha256),
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
        }
    }
}