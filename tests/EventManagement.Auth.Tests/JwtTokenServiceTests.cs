using EventManagement.Auth.Application.Options;
using EventManagement.Auth.Domain.Models;
using EventManagement.Auth.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace EventManagement.Auth.Tests
{
    public class JwtTokenServiceTests
    {
        private static readonly JwtSettings Settings = new()
        {
            Secret = "EventManagementSprint8DevSecretKey_Min32Chars!",
            Issuer = "EventManagement.Auth",
            Audience = "EventManagement",
            LifetimeMinutes = 60
        };

        [Fact]
        public void GenerateToken_ShouldContainSubRoleAndPassValidation()
        {
            var user = User.Create("jwt-user", "hash", UserRole.Admin);
            var service = new JwtTokenService(Options.Create(Settings));
            string token = service.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value
                .Should().Be(user.Id.ToString());
            jwt.Claims.First(claim => claim.Type == ClaimTypes.Role).Value
                .Should().Be(UserRole.Admin.ToString());

            handler.ValidateToken(token, CreateValidationParameters(), out SecurityToken _);
        }

        [Fact]
        public void GenerateToken_WithWrongSecret_ShouldFailValidation()
        {
            var user = User.Create("jwt-user", "hash");
            var service = new JwtTokenService(Options.Create(Settings));
            string token = service.GenerateToken(user);

            var invalidParameters = CreateValidationParameters();
            invalidParameters.IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("AnotherSecretKey_ForTests_Min32Chars!"));

            var action = () => new JwtSecurityTokenHandler().ValidateToken(token, invalidParameters, out _);

            action.Should().Throw<SecurityTokenException>();
        }

        private static TokenValidationParameters CreateValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Settings.Issuer,
                ValidAudience = Settings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Secret)),
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = ClaimTypes.Role,
            };
        }
    }
}
