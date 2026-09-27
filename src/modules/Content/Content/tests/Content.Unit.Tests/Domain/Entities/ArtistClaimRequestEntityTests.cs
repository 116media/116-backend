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
