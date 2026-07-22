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
    private readonly Guid _categoryId;

    public LyricsRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new LyricsRepository(_context);

        // Seed a real category so the repository's `Include(l => l.Category)` navigation
        // resolves correctly against the InMemory provider.
        var contentType = ContentTypeFactory.Create();
        _context.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        _context.Categories.Add(category);
        _context.SaveChanges();
        _categoryId = category.Id;
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
        _context.Lyrics.AddRange(LyricsFactory.CreateMany(_categoryId, 3));
        await _context.SaveChangesAsync();

        // Act
        var (lyrics, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        lyrics.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        _context.Lyrics.AddRange(LyricsFactory.CreateMany(_categoryId, 5));
        await _context.SaveChangesAsync();

        // Act
        var (lyrics, totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 2,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        lyrics.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Act
        var (lyrics, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        lyrics.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ShouldReturnOnlyMatchingCategory()
    {
        // Arrange
        var otherContentType = ContentTypeFactory.Create();
        _context.ContentTypes.Add(otherContentType);
        var otherCategory = CategoryFactory.Create(otherContentType.Id);
        _context.Categories.Add(otherCategory);
        _context.Lyrics.AddRange(LyricsFactory.CreateMany(_categoryId, 2));
        _context.Lyrics.Add(LyricsFactory.Create(otherCategory.Id));
        await _context.SaveChangesAsync();

        // Act
        var (lyrics, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: _categoryId
        );

        // Assert
        lyrics.Should().HaveCount(2);
        totalCount.Should().Be(2);
        lyrics.Should().OnlyContain(l => l.CategoryId == _categoryId);
    }

    // Note: GetAllAsync with search/status/language uses PostgreSQL ILike and enum conversion
    // which are not fully supported by InMemoryDatabase. Full filter coverage is in integration tests.

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenLyricsExist_ShouldReturnLyrics()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(_categoryId);
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
        LyricsEntity lyrics = LyricsFactory.Create(_categoryId);
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

    #region GetBySlugAsync Tests

    // Note: GetBySlugAsync uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // This method is covered in integration tests.

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddLyricsToContext()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(_categoryId);

        // Act
        await _repository.AddAsync(lyrics);
        await _context.SaveChangesAsync();

        // Assert
        LyricsEntity? saved = await _context.Lyrics.FirstOrDefaultAsync(l => l.Id == lyrics.Id);
        saved.Should().NotBeNull();
        saved.SongTitle.Should().Be(lyrics.SongTitle);
    }

    #endregion

    #region GetByVideoIdAsync Tests

    [Fact]
    public async Task GetByVideoIdAsync_WhenLyricsLinked_ShouldReturnLyrics()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(_categoryId, videoId);
        _context.Lyrics.Add(lyrics);
        await _context.SaveChangesAsync();

        // Act
        LyricsEntity? result = await _repository.GetByVideoIdAsync(videoId);

        // Assert
        result.Should().NotBeNull();
        result.VideoId.Should().Be(videoId);
    }

    [Fact]
    public async Task GetByVideoIdAsync_WhenNoLyricsLinked_ShouldReturnNull()
    {
        // Act
        LyricsEntity? result = await _repository.GetByVideoIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkLyricsAsModified()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(_categoryId);
        _context.Lyrics.Add(lyrics);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(lyrics);

        // Assert — update marks the entity; EF tracks it as Modified
        _context.Entry(lyrics).State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_ShouldDeleteLyricsFromContext()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(_categoryId);
        _context.Lyrics.Add(lyrics);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(lyrics);
        await _context.SaveChangesAsync();

        // Assert
        LyricsEntity? result = await _context.Lyrics.FirstOrDefaultAsync(l => l.Id == lyrics.Id);
        result.Should().BeNull();
    }

    #endregion

    #region GetSimilarAsync Tests

    /// <summary>
    /// A source lyrics page linked to a video finds another published lyrics page linked to a
    /// video in the same category — the first waterfall branch (spec 06).
    /// </summary>
    [Fact]
    public async Task GetSimilarAsync_WhenVideoCategoryMatchExists_ShouldReturnCategoryBranchMatch()
    {
        // Arrange
        VideoEntity sourceVideo = VideoFactory.CreatePublished(_categoryId);
        VideoEntity otherVideo = VideoFactory.CreatePublished(_categoryId);
        _context.Videos.AddRange(sourceVideo, otherVideo);

        LyricsEntity source = LyricsFactory.CreatePublishedForVideoWithSlug(_categoryId, sourceVideo.Id, "source-slug");
        LyricsEntity categoryMatch = LyricsFactory.CreatePublishedForVideoWithSlug(
            _categoryId,
            otherVideo.Id,
            "category-match-slug"
        );
        _context.Lyrics.AddRange(source, categoryMatch);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsEntity> similar = await _repository.GetSimilarAsync(source.Id);

        // Assert
        similar.Should().ContainSingle(l => l.Id == categoryMatch.Id);
        similar.Should().NotContain(l => l.Id == source.Id);
    }

    /// <summary>
    /// A standalone source lyrics page finds another published lyrics page sharing at least one
    /// tag — the second waterfall branch (spec 06), used when there is no video-category match.
    /// </summary>
    [Fact]
    public async Task GetSimilarAsync_WhenNoVideoCategoryMatchButSharedTagExists_ShouldReturnSharedTagsBranchMatch()
    {
        // Arrange
        Guid sharedTagId = Guid.NewGuid();
        LyricsEntity source = LyricsFactory.CreateWithTags(_categoryId, sharedTagId);
        source.Publish();
        LyricsEntity tagMatch = LyricsFactory.CreateWithTags(_categoryId, sharedTagId);
        tagMatch.Publish();
        _context.Lyrics.AddRange(source, tagMatch);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsEntity> similar = await _repository.GetSimilarAsync(source.Id);

        // Assert
        similar.Should().ContainSingle(l => l.Id == tagMatch.Id);
    }

    /// <summary>
    /// A standalone source lyrics page with no video and no shared tags falls through to the
    /// third waterfall branch — the most recent other standalone published lyrics pages.
    /// </summary>
    [Fact]
    public async Task GetSimilarAsync_WhenNoCategoryOrTagMatch_ShouldReturnStandaloneBranchMatch()
    {
        // Arrange
        LyricsEntity source = LyricsFactory.CreatePublished(_categoryId);
        LyricsEntity standaloneMatch = LyricsFactory.CreatePublished(_categoryId);
        _context.Lyrics.AddRange(source, standaloneMatch);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsEntity> similar = await _repository.GetSimilarAsync(source.Id);

        // Assert
        similar.Should().ContainSingle(l => l.Id == standaloneMatch.Id);
    }

    /// <summary>
    /// The resolved, deliberate fallthrough design (spec 06): a video-linked source lyrics page
    /// with zero same-category matches must still fall through to the shared-tags branch and
    /// return the tag-based matches, rather than stopping at an empty category branch. A
    /// regression here would silently break the approved waterfall design.
    /// </summary>
    [Fact]
    public async Task GetSimilarAsync_WhenVideoLinkedWithNoCategoryMatchButSharedTagExists_ShouldFallThroughToSharedTagsBranch()
    {
        // Arrange
        Guid sharedTagId = Guid.NewGuid();

        VideoEntity sourceVideo = VideoFactory.CreatePublished(_categoryId);
        _context.Videos.Add(sourceVideo);

        LyricsEntity source = LyricsFactory.CreatePublishedForVideoWithSlug(_categoryId, sourceVideo.Id, "source-slug");
        source.Tags.Add(LyricsTagEntity.Create(Guid.NewGuid(), source.Id, sharedTagId));

        // Standalone (no video) but sharing a tag — proves the category branch (empty, since no
        // other lyrics page is linked to a video in this category) falls through to tags rather
        // than stopping empty.
        LyricsEntity tagMatch = LyricsFactory.CreateWithTags(_categoryId, sharedTagId);
        tagMatch.Publish();

        _context.Lyrics.AddRange(source, tagMatch);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsEntity> similar = await _repository.GetSimilarAsync(source.Id);

        // Assert
        similar.Should().NotBeEmpty();
        similar.Should().ContainSingle(l => l.Id == tagMatch.Id);
    }

    /// <summary>
    /// When none of the three waterfall branches yield any matches, the result is an empty list,
    /// never a thrown exception.
    /// </summary>
    [Fact]
    public async Task GetSimilarAsync_WhenNoMatchesInAnyBranch_ShouldReturnEmptyList()
    {
        // Arrange
        LyricsEntity source = LyricsFactory.CreatePublished(_categoryId);
        _context.Lyrics.Add(source);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsEntity> similar = await _repository.GetSimilarAsync(source.Id);

        // Assert
        similar.Should().BeEmpty();
    }

    /// <summary>
    /// A missing source lyrics page id must throw, since <c>GetSimilarAsync</c> resolves the
    /// source via <c>GetByIdOrThrowAsync</c> before running the waterfall.
    /// </summary>
    [Fact]
    public async Task GetSimilarAsync_WhenSourceLyricsDoNotExist_ShouldThrowNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _repository.GetSimilarAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
