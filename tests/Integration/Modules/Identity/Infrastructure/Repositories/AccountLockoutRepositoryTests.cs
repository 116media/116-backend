using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IAccountLockoutRepository"/> exercising the atomic
/// set-based counter updates against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class AccountLockoutRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private async Task<UserEntity> SeedUserAsync()
    {
        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        UserEntity user = UserFactory.CreateVerifiedActive();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task RegisterFailedLoginAsync_FirstFailure_ProvisionsTheRowAndCountsOne()
    {
        // Arrange
        UserEntity user = await SeedUserAsync();
        var repo = Resolve<IAccountLockoutRepository>();

        // Act
        int count = await repo.RegisterFailedLoginAsync(user.Id, CancellationToken.None);

        // Assert
        count.Should().Be(1);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserLoginStateEntity state = await verifyContext.UserLoginStates.FirstAsync(s => s.Id == user.Id);
        state.FailedAttempts.Should().Be(1);
        state.LockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task RegisterFailedLoginAsync_ReachingTheCap_StampsTheLock()
    {
        // Arrange
        UserEntity user = await SeedUserAsync();
        var repo = Resolve<IAccountLockoutRepository>();

        // Act
        for (int attempt = 0; attempt < UserConstants.MaxLoginAttempts; attempt++)
        {
            await repo.RegisterFailedLoginAsync(user.Id, CancellationToken.None);
        }

        // Assert
        AccountLockoutState state = await repo.GetAsync(user.Id, CancellationToken.None);
        state.FailedLoginAttempts.Should().Be(UserConstants.MaxLoginAttempts);
        state.LockedUntil.Should().NotBeNull();
        state.LockedUntil.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ClearFailedLoginsAsync_ShouldResetTheCounterAndTheLock()
    {
        // Arrange
        UserEntity user = await SeedUserAsync();
        var repo = Resolve<IAccountLockoutRepository>();

        for (int attempt = 0; attempt < UserConstants.MaxLoginAttempts; attempt++)
        {
            await repo.RegisterFailedLoginAsync(user.Id, CancellationToken.None);
        }

        // Act
        await repo.ClearFailedLoginsAsync(user.Id, CancellationToken.None);

        // Assert
        AccountLockoutState state = await repo.GetAsync(user.Id, CancellationToken.None);
        state.FailedLoginAttempts.Should().Be(0);
        state.LockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ForAnAccountWithNoStateRows_ShouldReportNoFailures()
    {
        // Arrange
        UserEntity user = await SeedUserAsync();
        var repo = Resolve<IAccountLockoutRepository>();

        // Act
        AccountLockoutState state = await repo.GetAsync(user.Id, CancellationToken.None);

        // Assert
        state.FailedLoginAttempts.Should().Be(0);
        state.LockedUntil.Should().BeNull();
        state.OtpFailedAttempts.Should().Be(0);
        state.OtpLockedUntil.Should().BeNull();
    }
}
