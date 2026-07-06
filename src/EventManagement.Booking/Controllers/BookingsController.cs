using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Controllers
{
    /// <summary>
    /// Контроллер для получения информации о бронированиях.
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера бронирований.
        /// </summary>
        /// <param name="bookingService">Сервис бронирований.</param>
        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Создает бронь для указанного события.
        /// </summary>
        /// <param name="id">Идентификатор события.</param>
        /// <returns>Информация о созданной брони.</returns>
        /// <response code="202">Бронь принята в обработку.</response>
        /// <response code="400">Событие уже началось.</response>
        /// <response code="401">Требуется аутентификация.</response>
        /// <response code="404">Событие с указанным id не найдено.</response>
        /// <response code="409">Нет свободных мест или превышен лимит броней.</response>
        [HttpPost("events/{id:guid}/book")]
        [ProducesResponseType(typeof(BookingInfo), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateBookingAsync(Guid id)
        {
            var bookingInfo = await _bookingService.CreateBookingAsync(id, User.GetId());
            return Accepted($"/api/v1/bookings/{bookingInfo.Id}", bookingInfo);
        }

        /// <summary>
        /// Получает текущее состояние брони по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор брони.</param>
        /// <returns>Информация о брони.</returns>
        /// <response code="200">Информация о брони успешно получена.</response>
        /// <response code="401">Требуется аутентификация.</response>
        /// <response code="404">Бронь с указанным id не найдена.</response>
        [HttpGet("bookings/{id:guid}")]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetBookingByIdAsync(Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            return Ok(booking);
        }

        /// <summary>
        /// Отменяет бронь.
        /// </summary>
        /// <param name="id">Идентификатор брони.</param>
        /// <response code="204">Бронь успешно отменена.</response>
        /// <response code="401">Требуется аутентификация.</response>
        /// <response code="403">Недостаточно прав.</response>
        /// <response code="404">Бронь не найдена.</response>
        [HttpDelete("bookings/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelBookingAsync(Guid id)
        {
            await _bookingService.CancelBookingAsync(id, User.GetId(), User.GetRole());
            return NoContent();
        }
    }
}
