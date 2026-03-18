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
/// Unit tests for <see cref="ShortVideoRepository"/>.
/// </summary>
public class ShortVideoRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ShortVideoRepository _repository;

    public ShortVideoRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ShortVideoRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoFilters_ShouldReturnAllShortVideos()
    {
        // Arrange
        _context.ShortVideos.AddRange(ShortVideoFactory.CreateMany(3));
        await _context.SaveChangesAsync();

        // Act
        var (shortVideos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            isActive: null
        );

        // Assert
        shortVideos.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        _context.ShortVideos.AddRange(ShortVideoFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        var (shortVideos, totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 2,
            search: null,
            isActive: null
        );

        // Assert
        shortVideos.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithIsActiveFilter_ShouldReturnOnlyActiveShortVideos()
    {
        // Arrange
        _context.ShortVideos.Add(ShortVideoFactory.Create()); // active
        _context.ShortVideos.Add(ShortVideoFactory.CreateInactive()); // inactive
        await _context.SaveChangesAsync();

        // Act
        var (shortVideos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            isActive: true
        );

        // Assert
        shortVideos.Should().ContainSingle();
        totalCount.Should().Be(1);
        shortVideos.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WithIsActiveFilterFalse_ShouldReturnOnlyInactiveShortVideos()
    {
        // Arrange
        _context.ShortVideos.Add(ShortVideoFactory.Create()); // active
        _context.ShortVideos.Add(ShortVideoFactory.CreateInactive()); // inactive
        await _context.SaveChangesAsync();

        // Act
        var (shortVideos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            isActive: false
        );

        // Assert
        shortVideos.Should().ContainSingle();
        totalCount.Should().Be(1);
        shortVideos.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Act
        var (shortVideos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            isActive: null
        );

        // Assert
        shortVideos.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    // Note: GetAllAsync with search uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // Search filtering is covered in integration tests.

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenShortVideoExists_ShouldReturnShortVideo()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        await _context.SaveChangesAsync();

        // Act
        ShortVideoEntity? result = await _repository.GetByIdAsync(shortVideo.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(shortVideo.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenShortVideoDoesNotExist_ShouldReturnNull()
    {
        // Act
        ShortVideoEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenShortVideoExists_ShouldReturnShortVideo()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        await _context.SaveChangesAsync();

        // Act
        ShortVideoEntity result = await _repository.GetByIdOrThrowAsync(shortVideo.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(shortVideo.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenShortVideoDoesNotExist_ShouldThrowNotFoundException()
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
    public async Task AddAsync_ShouldAddShortVideoToContext()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        // Act
        await _repository.AddAsync(shortVideo);
        await _context.SaveChangesAsync();

        // Assert
        ShortVideoEntity? saved = await _context.ShortVideos.FirstOrDefaultAsync(s => s.Id == shortVideo.Id);
        saved.Should().NotBeNull();
        saved.Title.Should().Be(shortVideo.Title);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkShortVideoAsModified()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(shortVideo);

        // Assert
        _context.Entry(shortVideo).State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_ShouldRemoveShortVideoFromContext()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(shortVideo);
        await _context.SaveChangesAsync();

        // Assert
        ShortVideoEntity? deleted = await _context.ShortVideos.FirstOrDefaultAsync(s => s.Id == shortVideo.Id);
        deleted.Should().BeNull();
    }

    #endregion

    #region HasLikedAsync Tests

    [Fact]
    public async Task HasLikedAsync_WhenLikeExists_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        ShortVideoLikeEntity like = ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, shortVideo.Id);
        _context.ShortVideoLikes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        bool result = await _repository.HasLikedAsync(userId, shortVideo.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasLikedAsync_WhenLikeDoesNotExist_ShouldReturnFalse()
    {
        // Act
        bool result = await _repository.HasLikedAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddLikeAsync Tests

    [Fact]
    public async Task AddLikeAsync_ShouldAddLikeToContext()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        await _context.SaveChangesAsync();

        ShortVideoLikeEntity like = ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, shortVideo.Id);

        // Act
        await _repository.AddLikeAsync(like);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ShortVideoLikes.AnyAsync(l =>
            l.UserId == userId && l.ShortVideoId == shortVideo.Id
        );
        exists.Should().BeTrue();
    }

    #endregion

    #region RemoveLikeAsync Tests

    [Fact]
    public async Task RemoveLikeAsync_WhenLikeExists_ShouldRemoveIt()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        ShortVideoLikeEntity like = ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, shortVideo.Id);
        _context.ShortVideoLikes.Add(like);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveLikeAsync(userId, shortVideo.Id);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ShortVideoLikes.AnyAsync(l =>
            l.UserId == userId && l.ShortVideoId == shortVideo.Id
        );
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveLikeAsync_WhenLikeDoesNotExist_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _repository.RemoveLikeAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region HasBookmarkedAsync Tests

    [Fact]
    public async Task HasBookmarkedAsync_WhenBookmarkExists_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkEntity.Create(Guid.NewGuid(), userId, shortVideo.Id);
        _context.ShortVideoBookmarks.Add(bookmark);
        await _context.SaveChangesAsync();

        // Act
        bool result = await _repository.HasBookmarkedAsync(userId, shortVideo.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasBookmarkedAsync_WhenBookmarkDoesNotExist_ShouldReturnFalse()
    {
        // Act
        bool result = await _repository.HasBookmarkedAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddBookmarkAsync Tests

    [Fact]
    public async Task AddBookmarkAsync_ShouldAddBookmarkToContext()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        await _context.SaveChangesAsync();

        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkEntity.Create(Guid.NewGuid(), userId, shortVideo.Id);

        // Act
        await _repository.AddBookmarkAsync(bookmark);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ShortVideoBookmarks.AnyAsync(b =>
            b.UserId == userId && b.ShortVideoId == shortVideo.Id
        );
        exists.Should().BeTrue();
    }

    #endregion

    #region RemoveBookmarkAsync Tests

    [Fact]
    public async Task RemoveBookmarkAsync_WhenBookmarkExists_ShouldRemoveIt()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkEntity.Create(Guid.NewGuid(), userId, shortVideo.Id);
        _context.ShortVideoBookmarks.Add(bookmark);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveBookmarkAsync(userId, shortVideo.Id);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ShortVideoBookmarks.AnyAsync(b =>
            b.UserId == userId && b.ShortVideoId == shortVideo.Id
        );
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveBookmarkAsync_WhenBookmarkDoesNotExist_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _repository.RemoveBookmarkAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region AddShareAsync Tests

    [Fact]
    public async Task AddShareAsync_ShouldAddShareToContext()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _context.ShortVideos.Add(shortVideo);
        await _context.SaveChangesAsync();

        ShortVideoShareEntity share = ShortVideoShareEntity.Create(Guid.NewGuid(), null, shortVideo.Id);

        // Act
        await _repository.AddShareAsync(share);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.ShortVideoShares.AnyAsync(s => s.ShortVideoId == shortVideo.Id);
        exists.Should().BeTrue();
    }

    #endregion
}
