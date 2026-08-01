using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArticleShareEntity"/>.
/// </summary>
public class ArticleShareEntityTests
{
    [Fact]
    public void Create_ShouldRaisePositiveShareEngagementEvent()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        // Act
        ArticleShareEntity share = ArticleShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), articleId);

        // Assert
        share
            .DomainEvents.OfType<ArticleEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleEngagedEvent(articleId, EnumEngagementKind.Share, 1));
    }
}
