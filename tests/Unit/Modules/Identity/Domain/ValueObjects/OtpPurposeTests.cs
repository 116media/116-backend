using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
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
        EnumOtpPurpose purposeEnum = EnumOtpPurpose.EmailVerification;

        // Act
        OtpPurpose purpose = new(purposeEnum);

        // Assert
        Assert.NotNull(purpose);
        Assert.Equal(EnumOtpPurpose.EmailVerification, purpose.Value);
    }

    [Theory]
    [InlineData(EnumOtpPurpose.EmailVerification)]
    [InlineData(EnumOtpPurpose.PasswordReset)]
    public void Constructor_WithAllValidEnumValues_ShouldNotThrow(EnumOtpPurpose purposeEnum)
    {
        // Act
        OtpPurpose purpose = new(purposeEnum);

        // Assert
        Assert.NotNull(purpose);
        Assert.Equal(purposeEnum, purpose.Value);
    }

    [Fact]
    public void Constructor_WithInvalidEnumValue_ShouldThrowArgumentException()
    {
        // Arrange
        EnumOtpPurpose invalidEnum = (EnumOtpPurpose)999;

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new OtpPurpose(invalidEnum));
        Assert.Contains("Invalid OTP purpose", exception.Message);
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
        Assert.NotNull(purpose);
        Assert.Equal(EnumOtpPurpose.EmailVerification, purpose.Value);
    }

    [Theory]
    [InlineData("EmailVerification", EnumOtpPurpose.EmailVerification)]
    [InlineData("PasswordReset", EnumOtpPurpose.PasswordReset)]
    public void Constructor_WithValidStringValues_ShouldParseCorrectly(string input, EnumOtpPurpose expected)
    {
        // Act
        OtpPurpose purpose = new(input);

        // Assert
        Assert.Equal(expected, purpose.Value);
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
        Assert.Equal(EnumOtpPurpose.EmailVerification, purpose.Value);
    }

    [Fact]
    public void Constructor_WithInvalidStringValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new OtpPurpose("InvalidPurpose"));
        Assert.Contains("Invalid OTP purpose", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new OtpPurpose(string.Empty));
        Assert.Contains("Invalid OTP purpose", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullString_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new OtpPurpose((string)null!));
        Assert.Contains("Invalid OTP purpose", exception.Message);
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
        Assert.Equal(EnumOtpPurpose.PasswordReset, result);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEnumName()
    {
        // Arrange
        OtpPurpose purpose = new(EnumOtpPurpose.PasswordReset);

        // Act
        string result = purpose;

        // Assert
        Assert.Equal("PasswordReset", result);
    }

    [Fact]
    public void ImplicitConversionFromEnum_ShouldCreateInstance()
    {
        // Arrange
        EnumOtpPurpose purposeEnum = EnumOtpPurpose.EmailVerification;

        // Act
        OtpPurpose purpose = purposeEnum;

        // Assert
        Assert.NotNull(purpose);
        Assert.Equal(EnumOtpPurpose.EmailVerification, purpose.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateInstance()
    {
        // Arrange
        string purposeString = "PasswordReset";

        // Act
        OtpPurpose purpose = purposeString;

        // Assert
        Assert.NotNull(purpose);
        Assert.Equal(EnumOtpPurpose.PasswordReset, purpose.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidValue_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidPurpose = "InvalidPurpose";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            OtpPurpose purpose = invalidPurpose;
        });
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
        Assert.Equal(purpose1, purpose2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        OtpPurpose purpose1 = new(EnumOtpPurpose.EmailVerification);
        OtpPurpose purpose2 = new(EnumOtpPurpose.PasswordReset);

        // Act & Assert
        Assert.NotEqual(purpose1, purpose2);
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
        Assert.Equal(hash1, hash2);
    }

    #endregion
}
