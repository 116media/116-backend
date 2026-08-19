using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArticleCommentLikeEntity"/>.
/// </summary>
public class ArticleCommentLikeEntityTests
{
    [Fact]
    public void Create_ShouldRaisePositiveCommentEngagedEvent()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        // Act
        ArticleCommentLikeEntity like = ArticleCommentLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), commentId);

        // Assert
        like.DomainEvents.OfType<CommentEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new CommentEngagedEvent(commentId, 1));
    }

    [Fact]
    public void MarkRemoved_ShouldRaiseNegativeCommentEngagedEvent()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        ArticleCommentLikeEntity like = ArticleCommentLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), commentId);
        like.ClearDomainEvents();

        // Act
        like.MarkRemoved();

        // Assert
        like.DomainEvents.OfType<CommentEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new CommentEngagedEvent(commentId, -1));
    }
}
