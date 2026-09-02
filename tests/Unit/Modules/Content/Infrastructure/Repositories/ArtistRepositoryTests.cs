using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="ArtistRepository"/>.
/// </summary>
public class ArtistRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ArtistRepository _repository;

    public ArtistRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ArtistRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetBySlugAsync Tests

    [Fact]
    public async Task GetBySlugAsync_WhenNoArtistsExist_ShouldReturnNull()
    {
        // Act
        ArtistEntity? result = await _repository.GetBySlugAsync("nonexistent-slug");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenArtistExists_ShouldReturnArtist()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity? result = await _repository.GetByIdAsync(artist.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(artist.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenArtistDoesNotExist_ShouldReturnNull()
    {
        // Act
        ArtistEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArtistExists_ShouldReturnArtist()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity result = await _repository.GetByIdOrThrowAsync(artist.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(artist.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArtistDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenProfileIsClaimed_ShouldReturnArtist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        ArtistEntity claimed = ArtistFactory.CreateClaimed(userId);
        _context.Artists.AddRange(claimed, ArtistFactory.Create());
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity? result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(claimed.Id);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoProfileIsClaimedByUser_ShouldReturnNull()
    {
        // Arrange
        _context.Artists.Add(ArtistFactory.Create());
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity? result = await _repository.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoSearch_ShouldReturnAllArtistsOrderedByName()
    {
        // Arrange
        _context.Artists.AddRange(
            ArtistFactory.Create("Zao", "zao"),
            ArtistFactory.Create("Awilo", "awilo"),
            ArtistFactory.Create("Koffi", "koffi")
        );
        await _context.SaveChangesAsync();

        // Act
        (List<ArtistEntity> artists, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null
        );

        // Assert
        totalCount.Should().Be(3);
        artists.Select(artist => artist.Name).Should().ContainInOrder("Awilo", "Koffi", "Zao");
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnRequestedPage()
    {
        // Arrange
        _context.Artists.AddRange(ArtistFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        (List<ArtistEntity> artists, int totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 2,
            search: null
        );

        // Assert
        artists.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoArtists_ShouldReturnEmptyList()
    {
        // Act
        (List<ArtistEntity> artists, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null
        );

        // Assert
        artists.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddArtistToContext()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();

        // Act
        await _repository.AddAsync(artist);

        // Assert
        _context.Entry(artist).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        ArtistEntity? saved = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artist.Id);
        saved.Should().NotBeNull();
        saved.Name.Should().Be(artist.Name);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkArtistAsModified()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(artist);

        // Assert
        _context.Entry(artist).State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region Social Link Tests

    [Fact]
    public async Task GetSocialLinksAsync_ShouldReturnLinksOrderedByPlatform()
    {
        // Arrange — inserted out of order; the row must come back platform-ordered.
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        _context.ArtistSocialLinks.AddRange(
            ArtistSocialLinkEntity.Create(Guid.NewGuid(), artist.Id, EnumSocialPlatform.Website, "https://a.example"),
            ArtistSocialLinkEntity.Create(Guid.NewGuid(), artist.Id, EnumSocialPlatform.Instagram, "https://b.example")
        );
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ArtistSocialLinkEntity> result = await _repository.GetSocialLinksAsync(artist.Id);

        // Assert
        result.Select(l => l.Platform).Should().Equal(EnumSocialPlatform.Instagram, EnumSocialPlatform.Website);
    }

    [Fact]
    public async Task GetSocialLinkAsync_ShouldReturnTheMatchingSlotOrNull()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        ArtistSocialLinkEntity link = ArtistSocialLinkEntity.Create(
            Guid.NewGuid(),
            artist.Id,
            EnumSocialPlatform.TikTok,
            "https://tiktok.com/@x"
        );
        _context.Artists.Add(artist);
        _context.ArtistSocialLinks.Add(link);
        await _context.SaveChangesAsync();

        // Act & Assert
        (await _repository.GetSocialLinkAsync(artist.Id, EnumSocialPlatform.TikTok))!
            .Id.Should()
            .Be(link.Id);
        (await _repository.GetSocialLinkAsync(artist.Id, EnumSocialPlatform.Facebook)).Should().BeNull();
    }

    [Fact]
    public async Task AddUpdateRemoveSocialLink_ShouldTrackTheEntityStates()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        ArtistSocialLinkEntity link = ArtistSocialLinkEntity.Create(
            Guid.NewGuid(),
            artist.Id,
            EnumSocialPlatform.X,
            "https://x.com/someone"
        );

        // Act & Assert — add
        await _repository.AddSocialLinkAsync(link);
        await _context.SaveChangesAsync();
        (await _context.ArtistSocialLinks.FindAsync(link.Id)).Should().NotBeNull();

        // update
        link.UpdateUrl("https://x.com/renamed");
        _repository.UpdateSocialLink(link);
        _context.Entry(link).State.Should().Be(EntityState.Modified);
        await _context.SaveChangesAsync();

        // remove
        _repository.RemoveSocialLink(link);
        await _context.SaveChangesAsync();
        (await _context.ArtistSocialLinks.FindAsync(link.Id)).Should().BeNull();
    }

    #endregion

    #region Totals, Letters and Directory Tests

    [Fact]
    public async Task GetTotalsAsync_ShouldCountEachSurfaceSeparately()
    {
        // Arrange — one item per surface, plus non-counting rows (draft song, EP).
        ArtistEntity artist = ArtistFactory.Create();
        var categoryId = Guid.NewGuid();
        _context.Artists.Add(artist);
        _context.Lyrics.Add(LyricsFactory.CreatePublishedForArtist(categoryId, artist.Id));
        _context.Lyrics.Add(LyricsFactory.CreateForArtist(categoryId, artist.Id));
        _context.Albums.Add(AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Album));
        _context.Albums.Add(AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.Mixtape));
        _context.Albums.Add(AlbumFactory.CreateForArtist(artist.Id, EnumReleaseType.EP));
        ArticleEntity article = ArticleFactory.CreatePublished(categoryId);
        _context.Articles.Add(article);
        _context.ArticleArtists.Add(ArticleArtistEntity.Create(Guid.NewGuid(), article.Id, artist.Id));
        await _context.SaveChangesAsync();

        // Act
        ArtistTotals totals = await _repository.GetTotalsAsync(artist.Id);

        // Assert — the draft song and the EP count nowhere.
        totals.Should().Be(new ArtistTotals(Songs: 1, Videos: 0, Albums: 1, Mixtapes: 1, News: 1));
    }

    [Fact]
    public async Task GetTotalsAsync_WithUnknownArtist_ShouldReturnAllZeros()
    {
        ArtistTotals totals = await _repository.GetTotalsAsync(Guid.NewGuid());

        totals.Should().Be(new ArtistTotals(Songs: 0, Videos: 0, Albums: 0, Mixtapes: 0, News: 0));
    }

    [Fact]
    public async Task GetAvailableLettersAsync_ShouldReturnDistinctLettersOfListedArtistsOnly()
    {
        // Arrange — one artist with content, one stub without.
        var categoryId = Guid.NewGuid();
        ArtistEntity listed = ArtistFactory.Create("Fally Ipupa", $"listed-{Guid.NewGuid():N}");
        ArtistEntity stub = ArtistFactory.Create("Zed Stub", $"stub-{Guid.NewGuid():N}");
        _context.Artists.AddRange(listed, stub);
        _context.Lyrics.Add(LyricsFactory.CreatePublishedForArtist(categoryId, listed.Id));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<string> letters = await _repository.GetAvailableLettersAsync();

        // Assert — the stub's Z contributes nothing.
        letters.Should().Contain("F");
        letters.Should().NotContain("Z");
    }

    [Fact]
    public async Task GetPublicDirectoryAsync_ShouldFilterToContentAndCountPerCard()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        ArtistEntity listed = ArtistFactory.Create("Directory Artist", $"dir-{Guid.NewGuid():N}");
        ArtistEntity stub = ArtistFactory.Create("Directory Stub", $"stub-{Guid.NewGuid():N}");
        _context.Artists.AddRange(listed, stub);
        _context.Lyrics.Add(LyricsFactory.CreatePublishedForArtist(categoryId, listed.Id));
        _context.Albums.Add(AlbumFactory.CreateForArtist(listed.Id, EnumReleaseType.Album));
        await _context.SaveChangesAsync();

        // Act
        (List<ArtistDirectoryRow> rows, int totalCount) = await _repository.GetPublicDirectoryAsync(
            page: 1,
            pageSize: 30,
            letter: null,
            search: null
        );

        // Assert
        totalCount.Should().Be(1);
        rows.Should().ContainSingle();
        rows[0].Artist.Id.Should().Be(listed.Id);
        rows[0].ContentCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPublicDirectoryAsync_WithLetter_ShouldFilterOnTheStoredBucket()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        ArtistEntity elodie = ArtistFactory.Create("Élodie Unit", $"el-{Guid.NewGuid():N}");
        ArtistEntity fally = ArtistFactory.Create("Fally Unit", $"fa-{Guid.NewGuid():N}");
        _context.Artists.AddRange(elodie, fally);
        _context.Lyrics.Add(LyricsFactory.CreatePublishedForArtist(categoryId, elodie.Id));
        _context.Lyrics.Add(LyricsFactory.CreatePublishedForArtist(categoryId, fally.Id));
        await _context.SaveChangesAsync();

        // Act — the accented name buckets under its folded initial.
        (List<ArtistDirectoryRow> rows, int totalCount) = await _repository.GetPublicDirectoryAsync(
            page: 1,
            pageSize: 30,
            letter: "E",
            search: null
        );

        // Assert
        totalCount.Should().Be(1);
        rows.Should().ContainSingle(r => r.Artist.Id == elodie.Id);
    }

    #endregion
}
