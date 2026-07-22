using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
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
    private const string Description = TestConstants.Content.Editorial.Video.ValidDescription;

    #region CreateFree Tests

    [Fact]
    public void CreateFree_WithValidParams_ShouldCreateDraftVideo()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string title = TestConstants.Content.Editorial.Video.ValidTitle;
        const string slug = TestConstants.Content.Editorial.Video.ValidSlug;

        // Act
        VideoEntity video = VideoEntity.CreateFree(
            id,
            CategoryId,
            title,
            slug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );

        // Assert
        video.Id.Should().Be(id);
        video.CategoryId.Should().Be(CategoryId);
        video.Title.Should().Be(title);
        video.Slug.Should().Be(slug);
        video.AuthorId.Should().Be(AuthorId);
        video.Status.Should().Be(EnumContentStatus.Draft);
        video.CustomerId.Should().BeNull();
        video.YoutubeVideoUrl.Should().BeNull();
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
                AuthorId,
                Description,
                TestErrorsFactory.CreateVideoErrors()
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
                AuthorId,
                Description,
                TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
                AuthorId,
                Description,
                TestErrorsFactory.CreateVideoErrors()
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
                AuthorId,
                Description,
                TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();

        // Act
        bool result = video.Approve();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Publish_WithYoutubeUrl_ShouldTransitionToPublished_AndSetPublishedAt()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.AttachYoutubeVideoUrl(
            TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();

        // Act
        bool result = video.Publish(TestErrorsFactory.CreateVideoErrors());

        // Assert
        result.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.Published);
        video.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Publish_WithoutYoutubeUrl_ShouldThrow()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();

        // Act
        Action act = () => video.Publish(TestErrorsFactory.CreateVideoErrors());

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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.AttachYoutubeVideoUrl(
            TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestErrorsFactory.CreateVideoErrors());

        // Act
        bool result = video.Publish(TestErrorsFactory.CreateVideoErrors());

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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.AttachYoutubeVideoUrl(
            TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestErrorsFactory.CreateVideoErrors());

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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.AttachYoutubeVideoUrl(
            TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestErrorsFactory.CreateVideoErrors());
        video.Archive();

        // Act
        bool result = video.Archive();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Domain Method Tests

    [Fact]
    public void AttachYoutubeUrl_WhenNoShootScheduled_ShouldSetYoutubeVideoUrl()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        const string youtubeId = TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl;

        // Act
        video.AttachYoutubeVideoUrl(youtubeId, TestErrorsFactory.CreateVideoErrors());

        // Assert
        video.YoutubeVideoUrl.Should().Be(youtubeId);
    }

    [Fact]
    public void AttachYoutubeUrl_WhenShootIsInThePast_ShouldSetYoutubeVideoUrl()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.ScheduleShoot(DateTimeOffset.UtcNow.AddDays(-7));
        const string youtubeId = TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl;

        // Act
        video.AttachYoutubeVideoUrl(youtubeId, TestErrorsFactory.CreateVideoErrors());

        // Assert
        video.YoutubeVideoUrl.Should().Be(youtubeId);
    }

    [Fact]
    public void AttachYoutubeUrl_WhenShootIsInTheFuture_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        DateTimeOffset futureShoot = DateTimeOffset.UtcNow.AddDays(30);
        video.ScheduleShoot(futureShoot);
        const string youtubeId = TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl;

        // Act
        Action act = () => video.AttachYoutubeVideoUrl(youtubeId, TestErrorsFactory.CreateVideoErrors());

        // Assert
        act.Should().Throw<BadRequestException>().WithMessage($"*{futureShoot:yyyy-MM-dd}*");
    }

    [Fact]
    public void SetThumbnailFileId_ShouldSetThumbnailFileId()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        Guid fileId = Guid.NewGuid();

        // Act
        video.SetThumbnailFileId(fileId);

        // Assert
        video.ThumbnailFileId.Should().Be(fileId);
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );

        // Act
        video.IncrementShareCount();

        // Assert
        video.ShareCount.Should().Be(1);
    }

    [Fact]
    public void StampPromotion_ShouldSetIsPromotedAndPromotedUntil()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        DateTimeOffset until = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        video.StampPromotion(Guid.NewGuid(), until);

        // Assert
        video.IsPromoted.Should().BeTrue();
        video.PromotedUntil.Should().Be(until);
    }

    [Fact]
    public void ForceUnpromote_WhenVideoIsPromoted_ShouldClearPromotionAndRecordAudit()
    {
        // Arrange
        const string superAdminId = "super-admin-uuid";
        const string reason = "government takedown request";

        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        video.ForceUnpromote(superAdminId, reason);

        // Assert
        video.IsPromoted.Should().BeFalse();
        video.PromotedUntil.Should().BeNull();
        video.UnpromotedBy.Should().Be(superAdminId);
        video.UnpromotedReason.Should().Be(reason);
        video.UnpromotedAt.Should().NotBeNull();
        video.UnpromotedAt!.Value.Should().BeCloseTo(before, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ForceUnpromote_WhenVideoIsNotPromoted_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );

        // Act
        Action act = () => video.ForceUnpromote("super-admin-uuid", "reason");

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void ForceUnpromote_ShouldNotAffectOtherFields()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.AttachYoutubeVideoUrl(
            TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestErrorsFactory.CreateVideoErrors());
        video.StampSocialBoost();
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        video.ForceUnpromote("super-admin-uuid", "reason");

        // Assert
        video.Status.Should().Be(EnumContentStatus.Published);
        video.SocialBoost.Should().BeTrue();
        video.Title.Should().Be(TestConstants.Content.Editorial.Video.ValidTitle);
        video.Slug.Should().Be(TestConstants.Content.Editorial.Video.ValidSlug);
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        Guid newCategoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        // Act
        video.Update(
            categoryId: newCategoryId,
            title: "Updated Title",
            slug: "updated-slug",
            description: "Updated description",
            customerId: customerId,
            orderItemId: orderItemId,
            socialBoost: true,
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
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
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();

        // Act
        bool result = video.MarkPendingReview();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkPendingReview_WhenAlreadyPublished_ShouldReturnFalseAndNotDisturbPublishedStatus()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.AttachYoutubeVideoUrl(
            TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.MarkPendingReview();
        video.Approve();
        video.Publish(TestErrorsFactory.CreateVideoErrors());

        // Act
        bool result = video.MarkPendingReview();

        // Assert
        result.Should().BeFalse();
        video.Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public void LinkArtist_ShouldSetArtistId()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        Guid artistId = Guid.NewGuid();

        // Act
        video.LinkArtist(artistId);

        // Assert
        video.ArtistId.Should().Be(artistId);
    }

    [Fact]
    public void UnlinkArtist_ShouldClearArtistId()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.LinkArtist(Guid.NewGuid());

        // Act
        video.UnlinkArtist();

        // Assert
        video.ArtistId.Should().BeNull();
    }

    [Fact]
    public void UnlinkArtist_ShouldNotAffectOtherFields()
    {
        // Arrange
        VideoEntity video = VideoEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Video.ValidTitle,
            TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId,
            Description,
            TestErrorsFactory.CreateVideoErrors()
        );
        video.LinkArtist(Guid.NewGuid());

        // Act
        video.UnlinkArtist();

        // Assert
        video.Title.Should().Be(TestConstants.Content.Editorial.Video.ValidTitle);
        video.Slug.Should().Be(TestConstants.Content.Editorial.Video.ValidSlug);
    }

    #endregion
}
