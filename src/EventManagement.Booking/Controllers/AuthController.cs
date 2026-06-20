using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Controllers
{
    /// <summary>
    /// Контроллер аутентификации.
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        /// <param name="request">Данные регистрации.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="204">Пользователь успешно зарегистрирован.</response>
        /// <response code="400">Ошибка валидации.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.RegisterAsync(
                request.Login,
                request.Password,
                request.Role ?? UserRole.User,
                cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Выполняет вход и возвращает JWT-токен.
        /// </summary>
        /// <param name="request">Учётные данные.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Успешный вход.</response>
        /// <response code="404">Неверные учётные данные.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthTokenDto>> LoginAsync(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var token = await _authService.LoginAsync(request.Login, request.Password, cancellationToken);
            return Ok(token);
        }
    }
}
