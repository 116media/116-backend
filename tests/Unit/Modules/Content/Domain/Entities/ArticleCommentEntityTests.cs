using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArticleCommentEntity"/>.
/// </summary>
public class ArticleCommentEntityTests
{
    [Fact]
    public void Create_ShouldRaisePositiveCommentEngagementEvent()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        // Act
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            articleId,
            "a comment body"
        );

        // Assert
        comment
            .DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Comment, 1));
    }

    [Fact]
    public void CreateReply_ShouldRaisePositiveCommentEngagementEvent()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        // Act
        ArticleCommentEntity reply = ArticleCommentEntity.CreateReply(
            Guid.NewGuid(),
            Guid.NewGuid(),
            articleId,
            Guid.NewGuid(),
            "a reply body"
        );

        // Assert
        reply
            .DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Comment, 1));
    }

    [Fact]
    public void CreateReply_ShouldRaiseCommentReplyAddedEvent()
    {
        // Arrange
        var replyId = Guid.NewGuid();
        var replierId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();

        // Act
        ArticleCommentEntity reply = ArticleCommentEntity.CreateReply(
            replyId,
            replierId,
            articleId,
            parentCommentId,
            "a reply body"
        );

        // Assert
        reply
            .DomainEvents.OfType<CommentReplyAddedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new CommentReplyAddedEvent(replyId, parentCommentId, articleId, replierId));
    }

    [Fact]
    public void Create_ShouldNotRaiseCommentReplyAddedEvent()
    {
        // Act
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a comment body"
        );

        // Assert
        comment.DomainEvents.OfType<CommentReplyAddedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void SoftDelete_ShouldFlagDeletedAndRaiseNegativeCommentEngagementEvent()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            articleId,
            "a comment body"
        );
        comment.ClearDomainEvents();

        // Act
        bool deleted = comment.SoftDelete();

        // Assert
        deleted.Should().BeTrue();
        comment.IsDeleted.Should().BeTrue();
        comment.DeletedAt.Should().NotBeNull();
        comment
            .DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Comment, -1));
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldReturnFalseAndRaiseNothing()
    {
        // Arrange — the owner deleted the comment, then an admin moderates the
        // same row: a second decrement would drift the article's counter.
        ArticleCommentEntity comment = ArticleCommentEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a comment body"
        );
        comment.SoftDelete();
        DateTimeOffset? firstDeletedAt = comment.DeletedAt;
        comment.ClearDomainEvents();

        // Act
        bool deleted = comment.SoftDelete();

        // Assert
        deleted.Should().BeFalse();
        comment.IsDeleted.Should().BeTrue();
        comment.DeletedAt.Should().Be(firstDeletedAt);
        comment.DomainEvents.Should().BeEmpty();
    }
}
