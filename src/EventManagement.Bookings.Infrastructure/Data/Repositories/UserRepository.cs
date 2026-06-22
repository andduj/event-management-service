using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Репозиторий пользователей на базе EF Core.
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly BookingsDbContext _context;

        public UserRepository(BookingsDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        /// <inheritdoc/>
        public async Task<User?> FindByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(user => user.Login == login, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(user => user.Login == login, cancellationToken);
        }
    }
}
