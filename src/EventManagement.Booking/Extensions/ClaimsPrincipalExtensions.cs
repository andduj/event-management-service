using EventManagement.Bookings.Domain.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventManagement.Bookings.Extensions
{
    /// <summary>
    /// Расширения для получения данных пользователя из claims.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Возвращает идентификатор текущего пользователя.
        /// </summary>
        public static Guid GetId(this ClaimsPrincipal user)
        {
            string? subject = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (subject is null || !Guid.TryParse(subject, out Guid userId))
            {
                throw new UnauthorizedAccessException();
            }

            return userId;
        }

        /// <summary>
        /// Возвращает роль текущего пользователя.
        /// </summary>
        public static UserRole GetRole(this ClaimsPrincipal user)
        {
            string? role = user.FindFirstValue(ClaimTypes.Role);
            if (role is null || !Enum.TryParse(role, out UserRole userRole))
            {
                throw new UnauthorizedAccessException();
            }

            return userRole;
        }
    }
}
