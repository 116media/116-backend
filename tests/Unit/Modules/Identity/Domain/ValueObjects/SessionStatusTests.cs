using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SessionStatus"/> value object.
/// </summary>
public class SessionStatusTests
{
    #region Constructor Tests (Enum)

    [Fact]
    public void Constructor_WithValidEnumValue_ShouldCreateInstance()
    {
        // Arrange
        EnumSessionStatus statusEnum = EnumSessionStatus.Active;

        // Act
        SessionStatus status = new(statusEnum);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(EnumSessionStatus.Active, status.Value);
    }

    [Theory]
    [InlineData(EnumSessionStatus.Active)]
    [InlineData(EnumSessionStatus.Expired)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(EnumSessionStatus statusEnum)
    {
        // Act
        SessionStatus status = new(statusEnum);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(statusEnum, status.Value);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        EnumSessionStatus invalidEnum = (EnumSessionStatus)999;

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new SessionStatus(invalidEnum));
        Assert.Contains("Invalid session status", exception.Message);
    }

    #endregion

    #region Constructor Tests (String)

    [Fact]
    public void Constructor_WithValidStringValue_ShouldCreateInstance()
    {
        // Arrange
        string statusString = "Active";

        // Act
        SessionStatus status = new(statusString);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(EnumSessionStatus.Active, status.Value);
    }

    [Theory]
    [InlineData("Active", EnumSessionStatus.Active)]
    [InlineData("Expired", EnumSessionStatus.Expired)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, EnumSessionStatus expected)
    {
        // Act
        SessionStatus status = new(input);

        // Assert
        Assert.Equal(expected, status.Value);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("ACTIVE")]
    [InlineData("AcTiVe")]
    public void Constructor_WithCaseInsensitiveString_ShouldParseCorrectly(string input)
    {
        // Act
        SessionStatus status = new(input);

        // Assert
        Assert.Equal(EnumSessionStatus.Active, status.Value);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new SessionStatus("InvalidStatus"));
        Assert.Contains("Invalid session status", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new SessionStatus(string.Empty));
        Assert.Contains("Invalid session status", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new SessionStatus((string)null!));
        Assert.Contains("Invalid session status", exception.Message);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToEnum_ShouldReturnValue()
    {
        // Arrange
        SessionStatus status = new(EnumSessionStatus.Expired);

        // Act
        EnumSessionStatus result = status;

        // Assert
        Assert.Equal(EnumSessionStatus.Expired, result);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        SessionStatus status = new(EnumSessionStatus.Expired);

        // Act
        string result = status;

        // Assert
        Assert.Equal("Expired", result);
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        EnumSessionStatus statusEnum = EnumSessionStatus.Active;

        // Act
        SessionStatus status = statusEnum;

        // Assert
        Assert.NotNull(status);
        Assert.Equal(EnumSessionStatus.Active, status.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string statusString = "Expired";

        // Act
        SessionStatus status = statusString;

        // Assert
        Assert.NotNull(status);
        Assert.Equal(EnumSessionStatus.Expired, status.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidStatus = "InvalidStatus";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            SessionStatus status = invalidStatus;
        });
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        SessionStatus status1 = new(EnumSessionStatus.Active);
        SessionStatus status2 = new(EnumSessionStatus.Active);

        // Act & Assert
        Assert.Equal(status1, status2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        SessionStatus status1 = new(EnumSessionStatus.Active);
        SessionStatus status2 = new(EnumSessionStatus.Expired);

        // Act & Assert
        Assert.NotEqual(status1, status2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        SessionStatus status1 = new(EnumSessionStatus.Active);
        SessionStatus status2 = new(EnumSessionStatus.Active);

        // Act
        int hash1 = status1.GetHashCode();
        int hash2 = status2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    #endregion
}
