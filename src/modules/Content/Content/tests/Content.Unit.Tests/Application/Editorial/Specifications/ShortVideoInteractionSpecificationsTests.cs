using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
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
        ShortVideoLikeEntity like = ShortVideoLikeFactory.Create(userId, shortVideoId);
        var spec = new ShortVideoLikeByUserAndShortVideoSpecification(userId, shortVideoId);

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoLikeByUserAndShortVideoSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        ShortVideoLikeEntity like = ShortVideoLikeFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var spec = new ShortVideoLikeByUserAndShortVideoSpecification(Guid.NewGuid(), like.ShortVideoId);

        bool result = spec.IsSatisfiedBy(like);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShortVideoLikeByUserAndShortVideoSpecification_WithDifferentShortVideoId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        ShortVideoLikeEntity like = ShortVideoLikeFactory.Create(userId, Guid.NewGuid());
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
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkFactory.Create(userId, shortVideoId);
        var spec = new ShortVideoBookmarkByUserAndShortVideoSpecification(userId, shortVideoId);

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoBookmarkByUserAndShortVideoSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var spec = new ShortVideoBookmarkByUserAndShortVideoSpecification(Guid.NewGuid(), bookmark.ShortVideoId);

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShortVideoBookmarkByUserAndShortVideoSpecification_WithDifferentShortVideoId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkFactory.Create(userId, Guid.NewGuid());
        var spec = new ShortVideoBookmarkByUserAndShortVideoSpecification(userId, Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(bookmark);

        result.Should().BeFalse();
    }

    #endregion
}
