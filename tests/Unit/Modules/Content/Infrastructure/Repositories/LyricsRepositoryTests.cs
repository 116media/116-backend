using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="LyricsRepository"/>.
/// </summary>
public class LyricsRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly LyricsRepository _repository;

    public LyricsRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new LyricsRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoFilters_ShouldReturnAllLyrics()
    {
        // Arrange
        _context.Lyrics.AddRange(LyricsFactory.CreateMany(3));
        await _context.SaveChangesAsync();

        // Act
        var (lyrics, totalCount) = await _repository.GetAllAsync(page: 1, pageSize: 10, search: null);

        // Assert
        lyrics.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        _context.Lyrics.AddRange(LyricsFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        var (lyrics, totalCount) = await _repository.GetAllAsync(page: 2, pageSize: 2, search: null);

        // Assert
        lyrics.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Act
        var (lyrics, totalCount) = await _repository.GetAllAsync(page: 1, pageSize: 10, search: null);

        // Assert
        lyrics.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    // Note: GetAllAsync with search uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // Search filtering is covered in integration tests.

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenLyricsExist_ShouldReturnLyrics()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();
        _context.Lyrics.Add(lyrics);
        await _context.SaveChangesAsync();

        // Act
        LyricsEntity? result = await _repository.GetByIdAsync(lyrics.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLyricsDoNotExist_ShouldReturnNull()
    {
        // Act
        LyricsEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenLyricsExist_ShouldReturnLyrics()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();
        _context.Lyrics.Add(lyrics);
        await _context.SaveChangesAsync();

        // Act
        LyricsEntity result = await _repository.GetByIdOrThrowAsync(lyrics.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenLyricsDoNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetBySongTitleAndArtistAsync Tests

    // Note: GetBySongTitleAndArtistAsync uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // This method is covered in integration tests.

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddLyricsToContext()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();

        // Act
        await _repository.AddAsync(lyrics);
        await _context.SaveChangesAsync();

        // Assert
        LyricsEntity? saved = await _context.Lyrics.FirstOrDefaultAsync(l => l.Id == lyrics.Id);
        saved.Should().NotBeNull();
        saved.SongTitle.Should().Be(lyrics.SongTitle);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkLyricsAsModified()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();
        _context.Lyrics.Add(lyrics);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(lyrics);

        // Assert — update marks the entity; EF tracks it as Modified
        _context.Entry(lyrics).State.Should().Be(EntityState.Modified);
    }

    #endregion
}
