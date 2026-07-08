using EventManagement.Auth.Application.Interfaces;
using EventManagement.Auth.Domain.Models;
using EventManagement.Auth.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Auth.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Репозиторий пользователей на базе EF Core.
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
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
