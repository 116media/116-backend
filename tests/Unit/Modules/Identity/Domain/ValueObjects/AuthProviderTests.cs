using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="AuthProvider"/> value object.
/// </summary>
public class AuthProviderTests
{
    #region Constructor Tests (Enum)

    [Fact]
    public void Constructor_WithValidEnumValue_ShouldCreateInstance()
    {
        // Arrange
        var providerEnum = EnumAuthProvider.Local;

        // Act
        AuthProvider provider = new(providerEnum);

        // Assert
        provider.Should().NotBeNull();
        provider.Value.Should().Be(EnumAuthProvider.Local);
    }

    [Theory]
    [InlineData(EnumAuthProvider.Local)]
    [InlineData(EnumAuthProvider.Google)]
    [InlineData(EnumAuthProvider.Facebook)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(EnumAuthProvider providerEnum)
    {
        // Act
        AuthProvider provider = new(providerEnum);

        // Assert
        provider.Should().NotBeNull();
        provider.Value.Should().Be(providerEnum);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEnum = (EnumAuthProvider)999;

        // Act & Assert
        Action act = () => new AuthProvider(invalidEnum);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid auth provider");
    }

    #endregion

    #region Constructor Tests (String)

    [Fact]
    public void Constructor_WithValidStringValue_ShouldCreateInstance()
    {
        // Arrange
        string providerString = "Local";

        // Act
        AuthProvider provider = new(providerString);

        // Assert
        provider.Should().NotBeNull();
        provider.Value.Should().Be(EnumAuthProvider.Local);
    }

    [Theory]
    [InlineData("Local", EnumAuthProvider.Local)]
    [InlineData("Google", EnumAuthProvider.Google)]
    [InlineData("Facebook", EnumAuthProvider.Facebook)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, EnumAuthProvider expected)
    {
        // Act
        AuthProvider provider = new(input);

        // Assert
        provider.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("local")]
    [InlineData("LOCAL")]
    [InlineData("LocAL")]
    public void Constructor_WithCaseInsensitiveString_ShouldParseCorrectly(string input)
    {
        // Act
        AuthProvider provider = new(input);

        // Assert
        provider.Value.Should().Be(EnumAuthProvider.Local);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new AuthProvider("InvalidProvider");
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid auth provider");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new AuthProvider(string.Empty);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid auth provider");
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new AuthProvider((string)null!);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid auth provider");
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToEnum_ShouldReturnValue()
    {
        // Arrange
        AuthProvider provider = new(EnumAuthProvider.Google);

        // Act
        EnumAuthProvider result = provider;

        // Assert
        result.Should().Be(EnumAuthProvider.Google);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        AuthProvider provider = new(EnumAuthProvider.Google);

        // Act
        string result = provider;

        // Assert
        result.Should().Be("Google");
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        var providerEnum = EnumAuthProvider.Facebook;

        // Act
        AuthProvider provider = providerEnum;

        // Assert
        provider.Should().NotBeNull();
        provider.Value.Should().Be(EnumAuthProvider.Facebook);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string providerString = "Google";

        // Act
        AuthProvider provider = providerString;

        // Assert
        provider.Should().NotBeNull();
        provider.Value.Should().Be(EnumAuthProvider.Google);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidProvider = "InvalidProvider";

        // Act & Assert
        Action act = () =>
        {
            AuthProvider provider = invalidProvider;
        };
        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        AuthProvider provider1 = new(EnumAuthProvider.Local);
        AuthProvider provider2 = new(EnumAuthProvider.Local);

        // Act & Assert
        provider1.Should().Be(provider2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        AuthProvider provider1 = new(EnumAuthProvider.Local);
        AuthProvider provider2 = new(EnumAuthProvider.Google);

        // Act & Assert
        provider1.Should().NotBe(provider2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        AuthProvider provider1 = new(EnumAuthProvider.Local);
        AuthProvider provider2 = new(EnumAuthProvider.Local);

        // Act
        int hash1 = provider1.GetHashCode();
        int hash2 = provider2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion
}
