using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories.Identity;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="AccountLockoutRepository" />. The counter bumps
/// are set-based ExecuteUpdateAsync statements that only run against a
/// relational provider, so they are covered by the integration suite; unit
/// owns the combined state read.
/// </summary>
public class AccountLockoutRepositoryTests
{
    private readonly IdentityDbContext _context;
    private readonly AccountLockoutRepository _repository;

    public AccountLockoutRepositoryTests()
    {
        DbContextOptions<IdentityDbContext> options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new IdentityDbContext(options);
        _repository = new AccountLockoutRepository(_context);
    }

    [Fact]
    public async Task GetAsync_ForAnUnknownAccount_ShouldReportNoFailuresAndNoLocks()
    {
        // Act
        AccountLockoutState state = await _repository.GetAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        state.FailedLoginAttempts.Should().Be(0);
        state.LockedUntil.Should().BeNull();
        state.OtpFailedAttempts.Should().Be(0);
        state.OtpLockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithALoginStateRow_ShouldReadTheStoredLoginCounters()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        UserLoginStateEntity loginState = UserLoginStateEntity.Create(user.Id);
        _context.Users.Add(user);
        _context.UserLoginStates.Add(loginState);
        await _context.SaveChangesAsync();

        // Act
        AccountLockoutState state = await _repository.GetAsync(user.Id, CancellationToken.None);

        // Assert
        state.FailedLoginAttempts.Should().Be(loginState.FailedAttempts);
        state.LockedUntil.Should().Be(loginState.LockedUntil);
    }

    [Fact]
    public async Task GetAsync_WithAnOtpStateRow_ShouldReadTheStoredOtpCounters()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        UserOtpStateEntity otpState = UserOtpStateEntity.Create(user.Id);
        _context.Users.Add(user);
        _context.UserOtpStates.Add(otpState);
        await _context.SaveChangesAsync();

        // Act
        AccountLockoutState state = await _repository.GetAsync(user.Id, CancellationToken.None);

        // Assert
        state.OtpFailedAttempts.Should().Be(otpState.FailedAttempts);
        state.OtpLockedUntil.Should().Be(otpState.LockedUntil);
    }
}
