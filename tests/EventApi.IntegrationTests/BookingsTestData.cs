using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.Data.Repositories;
using EventManagement.Bookings.Infrastructure.DataAccess;

namespace EventApi.IntegrationTests;

internal static class BookingsTestData
{
    public static async Task<User> CreateUserAsync(BookingsDbContext context)
    {
        var userRepository = new UserRepository(context);
        var user = User.Create($"user-{Guid.NewGuid():N}", "HASH", UserRole.User);
        return await userRepository.CreateAsync(user);
    }
}
