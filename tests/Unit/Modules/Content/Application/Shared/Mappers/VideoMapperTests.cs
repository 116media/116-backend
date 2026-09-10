using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Application.DTOs;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="VideoMapper"/> extension methods and mapping registration.
/// </summary>
public class VideoMapperTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid ContentTypeId = Guid.NewGuid();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();

    /// <summary>
    /// Creates a video entity with the Category navigation property populated via reflection,
    /// so the mapper can access Category.Name without a database.
    /// </summary>
    private static VideoEntity CreateVideoWithCategory()
    {
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        VideoEntity video = new VideoBuilder(CategoryId).WithCategory(category).Build();
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

    /// <summary>
    /// Maps one video through the live batch member, so these tests exercise the same path
    /// the admin listings use.
    /// </summary>
    private async Task<VideoSummaryDto> MapSummaryAsync(VideoEntity video)
    {
        IReadOnlyList<VideoEntity> videos = [video];
        IReadOnlyList<VideoSummaryDto> dtos = await videos.ToVideoSummaryDtosAsync(Mapper, _fileStorageMock.Object);
        return dtos.Single();
    }

    #region ToVideoSummaryDtosAsync — category name

    [Fact]
    public async Task ToVideoSummaryDtosAsync_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        // Category nav property is null by default

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public async Task ToVideoSummaryDtosAsync_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.CategoryName.Should().NotBeEmpty();
    }

    #endregion

    #region ToVideoSummaryDtosAsync — core fields

    [Fact]
    public async Task ToVideoSummaryDtosAsync_ShouldMapCoreFields()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.Id.Should().Be(video.Id);
        dto.CategoryId.Should().Be(video.CategoryId);
        dto.Title.Should().Be(video.Title);
        dto.Slug.Should().Be(video.Slug);
        dto.AuthorId.Should().Be(video.AuthorId.ToString());
        dto.Status.Should().Be(video.Status);
        dto.YoutubeVideoUrl.Should().Be(video.YoutubeVideoUrl);
        dto.IsPromoted.Should().Be(video.IsPromoted);
        dto.HasLyrics.Should().Be(video.HasLyrics);
        dto.PublishedAt.Should().Be(video.PublishedAt);
        dto.ShootingScheduledAt.Should().Be(video.ShootingScheduledAt);
    }

    #endregion

    #region ToVideoSummaryDtosAsync — AuditableDto fields

    [Fact]
    public void ToVideoSummaryDto_ShouldInheritAuditableDto()
    {
        typeof(VideoSummaryDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public async Task ToVideoSummaryDtosAsync_ShouldMapAuditFields()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.CreatedAt = new DateTime(2025, 2, 10, 9, 0, 0, DateTimeKind.Utc);
        video.CreatedBy = "creator";
        video.UpdatedAt = new DateTime(2025, 5, 20, 14, 0, 0, DateTimeKind.Utc);
        video.UpdatedBy = "updater";

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.CreatedAt.Should().Be(video.CreatedAt);
        dto.CreatedBy.Should().Be("creator");
        dto.UpdatedAt.Should().Be(video.UpdatedAt);
        dto.UpdatedBy.Should().Be("updater");
    }

    #endregion

    #region ToVideoSummaryDtosAsync — promoted fields

    [Fact]
    public async Task ToVideoSummaryDtosAsync_WhenPromoted_ShouldMapIsPromotedTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId);

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.IsPromoted.Should().BeTrue();
    }

    [Fact]
    public async Task ToVideoSummaryDtosAsync_WhenNotPromoted_ShouldMapIsPromotedFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.IsPromoted.Should().BeFalse();
    }

    #endregion

    #region ToVideoSummaryDtosAsync — PromotionLevel nav property null safety

    [Fact]
    public async Task ToVideoSummaryDtosAsync_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        Func<Task> act = () => MapSummaryAsync(video);

        // Assert — must not NPE
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ToVideoSummaryDtosAsync — list mapping

    [Fact]
    public async Task ToVideoSummaryDtosAsync_ShouldMapAllEntities()
    {
        // Arrange
        IReadOnlyList<VideoEntity> videos = VideoFactory.CreateMany(CategoryId, 3);

        // Act
        IReadOnlyList<VideoSummaryDto> dtos = await videos.ToVideoSummaryDtosAsync(
            Mapper,
            _fileStorageMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos.Should().HaveCount(3);
    }

    [Fact]
    public async Task ToVideoSummaryDtosAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        IReadOnlyList<VideoEntity> videos = [];

        // Act
        IReadOnlyList<VideoSummaryDto> dtos = await videos.ToVideoSummaryDtosAsync(
            Mapper,
            _fileStorageMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos.Should().BeEmpty();
    }

    [Fact]
    public async Task ToVideoSummaryDtosAsync_ShouldPreserveOrder()
    {
        // Arrange
        VideoEntity first = VideoFactory.Create(CategoryId);
        VideoEntity second = VideoFactory.Create(CategoryId);
        IReadOnlyList<VideoEntity> videos = [first, second];

        // Act
        IReadOnlyList<VideoSummaryDto> dtos = await videos.ToVideoSummaryDtosAsync(
            Mapper,
            _fileStorageMock.Object,
            CancellationToken.None
        );

        // Assert
        dtos[0].Id.Should().Be(first.Id);
        dtos[1].Id.Should().Be(second.Id);
    }

    #endregion

    #region ToVideoDetailDtoAsync — AuditableDto inheritance

    [Fact]
    public void ToVideoDetailDto_ShouldInheritAuditableDto()
    {
        typeof(VideoDetailDto).Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var expectedCreatedAt = new DateTime(2024, 3, 20, 9, 0, 0, DateTimeKind.Utc);
        video.CreatedAt = expectedCreatedAt;

        // Act
        var dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.CreatedAt.Should().Be(expectedCreatedAt);
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        video.CreatedBy = "admin-user-id";

        // Act
        var dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.CreatedBy.Should().Be("admin-user-id");
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var expectedUpdatedAt = new DateTime(2025, 6, 15, 12, 30, 0, DateTimeKind.Utc);
        video.UpdatedAt = expectedUpdatedAt;

        // Act
        var dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.UpdatedAt.Should().Be(expectedUpdatedAt);
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_ShouldMapUpdatedByFromEntity()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        video.UpdatedBy = "super-admin-id";

        // Act
        var dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.UpdatedBy.Should().Be("super-admin-id");
    }

    #endregion

    #region ToVideoDetailDtoAsync — core field mapping

    [Fact]
    public async Task ToVideoDetailDtoAsync_ShouldMapCoreFields()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        var dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.Id.Should().Be(video.Id);
        dto.CategoryId.Should().Be(video.CategoryId);
        dto.AuthorId.Should().Be(video.AuthorId.ToString());
        dto.Status.Should().Be(video.Status);
        dto.Title.Should().Be(video.Title);
        dto.Slug.Should().Be(video.Slug);
        dto.Description.Should().Be(video.Description);
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

    #region ToVideoDetailDtoAsync — category name

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenCategoryIsNull_ShouldMapCategoryNameAsEmptyString()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenCategoryIsLoaded_ShouldMapCategoryName()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.CategoryName.Should().NotBeEmpty();
    }

    #endregion

    #region ToVideoDetailDtoAsync — promotion fields

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenPromoted_ShouldMapPromotionFields()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId);

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.IsPromoted.Should().BeTrue();
        dto.PromotedUntil.Should().NotBeNull();
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenNotPromoted_ShouldMapPromotionFieldsAsDefault()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.IsPromoted.Should().BeFalse();
        dto.PromotedUntil.Should().BeNull();
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenPromotionLevelIsNull_ShouldNotThrow()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        Func<Task> act = () => video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert — must not NPE
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ToVideoSummaryDtosAsync — engagement counters

    [Fact]
    public async Task ToVideoSummaryDtosAsync_ShouldMapEngagementCounters()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.WithShareCount(2);
        video.UpdateRating(4.5m, 10);

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.ShareCount.Should().Be(2);
        dto.RatingAverage.Should().Be(4.5m);
        dto.RatingCount.Should().Be(10);
    }

    [Fact]
    public async Task ToVideoSummaryDtosAsync_WhenNoInteractions_ShouldMapCountersAsZero()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoSummaryDto dto = await MapSummaryAsync(video);

        // Assert
        dto.ShareCount.Should().Be(0);
        dto.RatingAverage.Should().Be(0m);
        dto.RatingCount.Should().Be(0);
    }

    #endregion

    #region ToVideoDetailDtoAsync — engagement counters

    [Fact]
    public async Task ToVideoDetailDtoAsync_ShouldMapEngagementCounters()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.WithShareCount(1);
        video.UpdateRating(3.8m, 5);

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.ShareCount.Should().Be(1);
        dto.RatingAverage.Should().Be(3.8m);
        dto.RatingCount.Should().Be(5);
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenNoInteractions_ShouldMapCountersAsZero()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.ShareCount.Should().Be(0);
        dto.RatingAverage.Should().Be(0m);
        dto.RatingCount.Should().Be(0);
    }

    #endregion

    #region ToVideoDetailDtoAsync — customer and order item mapping

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenFreeVideo_ShouldMapCustomerIdAsNull()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.CustomerId.Should().BeNull();
        dto.CustomerName.Should().BeNull();
        dto.OrderItemId.Should().BeNull();
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenPaidVideo_ShouldMapCustomerId()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        CustomerEntity customer = CustomerFactory.Create();
        VideoEntity video = new VideoBuilder(CategoryId)
            .WithCustomer(customerId, orderItemId)
            .WithCategory(category)
            .WithCustomerNavigation(customer)
            .Build();

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public async Task ToVideoDetailDtoAsync_WhenCustomerNavIsNull_ShouldMapCustomerNameAsNull()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePaid(CategoryId, customerId, orderItemId);

        // Act
        VideoDetailDto dto = await video.ToVideoDetailDtoAsync(Mapper, _fileStorageMock.Object, CancellationToken.None);

        // Assert
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().BeNull();
    }

    #endregion

    [Fact]
    public void ToPublicVideoSummaryDtos_WithPreFetchedFiles_ShouldMapEveryEntity()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        FileReferenceDto thumbnail = FileReferenceDtoFactory.CreateWithStorageUrl(
            "https://cdn.116.test/thumbs/clip.jpg"
        );
        video.SetThumbnailFileId(thumbnail.Id);
        IReadOnlyDictionary<Guid, FileReferenceDto> files = new Dictionary<Guid, FileReferenceDto>
        {
            [thumbnail.Id] = thumbnail,
        };

        IReadOnlyList<VideoEntity> videos = [video];

        // Act
        IReadOnlyList<PublicVideoSummaryDto> dtos = videos.ToPublicVideoSummaryDtos(files);

        // Assert
        dtos.Should().ContainSingle().Which.ThumbnailUrl.Should().Be(thumbnail.StorageUrl);
    }

    [Fact]
    public async Task ToPublicVideoSummaryDtoAsync_WithThumbnail_ShouldResolveTheStorageUrl()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        FileReferenceDto thumbnail = FileReferenceDtoFactory.CreateWithStorageUrl(
            "https://cdn.116.test/thumbs/clip.jpg"
        );
        video.SetThumbnailFileId(thumbnail.Id);
        _fileStorageMock
            .Setup(x => x.ResolveAsync(thumbnail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thumbnail);

        // Act
        PublicVideoSummaryDto dto = await video.ToPublicVideoSummaryDtoAsync(_fileStorageMock.Object);

        // Assert
        dto.ThumbnailUrl.Should().Be(thumbnail.StorageUrl);
    }
}
