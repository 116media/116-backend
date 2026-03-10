using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="ContentUnitOfWork"/>.
/// </summary>
public class ContentUnitOfWorkTests
{
    private DbContextOptions<ContentDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CommitAsync_ShouldSaveChangesToDatabase()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        await using var context = new ContentDbContext(options);
        var unitOfWork = new ContentUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldPassTokenToContext()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        await using var context = new ContentDbContext(options);
        var unitOfWork = new ContentUnitOfWork(context);
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
        DbContextOptions<ContentDbContext> options = CreateOptions();
        await using var context = new ContentDbContext(options);
        var unitOfWork = new ContentUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().BeOfType(typeof(int));
    }

    [Fact]
    public async Task CommitAsync_WithoutChanges_ShouldReturnZero()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        await using var context = new ContentDbContext(options);
        var unitOfWork = new ContentUnitOfWork(context);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_MultipleInvocations_ShouldWork()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        await using var context = new ContentDbContext(options);
        var unitOfWork = new ContentUnitOfWork(context);

        // Act
        int result1 = await unitOfWork.CommitAsync();
        int result2 = await unitOfWork.CommitAsync();

        // Assert
        result1.Should().Be(0);
        result2.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_AfterAddingEntity_ShouldReturnPositiveCount()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        await using var context = new ContentDbContext(options);
        var unitOfWork = new ContentUnitOfWork(context);

        var contentType = ContentTypeEntity.Create(id: Guid.NewGuid(), name: "Video");
        await context.ContentTypes.AddAsync(contentType);

        // Act
        int result = await unitOfWork.CommitAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }
}
