using _116.Identity.Domain.ValueObjects;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="Email"/> value object.
/// </summary>
public class EmailTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateInstance()
    {
        // Arrange
        string validEmail = "test@example.com";

        // Act
        Email email = new(validEmail);

        // Assert
        Assert.NotNull(email);
        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Constructor_WithValidEmail_ShouldNormalizeToLowercase()
    {
        // Arrange
        string mixedCaseEmail = "Test@Example.COM";

        // Act
        Email email = new(mixedCaseEmail);

        // Assert
        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Email(null!));
        Assert.Contains("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Email(string.Empty));
        Assert.Contains("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Email("   "));
        Assert.Contains("Email cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Email("not-an-email"));
        Assert.Contains("Invalid email format", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmailMissingAtSign_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Email("testexample.com"));
        Assert.Contains("Invalid email format", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmailMissingDomain_ShouldThrowArgumentException()
    {
        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new Email("test@"));
        Assert.Contains("Invalid email format", exception.Message);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("123@example.com")]
    [InlineData("test@subdomain.example.com")]
    public void Constructor_WithVariousValidFormats_ShouldNotThrow(string validEmail)
    {
        // Act
        Email email = new(validEmail);

        // Assert
        Assert.NotNull(email);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToString_ShouldReturnValue()
    {
        // Arrange
        Email email = new("test@example.com");

        // Act
        string result = email;

        // Assert
        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateEmailInstance()
    {
        // Arrange
        string emailString = "test@example.com";

        // Act
        Email email = emailString;

        // Assert
        Assert.NotNull(email);
        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidEmail = "not-an-email";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Email email = invalidEmail;
        });
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        Email email1 = new("test@example.com");
        Email email2 = new("test@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Equals_WithDifferentCase_ShouldBeEqual()
    {
        // Arrange
        Email email1 = new("Test@Example.com");
        Email email2 = new("test@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        Email email1 = new("test1@example.com");
        Email email2 = new("test2@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        Email email1 = new("test@example.com");
        Email email2 = new("test@example.com");

        // Act
        int hash1 = email1.GetHashCode();
        int hash2 = email2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    #endregion
}
