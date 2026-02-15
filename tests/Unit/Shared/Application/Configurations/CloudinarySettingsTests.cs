using _116.Shared.Application.Configurations;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Configurations;

/// <summary>
/// Unit tests for <see cref="CloudinarySettings"/>.
/// </summary>
public class CloudinarySettingsTests
{
    [Fact]
    public void IsValid_WithAllPropertiesSet_ShouldReturnTrue()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyCloudName_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullCloudName_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = null!,
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceCloudName_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "   ",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyApiKey_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "",
            ApiSecret = "test-secret",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullApiKey_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = null!,
            ApiSecret = "test-secret",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceApiKey_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "   ",
            ApiSecret = "test-secret",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyApiSecret_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithNullApiSecret_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = null!,
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceApiSecret_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "   ",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithAllPropertiesEmpty_ShouldReturnFalse()
    {
        // Arrange
        var settings = new CloudinarySettings
        {
            CloudName = "",
            ApiKey = "",
            ApiSecret = "",
        };

        // Act
        bool isValid = settings.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyStrings()
    {
        // Act
        var settings = new CloudinarySettings();

        // Assert
        settings.CloudName.Should().Be(string.Empty);
        settings.ApiKey.Should().Be(string.Empty);
        settings.ApiSecret.Should().Be(string.Empty);
    }
}
