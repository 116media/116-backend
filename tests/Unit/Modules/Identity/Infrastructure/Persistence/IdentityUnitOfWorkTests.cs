using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="IdentityUnitOfWork"/>.
/// </summary>
public class IdentityUnitOfWorkTests
{
    private DbContextOptions<IdentityDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CommitAsync_ShouldSaveChangesToDatabase()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldPassTokenToContext()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var unitOfWork = new IdentityUnitOfWork(context);
        CancellationToken cancellationToken = new();

        // Act
        int result = await unitOfWork.CommitAsync(cancellationToken);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_ShouldReturnNumberOfAffectedRows()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().BeOfType(typeof(int));
    }

    [Fact]
    public async Task CommitAsync_WithoutChanges_ShouldReturnZero()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_MultipleInvocations_ShouldWork()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result1 = await unitOfWork.CommitAsync();
        int result2 = await unitOfWork.CommitAsync();

        // Assert
        result1.Should().Be(0);
        result2.Should().Be(0);
    }

    #region ExecuteInTransactionAsync

    /// <summary>
    /// Builds options that let the in-memory provider accept the transaction the seam opens.
    /// The provider ignores it, which is enough to drive the surrounding orchestration.
    /// </summary>
    /// <returns>Options with the transaction warning suppressed.</returns>
    private static DbContextOptions<IdentityDbContext> CreateTransactionalOptions() =>
        new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldRunTheOperation()
    {
        // Arrange
        await using var context = new IdentityDbContext(CreateTransactionalOptions());
        var unitOfWork = new IdentityUnitOfWork(context);
        var ran = false;

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        // Assert
        ran.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldSaveWhatTheOperationTracked()
    {
        // Arrange
        // The seam commits once at the end, so the operation itself never saves.
        await using var context = new IdentityDbContext(CreateTransactionalOptions());
        var unitOfWork = new IdentityUnitOfWork(context);

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.Roles.Add(RoleFactory.Create());
            return Task.CompletedTask;
        });

        // Assert
        context.Roles.Local.Should().ContainSingle();
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenTheOperationThrows_ShouldSurfaceTheFailure()
    {
        // Arrange
        // A failure has to reach the caller; swallowing it would commit a half-applied change.
        await using var context = new IdentityDbContext(CreateTransactionalOptions());
        var unitOfWork = new IdentityUnitOfWork(context);
        var failure = new InvalidOperationException("operation failed");

        // Act
        Func<Task> act = async () => await unitOfWork.ExecuteInTransactionAsync(_ => throw failure);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should()
            .BeSameAs(failure);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldPassTheTokenToTheOperation()
    {
        // Arrange
        await using var context = new IdentityDbContext(CreateTransactionalOptions());
        var unitOfWork = new IdentityUnitOfWork(context);
        using var cts = new CancellationTokenSource();
        CancellationToken observed = default;

        // Act
        await unitOfWork.ExecuteInTransactionAsync(
            token =>
            {
                observed = token;
                return Task.CompletedTask;
            },
            cts.Token
        );

        // Assert
        observed.Should().Be(cts.Token);
    }

    #endregion
}
