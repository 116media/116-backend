using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Seeds;

/// <summary>
/// Unit tests for <see cref="ContentTypeSeeder"/> using InMemory database.
/// </summary>
public class ContentTypeSeederTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ContentTypeSeeder _seeder;

    public ContentTypeSeederTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        var loggerMock = new Mock<ILogger<ContentTypeSeeder>>();
        _seeder = new ContentTypeSeeder(_context, loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region SeedAllAsync — Empty Database

    [Fact]
    public async Task SeedAllAsync_WhenDatabaseIsEmpty_ShouldCreateThreeContentTypes()
    {
        // Act
        await _seeder.SeedAllAsync();

        // Assert
        List<ContentTypeEntity> contentTypes = await _context.ContentTypes.ToListAsync();
        contentTypes.Should().HaveCount(3);
    }

    [Fact]
    public async Task SeedAllAsync_WhenDatabaseIsEmpty_ShouldCreateArticleVideoShort()
    {
        // Act
        await _seeder.SeedAllAsync();

        // Assert
        List<string> names = await _context.ContentTypes.Select(c => c.Name).ToListAsync();
        names.Should().Contain("Article");
        names.Should().Contain("Video");
        names.Should().Contain("Short");
    }

    [Fact]
    public async Task SeedAllAsync_WhenDatabaseIsEmpty_ShouldAssignUniqueIds()
    {
        // Act
        await _seeder.SeedAllAsync();

        // Assert
        List<Guid> ids = await _context.ContentTypes.Select(c => c.Id).ToListAsync();
        ids.Should().OnlyHaveUniqueItems();
        ids.Should().NotContain(Guid.Empty);
    }

    #endregion

    #region SeedAllAsync — Already Seeded (Idempotency)

    [Fact]
    public async Task SeedAllAsync_WhenAlreadySeeded_ShouldNotAddMoreContentTypes()
    {
        // Arrange — seed once
        await _seeder.SeedAllAsync();

        // Act — seed again
        await _seeder.SeedAllAsync();

        // Assert — still only 3
        int count = await _context.ContentTypes.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task SeedAllAsync_WhenAlreadySeeded_ShouldCompleteWithoutError()
    {
        // Arrange
        await _seeder.SeedAllAsync();

        // Act & Assert — second call should not throw
        Func<Task> act = async () => await _seeder.SeedAllAsync();
        await act.Should().NotThrowAsync();
    }

    #endregion
}
