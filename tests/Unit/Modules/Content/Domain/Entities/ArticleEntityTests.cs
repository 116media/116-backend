using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArticleEntity"/>.
/// </summary>
public class ArticleEntityTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    #region CreateFree Tests

    [Fact]
    public void CreateFree_WithValidParams_ShouldCreateDraftArticle()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string title = TestConstants.Content.Editorial.Article.ValidTitle;
        const string slug = TestConstants.Content.Editorial.Article.ValidSlug;

        // Act
        ArticleEntity article = ArticleEntity.CreateFree(
            id,
            CategoryId,
            title,
            slug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Assert
        article.Id.Should().Be(id);
        article.CategoryId.Should().Be(CategoryId);
        article.Title.Should().Be(title);
        article.Slug.Should().Be(slug);
        article.AuthorId.Should().Be(AuthorId);
        article.Status.Should().Be(EnumContentStatus.Draft);
        article.CustomerId.Should().BeNull();
        article.OrderItemId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFree_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
    {
        // Act
        Action act = () =>
            ArticleEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                invalidTitle!,
                TestConstants.Content.Editorial.Article.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateArticleErrors()
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
            ArticleEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                TestConstants.Content.Editorial.Article.ValidTitle,
                invalidSlug!,
                AuthorId,
                TestErrorsFactory.CreateArticleErrors()
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
        ArticleEntity article = ArticleEntity.CreatePaid(
            Guid.NewGuid(),
            customerId,
            orderItemId,
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Assert
        article.CustomerId.Should().Be(customerId);
        article.OrderItemId.Should().Be(orderItemId);
        article.Status.Should().Be(EnumContentStatus.Draft);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreatePaid_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
    {
        // Act
        Action act = () =>
            ArticleEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                invalidTitle!,
                TestConstants.Content.Editorial.Article.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateArticleErrors()
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
            ArticleEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                TestConstants.Content.Editorial.Article.ValidTitle,
                invalidSlug!,
                AuthorId,
                TestErrorsFactory.CreateArticleErrors()
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
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        bool result = article.Submit();

        // Assert
        result.Should().BeTrue();
        article.Status.Should().Be(EnumContentStatus.PendingPayment);
    }

    [Fact]
    public void Submit_WhenAlreadyPendingPayment_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.Submit();

        // Act
        bool result = article.Submit();

        // Assert
        result.Should().BeFalse();
        article.Status.Should().Be(EnumContentStatus.PendingPayment);
    }

    [Fact]
    public void MarkPendingReview_ShouldTransitionToPendingReview()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        bool result = article.MarkPendingReview();

        // Assert
        result.Should().BeTrue();
        article.Status.Should().Be(EnumContentStatus.PendingReview);
    }

    [Fact]
    public void MarkPendingReview_WhenAlreadyPendingReview_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();

        // Act
        bool result = article.MarkPendingReview();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkPendingReview_WhenAlreadyPublished_ShouldReturnFalseAndNotDisturbPublishedStatus()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();
        article.Approve();
        article.Publish();

        // Act
        bool result = article.MarkPendingReview();

        // Assert
        result.Should().BeFalse();
        article.Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public void Approve_ShouldTransitionToApproved()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();

        // Act
        bool result = article.Approve();

        // Assert
        result.Should().BeTrue();
        article.Status.Should().Be(EnumContentStatus.Approved);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();
        article.Approve();

        // Act
        bool result = article.Approve();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Publish_ShouldTransitionToPublished_AndSetPublishedAt()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();
        article.Approve();

        // Act
        bool result = article.Publish();

        // Assert
        result.Should().BeTrue();
        article.Status.Should().Be(EnumContentStatus.Published);
        article.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();
        article.Approve();
        article.Publish();

        // Act
        bool result = article.Publish();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Reject_ShouldSetRejectionReason_AndTransitionToRejected()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        const string reason = TestConstants.Content.Editorial.Article.ValidRejectionReason;

        // Act
        bool result = article.Reject(reason);

        // Assert
        result.Should().BeTrue();
        article.Status.Should().Be(EnumContentStatus.Rejected);
        article.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.Reject(TestConstants.Content.Editorial.Article.ValidRejectionReason);

        // Act
        bool result = article.Reject(TestConstants.Content.Editorial.Article.ValidRejectionReason);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Archive_ShouldTransitionToArchived()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();
        article.Approve();
        article.Publish();

        // Act
        bool result = article.Archive();

        // Assert
        result.Should().BeTrue();
        article.Status.Should().Be(EnumContentStatus.Archived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFalse()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();
        article.Approve();
        article.Publish();
        article.Archive();

        // Act
        bool result = article.Archive();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Stamp Tests

    [Fact]
    public void StampSocialBoost_ShouldSetSocialBoostTrue()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.StampSocialBoost();

        // Assert
        article.SocialBoost.Should().BeTrue();
    }

    [Fact]
    public void StampPromotion_ShouldSetIsPromotedAndPromotedUntil()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        DateTimeOffset until = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        article.StampPromotion(Guid.NewGuid(), until);

        // Assert
        article.IsPromoted.Should().BeTrue();
        article.PromotedUntil.Should().Be(until);
    }

    #endregion

    #region ForceUnpromote Tests

    [Fact]
    public void ForceUnpromote_WhenArticleIsPromoted_ShouldClearPromotionAndRecordAudit()
    {
        // Arrange
        const string superAdminId = "super-admin-uuid";
        const string reason = "government takedown request";

        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        article.ForceUnpromote(superAdminId, reason);

        // Assert
        article.IsPromoted.Should().BeFalse();
        article.PromotedUntil.Should().BeNull();
        article.UnpromotedBy.Should().Be(superAdminId);
        article.UnpromotedReason.Should().Be(reason);
        article.UnpromotedAt.Should().NotBeNull();
        article.UnpromotedAt!.Value.Should().BeCloseTo(before, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ForceUnpromote_WhenArticleIsNotPromoted_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        Action act = () => article.ForceUnpromote("super-admin-uuid", "reason");

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void ForceUnpromote_ShouldNotAffectOtherFields()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.MarkPendingReview();
        article.Approve();
        article.Publish();
        article.StampSocialBoost();
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));

        // Act
        article.ForceUnpromote("super-admin-uuid", "reason");

        // Assert
        article.Status.Should().Be(EnumContentStatus.Published);
        article.SocialBoost.Should().BeTrue();
        article.Title.Should().Be(TestConstants.Content.Editorial.Article.ValidTitle);
        article.Slug.Should().Be(TestConstants.Content.Editorial.Article.ValidSlug);
    }

    #endregion

    #region Counter Tests

    [Fact]
    public void IncrementLikeCount_ShouldIncrement()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.IncrementLikeCount();

        // Assert
        article.LikeCount.Should().Be(1);
    }

    [Fact]
    public void DecrementLikeCount_WhenAboveZero_ShouldDecrement()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        article.IncrementLikeCount();

        // Act
        article.DecrementLikeCount();

        // Assert
        article.LikeCount.Should().Be(0);
    }

    [Fact]
    public void DecrementLikeCount_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.DecrementLikeCount();

        // Assert
        article.LikeCount.Should().Be(0);
    }

    [Fact]
    public void IncrementCommentCount_ShouldIncrement()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.IncrementCommentCount();

        // Assert
        article.CommentCount.Should().Be(1);
    }

    [Fact]
    public void DecrementCommentCount_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.DecrementCommentCount();

        // Assert
        article.CommentCount.Should().Be(0);
    }

    [Fact]
    public void IncrementShareCount_ShouldIncrement()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.IncrementShareCount();

        // Assert
        article.ShareCount.Should().Be(1);
    }

    [Fact]
    public void IncrementBookmarkCount_ShouldIncrement()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.IncrementBookmarkCount();

        // Assert
        article.BookmarkCount.Should().Be(1);
    }

    [Fact]
    public void DecrementBookmarkCount_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.DecrementBookmarkCount();

        // Assert
        article.BookmarkCount.Should().Be(0);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void UpdateSeo_ShouldSetMetaFields()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );

        // Act
        article.UpdateSeo("My SEO Title", "My SEO Description");

        // Assert
        article.MetaTitle.Should().Be("My SEO Title");
        article.MetaDescription.Should().Be("My SEO Description");
    }

    [Fact]
    public void UpdateCoverImage_ShouldSetCoverImageFileId()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        Guid coverImageFileId = Guid.NewGuid();

        // Act
        article.UpdateCoverImage(coverImageFileId: coverImageFileId);

        // Assert
        article.CoverImageFileId.Should().Be(coverImageFileId);
    }

    [Fact]
    public void Update_ShouldUpdateAllFields()
    {
        // Arrange
        ArticleEntity article = ArticleEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            TestConstants.Content.Editorial.Article.ValidTitle,
            TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateArticleErrors()
        );
        Guid newCategoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        // Act
        article.Update(
            categoryId: newCategoryId,
            title: "Updated Title",
            slug: "updated-slug",
            headline: "Updated headline for the article",
            body: "<p>Updated body</p>",
            customerId: customerId,
            orderItemId: orderItemId,
            socialBoost: true,
            metaTitle: "Updated Meta",
            metaDescription: "Updated description"
        );

        // Assert
        article.CategoryId.Should().Be(newCategoryId);
        article.Title.Should().Be("Updated Title");
        article.Slug.Should().Be("updated-slug");
        article.Headline.Should().Be("Updated headline for the article");
        article.Body.Should().Be("<p>Updated body</p>");
        article.CustomerId.Should().Be(customerId);
        article.OrderItemId.Should().Be(orderItemId);
        article.SocialBoost.Should().BeTrue();
        article.MetaTitle.Should().Be("Updated Meta");
        article.MetaDescription.Should().Be("Updated description");
    }

    #endregion
}
