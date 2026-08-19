using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArtistClaimRequestEntity"/>.
/// </summary>
public class ArtistClaimRequestEntityTests
{
    [Fact]
    public void Create_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        ArtistClaimRequestEntity request = ArtistClaimRequestEntity.Create(id, artistId, userId);

        // Assert
        request.Id.Should().Be(id);
        request.ArtistId.Should().Be(artistId);
        request.UserId.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldRaiseClaimRequestedEvent()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        ArtistClaimRequestEntity request = ArtistClaimRequestEntity.Create(Guid.NewGuid(), artistId, userId);

        // Assert
        request
            .DomainEvents.OfType<ArtistClaimRequestedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArtistClaimRequestedEvent(artistId, userId));
    }
}
