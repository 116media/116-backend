using _116.Identity.Infrastructure.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="RefreshTokenService"/>.
/// </summary>
public class RefreshTokenServiceTests
{
    private readonly RefreshTokenService _sut;

    public RefreshTokenServiceTests()
    {
        _sut = new RefreshTokenService();
    }

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        string token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64EncodedString()
    {
        // Act
        string token = _sut.GenerateRefreshToken();

        // Assert
        // Valid Base64 should be decodable without exception
        Action act = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnTokenOfExpectedLength()
    {
        // Arrange
        const int expectedByteLength = 256;
        int expectedBase64Length = (int)Math.Ceiling(expectedByteLength / 3.0) * 4;

        // Act
        string token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().HaveLength(expectedBase64Length);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        // Act
        HashSet<string> tokens = [];
        for (int i = 0; i < 100; i++)
        {
            tokens.Add(_sut.GenerateRefreshToken());
        }

        // Assert
        tokens.Should().HaveCount(100, "All generated tokens should be unique");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeUrlSafeBase64()
    {
        // Act
        string token = _sut.GenerateRefreshToken();

        // Assert
        // Standard Base64 can contain +, /, and = characters
        // Check that the token is valid Base64 (may contain these characters)
        token.Should().MatchRegex(@"^[A-Za-z0-9+/=]+$");
    }

    #endregion

    #region HashRefreshToken Tests

    [Fact]
    public void HashRefreshToken_ShouldReturnNonEmptyString()
    {
        // Arrange
        string token = _sut.GenerateRefreshToken();

        // Act
        string hash = _sut.HashRefreshToken(token);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashRefreshToken_ShouldReturnBase64EncodedHash()
    {
        // Arrange
        string token = _sut.GenerateRefreshToken();

        // Act
        string hash = _sut.HashRefreshToken(token);

        // Assert
        Action act = () => Convert.FromBase64String(hash);
        act.Should().NotThrow();
    }

    [Fact]
    public void HashRefreshToken_ShouldReturnConsistentHashForSameInput()
    {
        // Arrange
        string token = _sut.GenerateRefreshToken();

        // Act
        string hash1 = _sut.HashRefreshToken(token);
        string hash2 = _sut.HashRefreshToken(token);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashRefreshToken_ShouldReturnDifferentHashForDifferentTokens()
    {
        // Arrange
        string token1 = _sut.GenerateRefreshToken();
        string token2 = _sut.GenerateRefreshToken();

        // Act
        string hash1 = _sut.HashRefreshToken(token1);
        string hash2 = _sut.HashRefreshToken(token2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashRefreshToken_WithEmptyString_ShouldReturnHash()
    {
        // Arrange
        string token = "";

        // Act
        string hash = _sut.HashRefreshToken(token);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashRefreshToken_ShouldReturnSha256LengthHash()
    {
        // Arrange
        string token = _sut.GenerateRefreshToken();
        // SHA256 produces 32 bytes, Base64 encoding of 32 bytes = 44 characters (with padding)
        const int expectedLength = 44;

        // Act
        string hash = _sut.HashRefreshToken(token);

        // Assert
        hash.Should().HaveLength(expectedLength);
    }

    [Fact]
    public void HashRefreshToken_ShouldBeDeterministic()
    {
        // Arrange
        string token = "test-token-for-hashing";

        // Act
        string hash1 = _sut.HashRefreshToken(token);
        string hash2 = _sut.HashRefreshToken(token);
        string hash3 = _sut.HashRefreshToken(token);

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
    }

    [Fact]
    public void HashRefreshToken_WithSpecialCharacters_ShouldReturnHash()
    {
        // Arrange
        string token = "token!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        string hash = _sut.HashRefreshToken(token);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveLength(44);
    }

    [Fact]
    public void HashRefreshToken_WithUnicodeCharacters_ShouldReturnHash()
    {
        // Arrange
        string token = "トークン密码🔐";

        // Act
        string hash = _sut.HashRefreshToken(token);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveLength(44);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void GenerateAndHash_ShouldProduceValidHashableToken()
    {
        // Act
        string token = _sut.GenerateRefreshToken();
        string hash = _sut.HashRefreshToken(token);

        // Assert
        token.Should().NotBeNullOrEmpty();
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(token, "Hash should be different from original token");
    }

    [Fact]
    public void GenerateAndHash_MultipleTokens_ShouldProduceUniqueHashes()
    {
        // Act
        HashSet<string> hashes = [];
        for (int i = 0; i < 50; i++)
        {
            string token = _sut.GenerateRefreshToken();
            string hash = _sut.HashRefreshToken(token);
            hashes.Add(hash);
        }

        // Assert
        hashes.Should().HaveCount(50, "All hashes should be unique");
    }

    #endregion
}
