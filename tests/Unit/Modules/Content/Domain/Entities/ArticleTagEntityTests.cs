using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArticleTagEntity" />, a member of the article aggregate: every arrangement
/// goes through <see cref="ArticleEntity.ReplaceTags" />, the only writer of its rows.
/// </summary>
public class ArticleTagEntityTests
{
    [Fact]
    public void ReplaceTags_AddingATag_ShouldCreateTheRowAndRaiseTagGraphChanged()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        article.ClearDomainEvents();

        // Act
        bool changed = article.ReplaceTags([tagId]);

        // Assert
        changed.Should().BeTrue();
        article.Tags.Should().ContainSingle().Which.TagId.Should().Be(tagId);
        article
            .DomainEvents.OfType<TagGraphChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new TagGraphChangedEvent(tagId));
    }

    [Fact]
    public void ReplaceTags_RemovingATag_ShouldDropTheRowAndRaiseTagGraphChanged()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        article.ReplaceTags([tagId]);
        article.ClearDomainEvents();

        // Act
        bool changed = article.ReplaceTags([]);

        // Assert
        changed.Should().BeTrue();
        article.Tags.Should().BeEmpty();
        article
            .DomainEvents.OfType<TagGraphChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new TagGraphChangedEvent(tagId));
    }

    [Fact]
    public void ReplaceTags_SwappingATag_ShouldRaiseOneEventPerChangedTag()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        var keptTagId = Guid.NewGuid();
        var removedTagId = Guid.NewGuid();
        var addedTagId = Guid.NewGuid();
        article.ReplaceTags([keptTagId, removedTagId]);
        article.ClearDomainEvents();

        // Act
        bool changed = article.ReplaceTags([keptTagId, addedTagId]);

        // Assert
        changed.Should().BeTrue();
        article.Tags.Select(tag => tag.TagId).Should().BeEquivalentTo([keptTagId, addedTagId]);
        article
            .DomainEvents.OfType<TagGraphChangedEvent>()
            .Select(domainEvent => domainEvent.TagId)
            .Should()
            .BeEquivalentTo([removedTagId, addedTagId]);
    }

    [Fact]
    public void ReplaceTags_WithAnIdenticalSet_ShouldReportNoChangeAndRaiseNothing()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        article.ReplaceTags([tagId]);
        article.ClearDomainEvents();

        // Act
        bool changed = article.ReplaceTags([tagId]);

        // Assert
        changed.Should().BeFalse();
        article.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTags_ShouldStampTheOwningRowOnEveryTagRow()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());

        // Act
        article.ReplaceTags([Guid.NewGuid(), Guid.NewGuid()]);

        // Assert
        article.Tags.Should().OnlyContain(tag => tag.ArticleId == article.Id);
        article.Tags.Should().OnlyContain(tag => tag.Id != Guid.Empty);
    }
}
