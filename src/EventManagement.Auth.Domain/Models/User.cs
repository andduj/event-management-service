using System;

namespace EventManagement.Auth.Domain.Models
{
    /// <summary>
    /// Пользователь системы.
    /// </summary>
    public class User
    {
        private User()
        {
            Login = null!;
            PasswordHash = null!;
        }

        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Уникальный логин пользователя.
        /// </summary>
        public string Login { get; private set; }

        /// <summary>
        /// Хеш пароля пользователя.
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Роль пользователя.
        /// </summary>
        public UserRole Role { get; private set; }

        /// <summary>
        /// Создает нового пользователя.
        /// </summary>
        public static User Create(string login, string passwordHash, UserRole role = UserRole.User)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentException("Логин не может быть пустым.", nameof(login));
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Хеш пароля не может быть пустым.", nameof(passwordHash));
            }

            return new User
            {
                Id = Guid.NewGuid(),
                Login = login.Trim(),
                PasswordHash = passwordHash,
                Role = role,
            };
        }
    }
}
