using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Extensions;
using FluentAssertions;
using System;
using System.Security.Claims;

namespace EventManagement.Bookings.Tests
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void GetId_WhenSubClaimPresent_ShouldReturnUserId()
        {
            var userId = Guid.NewGuid();
            var principal = CreatePrincipal(
                new Claim("sub", userId.ToString()),
                new Claim(ClaimTypes.Role, UserRole.User.ToString()));

            principal.GetId().Should().Be(userId);
        }

        [Fact]
        public void GetRole_WhenRoleClaimPresent_ShouldReturnUserRole()
        {
            var principal = CreatePrincipal(
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, UserRole.Admin.ToString()));

            principal.GetRole().Should().Be(UserRole.Admin);
        }

        [Fact]
        public void GetId_WhenSubClaimMissing_ShouldThrowUnauthorizedAccessException()
        {
            var principal = CreatePrincipal(new Claim(ClaimTypes.Role, UserRole.User.ToString()));

            var action = () => principal.GetId();

            action.Should().Throw<UnauthorizedAccessException>();
        }

        private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, authenticationType: "Bearer");
            return new ClaimsPrincipal(identity);
        }
    }
}
