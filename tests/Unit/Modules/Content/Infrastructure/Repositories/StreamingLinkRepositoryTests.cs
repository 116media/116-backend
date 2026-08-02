using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="StreamingLinkRepository"/>.
/// </summary>
public class StreamingLinkRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly StreamingLinkRepository _repository;
    private readonly Guid _albumId = Guid.NewGuid();
    private readonly Guid _lyricsId = Guid.NewGuid();

    public StreamingLinkRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new StreamingLinkRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByAlbumAndPlatformAsync Tests

    [Fact]
    public async Task GetByAlbumAndPlatformAsync_WhenLinkExists_ShouldReturnLink()
    {
        // Arrange
        StreamingLinkEntity spotify = StreamingLinkFactory.CreateForAlbum(_albumId);
        StreamingLinkEntity youtubeMusic = StreamingLinkFactory.CreateForAlbum(
            _albumId,
            EnumStreamingPlatform.YoutubeMusic,
            "https://music.youtube.com/album/1"
        );
        _context.StreamingLinks.AddRange(spotify, youtubeMusic);
        await _context.SaveChangesAsync();

        // Act
        StreamingLinkEntity? result = await _repository.GetByAlbumAndPlatformAsync(
            _albumId,
            EnumStreamingPlatform.YoutubeMusic
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(youtubeMusic.Id);
    }

    [Fact]
    public async Task GetByAlbumAndPlatformAsync_WhenLinkDoesNotExist_ShouldReturnNull()
    {
        // Act
        StreamingLinkEntity? result = await _repository.GetByAlbumAndPlatformAsync(
            Guid.NewGuid(),
            EnumStreamingPlatform.Spotify
        );

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByAlbumAsync Tests

    [Fact]
    public async Task GetByAlbumAsync_ShouldReturnPlatformToUrlMapForTheAlbum()
    {
        // Arrange
        StreamingLinkEntity spotify = StreamingLinkFactory.CreateForAlbum(
            _albumId,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/album/a"
        );
        StreamingLinkEntity youtubeMusic = StreamingLinkFactory.CreateForAlbum(
            _albumId,
            EnumStreamingPlatform.YoutubeMusic,
            "https://music.youtube.com/album/b"
        );
        StreamingLinkEntity otherAlbum = StreamingLinkFactory.CreateForAlbum(Guid.NewGuid());
        _context.StreamingLinks.AddRange(spotify, youtubeMusic, otherAlbum);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await _repository.GetByAlbumAsync(_albumId);

        // Assert
        result.Should().HaveCount(2);
        result[EnumStreamingPlatform.Spotify].Should().Be("https://open.spotify.com/album/a");
        result[EnumStreamingPlatform.YoutubeMusic].Should().Be("https://music.youtube.com/album/b");
    }

    [Fact]
    public async Task GetByAlbumAsync_WhenNoLinks_ShouldReturnEmptyMap()
    {
        // Act
        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await _repository.GetByAlbumAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByLyricsAndPlatformAsync Tests

    [Fact]
    public async Task GetByLyricsAndPlatformAsync_WhenLinkExists_ShouldReturnLink()
    {
        // Arrange
        StreamingLinkEntity link = StreamingLinkFactory.CreateForLyrics(_lyricsId, EnumStreamingPlatform.AppleMusic);
        _context.StreamingLinks.AddRange(link, StreamingLinkFactory.CreateForLyrics(_lyricsId));
        await _context.SaveChangesAsync();

        // Act
        StreamingLinkEntity? result = await _repository.GetByLyricsAndPlatformAsync(
            _lyricsId,
            EnumStreamingPlatform.AppleMusic
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(link.Id);
    }

    [Fact]
    public async Task GetByLyricsAndPlatformAsync_WhenLinkDoesNotExist_ShouldReturnNull()
    {
        // Act
        StreamingLinkEntity? result = await _repository.GetByLyricsAndPlatformAsync(
            Guid.NewGuid(),
            EnumStreamingPlatform.Spotify
        );

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByLyricsAsync Tests

    [Fact]
    public async Task GetByLyricsAsync_ShouldReturnPlatformToUrlMapForTheSingle()
    {
        // Arrange
        StreamingLinkEntity spotify = StreamingLinkFactory.CreateForLyrics(
            _lyricsId,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/track/a"
        );
        StreamingLinkEntity youtubeMusic = StreamingLinkFactory.CreateForLyrics(
            _lyricsId,
            EnumStreamingPlatform.YoutubeMusic,
            "https://music.youtube.com/track/b"
        );
        _context.StreamingLinks.AddRange(spotify, youtubeMusic, StreamingLinkFactory.CreateForLyrics(Guid.NewGuid()));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await _repository.GetByLyricsAsync(_lyricsId);

        // Assert
        result.Should().HaveCount(2);
        result[EnumStreamingPlatform.Spotify].Should().Be("https://open.spotify.com/track/a");
        result[EnumStreamingPlatform.YoutubeMusic].Should().Be("https://music.youtube.com/track/b");
    }

    [Fact]
    public async Task GetByLyricsAsync_WhenNoLinks_ShouldReturnEmptyMap()
    {
        // Act
        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await _repository.GetByLyricsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddStreamingLinkToContext()
    {
        // Arrange
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(_albumId);

        // Act
        await _repository.AddAsync(link);

        // Assert
        _context.Entry(link).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        StreamingLinkEntity? saved = await _context.StreamingLinks.FirstOrDefaultAsync(l => l.Id == link.Id);
        saved.Should().NotBeNull();
        saved.AlbumId.Should().Be(_albumId);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkStreamingLinkAsModified()
    {
        // Arrange
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(_albumId);
        _context.StreamingLinks.Add(link);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(link);

        // Assert
        _context.Entry(link).State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_ShouldRemoveStreamingLinkFromContext()
    {
        // Arrange
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(_albumId);
        _context.StreamingLinks.Add(link);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(link);

        // Assert
        _context.Entry(link).State.Should().Be(EntityState.Deleted);

        await _context.SaveChangesAsync();
        StreamingLinkEntity? deleted = await _context.StreamingLinks.FirstOrDefaultAsync(l => l.Id == link.Id);
        deleted.Should().BeNull();
    }

    #endregion
}
