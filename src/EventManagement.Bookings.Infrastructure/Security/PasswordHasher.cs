using EventManagement.Bookings.Application.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EventManagement.Bookings.Infrastructure.Security
{
    /// <summary>
    /// Хеширование паролей по алгоритму SHA-256.
    /// </summary>
    public sealed class PasswordHasher : IPasswordHasher
    {
        /// <inheritdoc/>
        public string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        /// <inheritdoc/>
        public bool Verify(string password, string passwordHash)
        {
            return Hash(password) == passwordHash;
        }
    }
}
