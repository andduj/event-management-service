using EventManagement.Auth.Domain.Models;
using EventManagement.Auth.Infrastructure.Data.Repositories;
using EventManagement.Auth.Infrastructure.DataAccess;
using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(PostgresDbFixture.CollectionName)]
public sealed class UserTests
{
    private readonly PostgresDbFixture _fixture;

    public UserTests(PostgresDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateAsync_Persists_User()
    {
        await _fixture.ResetAsync();
        await using var ctx = new AuthDbContext(_fixture.CreateAuthOptions());
        var repo = new UserRepository(ctx);
        var user = User.Create($"user-{Guid.NewGuid():N}", "HASH", UserRole.User);

        var saved = await repo.CreateAsync(user);

        Assert.Equal(user.Id, saved.Id);
        Assert.Equal(user.Login, saved.Login);

        await using var verifyCtx = new AuthDbContext(_fixture.CreateAuthOptions());
        var fromDb = await verifyCtx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == saved.Id);
        Assert.NotNull(fromDb);
    }

    [Fact]
    public async Task FindByLoginAsync_Returns_User()
    {
        await _fixture.ResetAsync();
        await using var ctx = new AuthDbContext(_fixture.CreateAuthOptions());
        var repo = new UserRepository(ctx);
        var user = User.Create($"user-{Guid.NewGuid():N}", "HASH", UserRole.User);
        await repo.CreateAsync(user);

        await using var actCtx = new AuthDbContext(_fixture.CreateAuthOptions());
        var found = await new UserRepository(actCtx).FindByLoginAsync(user.Login);

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task ExistsByLoginAsync_Returns_True_When_Login_Exists()
    {
        await _fixture.ResetAsync();
        await using var ctx = new AuthDbContext(_fixture.CreateAuthOptions());
        var repo = new UserRepository(ctx);
        var user = User.Create($"user-{Guid.NewGuid():N}", "HASH", UserRole.User);
        await repo.CreateAsync(user);

        await using var actCtx = new AuthDbContext(_fixture.CreateAuthOptions());
        var exists = await new UserRepository(actCtx).ExistsByLoginAsync(user.Login);

        Assert.True(exists);
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Login_Not_Unique()
    {
        await _fixture.ResetAsync();
        await using var ctx = new AuthDbContext(_fixture.CreateAuthOptions());
        var repo = new UserRepository(ctx);
        const string login = "duplicate-login";
        await repo.CreateAsync(User.Create(login, "HASH", UserRole.User));

        await using var actCtx = new AuthDbContext(_fixture.CreateAuthOptions());
        var action = () => new UserRepository(actCtx).CreateAsync(User.Create(login, "OTHER", UserRole.User));

        await Assert.ThrowsAsync<DbUpdateException>(action);
    }
}
