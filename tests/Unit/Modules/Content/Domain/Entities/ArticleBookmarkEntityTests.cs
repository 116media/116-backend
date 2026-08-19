using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArticleBookmarkEntity"/>.
/// </summary>
public class ArticleBookmarkEntityTests
{
    [Fact]
    public void Create_ShouldAssignFieldsAndRaisePositiveEngagementEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var articleId = Guid.NewGuid();

        // Act
        ArticleBookmarkEntity row = ArticleBookmarkEntity.Create(id, userId, articleId);

        // Assert
        row.Id.Should().Be(id);
        row.UserId.Should().Be(userId);
        row.ArticleId.Should().Be(articleId);
        row.DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Bookmark, 1));
    }

    [Fact]
    public void MarkRemoved_ShouldRaiseNegativeEngagementEvent()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        ArticleBookmarkEntity row = ArticleBookmarkEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleId);
        row.ClearDomainEvents();

        // Act
        row.MarkRemoved();

        // Assert
        row.DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Bookmark, -1));
    }
}
