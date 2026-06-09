using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Shared.Application.DTOs;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="VideoMapper"/> extension methods and mapping registration.
/// </summary>
public class VideoMapperTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    /// <summary>
    /// Creates a video entity with the Category navigation property populated via reflection,
    /// so the mapper can access Category.Name without a database.
    /// </summary>
    private static VideoEntity CreateVideoWithCategory()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        video.GetType().GetProperty("Category")!.SetValue(video, category);
        return video;
    }

    #region Register

    [Fact]
    public void Register_ShouldNotThrowException()
    {
        Action act = () => MappingRegistration.CreateConfiguration();

        act.Should().NotThrow();
    }

    #endregion

    #region ToVideoSummaryDto — category name

    [Fact]
    public void ToVideoSummaryDto_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        // Category nav property is null by default

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public void ToVideoSummaryDto_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.CategoryName.Should().NotBeEmpty();
    }

    #endregion

    #region ToVideoSummaryDto — core fields

    [Fact]
    public void ToVideoSummaryDto_ShouldMapCoreFields()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.Id.Should().Be(video.Id);
        dto.CategoryId.Should().Be(video.CategoryId);
        dto.Title.Should().Be(video.Title);
        dto.Slug.Should().Be(video.Slug);
        dto.ThumbnailUrl.Should().Be(video.ThumbnailUrl);
        dto.AuthorId.Should().Be(video.AuthorId.ToString());
        dto.Status.Should().Be(video.Status);
        dto.YoutubeVideoUrl.Should().Be(video.YoutubeVideoUrl);
        dto.IsPromoted.Should().Be(video.IsPromoted);
        dto.HasLyrics.Should().Be(video.HasLyrics);
        dto.PublishedAt.Should().Be(video.PublishedAt);
        dto.ShootingScheduledAt.Should().Be(video.ShootingScheduledAt);
    }

    #endregion

    #region ToVideoSummaryDto — AuditableDto fields

    [Fact]
    public void ToVideoSummaryDto_ShouldInheritAuditableDto()
    {
        typeof(VideoSummaryDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToVideoSummaryDto_ShouldMapAuditFields()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.CreatedAt = new DateTime(2025, 2, 10, 9, 0, 0, DateTimeKind.Utc);
        video.CreatedBy = "creator";
        video.UpdatedAt = new DateTime(2025, 5, 20, 14, 0, 0, DateTimeKind.Utc);
        video.UpdatedBy = "updater";

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.CreatedAt.Should().Be(video.CreatedAt);
        dto.CreatedBy.Should().Be("creator");
        dto.UpdatedAt.Should().Be(video.UpdatedAt);
        dto.UpdatedBy.Should().Be("updater");
    }

    #endregion

    #region ToVideoSummaryDto — promoted fields

    [Fact]
    public void ToVideoSummaryDto_WhenPromoted_ShouldMapIsPromotedTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId);

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeTrue();
    }

    [Fact]
    public void ToVideoSummaryDto_WhenNotPromoted_ShouldMapIsPromotedFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeFalse();
    }

    #endregion

    #region ToVideoSummaryDto — PromotionLevel nav property null safety

    [Fact]
    public void ToVideoSummaryDto_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange — video with IsPromoted=true but PromotionLevel nav not loaded (null)
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        Action act = () => video.ToVideoSummaryDto(Mapper);

        // Assert — must not NPE
        act.Should().NotThrow();
    }

    #endregion

    #region ToVideoSummaryDtos — list mapping

    [Fact]
    public void ToVideoSummaryDtos_ShouldMapAllEntities()
    {
        // Arrange
        IReadOnlyList<VideoEntity> videos = VideoFactory.CreateMany(CategoryId, 3);

        // Act
        IReadOnlyList<VideoSummaryDto> dtos = videos.ToVideoSummaryDtos(Mapper);

        // Assert
        dtos.Should().HaveCount(3);
    }

    [Fact]
    public void ToVideoSummaryDtos_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<VideoEntity> videos = [];

        // Act
        IReadOnlyList<VideoSummaryDto> dtos = videos.ToVideoSummaryDtos(Mapper);

        // Assert
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void ToVideoSummaryDtos_ShouldPreserveOrder()
    {
        // Arrange
        VideoEntity first = VideoFactory.Create(CategoryId);
        VideoEntity second = VideoFactory.Create(CategoryId);
        IReadOnlyList<VideoEntity> videos = [first, second];

        // Act
        IReadOnlyList<VideoSummaryDto> dtos = videos.ToVideoSummaryDtos(Mapper);

        // Assert
        dtos[0].Id.Should().Be(first.Id);
        dtos[1].Id.Should().Be(second.Id);
    }

    #endregion

    #region ToVideoDetailDto — AuditableDto inheritance

    [Fact]
    public void ToVideoDetailDto_ShouldInheritAuditableDto()
    {
        typeof(VideoDetailDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var expectedCreatedAt = new DateTime(2024, 3, 20, 9, 0, 0, DateTimeKind.Utc);
        video.CreatedAt = expectedCreatedAt;

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CreatedAt.Should().Be(expectedCreatedAt);
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        video.CreatedBy = "admin-user-id";

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CreatedBy.Should().Be("admin-user-id");
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var expectedUpdatedAt = new DateTime(2025, 6, 15, 12, 30, 0, DateTimeKind.Utc);
        video.UpdatedAt = expectedUpdatedAt;

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.UpdatedAt.Should().Be(expectedUpdatedAt);
    }

    [Fact]
    public void ToVideoDetailDto_ShouldMapUpdatedByFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        video.UpdatedBy = "super-admin-id";

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.UpdatedBy.Should().Be("super-admin-id");
    }

    #endregion

    #region ToVideoDetailDto — core field mapping

    [Fact]
    public void ToVideoDetailDto_ShouldMapCoreFields()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        var dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.Id.Should().Be(video.Id);
        dto.CategoryId.Should().Be(video.CategoryId);
        dto.AuthorId.Should().Be(video.AuthorId.ToString());
        dto.Status.Should().Be(video.Status);
        dto.Title.Should().Be(video.Title);
        dto.Slug.Should().Be(video.Slug);
        dto.Description.Should().Be(video.Description);
        dto.ThumbnailUrl.Should().Be(video.ThumbnailUrl);
        dto.ThumbnailStorageKey.Should().Be(video.ThumbnailStorageKey);
        dto.YoutubeVideoUrl.Should().Be(video.YoutubeVideoUrl);
        dto.HasLyrics.Should().Be(video.HasLyrics);
        dto.ShootingScheduledAt.Should().Be(video.ShootingScheduledAt);
        dto.PublishedAt.Should().Be(video.PublishedAt);
        dto.MetaTitle.Should().Be(video.MetaTitle);
        dto.MetaDescription.Should().Be(video.MetaDescription);
        dto.RejectionReason.Should().Be(video.RejectionReason);
        dto.SocialBoost.Should().Be(video.SocialBoost);
    }

    #endregion

    #region ToVideoDetailDto — category name

    [Fact]
    public void ToVideoDetailDto_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public void ToVideoDetailDto_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CategoryName.Should().NotBeEmpty();
    }

    #endregion

    #region ToVideoDetailDto — promotion fields

    [Fact]
    public void ToVideoDetailDto_WhenPromoted_ShouldMapPromotionFields()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId);

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeTrue();
        dto.PromotedUntil.Should().NotBeNull();
    }

    [Fact]
    public void ToVideoDetailDto_WhenNotPromoted_ShouldMapPromotionFieldsAsDefault()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.IsPromoted.Should().BeFalse();
        dto.PromotedUntil.Should().BeNull();
    }

    [Fact]
    public void ToVideoDetailDto_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange — video has IsPromoted=true but PromotionLevel nav not loaded
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        Action act = () => video.ToVideoDetailDto(Mapper);

        // Assert — must not NPE
        act.Should().NotThrow();
    }

    #endregion

    #region ToVideoSummaryDto — engagement counters

    [Fact]
    public void ToVideoSummaryDto_ShouldMapEngagementCounters()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.IncrementShareCount();
        video.IncrementShareCount();
        video.UpdateRating(4.5m, 10);

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.ShareCount.Should().Be(2);
        dto.RatingAverage.Should().Be(4.5m);
        dto.RatingCount.Should().Be(10);
    }

    [Fact]
    public void ToVideoSummaryDto_WhenNoInteractions_ShouldMapCountersAsZero()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoSummaryDto dto = video.ToVideoSummaryDto(Mapper);

        // Assert
        dto.ShareCount.Should().Be(0);
        dto.RatingAverage.Should().Be(0m);
        dto.RatingCount.Should().Be(0);
    }

    #endregion

    #region ToVideoDetailDto — engagement counters

    [Fact]
    public void ToVideoDetailDto_ShouldMapEngagementCounters()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.IncrementShareCount();
        video.UpdateRating(3.8m, 5);

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.ShareCount.Should().Be(1);
        dto.RatingAverage.Should().Be(3.8m);
        dto.RatingCount.Should().Be(5);
    }

    [Fact]
    public void ToVideoDetailDto_WhenNoInteractions_ShouldMapCountersAsZero()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.ShareCount.Should().Be(0);
        dto.RatingAverage.Should().Be(0m);
        dto.RatingCount.Should().Be(0);
    }

    #endregion

    #region ToVideoDetailDto — customer and order item mapping

    [Fact]
    public void ToVideoDetailDto_WhenFreeVideo_ShouldMapCustomerIdAsNull()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CustomerId.Should().BeNull();
        dto.CustomerName.Should().BeNull();
        dto.OrderItemId.Should().BeNull();
    }

    [Fact]
    public void ToVideoDetailDto_WhenPaidVideo_ShouldMapCustomerId()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        VideoEntity video = VideoFactory.CreatePaid(CategoryId, customerId, orderItemId);
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        CustomerEntity customer = CustomerFactory.Create();

        video.GetType().GetProperty("Category")!.SetValue(video, category);
        video.GetType().GetProperty("Customer")!.SetValue(video, customer);

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public void ToVideoDetailDto_WhenCustomerNavIsNull_ShouldMapCustomerNameAsNull()
    {
        // Arrange — CustomerId is set but Customer nav not loaded
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePaid(CategoryId, customerId, orderItemId);

        // Act
        VideoDetailDto dto = video.ToVideoDetailDto(Mapper);

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().BeNull();
    }

    #endregion
}
