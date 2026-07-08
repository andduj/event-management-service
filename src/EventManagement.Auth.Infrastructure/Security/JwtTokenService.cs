using EventManagement.Auth.Application.Interfaces;
using EventManagement.Auth.Application.Options;
using EventManagement.Auth.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventManagement.Auth.Infrastructure.Security
{
    /// <summary>
    /// Генерация подписанных JWT-токенов.
    /// </summary>
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        /// <inheritdoc/>
        public string GenerateToken(User user)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.LifetimeMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
