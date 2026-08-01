using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="VideoTagEntity"/>.
/// </summary>
public class VideoTagEntityTests
{
    [Fact]
    public void Create_ShouldRaiseTagGraphChangedEvent()
    {
        // Arrange
        var tagId = Guid.NewGuid();

        // Act
        VideoTagEntity association = VideoTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), tagId);

        // Assert
        association
            .DomainEvents.OfType<TagGraphChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new TagGraphChangedEvent(tagId));
    }

    [Fact]
    public void MarkRemoved_ShouldRaiseTagGraphChangedEvent()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        VideoTagEntity association = VideoTagEntity.Create(Guid.NewGuid(), Guid.NewGuid(), tagId);
        association.ClearDomainEvents();

        // Act
        association.MarkRemoved();

        // Assert
        association
            .DomainEvents.OfType<TagGraphChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new TagGraphChangedEvent(tagId));
    }
}
