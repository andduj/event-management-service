using EventManagement.Auth.Application.Interfaces;
using EventManagement.Auth.Application.Services;
using EventManagement.Auth.Domain.Exceptions;
using EventManagement.Auth.Domain.Models;
using EventManagement.Auth.Infrastructure.Data.Repositories;
using EventManagement.Auth.Infrastructure.DataAccess;
using EventManagement.Auth.Infrastructure.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventManagement.Auth.Tests
{
    public class AuthServiceTests
    {
        private static (AuthService AuthService, IUserRepository UserRepository, Mock<IJwtTokenService> JwtTokenService, PasswordHasher PasswordHasher) CreateSut()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AuthDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IUserRepository, UserRepository>();

            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var passwordHasher = new PasswordHasher();
            var jwtTokenService = new Mock<IJwtTokenService>();
            jwtTokenService
                .Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns("test-jwt-token");

            var authService = new AuthService(userRepository, passwordHasher, jwtTokenService.Object);
            return (authService, userRepository, jwtTokenService, passwordHasher);
        }

        [Fact]
        public async Task RegisterAsync_ShouldStorePasswordHashNotPlainText()
        {
            var (authService, userRepository, _, passwordHasher) = CreateSut();
            const string password = "secret-password";

            await authService.RegisterAsync("test-user", password);

            var user = await userRepository.FindByLoginAsync("test-user");
            user.Should().NotBeNull();
            user!.PasswordHash.Should().Be(passwordHasher.Hash(password));
            user.PasswordHash.Should().NotBe(password);
        }

        [Fact]
        public async Task RegisterAsync_WhenLoginAlreadyExists_ShouldThrowLoginAlreadyExistsException()
        {
            var (authService, _, _, _) = CreateSut();
            await authService.RegisterAsync("duplicate-user", "password");

            var action = () => authService.RegisterAsync("duplicate-user", "other-password");

            await action.Should().ThrowAsync<LoginAlreadyExistsException>();
        }

        [Fact]
        public async Task RegisterAsync_WhenLoginIsEmpty_ShouldThrowArgumentException()
        {
            var (authService, _, _, _) = CreateSut();

            var action = () => authService.RegisterAsync("  ", "password");

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
        {
            var (authService, _, jwtTokenService, _) = CreateSut();
            await authService.RegisterAsync("login-user", "password");

            var result = await authService.LoginAsync("login-user", "password");

            result.Token.Should().Be("test-jwt-token");
            jwtTokenService.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
        {
            var (authService, _, _, _) = CreateSut();
            await authService.RegisterAsync("login-user", "password");

            var action = () => authService.LoginAsync("login-user", "wrong-password");

            await action.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ShouldThrowInvalidCredentialsException()
        {
            var (authService, _, _, _) = CreateSut();

            var action = () => authService.LoginAsync("missing-user", "password");

            await action.Should().ThrowAsync<InvalidCredentialsException>();
        }
    }
}
