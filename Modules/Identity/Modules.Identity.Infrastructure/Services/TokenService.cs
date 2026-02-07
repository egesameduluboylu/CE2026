using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Modules.Identity.Infrastructure.Services
{
    public class TokenService
    {
        private readonly IConfiguration _cfg;

        public TokenService(IConfiguration cfg) => _cfg = cfg;

        public string CreateAccessToken(Guid userId, string email)
        {
            var issuer = _cfg["Jwt:Issuer"]!;
            var audience = _cfg["Jwt:Audience"]!;
            var key = _cfg["Jwt:Key"]!;
            var minutes = int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "15");

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, email)
        };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string rawToken, string tokenHash) CreateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var raw = Base64UrlEncoder.Encode(bytes);
            var hash = Sha256(raw);
            return (raw, hash);
        }

        public string HashRefreshToken(string raw) => Sha256(raw);

        private static string Sha256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // uppercase hex
        }
    }
}
