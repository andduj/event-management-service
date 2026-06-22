using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Bookings.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Services
{
    /// <summary>
    /// Сервис регистрации и аутентификации пользователей.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        /// <inheritdoc/>
        public async Task RegisterAsync(
            string login,
            string password,
            UserRole role = UserRole.User,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentException("Логин не может быть пустым.", nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));
            }

            login = login.Trim();

            if (await _userRepository.ExistsByLoginAsync(login, cancellationToken))
            {
                throw new LoginAlreadyExistsException();
            }

            string passwordHash = _passwordHasher.Hash(password);
            var user = User.Create(login, passwordHash, role);
            await _userRepository.CreateAsync(user, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<AuthTokenDto> LoginAsync(
            string login,
            string password,
            CancellationToken cancellationToken = default)
        {
            login = login.Trim();

            var user = await _userRepository.FindByLoginAsync(login, cancellationToken);
            if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            return new AuthTokenDto
            {
                Token = _jwtTokenService.GenerateToken(user),
            };
        }
    }
}
