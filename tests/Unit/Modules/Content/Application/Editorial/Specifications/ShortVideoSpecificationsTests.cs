using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for short video specification classes.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
/// </summary>
public class ShortVideoSpecificationsTests
{
    #region ShortVideoByIdSpecification

    [Fact]
    public void ShortVideoByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ShortVideoByIdSpecification(shortVideo.Id);

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ShortVideoByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ShortVideoSearchSpecification

    [Theory]
    [InlineData("fally", true)]
    [InlineData("FALLY IPUPA", true)]
    [InlineData("teaser", true)]
    [InlineData("koffi", false)]
    public void ShortVideoSearchSpecification_ShouldMatchTitleSubstringCaseInsensitively(string search, bool expected)
    {
        // Arrange
        ShortVideoEntity shortVideo = new ShortVideoBuilder().WithTitle("Teaser Fally Ipupa Focus").Build();
        var spec = new ShortVideoSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(shortVideo);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ShortVideoBySlugSpecification

    [Fact]
    public void ShortVideoBySlugSpecification_WithMatchingSlug_ShouldReturnTrue()
    {
        // Arrange
        const string slug = "teaser-fally-focus";
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithSlug(slug);
        var spec = new ShortVideoBySlugSpecification(slug);

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShortVideoBySlugSpecification_WithDifferentSlug_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ShortVideoBySlugSpecification("different-slug");

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActiveShortVideoSpecification

    [Fact]
    public void ActiveShortVideoSpecification_WithActiveShortVideo_ShouldReturnTrue()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var spec = new ActiveShortVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActiveShortVideoSpecification_WithInactiveShortVideo_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateInactive();
        var spec = new ActiveShortVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(shortVideo);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
