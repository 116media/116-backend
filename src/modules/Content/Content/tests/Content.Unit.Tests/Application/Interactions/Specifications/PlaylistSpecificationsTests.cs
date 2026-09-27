using _116.Content.Application.Interactions.Specifications;
using _116.Content.Domain.Entities;
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

namespace _116.Content.Unit.Tests.Application.Interactions.Specifications;

/// <summary>
/// Unit tests for playlist specification classes.
/// </summary>
public class PlaylistSpecificationsTests
{
    #region PlaylistByIdSpecification

    [Fact]
    public void PlaylistByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        Guid id = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistEntity.Create(id, userId: Guid.NewGuid(), name: "My Playlist");
        var spec = new PlaylistByIdSpecification(id);

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeTrue();
    }

    [Fact]
    public void PlaylistByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        PlaylistEntity playlist = PlaylistEntity.Create(Guid.NewGuid(), userId: Guid.NewGuid(), name: "My Playlist");
        var spec = new PlaylistByIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeFalse();
    }

    #endregion

    #region PlaylistByUserIdSpecification

    [Fact]
    public void PlaylistByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistEntity.Create(Guid.NewGuid(), userId: userId, name: "My Playlist");
        var spec = new PlaylistByUserIdSpecification(userId);

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeTrue();
    }

    [Fact]
    public void PlaylistByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        PlaylistEntity playlist = PlaylistEntity.Create(Guid.NewGuid(), userId: Guid.NewGuid(), name: "My Playlist");
        var spec = new PlaylistByUserIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(playlist);

        result.Should().BeFalse();
    }

    #endregion
}
