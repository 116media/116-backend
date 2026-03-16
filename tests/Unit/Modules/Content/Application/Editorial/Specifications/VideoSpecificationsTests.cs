using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for video specification classes.
/// Note: Specifications using EF.Functions.ILike require a real PostgreSQL provider —
/// those are covered via ToExpression().Compile() only.
/// </summary>
public class VideoSpecificationsTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    #region VideoByIdSpecification

    [Fact]
    public void VideoByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByIdSpecification(video.Id);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoBySlugSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void VideoBySlugSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new VideoBySlugSpecification("116-le-focus-fally-ipupa");

        // Act
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region VideoByStatusSpecification

    [Fact]
    public void VideoByStatusSpecification_WithMatchingStatus_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByStatusSpecification(EnumContentStatus.Draft);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByStatusSpecification_WithDifferentStatus_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByStatusSpecification(EnumContentStatus.Published);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoByCategorySpecification

    [Fact]
    public void VideoByCategorySpecification_WithMatchingCategoryId_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByCategorySpecification(CategoryId);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByCategorySpecification_WithDifferentCategoryId_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByCategorySpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoSearchSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void VideoSearchSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new VideoSearchSpecification("fally");

        // Act
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region FeaturedVideoSpecification

    [Fact]
    public void FeaturedVideoSpecification_WithFeaturedPublishedVideo_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateFeatured(CategoryId);
        var spec = new FeaturedVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FeaturedVideoSpecification_WithNonFeaturedPublishedVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        var spec = new FeaturedVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FeaturedVideoSpecification_WithFeaturedDraftVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.StampFeatured(DateTimeOffset.UtcNow.AddDays(7));
        var spec = new FeaturedVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
