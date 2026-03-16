using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="VideoEntity"/>.
/// </summary>
public class VideoEntityTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    #region CreateFree Tests

    [Fact]
    public void CreateFree_WithValidParams_ShouldCreateDraftVideo()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string title = TestConstants.Content.Editorial.Video.ValidTitle;
        const string slug = TestConstants.Content.Editorial.Video.ValidSlug;

        // Act
        VideoEntity video = VideoEntity.CreateFree(id, CategoryId, title, slug, AuthorId);

        // Assert
        video.Id.Should().Be(id);
        video.CategoryId.Should().Be(CategoryId);
        video.Title.Should().Be(title);
        video.Slug.Should().Be(slug);
        video.AuthorId.Should().Be(AuthorId);
        video.Status.Should().Be(EnumContentStatus.Draft);
        video.CustomerId.Should().BeNull();
        video.YoutubeVideoId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFree_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
    {
        // Act
        Action act = () =>
            VideoEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                invalidTitle!,
                TestConstants.Content.Editorial.Video.ValidSlug,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFree_WithEmptySlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () =>
            VideoEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                TestConstants.Content.Editorial.Video.ValidTitle,
                invalidSlug!,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region CreatePaid Tests

    [Fact]
    public void CreatePaid_WithValidParams_ShouldSetCustomerAndOrderItem()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        // Act
        VideoEntity video = VideoEntity.CreatePaid(
            Guid.NewGuid(),
            customerId,
            orderItemId,
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Assert
        video.CustomerId.Should().Be(customerId);
        video.OrderItemId.Should().Be(orderItemId);
        video.Status.Should().Be(EnumContentStatus.Draft);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreatePaid_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
    {
        // Act
        Action act = () =>
            VideoEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                invalidTitle!,
                TestConstants.Content.Editorial.Video.ValidSlug,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreatePaid_WithEmptySlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () =>
            VideoEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                TestConstants.Content.Editorial.Video.ValidTitle,
                invalidSlug!,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void Submit_WhenDraft_ShouldTransitionToPendingPayment()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Act
        bool result = video.Submit();

        // Assert
        result.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.PendingPayment);
    }

    [Fact]
    public void Submit_WhenAlreadyPendingPayment_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.Submit();

        // Act
        bool result = video.Submit();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldTransitionToApproved()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.MarkPendingReview();

        // Act
        bool result = video.Approve();

        // Assert
        result.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.Approved);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.MarkPendingReview();
        video.Approve();

        // Act
        bool result = video.Approve();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Publish_WithYoutubeId_ShouldTransitionToPublished_AndSetPublishedAt()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.AttachYoutubeId(TestConstants.Content.Editorial.Video.ValidYoutubeVideoId);
        video.MarkPendingReview();
        video.Approve();

        // Act
        bool result = video.Publish();

        // Assert
        result.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.Published);
        video.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Publish_WithoutYoutubeId_ShouldThrow()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.MarkPendingReview();
        video.Approve();

        // Act
        Action act = () => video.Publish();

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.AttachYoutubeId(TestConstants.Content.Editorial.Video.ValidYoutubeVideoId);
        video.MarkPendingReview();
        video.Approve();
        video.Publish();

        // Act
        bool result = video.Publish();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Reject_ShouldSetRejectionReason()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        const string reason = TestConstants.Content.Editorial.Video.ValidRejectionReason;

        // Act
        bool result = video.Reject(reason);

        // Assert
        result.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.Rejected);
        video.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.Reject(TestConstants.Content.Editorial.Video.ValidRejectionReason);

        // Act
        bool result = video.Reject(TestConstants.Content.Editorial.Video.ValidRejectionReason);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Archive_ShouldTransitionToArchived()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.AttachYoutubeId(TestConstants.Content.Editorial.Video.ValidYoutubeVideoId);
        video.MarkPendingReview();
        video.Approve();
        video.Publish();

        // Act
        bool result = video.Archive();

        // Assert
        result.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.Archived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.AttachYoutubeId(TestConstants.Content.Editorial.Video.ValidYoutubeVideoId);
        video.MarkPendingReview();
        video.Approve();
        video.Publish();
        video.Archive();

        // Act
        bool result = video.Archive();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Domain Method Tests

    [Fact]
    public void AttachYoutubeId_ShouldSetYoutubeVideoId()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        const string youtubeId = TestConstants.Content.Editorial.Video.ValidYoutubeVideoId;

        // Act
        video.AttachYoutubeId(youtubeId);

        // Assert
        video.YoutubeVideoId.Should().Be(youtubeId);
    }

    [Fact]
    public void UpdateThumbnail_ShouldSetThumbnailUrlAndStorageKey()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        const string url = TestConstants.Content.Editorial.Cloudinary.ValidSecureUrl;
        const string storageKey = TestConstants.Content.Editorial.Cloudinary.ValidPublicId;

        // Act
        video.UpdateThumbnail(url, storageKey);

        // Assert
        video.ThumbnailUrl.Should().Be(url);
        video.ThumbnailStorageKey.Should().Be(storageKey);
    }

    [Fact]
    public void ScheduleShoot_ShouldSetShootingScheduledAt()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        DateTimeOffset scheduledAt = DateTimeOffset.UtcNow.AddDays(14);

        // Act
        video.ScheduleShoot(scheduledAt);

        // Assert
        video.ShootingScheduledAt.Should().Be(scheduledAt);
    }

    [Fact]
    public void MarkHasLyrics_ShouldSetHasLyricsTrue()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Act
        video.MarkHasLyrics();

        // Assert
        video.HasLyrics.Should().BeTrue();
    }

    [Fact]
    public void UpdateRating_ShouldSetAverageAndCount()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Act
        video.UpdateRating(average: 4.5m, count: 10);

        // Assert
        video.RatingAverage.Should().Be(4.5m);
        video.RatingCount.Should().Be(10);
    }

    [Fact]
    public void IncrementShareCount_ShouldIncrement()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Act
        video.IncrementShareCount();

        // Assert
        video.ShareCount.Should().Be(1);
    }

    [Fact]
    public void StampFeatured_ShouldSetIsFeaturedAndFeaturedUntil()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        DateTimeOffset until = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        video.StampFeatured(until);

        // Assert
        video.IsFeatured.Should().BeTrue();
        video.FeaturedUntil.Should().Be(until);
    }

    [Fact]
    public void Update_ShouldUpdateAllFields()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        Guid newCategoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        DateTimeOffset featuredUntil = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        video.Update(
            categoryId: newCategoryId,
            title: "Updated Title",
            slug: "updated-slug",
            description: "Updated description",
            customerId: customerId,
            orderItemId: orderItemId,
            socialBoost: true,
            isFeatured: true,
            featuredUntil: featuredUntil,
            metaTitle: "Updated Meta",
            metaDescription: "Updated description"
        );

        // Assert
        video.CategoryId.Should().Be(newCategoryId);
        video.Title.Should().Be("Updated Title");
        video.Slug.Should().Be("updated-slug");
        video.Description.Should().Be("Updated description");
        video.CustomerId.Should().Be(customerId);
        video.OrderItemId.Should().Be(orderItemId);
        video.SocialBoost.Should().BeTrue();
        video.IsFeatured.Should().BeTrue();
        video.FeaturedUntil.Should().Be(featuredUntil);
        video.MetaTitle.Should().Be("Updated Meta");
        video.MetaDescription.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateSeo_ShouldSetMetaFields()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Act
        video.UpdateSeo("My SEO Title", "My SEO Description");

        // Assert
        video.MetaTitle.Should().Be("My SEO Title");
        video.MetaDescription.Should().Be("My SEO Description");
    }

    [Fact]
    public void StampSocialBoost_ShouldSetSocialBoostTrue()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Act
        video.StampSocialBoost();

        // Assert
        video.SocialBoost.Should().BeTrue();
    }

    [Fact]
    public void MarkPendingReview_ShouldTransitionToPendingReview()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );

        // Act
        bool result = video.MarkPendingReview();

        // Assert
        result.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.PendingReview);
    }

    [Fact]
    public void MarkPendingReview_WhenAlreadyPendingReview_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId
        );
        video.MarkPendingReview();

        // Act
        bool result = video.MarkPendingReview();

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
