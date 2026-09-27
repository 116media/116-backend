using _116.Content.Application.Editorial.Specifications;
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

namespace _116.Content.Unit.Tests.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for the short video interaction specification classes.
/// </summary>
public class ShortVideoInteractionSpecificationsTests
{
    #region ShortVideoLikeByUserAndShortVideoSpecification

    [Fact]
    public void ShortVideoLikeByUserAndShortVideoSpecification_WithMatchingUserAndShortVideo_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        Guid shortVideoId = Guid.NewGuid();
        ShortVideoLikeEntity like = ShortVideoLikeEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            shortVideoId: shortVideoId
        );
        var spec = new ShortVideoLikeByUserAndShortVideoSpecification(userId, shortVideoId);

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoLikeByUserAndShortVideoSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        ShortVideoLikeEntity like = ShortVideoLikeEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            shortVideoId: Guid.NewGuid()
        );
        var spec = new ShortVideoLikeByUserAndShortVideoSpecification(Guid.NewGuid(), like.ShortVideoId);

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShortVideoLikeByUserAndShortVideoSpecification_WithDifferentShortVideoId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        ShortVideoLikeEntity like = ShortVideoLikeEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            shortVideoId: Guid.NewGuid()
        );
        var spec = new ShortVideoLikeByUserAndShortVideoSpecification(userId, Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeFalse();
    }

    #endregion

    #region ShortVideoBookmarkByUserAndShortVideoSpecification

    [Fact]
    public void ShortVideoBookmarkByUserAndShortVideoSpecification_WithMatchingUserAndShortVideo_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        Guid shortVideoId = Guid.NewGuid();
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            shortVideoId: shortVideoId
        );
        var spec = new ShortVideoBookmarkByUserAndShortVideoSpecification(userId, shortVideoId);

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoBookmarkByUserAndShortVideoSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            shortVideoId: Guid.NewGuid()
        );
        var spec = new ShortVideoBookmarkByUserAndShortVideoSpecification(Guid.NewGuid(), bookmark.ShortVideoId);

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShortVideoBookmarkByUserAndShortVideoSpecification_WithDifferentShortVideoId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            shortVideoId: Guid.NewGuid()
        );
        var spec = new ShortVideoBookmarkByUserAndShortVideoSpecification(userId, Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeFalse();
    }

    #endregion
}
