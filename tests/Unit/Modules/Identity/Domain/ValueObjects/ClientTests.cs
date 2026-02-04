using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
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
        Assert.NotNull(client);
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
        Assert.NotNull(client);
        Assert.Equal(clientEnum, client.Value);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        EnumClient invalidEnum = (EnumClient)999;

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Client(invalidEnum));
        Assert.Contains("Invalid client platform", exception.Message);
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
        Assert.NotNull(client);
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
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Client("InvalidClient"));
        Assert.Contains("Invalid client platform", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Client(string.Empty));
        Assert.Contains("Invalid client platform", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Client((string)null!));
        Assert.Contains("Invalid client platform", exception.Message);
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
        Assert.Equal("MobileApp", result);
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        EnumClient clientEnum = EnumClient.WebApp;

        // Act
        Client client = clientEnum;

        // Assert
        Assert.NotNull(client);
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
        Assert.NotNull(client);
        Assert.Equal(EnumClient.MobileApp, client.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidClient = "InvalidClient";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Client client = invalidClient;
        });
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
