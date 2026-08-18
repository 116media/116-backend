using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="OtpService"/>.
/// </summary>
public class OtpServiceTests
{
    /// <summary>
    /// The instant the service's clock is pinned to. Expiration assertions are literal offsets
    /// from it rather than reads of the same clock the service uses.
    /// </summary>
    private static readonly DateTime StartInstant = new(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc);

    private readonly OtpService _sut;

    public OtpServiceTests()
    {
        _sut = new OtpService(TestConstants.Otp.Pepper, new FakeTimeProvider(new DateTimeOffset(StartInstant)));
    }

    #region GenerateOtpCode Tests

    [Fact]
    public void GenerateOtpCode_ShouldReturnCodeOfCorrectLength()
    {
        // Act
        string code = _sut.GenerateOtpCode();

        // Assert
        code.Should().HaveLength(UserConstants.OtpCodeLength);
    }

    [Fact]
    public void GenerateOtpCode_ShouldReturnNumericCode()
    {
        // Act
        string code = _sut.GenerateOtpCode();

        // Assert
        code.Should().MatchRegex(@"^\d+$", "OTP code should contain only digits");
    }

    [Fact]
    public void GenerateOtpCode_ShouldGenerateDifferentCodes()
    {
        // Act
        HashSet<string> codes = [];
        for (int i = 0; i < 100; i++)
        {
            codes.Add(_sut.GenerateOtpCode());
        }

        // Assert
        codes.Count.Should().BeGreaterThan(90, "Most generated codes should be unique");
    }

    [Fact]
    public void GenerateOtpCode_ShouldPadWithZeros()
    {
        // Act - Generate many codes to increase chance of getting one starting with 0
        List<string> codes = [];
        for (int i = 0; i < 1000; i++)
        {
            codes.Add(_sut.GenerateOtpCode());
        }

        // Assert - All codes should have consistent length regardless of leading zeros
        codes.Should().OnlyContain(c => c.Length == UserConstants.OtpCodeLength);
    }

    #endregion

    #region CreateOtp Tests

    [Fact]
    public void CreateOtp_WithValidParameters_ShouldReturnOtpEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpCreationResult result = _sut.CreateOtp(userId, purpose);

        // Assert
        result.Otp.Should().NotBeNull();
        result.Otp.Id.Should().NotBe(Guid.Empty);
        result.Otp.UserId.Should().Be(userId);
        result.Otp.Purpose.Should().Be(purpose);
    }

    [Fact]
    public void CreateOtp_ShouldReturnAPlainCodeOfTheGeneratedShape()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpCreationResult result = _sut.CreateOtp(userId, purpose);

        // Assert
        result.PlainCode.Should().HaveLength(UserConstants.OtpCodeLength);
        result.PlainCode.Should().MatchRegex(@"^\d+$");
    }

    [Fact]
    public void CreateOtp_ShouldStoreAHashInsteadOfThePlainCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpCreationResult result = _sut.CreateOtp(userId, purpose);

        // Assert
        result.Otp.CodeHash.Should().NotBe(result.PlainCode);
        result.Otp.CodeHash.Length.Should().BeLessThanOrEqualTo(UserConstants.OtpCodeHashLength);
    }

    [Fact]
    public void CreateOtp_ShouldStoreAHashThatVerifiesAgainstThePlainCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpCreationResult result = _sut.CreateOtp(userId, purpose);

        // Assert
        _sut.Verify(result.PlainCode, result.Otp.CodeHash).Should().BeTrue();
    }

    [Fact]
    public void CreateOtp_CalledTwice_ShouldHashEachGeneratedCodeInTurn()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpCreationResult first = _sut.CreateOtp(userId, purpose);
        OtpCreationResult second = _sut.CreateOtp(userId, purpose);

        // Assert — each stored hash answers only to the code it was derived from
        _sut.Verify(first.PlainCode, first.Otp.CodeHash).Should().BeTrue();
        _sut.Verify(second.PlainCode, second.Otp.CodeHash).Should().BeTrue();
        _sut.Verify(first.PlainCode, second.Otp.CodeHash).Should().Be(first.PlainCode == second.PlainCode);
    }

    [Fact]
    public void CreateOtp_ShouldSetExpirationTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpCreationResult result = _sut.CreateOtp(userId, purpose);

        // Assert
        result.Otp.ExpiresAt.Should().Be(StartInstant.AddMinutes(UserConstants.OtpExpirationMinutes));
    }

    [Fact]
    public void CreateOtp_WithPasswordResetPurpose_ShouldCreateOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.PasswordReset;

        // Act
        OtpCreationResult result = _sut.CreateOtp(userId, purpose);

        // Assert
        result.Otp.Purpose.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    [Fact]
    public void CreateOtp_ShouldGenerateUniqueIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpCreationResult first = _sut.CreateOtp(userId, purpose);
        OtpCreationResult second = _sut.CreateOtp(userId, purpose);

        // Assert
        first.Otp.Id.Should().NotBe(second.Otp.Id);
    }

    #endregion

    #region CalculateExpirationTime Tests

    [Fact]
    public void CalculateExpirationTime_ShouldReturnFutureTime()
    {
        // Act
        DateTime expirationTime = _sut.CalculateExpirationTime();

        // Assert
        expirationTime.Should().BeAfter(StartInstant);
    }

    [Fact]
    public void CalculateExpirationTime_ShouldReturnCorrectExpiration()
    {
        // Act
        DateTime expirationTime = _sut.CalculateExpirationTime();

        // Assert
        expirationTime.Should().Be(StartInstant.AddMinutes(UserConstants.OtpExpirationMinutes));
    }

    #endregion

    #region Hash and Verify Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithoutAPepper_ShouldThrow(string? pepper)
    {
        // Act
        var construct = () => new OtpService(pepper, TimeProvider.System);

        // Assert
        construct.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Hash_ShouldNotReturnThePlainCode()
    {
        // Act
        string hash = _sut.Hash(TestConstants.Otp.ValidCode);

        // Assert
        hash.Should().StartWith("h1:").And.NotContain(TestConstants.Otp.ValidCode);
    }

    [Fact]
    public void Hash_ShouldFitTheStoredColumn()
    {
        // Act
        string hash = _sut.Hash(TestConstants.Otp.ValidCode);

        // Assert
        hash.Length.Should().BeLessThanOrEqualTo(UserConstants.OtpCodeHashLength);
    }

    [Fact]
    public void Hash_ForTheSameCode_ShouldBeDeterministic()
    {
        // Act
        string first = _sut.Hash(TestConstants.Otp.ValidCode);
        string second = _sut.Hash(TestConstants.Otp.ValidCode);

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void Hash_ForDifferentCodes_ShouldDiffer()
    {
        // Act
        string valid = _sut.Hash(TestConstants.Otp.ValidCode);
        string invalid = _sut.Hash(TestConstants.Otp.InvalidCode);

        // Assert
        valid.Should().NotBe(invalid);
    }

    [Fact]
    public void Verify_WithTheHashedCode_ShouldSucceed()
    {
        // Arrange
        string hash = _sut.Hash(TestConstants.Otp.ValidCode);

        // Act & Assert
        _sut.Verify(TestConstants.Otp.ValidCode, hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithAnotherCode_ShouldFail()
    {
        // Arrange
        string hash = _sut.Hash(TestConstants.Otp.ValidCode);

        // Act & Assert
        _sut.Verify(TestConstants.Otp.InvalidCode, hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_UnderADifferentPepper_ShouldFail()
    {
        // Arrange — a leaked table is worthless without the deployment key
        string hash = _sut.Hash(TestConstants.Otp.ValidCode);
        var otherKey = new OtpService("a-different-pepper", TimeProvider.System);

        // Act & Assert
        otherKey.Verify(TestConstants.Otp.ValidCode, hash).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-scheme-prefix")]
    [InlineData("h1:not-valid-base64!!")]
    public void Verify_WithAnUnusableHash_ShouldFail(string? hash)
    {
        // Act & Assert
        _sut.Verify(TestConstants.Otp.ValidCode, hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_WithATamperedHash_ShouldFail()
    {
        // Arrange
        string hash = _sut.Hash(TestConstants.Otp.ValidCode);
        string tampered = hash[..^2] + (hash.EndsWith("AA", StringComparison.Ordinal) ? "BB" : "AA");

        // Act & Assert
        _sut.Verify(TestConstants.Otp.ValidCode, tampered).Should().BeFalse();
    }

    #endregion
}
