using _116.Content.Application.Catalog.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.Specifications;

/// <summary>
/// Unit tests for the category specification classes not already covered by
/// <see cref="CatalogSpecificationsTests"/> and <see cref="CategorySpecificationTests"/>.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
/// </summary>
public class CategorySpecificationsTests
{
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    #region CategoryBySlugSpecification

    [Theory]
    [InlineData("music-videos", true)]
    [InlineData("MUSIC-VIDEOS", true)]
    [InlineData("music", false)]
    [InlineData("artist-profile", false)]
    public void CategoryBySlugSpecification_ShouldMatchWholeSlugCaseInsensitively(string slug, bool expected)
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId, "Music Videos", "music-videos");
        var spec = new CategoryBySlugSpecification(slug);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(category);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region DefaultLyricsCategorySpecification

    [Fact]
    public void DefaultLyricsCategorySpecification_WithActiveDefaultLyricsCategory_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(ContentTypeId);
        var spec = new DefaultLyricsCategorySpecification();

        // Act
        bool result = spec.IsSatisfiedBy(category);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DefaultLyricsCategorySpecification_WithNonDefaultCategory_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var spec = new DefaultLyricsCategorySpecification();

        // Act
        bool result = spec.IsSatisfiedBy(category);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DefaultLyricsCategorySpecification_WithInactiveDefaultLyricsCategory_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(ContentTypeId);
        category.Deactivate();
        var spec = new DefaultLyricsCategorySpecification();

        // Act
        bool result = spec.IsSatisfiedBy(category);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
