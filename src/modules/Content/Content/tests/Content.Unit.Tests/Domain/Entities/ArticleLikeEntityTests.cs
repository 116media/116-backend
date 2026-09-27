using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
/// Unit tests for <see cref="ArticleLikeEntity"/>.
/// </summary>
public class ArticleLikeEntityTests
{
    [Fact]
    public void Create_ShouldAssignFieldsAndRaisePositiveEngagementEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var articleId = Guid.NewGuid();

        // Act
        ArticleLikeEntity row = ArticleLikeEntity.Create(id, userId, articleId);

        // Assert
        row.Id.Should().Be(id);
        row.UserId.Should().Be(userId);
        row.ArticleId.Should().Be(articleId);
        row.DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Like, 1));
    }

    [Fact]
    public void MarkRemoved_ShouldRaiseNegativeEngagementEvent()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        ArticleLikeEntity row = ArticleLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleId);
        row.ClearDomainEvents();

        // Act
        row.MarkRemoved();

        // Assert
        row.DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Like, -1));
    }
}
