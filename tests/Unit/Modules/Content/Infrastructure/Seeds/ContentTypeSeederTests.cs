using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Tests.Fixtures.Helpers;
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

    #region SeedAsync — Empty Database

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_ShouldCreateFourContentTypes()
    {
        // Act
        await _seeder.SeedAsync();

        // Assert
        List<ContentTypeEntity> contentTypes = await _context.ContentTypes.ToListAsync();
        contentTypes.Should().HaveCount(4);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_ShouldCreateArticleVideoShortLyrics()
    {
        // Act
        await _seeder.SeedAsync();

        // Assert
        List<string> names = await _context.ContentTypes.Select(c => c.Name).ToListAsync();
        names.Should().Contain("Article");
        names.Should().Contain("Video");
        names.Should().Contain("Short");
        names.Should().Contain("Lyrics");
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_ShouldAssignUniqueIds()
    {
        // Act
        await _seeder.SeedAsync();

        // Assert
        List<Guid> ids = await _context.ContentTypes.Select(c => c.Id).ToListAsync();
        ids.Should().OnlyHaveUniqueItems();
        ids.Should().NotContain(Guid.Empty);
    }

    #endregion

    #region SeedAsync — Idempotency

    [Fact]
    public async Task SeedAsync_WhenAlreadySeeded_ShouldNotAddMoreContentTypes()
    {
        // Arrange — seed once
        await _seeder.SeedAsync();

        // Act — seed again
        await _seeder.SeedAsync();

        // Assert — still only 4
        int count = await _context.ContentTypes.CountAsync();
        count.Should().Be(4);
    }

    [Fact]
    public async Task SeedAsync_WhenAlreadySeeded_ShouldCompleteWithoutError()
    {
        // Arrange
        await _seeder.SeedAsync();

        // Act & Assert — second call should not throw
        Func<Task> act = async () => await _seeder.SeedAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedAsync_WhenOneTypeIsMissing_ShouldSeedOnlyTheMissingOne()
    {
        // Arrange — a database first seeded before Lyrics existed
        await _seeder.SeedAsync();
        ContentTypeEntity lyrics = await _context.ContentTypes.SingleAsync(c => c.Name == "Lyrics");
        _context.ContentTypes.Remove(lyrics);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync();

        // Assert — the missing row heals; the surviving rows are untouched
        List<string> names = await _context.ContentTypes.Select(c => c.Name).ToListAsync();
        names.Should().HaveCount(4);
        names.Should().Contain("Lyrics");
    }

    #endregion
}
