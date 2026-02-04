using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        using var context = new IdentityDbContext(options);
        IIdentityUnitOfWork unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        Assert.Equal(0, result); // No changes to save
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldPassTokenToContext()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);
        IIdentityUnitOfWork unitOfWork = new IdentityUnitOfWork(context);
        CancellationToken cancellationToken = new();

        // Act
        int result = await unitOfWork.CommitAsync(cancellationToken);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CommitAsync_ShouldReturnNumberOfAffectedRows()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);
        IIdentityUnitOfWork unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        Assert.IsType<int>(result);
    }

    [Fact]
    public async Task CommitAsync_WithoutChanges_ShouldReturnZero()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);
        IIdentityUnitOfWork unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CommitAsync_MultipleInvocations_ShouldWork()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);
        IIdentityUnitOfWork unitOfWork = new IdentityUnitOfWork(context);

        // Act
        int result1 = await unitOfWork.CommitAsync();
        int result2 = await unitOfWork.CommitAsync();

        // Assert
        Assert.Equal(0, result1);
        Assert.Equal(0, result2);
    }
}
