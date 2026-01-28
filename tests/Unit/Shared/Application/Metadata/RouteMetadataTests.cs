using _116.Shared.Application.Metadata;
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
        string name = "TestRoute";
        string summary = "Test summary";
        string description = "Test description with more details";

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        Assert.Equal(name, metadata.Name);
        Assert.Equal(summary, metadata.Summary);
        Assert.Equal(description, metadata.Description);
    }

    [Fact]
    public void Constructor_WithEmptyStrings_ShouldAllowEmptyValues()
    {
        // Arrange
        string name = "";
        string summary = "";
        string description = "";

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        Assert.Equal(string.Empty, metadata.Name);
        Assert.Equal(string.Empty, metadata.Summary);
        Assert.Equal(string.Empty, metadata.Description);
    }

    [Fact]
    public void Constructor_WithLongDescription_ShouldHandleLongStrings()
    {
        // Arrange
        string name = "ComplexRoute";
        string summary = "A complex route with extensive functionality";
        string description = new string('x', 5000); // Very long description

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        Assert.Equal(name, metadata.Name);
        Assert.Equal(summary, metadata.Summary);
        Assert.Equal(5000, metadata.Description.Length);
    }

    [Fact]
    public void Constructor_WithMultilineDescription_ShouldPreserveFormatting()
    {
        // Arrange
        string name = "MultilineRoute";
        string summary = "Route with multiline description";
        string description = """
            Line 1
            Line 2
            Line 3
            """;

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        Assert.Equal(name, metadata.Name);
        Assert.Equal(summary, metadata.Summary);
        Assert.Contains("Line 1", metadata.Description);
        Assert.Contains("Line 2", metadata.Description);
        Assert.Contains("Line 3", metadata.Description);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var metadata = new RouteMetadata("Test", "Summary", "Description");

        // Act & Assert
        var nameProperty = typeof(RouteMetadata).GetProperty(nameof(RouteMetadata.Name));
        var summaryProperty = typeof(RouteMetadata).GetProperty(nameof(RouteMetadata.Summary));
        var descriptionProperty = typeof(RouteMetadata).GetProperty(nameof(RouteMetadata.Description));

        Assert.NotNull(nameProperty);
        Assert.NotNull(summaryProperty);
        Assert.NotNull(descriptionProperty);

        Assert.True(nameProperty.CanRead);
        Assert.False(nameProperty.CanWrite || nameProperty.SetMethod?.IsPublic == true);

        Assert.True(summaryProperty.CanRead);
        Assert.False(summaryProperty.CanWrite || summaryProperty.SetMethod?.IsPublic == true);

        Assert.True(descriptionProperty.CanRead);
        Assert.False(descriptionProperty.CanWrite || descriptionProperty.SetMethod?.IsPublic == true);
    }

    [Fact]
    public void RouteMetadata_ShouldBeValueType()
    {
        // Arrange & Act
        var type = typeof(RouteMetadata);

        // Assert
        Assert.True(type.IsValueType);
    }

    [Fact]
    public void RouteMetadata_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var metadata1 = new RouteMetadata("Route", "Summary", "Description");
        var metadata2 = new RouteMetadata("Route", "Summary", "Description");

        // Act & Assert
        Assert.Equal(metadata1, metadata2);
    }

    [Fact]
    public void RouteMetadata_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var metadata1 = new RouteMetadata("Route1", "Summary", "Description");
        var metadata2 = new RouteMetadata("Route2", "Summary", "Description");

        // Act & Assert
        Assert.NotEqual(metadata1, metadata2);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        string name = "Route-With_Special.Characters!@#";
        string summary = "Summary with émojis 🚀 and symbols";
        string description = "Description with\ttabs\nand\r\nnewlines";

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        Assert.Equal(name, metadata.Name);
        Assert.Equal(summary, metadata.Summary);
        Assert.Equal(description, metadata.Description);
    }

    [Fact]
    public void Constructor_WithUnicodeCharacters_ShouldPreserveUnicode()
    {
        // Arrange
        string name = "路由名称"; // Chinese characters
        string summary = "Résumé de l'itinéraire"; // French with accents
        string description = "説明 😀 🎉"; // Japanese with emojis

        // Act
        var metadata = new RouteMetadata(name, summary, description);

        // Assert
        Assert.Equal(name, metadata.Name);
        Assert.Equal(summary, metadata.Summary);
        Assert.Equal(description, metadata.Description);
    }
}
