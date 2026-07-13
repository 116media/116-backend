using _116.Content.Domain.Enums;
using _116.Content.Domain.ValueObjects;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ShareChannel"/> value object.
/// </summary>
public class ShareChannelTests
{
    #region Constructor Tests (Enum)

    [Theory]
    [InlineData(EnumShareChannel.Facebook)]
    [InlineData(EnumShareChannel.X)]
    [InlineData(EnumShareChannel.WhatsApp)]
    [InlineData(EnumShareChannel.Clipboard)]
    [InlineData(EnumShareChannel.WebShare)]
    public void Constructor_WithValidEnumValue_ShouldCreateInstance(EnumShareChannel value)
    {
        // Act
        ShareChannel channel = new(value);

        // Assert
        channel.Should().NotBeNull();
        channel.Value.Should().Be(value);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        var invalid = (EnumShareChannel)999;

        // Act & Assert
        Action act = () => new ShareChannel(invalid);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid share channel");
    }

    #endregion

    #region Constructor Tests (String)

    [Theory]
    [InlineData("Facebook", EnumShareChannel.Facebook)]
    [InlineData("facebook", EnumShareChannel.Facebook)]
    [InlineData("X", EnumShareChannel.X)]
    [InlineData("x", EnumShareChannel.X)]
    [InlineData("WhatsApp", EnumShareChannel.WhatsApp)]
    [InlineData("whatsapp", EnumShareChannel.WhatsApp)]
    [InlineData("Clipboard", EnumShareChannel.Clipboard)]
    [InlineData("WebShare", EnumShareChannel.WebShare)]
    [InlineData("webshare", EnumShareChannel.WebShare)]
    public void Constructor_WithValidStringValue_ShouldParseCaseInsensitively(string input, EnumShareChannel expected)
    {
        // Act
        ShareChannel channel = new(input);

        // Assert
        channel.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("web-share")]
    [InlineData("instagram")]
    [InlineData("not-a-channel")]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException(string input)
    {
        // Act & Assert
        Action act = () => new ShareChannel(input);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid share channel");
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromEnum_ShouldWrapValue()
    {
        // Act
        ShareChannel channel = EnumShareChannel.WebShare;

        // Assert
        channel.Value.Should().Be(EnumShareChannel.WebShare);
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldParseValue()
    {
        // Act
        ShareChannel channel = "whatsapp";

        // Assert
        channel.Value.Should().Be(EnumShareChannel.WhatsApp);
    }

    [Fact]
    public void ImplicitConversion_ToEnum_ShouldReturnValue()
    {
        // Arrange
        ShareChannel channel = new(EnumShareChannel.Facebook);

        // Act
        EnumShareChannel value = channel;

        // Assert
        value.Should().Be(EnumShareChannel.Facebook);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnEnumName()
    {
        // Arrange
        ShareChannel channel = new(EnumShareChannel.WebShare);

        // Act
        string value = channel;

        // Assert
        value.Should().Be("WebShare");
    }

    #endregion

    #region TryFrom (untrusted boundary)

    [Theory]
    [InlineData("Facebook", EnumShareChannel.Facebook)]
    [InlineData("x", EnumShareChannel.X)]
    [InlineData("WhatsApp", EnumShareChannel.WhatsApp)]
    [InlineData("webshare", EnumShareChannel.WebShare)]
    public void TryFrom_WithValidLabel_ShouldReturnChannel(string label, EnumShareChannel expected)
    {
        // Act
        ShareChannel? channel = ShareChannel.TryFrom(label);

        // Assert
        channel.Should().NotBeNull();
        channel!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("web-share")]
    [InlineData("instagram")]
    public void TryFrom_WhenMissingOrUnknown_ShouldReturnNull(string? label)
    {
        // Act & Assert
        ShareChannel.TryFrom(label).Should().BeNull();
    }

    #endregion
}
