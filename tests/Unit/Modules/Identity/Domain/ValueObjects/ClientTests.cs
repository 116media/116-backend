using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="Client"/> value object.
/// </summary>
public class ClientTests
{
    #region Constructor Tests (Enum)

    [Fact]
    public void Constructor_WithValidEnumValue_ShouldCreateInstance()
    {
        // Arrange
        EnumClient clientEnum = EnumClient.WebApp;

        // Act
        Client client = new(clientEnum);

        // Assert
        client.Should().NotBeNull();
        Assert.Equal(EnumClient.WebApp, client.Value);
    }

    [Theory]
    [InlineData(EnumClient.WebApp)]
    [InlineData(EnumClient.MobileApp)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(EnumClient clientEnum)
    {
        // Act
        Client client = new(clientEnum);

        // Assert
        client.Should().NotBeNull();
        Assert.Equal(clientEnum, client.Value);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        EnumClient invalidEnum = (EnumClient)999;

        // Act & Assert
        Action act = () => new Client(invalidEnum);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid client platform");
    }

    #endregion

    #region Constructor Tests (String)

    [Fact]
    public void Constructor_WithValidStringValue_ShouldCreateInstance()
    {
        // Arrange
        string clientString = "WebApp";

        // Act
        Client client = new(clientString);

        // Assert
        client.Should().NotBeNull();
        Assert.Equal(EnumClient.WebApp, client.Value);
    }

    [Theory]
    [InlineData("WebApp", EnumClient.WebApp)]
    [InlineData("MobileApp", EnumClient.MobileApp)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, EnumClient expected)
    {
        // Act
        Client client = new(input);

        // Assert
        Assert.Equal(expected, client.Value);
    }

    [Theory]
    [InlineData("webapp")]
    [InlineData("WEBAPP")]
    [InlineData("WebAPP")]
    public void Constructor_WithCaseInsensitiveString_ShouldParseCorrectly(string input)
    {
        // Act
        Client client = new(input);

        // Assert
        Assert.Equal(EnumClient.WebApp, client.Value);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Client("InvalidClient");
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid client platform");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Client(string.Empty);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid client platform");
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Client((string)null!);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid client platform");
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToEnum_ShouldReturnValue()
    {
        // Arrange
        Client client = new(EnumClient.MobileApp);

        // Act
        EnumClient result = client;

        // Assert
        Assert.Equal(EnumClient.MobileApp, result);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        Client client = new(EnumClient.MobileApp);

        // Act
        string result = client;

        // Assert
        result.Should().Be("MobileApp");
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        EnumClient clientEnum = EnumClient.WebApp;

        // Act
        Client client = clientEnum;

        // Assert
        client.Should().NotBeNull();
        Assert.Equal(EnumClient.WebApp, client.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string clientString = "MobileApp";

        // Act
        Client client = clientString;

        // Assert
        client.Should().NotBeNull();
        Assert.Equal(EnumClient.MobileApp, client.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidClient = "InvalidClient";

        // Act & Assert
        Action act = () =>
        {
            Client client = invalidClient;
        };
        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        Client client1 = new(EnumClient.WebApp);
        Client client2 = new(EnumClient.WebApp);

        // Act & Assert
        Assert.Equal(client1, client2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        Client client1 = new(EnumClient.WebApp);
        Client client2 = new(EnumClient.MobileApp);

        // Act & Assert
        Assert.NotEqual(client1, client2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        Client client1 = new(EnumClient.WebApp);
        Client client2 = new(EnumClient.WebApp);

        // Act
        int hash1 = client1.GetHashCode();
        int hash2 = client2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    #endregion
}
