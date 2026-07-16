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
/// Unit tests for <see cref="VideoRepository"/>.
/// </summary>
public class VideoRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly VideoRepository _repository;

    public VideoRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new VideoRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Seeds a ContentType and Category, returning the Category ID.
    /// </summary>
    private async Task<Guid> SeedCategoryAsync()
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _context.ContentTypes.Add(contentType);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    private static void SetActivityTime(VideoRatingEntity rating, DateTime createdAt, DateTime? updatedAt = null)
    {
        rating.CreatedAt = createdAt;
        rating.UpdatedAt = updatedAt;
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoFilters_ShouldReturnAllVideos()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Videos.AddRange(VideoFactory.CreateMany(categoryId, 3));
        await _context.SaveChangesAsync();

        // Act
        var (videos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        videos.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Videos.AddRange(VideoFactory.CreateMany(categoryId, 5));
        await _context.SaveChangesAsync();

        // Act
        var (videos, totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 2,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        videos.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Videos.Add(VideoFactory.Create(categoryId)); // Draft
        _context.Videos.Add(VideoFactory.CreatePublished(categoryId)); // Published
        await _context.SaveChangesAsync();

        // Act
        var (videos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: EnumContentStatus.Draft,
            categoryId: null
        );

        // Assert
        videos.Should().ContainSingle();
        totalCount.Should().Be(1);
        videos.First().Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        Guid otherCategoryId = await SeedCategoryAsync();

        _context.Videos.Add(VideoFactory.Create(categoryId));
        _context.Videos.Add(VideoFactory.Create(otherCategoryId));
        await _context.SaveChangesAsync();

        // Act
        var (videos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: categoryId
        );

        // Assert
        videos.Should().ContainSingle();
        totalCount.Should().Be(1);
        videos.First().CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Act
        var (videos, totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null,
            status: null,
            categoryId: null
        );

        // Assert
        videos.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    // Note: GetAllAsync with search uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // Search filtering is covered in integration tests.

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenVideoExists_ShouldReturnVideo()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        // Act
        VideoEntity? result = await _repository.GetByIdAsync(video.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(video.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVideoDoesNotExist_ShouldReturnNull()
    {
        // Act
        VideoEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenVideoExists_ShouldReturnVideo()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        // Act
        VideoEntity result = await _repository.GetByIdOrThrowAsync(video.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(video.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenVideoDoesNotExist_ShouldThrowNotFoundException()
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

    #region GetPromotedAsync Tests

    [Fact]
    public async Task GetPromotedAsync_ShouldReturnPromotedPublishedVideos()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity promoted = VideoFactory.CreatePromoted(categoryId);
        VideoEntity draft = VideoFactory.Create(categoryId);
        _context.Videos.AddRange(promoted, draft);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<VideoEntity> result = await _repository.GetPromotedAsync();

        // Assert
        result.Should().ContainSingle();
        result.First().IsPromoted.Should().BeTrue();
        result.First().Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetPromotedAsync_WhenNoPromotedVideos_ShouldReturnEmptyList()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        _context.Videos.Add(VideoFactory.Create(categoryId));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<VideoEntity> result = await _repository.GetPromotedAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddVideoToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);

        // Act
        await _repository.AddAsync(video);
        await _context.SaveChangesAsync();

        // Assert
        VideoEntity? saved = await _context.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);
        saved.Should().NotBeNull();
        saved.Title.Should().Be(video.Title);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkVideoAsModified()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(video);

        // Assert
        _context.Entry(video).State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region UpdateRating Tests

    [Fact]
    public async Task UpdateRating_ShouldMarkRatingAsModified()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        VideoRatingEntity rating = VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 4);
        _context.VideoRatings.Add(rating);
        await _context.SaveChangesAsync();

        // Act
        _repository.UpdateRating(rating);

        // Assert
        _context.Entry(rating).State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_ShouldRemoveVideoFromContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(video);
        await _context.SaveChangesAsync();

        // Assert
        VideoEntity? deleted = await _context.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);
        deleted.Should().BeNull();
    }

    #endregion

    #region GetRatingAsync Tests

    [Fact]
    public async Task GetRatingAsync_WhenRatingExists_ShouldReturnRating()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        VideoRatingEntity rating = VideoRatingFactory.Create(video.Id, userId, stars: 4);
        _context.VideoRatings.Add(rating);
        await _context.SaveChangesAsync();

        // Act
        VideoRatingEntity? result = await _repository.GetRatingAsync(userId, video.Id);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetRatingAsync_WhenRatingDoesNotExist_ShouldReturnNull()
    {
        // Act
        VideoRatingEntity? result = await _repository.GetRatingAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddRatingAsync Tests

    [Fact]
    public async Task AddRatingAsync_ShouldAddRatingToContext()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        VideoRatingEntity rating = VideoRatingFactory.Create(video.Id, userId, stars: 5);

        // Act
        await _repository.AddRatingAsync(rating);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.VideoRatings.AnyAsync(r => r.UserId == userId && r.VideoId == video.Id);
        exists.Should().BeTrue();
    }

    #endregion

    #region GetAllRatingsForVideoAsync Tests

    [Fact]
    public async Task GetAllRatingsForVideoAsync_WhenRatingsExist_ShouldReturnAll()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        _context.VideoRatings.AddRange(
            VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 3),
            VideoRatingFactory.Create(video.Id, Guid.NewGuid(), stars: 5)
        );
        await _context.SaveChangesAsync();

        // Act
        List<VideoRatingEntity> result = await _repository.GetAllRatingsForVideoAsync(video.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllRatingsForVideoAsync_WhenNoRatings_ShouldReturnEmpty()
    {
        // Act
        List<VideoRatingEntity> result = await _repository.GetAllRatingsForVideoAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddShareAsync Tests

    [Fact]
    public async Task AddShareAsync_ShouldAddShareToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        VideoShareEntity share = VideoShareEntity.Create(Guid.NewGuid(), null, video.Id);

        // Act
        await _repository.AddShareAsync(share);
        await _context.SaveChangesAsync();

        // Assert
        bool exists = await _context.VideoShares.AnyAsync(s => s.VideoId == video.Id);
        exists.Should().BeTrue();
    }

    #endregion

    #region AddTagAsync / GetTagsByVideoIdAsync / RemoveTag Tests

    [Fact]
    public async Task AddTagAsync_ShouldAddTagJunctionToContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);

        TagEntity tag = TagFactory.CreateDefault();
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var videoTag = VideoTagEntity.Create(id: Guid.NewGuid(), videoId: video.Id, tagId: tag.Id);

        // Act
        await _repository.AddTagAsync(videoTag);
        await _context.SaveChangesAsync();

        // Assert
        IReadOnlyList<VideoTagEntity> result = await _repository.GetTagsByVideoIdAsync(video.Id);
        result.Should().ContainSingle();
        result.First().TagId.Should().Be(tag.Id);
    }

    [Fact]
    public async Task GetTagsByVideoIdAsync_WhenNoTags_ShouldReturnEmptyList()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<VideoTagEntity> result = await _repository.GetTagsByVideoIdAsync(video.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveTag_ShouldRemoveTagJunctionFromContext()
    {
        // Arrange
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);

        TagEntity tag = TagFactory.CreateDefault();
        _context.Tags.Add(tag);

        var videoTag = VideoTagEntity.Create(id: Guid.NewGuid(), videoId: video.Id, tagId: tag.Id);
        _context.VideoTags.Add(videoTag);
        await _context.SaveChangesAsync();

        // Act
        _repository.RemoveTag(videoTag);
        await _context.SaveChangesAsync();

        // Assert
        IReadOnlyList<VideoTagEntity> result = await _repository.GetTagsByVideoIdAsync(video.Id);
        result.Should().BeEmpty();
    }

    #endregion
    #region Favorite Collection Read Tests

    [Fact]
    public async Task GetRatedVideosByUserAsync_ReturnsOwnPublishedRatingsByLatestActivity()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userId = Guid.NewGuid();
        VideoEntity olderVideo = VideoFactory.CreatePublished(categoryId);
        VideoEntity reratedVideo = VideoFactory.CreatePublished(categoryId);
        VideoEntity unpublishedVideo = VideoFactory.Create(categoryId);
        _context.Videos.AddRange(olderVideo, reratedVideo, unpublishedVideo);
        VideoRatingEntity older = VideoRatingEntity.Create(Guid.NewGuid(), userId, olderVideo.Id, 2);
        VideoRatingEntity rerated = VideoRatingEntity.Create(Guid.NewGuid(), userId, reratedVideo.Id, 5);
        VideoRatingEntity unpublished = VideoRatingEntity.Create(Guid.NewGuid(), userId, unpublishedVideo.Id, 4);
        VideoRatingEntity otherUser = VideoRatingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), olderVideo.Id, 1);
        DateTime now = DateTime.UtcNow;
        SetActivityTime(older, now.AddDays(-2));
        SetActivityTime(rerated, now.AddDays(-3), now);
        SetActivityTime(unpublished, now.AddDays(1));
        SetActivityTime(otherUser, now.AddDays(2));
        _context.VideoRatings.AddRange(older, rerated, unpublished, otherUser);
        await _context.SaveChangesAsync();

        var (activities, totalCount) = await _repository.GetRatedVideosByUserAsync(userId, 1, 10);

        totalCount.Should().Be(2);
        activities.Select(activity => activity.Video.Id).Should().Equal(reratedVideo.Id, olderVideo.Id);
        activities[0].Stars.Should().Be(5);
        activities[0].LastInteractedAt.Should().Be(now);
    }

    [Fact]
    public async Task GetRatedVideosByUserAsync_AppliesStablePagination()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userId = Guid.NewGuid();
        VideoEntity[] videos = VideoFactory.CreateManyPublished(categoryId, 3).ToArray();
        _context.Videos.AddRange(videos);
        DateTime tie = DateTime.UtcNow;
        foreach (VideoEntity video in videos)
        {
            VideoRatingEntity rating = VideoRatingEntity.Create(Guid.NewGuid(), userId, video.Id, 3);
            SetActivityTime(rating, tie);
            _context.VideoRatings.Add(rating);
        }
        await _context.SaveChangesAsync();

        var (activities, totalCount) = await _repository.GetRatedVideosByUserAsync(userId, 2, 1);

        totalCount.Should().Be(3);
        activities.Should().ContainSingle();
        activities[0].Video.Id.Should().Be(videos.Select(video => video.Id).Order().ElementAt(1));
    }

    [Fact]
    public async Task GetSharedVideosByUserAsync_GroupsOwnEventsAndExcludesAnonymousOtherUserAndUnpublished()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userId = Guid.NewGuid();
        VideoEntity published = VideoFactory.CreatePublished(categoryId);
        VideoEntity unpublished = VideoFactory.Create(categoryId);
        _context.Videos.AddRange(published, unpublished);
        DateTime latest = DateTime.UtcNow;
        VideoShareEntity older = VideoShareEntity.Create(
            Guid.NewGuid(),
            userId,
            published.Id,
            EnumShareChannel.Facebook
        );
        VideoShareEntity newer = VideoShareEntity.Create(
            Guid.NewGuid(),
            userId,
            published.Id,
            EnumShareChannel.WhatsApp
        );
        older.CreatedAt = latest.AddDays(-1);
        newer.CreatedAt = latest;
        _context.VideoShares.AddRange(
            older,
            newer,
            VideoShareEntity.Create(Guid.NewGuid(), null, published.Id, EnumShareChannel.X),
            VideoShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), published.Id, EnumShareChannel.Clipboard),
            VideoShareEntity.Create(Guid.NewGuid(), userId, unpublished.Id, EnumShareChannel.WebShare)
        );
        await _context.SaveChangesAsync();

        var (activities, totalCount) = await _repository.GetSharedVideosByUserAsync(userId, 1, 10);

        totalCount.Should().Be(1);
        SharedVideoActivity activity = activities.Should().ContainSingle().Subject;
        activity.Video.Id.Should().Be(published.Id);
        activity.ShareCount.Should().Be(2);
        activity.LastInteractedAt.Should().Be(latest);
        activity.LastShareChannel.Should().Be(EnumShareChannel.WhatsApp);
    }

    [Fact]
    public async Task GetSharedVideosByUserAsync_CountsDistinctVideosAndPaginatesGroups()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userId = Guid.NewGuid();
        VideoEntity[] videos = VideoFactory.CreateManyPublished(categoryId, 3).ToArray();
        _context.Videos.AddRange(videos);
        DateTime baseTime = DateTime.UtcNow;
        for (int index = 0; index < videos.Length; index++)
        {
            VideoShareEntity share = VideoShareEntity.Create(Guid.NewGuid(), userId, videos[index].Id);
            share.CreatedAt = baseTime.AddMinutes(index);
            _context.VideoShares.Add(share);
        }
        await _context.SaveChangesAsync();

        var (activities, totalCount) = await _repository.GetSharedVideosByUserAsync(userId, 2, 1);

        totalCount.Should().Be(3);
        activities.Should().ContainSingle();
        activities[0].Video.Id.Should().Be(videos[1].Id);
    }

    #endregion
}
