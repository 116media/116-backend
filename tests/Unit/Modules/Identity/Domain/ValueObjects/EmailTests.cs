using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Identity.Domain.ValueObjects;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
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
        email.Should().NotBeNull();
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_WithValidEmail_ShouldNormalizeToLowercase()
    {
        // Arrange
        string mixedCaseEmail = "Test@Example.COM";

        // Act
        Email email = new(mixedCaseEmail);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Email(null!);
        act.Should().ThrowExactly<IdentityRuleException>().Which.Code.Should().Be(IdentityRuleCodes.InvalidEmail);
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Email(string.Empty);
        act.Should().ThrowExactly<IdentityRuleException>().Which.Code.Should().Be(IdentityRuleCodes.InvalidEmail);
    }

    [Fact]
    public void Constructor_WithWhitespaceEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Email("   ");
        act.Should().ThrowExactly<IdentityRuleException>().Which.Code.Should().Be(IdentityRuleCodes.InvalidEmail);
    }

    [Fact]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Email("not-an-email");
        act.Should().ThrowExactly<IdentityRuleException>().Which.Code.Should().Be(IdentityRuleCodes.InvalidEmail);
    }

    [Fact]
    public void Constructor_WithEmailMissingAtSign_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Email("testexample.com");
        act.Should().ThrowExactly<IdentityRuleException>().Which.Code.Should().Be(IdentityRuleCodes.InvalidEmail);
    }

    [Fact]
    public void Constructor_WithEmailMissingDomain_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Email("test@");
        act.Should().ThrowExactly<IdentityRuleException>().Which.Code.Should().Be(IdentityRuleCodes.InvalidEmail);
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
        email.Should().NotBeNull();
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
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateEmailInstance()
    {
        // Arrange
        string emailString = "test@example.com";

        // Act
        Email email = emailString;

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitConversionFromString_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        string invalidEmail = "not-an-email";

        // Act & Assert
        Action act = () =>
        {
            Email email = invalidEmail;
        };
        act.Should().ThrowExactly<IdentityRuleException>().Which.Code.Should().Be(IdentityRuleCodes.InvalidEmail);
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
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentCase_ShouldBeEqual()
    {
        // Arrange
        Email email1 = new("Test@Example.com");
        Email email2 = new("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        Email email1 = new("test1@example.com");
        Email email2 = new("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
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
        hash1.Should().Be(hash2);
    }

    #endregion
}
