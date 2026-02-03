using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
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
        status.Should().NotBeNull();
        status.Value.Should().Be(EnumSessionStatus.Active);
    }

    [Theory]
    [InlineData(EnumSessionStatus.Active)]
    [InlineData(EnumSessionStatus.Expired)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(EnumSessionStatus statusEnum)
    {
        // Act
        SessionStatus status = new(statusEnum);

        // Assert
        status.Should().NotBeNull();
        status.Value.Should().Be(statusEnum);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        EnumSessionStatus invalidEnum = (EnumSessionStatus)999;

        // Act & Assert
        Action act = () => new SessionStatus(invalidEnum);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid session status");
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
        status.Should().NotBeNull();
        status.Value.Should().Be(EnumSessionStatus.Active);
    }

    [Theory]
    [InlineData("Active", EnumSessionStatus.Active)]
    [InlineData("Expired", EnumSessionStatus.Expired)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, EnumSessionStatus expected)
    {
        // Act
        SessionStatus status = new(input);

        // Assert
        status.Value.Should().Be(expected);
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
        status.Value.Should().Be(EnumSessionStatus.Active);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new SessionStatus("InvalidStatus");
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid session status");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new SessionStatus(string.Empty);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid session status");
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new SessionStatus((string)null!);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid session status");
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
        result.Should().Be(EnumSessionStatus.Expired);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        SessionStatus status = new(EnumSessionStatus.Expired);

        // Act
        string result = status;

        // Assert
        result.Should().Be("Expired");
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        EnumSessionStatus statusEnum = EnumSessionStatus.Active;

        // Act
        SessionStatus status = statusEnum;

        // Assert
        status.Should().NotBeNull();
        status.Value.Should().Be(EnumSessionStatus.Active);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string statusString = "Expired";

        // Act
        SessionStatus status = statusString;

        // Assert
        status.Should().NotBeNull();
        status.Value.Should().Be(EnumSessionStatus.Expired);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidStatus = "InvalidStatus";

        // Act & Assert
        Action act = () =>
        {
            SessionStatus status = invalidStatus;
        };
        act.Should().ThrowExactly<ArgumentException>();
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
        status1.Should().Be(status2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        SessionStatus status1 = new(EnumSessionStatus.Active);
        SessionStatus status2 = new(EnumSessionStatus.Expired);

        // Act & Assert
        status1.Should().NotBe(status2);
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
        hash1.Should().Be(hash2);
    }

    #endregion
}
