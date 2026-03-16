using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="OtpPurpose"/> value object.
/// </summary>
public class OtpPurposeTests
{
    #region Constructor Tests (Enum)

    [Fact]
    public void Constructor_WithValidEnumValue_ShouldCreateInstance()
    {
        // Arrange
        var purposeEnum = EnumOtpPurpose.EmailVerification;

        // Act
        OtpPurpose purpose = new(purposeEnum);

        // Assert
        purpose.Should().NotBeNull();
        purpose.Value.Should().Be(EnumOtpPurpose.EmailVerification);
    }

    [Theory]
    [InlineData(EnumOtpPurpose.EmailVerification)]
    [InlineData(EnumOtpPurpose.PasswordReset)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(EnumOtpPurpose purposeEnum)
    {
        // Act
        OtpPurpose purpose = new(purposeEnum);

        // Assert
        purpose.Should().NotBeNull();
        purpose.Value.Should().Be(purposeEnum);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEnum = (EnumOtpPurpose)999;

        // Act & Assert
        Action act = () => new OtpPurpose(invalidEnum);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid OTP purpose");
    }

    #endregion

    #region Constructor Tests (String)

    [Fact]
    public void Constructor_WithValidStringValue_ShouldCreateInstance()
    {
        // Arrange
        string purposeString = "EmailVerification";

        // Act
        OtpPurpose purpose = new(purposeString);

        // Assert
        purpose.Should().NotBeNull();
        purpose.Value.Should().Be(EnumOtpPurpose.EmailVerification);
    }

    [Theory]
    [InlineData("EmailVerification", EnumOtpPurpose.EmailVerification)]
    [InlineData("PasswordReset", EnumOtpPurpose.PasswordReset)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, EnumOtpPurpose expected)
    {
        // Act
        OtpPurpose purpose = new(input);

        // Assert
        purpose.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("emailverification")]
    [InlineData("EMAILVERIFICATION")]
    [InlineData("EmailVERIFICATION")]
    public void Constructor_WithCaseInsensitiveString_ShouldParseCorrectly(string input)
    {
        // Act
        OtpPurpose purpose = new(input);

        // Assert
        purpose.Value.Should().Be(EnumOtpPurpose.EmailVerification);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new OtpPurpose("InvalidPurpose");
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid OTP purpose");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new OtpPurpose(string.Empty);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid OTP purpose");
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new OtpPurpose((string)null!);
        ExceptionAssertions<ArgumentException>? exception = act.Should().ThrowExactly<ArgumentException>();
        exception.Which.Message.Should().Contain("Invalid OTP purpose");
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToEnum_ShouldReturnValue()
    {
        // Arrange
        OtpPurpose purpose = new(EnumOtpPurpose.PasswordReset);

        // Act
        EnumOtpPurpose result = purpose;

        // Assert
        result.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        OtpPurpose purpose = new(EnumOtpPurpose.PasswordReset);

        // Act
        string result = purpose;

        // Assert
        result.Should().Be("PasswordReset");
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        var purposeEnum = EnumOtpPurpose.EmailVerification;

        // Act
        OtpPurpose purpose = purposeEnum;

        // Assert
        purpose.Should().NotBeNull();
        purpose.Value.Should().Be(EnumOtpPurpose.EmailVerification);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string purposeString = "PasswordReset";

        // Act
        OtpPurpose purpose = purposeString;

        // Assert
        purpose.Should().NotBeNull();
        purpose.Value.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidPurpose = "InvalidPurpose";

        // Act & Assert
        Action act = () =>
        {
            OtpPurpose purpose = invalidPurpose;
        };
        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        OtpPurpose purpose1 = new(EnumOtpPurpose.EmailVerification);
        OtpPurpose purpose2 = new(EnumOtpPurpose.EmailVerification);

        // Act & Assert
        purpose1.Should().Be(purpose2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        OtpPurpose purpose1 = new(EnumOtpPurpose.EmailVerification);
        OtpPurpose purpose2 = new(EnumOtpPurpose.PasswordReset);

        // Act & Assert
        purpose1.Should().NotBe(purpose2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        OtpPurpose purpose1 = new(EnumOtpPurpose.EmailVerification);
        OtpPurpose purpose2 = new(EnumOtpPurpose.EmailVerification);

        // Act
        int hash1 = purpose1.GetHashCode();
        int hash2 = purpose2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion
}
