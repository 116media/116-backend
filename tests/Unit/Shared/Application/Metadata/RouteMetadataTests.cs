using System.Reflection;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Metadata;

/// <summary>
/// Unit tests for <see cref="RouteMetadata"/>.
/// </summary>
public class RouteMetadataTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange
        const string name = "TestRoute";
        const string summary = "Test summary";
        const string description = "Test description with more details";

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        metadata.Name.Should().Be(name);
        metadata.Summary.Should().Be(summary);
        metadata.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_WithEmptyStrings_ShouldAllowEmptyValues()
    {
        // Arrange
        const string name = "";
        const string summary = "";
        const string description = "";

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        metadata.Name.Should().Be(string.Empty);
        metadata.Summary.Should().Be(string.Empty);
        metadata.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_WithLongDescription_ShouldHandleLongStrings()
    {
        // Arrange
        const string name = "ComplexRoute";
        const string summary = "A complex route with extensive functionality";
        string description = new('x', 5000); // Very long description

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        metadata.Name.Should().Be(name);
        metadata.Summary.Should().Be(summary);
        metadata.Description.Should().HaveLength(5000);
    }

    [Fact]
    public void Constructor_WithMultilineDescription_ShouldPreserveFormatting()
    {
        // Arrange
        const string name = "MultilineRoute";
        const string summary = "Route with multiline description";
        const string description = """
            Line 1
            Line 2
            Line 3
            """;

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        metadata.Name.Should().Be(name);
        metadata.Summary.Should().Be(summary);
        metadata.Description.Should().Contain("Line 1");
        metadata.Description.Should().Contain("Line 2");
        metadata.Description.Should().Contain("Line 3");
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var metadata = new RouteMetadata("Test", "Summary", "Description");

        // Act & Assert
        PropertyInfo? nameProperty = typeof(RouteMetadata).GetProperty(nameof(RouteMetadata.Name));
        PropertyInfo? summaryProperty = typeof(RouteMetadata).GetProperty(nameof(RouteMetadata.Summary));
        PropertyInfo? descriptionProperty = typeof(RouteMetadata).GetProperty(nameof(RouteMetadata.Description));

        nameProperty.Should().NotBeNull();
        summaryProperty.Should().NotBeNull();
        descriptionProperty.Should().NotBeNull();

        nameProperty.CanRead.Should().BeTrue();
        (nameProperty.CanWrite || nameProperty.SetMethod?.IsPublic == true).Should().BeFalse();

        summaryProperty.CanRead.Should().BeTrue();
        (summaryProperty.CanWrite || summaryProperty.SetMethod?.IsPublic == true).Should().BeFalse();

        descriptionProperty.CanRead.Should().BeTrue();
        (descriptionProperty.CanWrite || descriptionProperty.SetMethod?.IsPublic == true).Should().BeFalse();
    }

    [Fact]
    public void RouteMetadata_ShouldBeValueType()
    {
        // Arrange & Act
        Type type = typeof(RouteMetadata);

        // Assert
        type.IsValueType.Should().BeTrue();
    }

    [Fact]
    public void RouteMetadata_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var metadata1 = new RouteMetadata("Route", "Summary", "Description");
        var metadata2 = new RouteMetadata("Route", "Summary", "Description");

        // Act & Assert
        metadata1.Should().Be(metadata2);
    }

    [Fact]
    public void RouteMetadata_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var metadata1 = new RouteMetadata("Route1", "Summary", "Description");
        var metadata2 = new RouteMetadata("Route2", "Summary", "Description");

        // Act & Assert
        metadata1.Should().NotBe(metadata2);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        const string name = "Route-With_Special.Characters!@#";
        const string summary = "Summary with émojis 🚀 and symbols";
        const string description = "Description with\ttabs\nand\r\nnewlines";

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        metadata.Name.Should().Be(name);
        metadata.Summary.Should().Be(summary);
        metadata.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_WithUnicodeCharacters_ShouldPreserveUnicode()
    {
        // Arrange
        const string name = "路由名称"; // Chinese characters
        const string summary = "Résumé de l'itinéraire"; // French with accents
        const string description = "説明 😀 🎉"; // Japanese with emojis

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        metadata.Name.Should().Be(name);
        metadata.Summary.Should().Be(summary);
        metadata.Description.Should().Be(description);
    }
}
