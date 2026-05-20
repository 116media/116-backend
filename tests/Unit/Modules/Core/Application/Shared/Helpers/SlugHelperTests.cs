using _116.Core.Application.Shared.Helpers;
using _116.Tests.Fixtures.Factories.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Helpers;

/// <summary>
/// Unit tests for <see cref="SlugHelper"/>.
/// </summary>
public class SlugHelperTests
{
    #region ToSlug Tests

    [Fact]
    public void ToSlug_WithMultiWordName_ShouldReturnLowercaseHyphenated()
    {
        // Arrange
        string input = SlugInputFactory.Create("Fally Ipupa");

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().Be("fally-ipupa");
    }

    [Fact]
    public void ToSlug_WithNumbers_ShouldPreserveNumbers()
    {
        // Arrange
        string input = SlugInputFactory.Create("116 Le Focus");

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().Be("116-le-focus");
    }

    [Fact]
    public void ToSlug_WithDiacritics_ShouldStripAccents()
    {
        // Arrange
        string input = SlugInputFactory.Create("café & crème");

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().Be("cafe-creme");
    }

    [Fact]
    public void ToSlug_WithUppercaseDiacritics_ShouldNormalizeAndLowercase()
    {
        // Arrange
        string input = SlugInputFactory.Create("Café & Crème");

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().Be("cafe-creme");
    }

    [Fact]
    public void ToSlug_WithLeadingAndTrailingSpaces_ShouldTrimAndHyphenate()
    {
        // Arrange
        string input = SlugInputFactory.Create("  Hello   World  ");

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().Be("hello-world");
    }

    [Fact]
    public void ToSlug_WithComplexDiacritics_ShouldStripAllAccents()
    {
        // Arrange
        string input = SlugInputFactory.Create("Ñoño");

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().Be("nono");
    }

    [Fact]
    public void ToSlug_WithRandomUppercaseInput_ShouldReturnOnlyLowercase()
    {
        // Arrange
        string input = SlugInputFactory.CreateUppercase();

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().Be(result.ToLowerInvariant());
    }

    [Fact]
    public void ToSlug_WithRandomUnderscores_ShouldNotContainUnderscores()
    {
        // Arrange
        string input = SlugInputFactory.CreateWithUnderscores();

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().NotContain("_");
        result.Should().MatchRegex(@"^[a-z0-9]+(-[a-z0-9]+)*$");
    }

    [Fact]
    public void ToSlug_WithRandomMultipleSpaces_ShouldCollapseToSingleHyphens()
    {
        // Arrange
        string input = SlugInputFactory.CreateWithMultipleSpaces();

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().NotContain("--");
        result.Should().MatchRegex(@"^[a-z0-9]+(-[a-z0-9]+)*$");
    }

    [Fact]
    public void ToSlug_WithRandomSpecialCharacters_ShouldRemoveAllSpecialChars()
    {
        // Arrange
        string input = SlugInputFactory.CreateWithSpecialCharacters();

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().MatchRegex(@"^[a-z0-9-]+$");
    }

    [Fact]
    public void ToSlug_WithRandomInput_ShouldNeverHaveLeadingOrTrailingHyphens()
    {
        // Arrange
        string input = SlugInputFactory.Create();

        // Act
        string result = SlugHelper.ToSlug(input);

        // Assert
        result.Should().NotStartWith("-");
        result.Should().NotEndWith("-");
    }

    #endregion

    #region ToUniqueSlug Tests

    [Fact]
    public void ToUniqueSlug_ShouldStartWithBaseSlug()
    {
        // Arrange
        string input = SlugInputFactory.Create("Fally Ipupa");

        // Act
        string result = SlugHelper.ToUniqueSlug(input);

        // Assert
        result.Should().StartWith("fally-ipupa-");
    }

    [Fact]
    public void ToUniqueSlug_ShouldAppendEightCharacterAlphanumericSuffix()
    {
        // Arrange
        string input = SlugInputFactory.Create("Fally Ipupa");
        string expectedBase = "fally-ipupa-";

        // Act
        string result = SlugHelper.ToUniqueSlug(input);
        string suffix = result[expectedBase.Length..];

        // Assert
        suffix.Should().HaveLength(8);
        suffix.Should().MatchRegex("^[a-z0-9]{8}$");
    }

    [Fact]
    public void ToUniqueSlug_WithDiacritics_ShouldStripAccentsInBaseSlug()
    {
        // Arrange
        string input = SlugInputFactory.Create("Café & Crème");

        // Act
        string result = SlugHelper.ToUniqueSlug(input);

        // Assert
        result.Should().StartWith("cafe-creme-");
    }

    [Fact]
    public void ToUniqueSlug_CalledTwice_ShouldProduceDifferentSlugs()
    {
        // Arrange
        string input = SlugInputFactory.Create("Fally Ipupa");

        // Act
        string first = SlugHelper.ToUniqueSlug(input);
        string second = SlugHelper.ToUniqueSlug(input);

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void ToUniqueSlug_WithRandomInput_ShouldOnlyContainLowercaseAlphanumericAndHyphens()
    {
        // Arrange
        string input = SlugInputFactory.Create();

        // Act
        string result = SlugHelper.ToUniqueSlug(input);

        // Assert
        result.Should().MatchRegex(@"^[a-z0-9-]+$");
    }

    [Fact]
    public void ToUniqueSlug_WithRandomInput_ShouldAlwaysHaveSuffix()
    {
        // Arrange
        string input = SlugInputFactory.Create();

        // Act
        string baseSlug = SlugHelper.ToSlug(input);
        string uniqueSlug = SlugHelper.ToUniqueSlug(input);

        // Assert
        uniqueSlug.Length.Should().BeGreaterThan(baseSlug.Length);
        uniqueSlug.Should().StartWith(baseSlug + "-");
    }

    #endregion
}
