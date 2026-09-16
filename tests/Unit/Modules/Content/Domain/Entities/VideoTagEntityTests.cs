using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="VideoTagEntity" />, a member of the video aggregate: every arrangement
/// goes through <see cref="VideoEntity.ReplaceTags" />, the only writer of its rows.
/// </summary>
public class VideoTagEntityTests
{
    [Fact]
    public void ReplaceTags_AddingATag_ShouldCreateTheRowAndRaiseTagGraphChanged()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        video.ClearDomainEvents();

        // Act
        bool changed = video.ReplaceTags([tagId]);

        // Assert
        changed.Should().BeTrue();
        video.Tags.Should().ContainSingle().Which.TagId.Should().Be(tagId);
        video
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
        VideoEntity video = VideoFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        video.ReplaceTags([tagId]);
        video.ClearDomainEvents();

        // Act
        bool changed = video.ReplaceTags([]);

        // Assert
        changed.Should().BeTrue();
        video.Tags.Should().BeEmpty();
        video
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
        VideoEntity video = VideoFactory.Create(Guid.NewGuid());
        var keptTagId = Guid.NewGuid();
        var removedTagId = Guid.NewGuid();
        var addedTagId = Guid.NewGuid();
        video.ReplaceTags([keptTagId, removedTagId]);
        video.ClearDomainEvents();

        // Act
        bool changed = video.ReplaceTags([keptTagId, addedTagId]);

        // Assert
        changed.Should().BeTrue();
        video.Tags.Select(tag => tag.TagId).Should().BeEquivalentTo([keptTagId, addedTagId]);
        video
            .DomainEvents.OfType<TagGraphChangedEvent>()
            .Select(domainEvent => domainEvent.TagId)
            .Should()
            .BeEquivalentTo([removedTagId, addedTagId]);
    }

    [Fact]
    public void ReplaceTags_WithAnIdenticalSet_ShouldReportNoChangeAndRaiseNothing()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(Guid.NewGuid());
        var tagId = Guid.NewGuid();
        video.ReplaceTags([tagId]);
        video.ClearDomainEvents();

        // Act
        bool changed = video.ReplaceTags([tagId]);

        // Assert
        changed.Should().BeFalse();
        video.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTags_ShouldStampTheOwningRowOnEveryTagRow()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(Guid.NewGuid());

        // Act
        video.ReplaceTags([Guid.NewGuid(), Guid.NewGuid()]);

        // Assert
        video.Tags.Should().OnlyContain(tag => tag.VideoId == video.Id);
        video.Tags.Should().OnlyContain(tag => tag.Id != Guid.Empty);
    }
}
