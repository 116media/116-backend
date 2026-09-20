using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsTagEntity" />, a member of the lyrics aggregate: every arrangement
/// goes through <see cref="LyricsEntity.ReplaceTags" />, the only writer of its rows.
/// </summary>
public class LyricsTagEntityTests
{
    [Fact]
    public void ReplaceTags_AddingATag_ShouldCreateTheRowAndRaiseTagGraphChanged()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        lyrics.ClearDomainEvents();

        // Act
        bool changed = lyrics.ReplaceTags([tagId]);

        // Assert
        changed.Should().BeTrue();
        lyrics.Tags.Should().ContainSingle().Which.TagId.Should().Be(tagId);
        lyrics
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
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        lyrics.ReplaceTags([tagId]);
        lyrics.ClearDomainEvents();

        // Act
        bool changed = lyrics.ReplaceTags([]);

        // Assert
        changed.Should().BeTrue();
        lyrics.Tags.Should().BeEmpty();
        lyrics
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
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        var keptTagId = Guid.NewGuid();
        var removedTagId = Guid.NewGuid();
        var addedTagId = Guid.NewGuid();
        lyrics.ReplaceTags([keptTagId, removedTagId]);
        lyrics.ClearDomainEvents();

        // Act
        bool changed = lyrics.ReplaceTags([keptTagId, addedTagId]);

        // Assert
        changed.Should().BeTrue();
        lyrics.Tags.Select(tag => tag.TagId).Should().BeEquivalentTo([keptTagId, addedTagId]);
        lyrics
            .DomainEvents.OfType<TagGraphChangedEvent>()
            .Select(domainEvent => domainEvent.TagId)
            .Should()
            .BeEquivalentTo([removedTagId, addedTagId]);
    }

    [Fact]
    public void ReplaceTags_WithAnIdenticalSet_ShouldReportNoChangeAndRaiseNothing()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        lyrics.ReplaceTags([tagId]);
        lyrics.ClearDomainEvents();

        // Act
        bool changed = lyrics.ReplaceTags([tagId]);

        // Assert
        changed.Should().BeFalse();
        lyrics.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTags_ShouldStampTheOwningRowOnEveryTagRow()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());

        // Act
        lyrics.ReplaceTags([Guid.NewGuid(), Guid.NewGuid()]);

        // Assert
        lyrics.Tags.Should().OnlyContain(tag => tag.LyricsId == lyrics.Id);
        lyrics.Tags.Should().OnlyContain(tag => tag.Id != Guid.Empty);
    }
}
