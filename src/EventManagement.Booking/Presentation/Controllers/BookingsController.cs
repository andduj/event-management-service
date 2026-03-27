using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Exceptions;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Presentation.Controllers
{
    /// <summary>
    /// Контроллер для получения информации о бронированиях.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
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
        /// <response code="404">Событие с указанным id не найдено.</response>
        [HttpPost("events/{id:guid}/book")]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateBookingAsync(Guid id)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(id);

                return Accepted($"/bookings/{booking.Id}", new { booking.Id, booking.EventId, booking.Status });
            }
            catch (ApiException exception) when (exception.StatusCode == StatusCodes.Status404NotFound)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Получает текущее состояние брони по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор брони.</param>
        /// <returns>Информация о брони.</returns>
        /// <response code="200">Информация о брони успешно получена.</response>
        /// <response code="404">Бронь с указанным id не найдена.</response>
        [HttpGet("bookings/{id:guid}")]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetBookingByIdAsync(Guid id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                return Ok(booking);
            }
            catch (BookingNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
