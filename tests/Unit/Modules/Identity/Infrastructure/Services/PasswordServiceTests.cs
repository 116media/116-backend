using _116.Identity.Infrastructure.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="PasswordService"/>.
/// </summary>
public class PasswordServiceTests
{
    private readonly PasswordService _sut;

    public PasswordServiceTests()
    {
        _sut = new PasswordService();
    }

    #region Hash Tests

    [Fact]
    public void Hash_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        string hash = _sut.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("v1:");
    }

    [Fact]
    public void Hash_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        string hash1 = _sut.Hash(password);
        string hash2 = _sut.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2, "because each hash should use a unique salt");
    }

    [Fact]
    public void Hash_WithEmptyPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        string password = "";

        // Act
        string hash = _sut.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("v1:");
    }

    [Fact]
    public void Hash_WithLongPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        string password = new('a', 1000);

        // Act
        string hash = _sut.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("v1:");
    }

    [Fact]
    public void Hash_WithSpecialCharacters_ShouldReturnHashedPassword()
    {
        // Arrange
        string password = "P@$$w0rd!#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        string hash = _sut.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("v1:");
    }

    [Fact]
    public void Hash_WithUnicodeCharacters_ShouldReturnHashedPassword()
    {
        // Arrange
        string password = "密码テスト🔐";

        // Act
        string hash = _sut.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("v1:");
    }

    #endregion

    #region Verify Tests

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        string password = "TestPassword123!";
        string hash = _sut.Hash(password);

        // Act
        bool result = _sut.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string wrongPassword = "WrongPassword456!";
        string hash = _sut.Hash(password);

        // Act
        bool result = _sut.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithNullHash_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        bool result = _sut.Verify(password, null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyHash_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        bool result = _sut.Verify(password, "");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithWhitespaceHash_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        bool result = _sut.Verify(password, "   ");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithHashMissingVersionPrefix_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string invalidHash = "somebase64string==";

        // Act
        bool result = _sut.Verify(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithInvalidBase64InHash_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string invalidHash = "v1:not-valid-base64!!!";

        // Act
        bool result = _sut.Verify(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithTruncatedHash_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string hash = _sut.Hash(password);
        string truncatedHash = hash[..20]; // Truncate the hash

        // Act
        bool result = _sut.Verify(password, truncatedHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithModifiedHash_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string hash = _sut.Hash(password);
        // Modify a character in the base64 portion
        char[] chars = hash.ToCharArray();
        chars[5] = chars[5] == 'A' ? 'B' : 'A';
        string modifiedHash = new(chars);

        // Act
        bool result = _sut.Verify(password, modifiedHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithCaseSensitivePassword_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string wrongCasePassword = "testpassword123!";
        string hash = _sut.Hash(password);

        // Act
        bool result = _sut.Verify(wrongCasePassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ShouldVerifyCorrectly()
    {
        // Arrange
        string password = "";
        string hash = _sut.Hash(password);

        // Act
        bool result = _sut.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithUnicodePassword_ShouldVerifyCorrectly()
    {
        // Arrange
        string password = "密码テスト🔐";
        string hash = _sut.Hash(password);

        // Act
        bool result = _sut.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongVersionPrefix_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string hash = _sut.Hash(password);
        string wrongVersionHash = "v2:" + hash[3..];

        // Act
        bool result = _sut.Verify(password, wrongVersionHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithHashOfWrongLength_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        // Create a valid base64 string but with wrong length
        string shortHash = "v1:" + Convert.ToBase64String(new byte[10]);

        // Act
        bool result = _sut.Verify(password, shortHash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
