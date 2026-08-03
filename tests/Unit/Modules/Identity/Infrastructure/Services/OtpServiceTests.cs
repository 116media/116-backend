using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="OtpService"/>.
/// </summary>
public class OtpServiceTests
{
    private readonly PasswordService _passwordService;
    private readonly OtpService _sut;

    public OtpServiceTests()
    {
        // The real hashing service: it has no dependencies, and the assertions below are about
        // the hash the service actually stores.
        _passwordService = new PasswordService();
        _sut = new OtpService(_passwordService);
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
        result.Otp.CodeHash.Should().StartWith("v1:");
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
        _passwordService.Verify(result.PlainCode, result.Otp.CodeHash).Should().BeTrue();
        _passwordService
            .Verify("000000" == result.PlainCode ? "111111" : "000000", result.Otp.CodeHash)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void CreateOtp_CalledTwice_ShouldProduceDifferentHashes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act - the per-hash salt means even an identical code never yields an identical hash
        OtpCreationResult first = _sut.CreateOtp(userId, purpose);
        OtpCreationResult second = _sut.CreateOtp(userId, purpose);

        // Assert
        first.Otp.CodeHash.Should().NotBe(second.Otp.CodeHash);
    }

    [Fact]
    public void CreateOtp_ShouldSetExpirationTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        OtpCreationResult result = _sut.CreateOtp(userId, purpose);

        // Assert
        DateTime expectedExpiration = beforeCreation.AddMinutes(UserConstants.OtpExpirationMinutes);
        result.Otp.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(2));
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
        // Arrange
        DateTime now = DateTime.UtcNow;

        // Act
        DateTime expirationTime = _sut.CalculateExpirationTime();

        // Assert
        expirationTime.Should().BeAfter(now);
    }

    [Fact]
    public void CalculateExpirationTime_ShouldReturnCorrectExpiration()
    {
        // Arrange
        DateTime beforeCall = DateTime.UtcNow;

        // Act
        DateTime expirationTime = _sut.CalculateExpirationTime();

        // Assert
        DateTime expectedExpiration = beforeCall.AddMinutes(UserConstants.OtpExpirationMinutes);
        expirationTime.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void CalculateExpirationTime_CalledMultipleTimes_ShouldReturnIncreasingTimes()
    {
        // Act
        DateTime time1 = _sut.CalculateExpirationTime();
        Thread.Sleep(10); // Small delay
        DateTime time2 = _sut.CalculateExpirationTime();

        // Assert
        time2.Should().BeOnOrAfter(time1);
    }

    #endregion
}
