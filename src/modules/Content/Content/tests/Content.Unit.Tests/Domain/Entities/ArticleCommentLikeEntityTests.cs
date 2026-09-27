using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Domain.Entities;

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
