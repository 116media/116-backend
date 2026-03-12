using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;
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
    private readonly OtpService _sut;

    public OtpServiceTests()
    {
        _sut = new OtpService();
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
        OtpEntity otp = _sut.CreateOtp(userId, purpose);

        // Assert
        otp.Should().NotBeNull();
        otp.Id.Should().NotBe(Guid.Empty);
        otp.UserId.Should().Be(userId);
        otp.Purpose.Should().Be(purpose);
    }

    [Fact]
    public void CreateOtp_ShouldGenerateValidCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpEntity otp = _sut.CreateOtp(userId, purpose);

        // Assert
        otp.Code.Should().HaveLength(UserConstants.OtpCodeLength);
        otp.Code.Should().MatchRegex(@"^\d+$");
    }

    [Fact]
    public void CreateOtp_ShouldSetExpirationTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;
        DateTime beforeCreation = DateTime.UtcNow;

        // Act
        OtpEntity otp = _sut.CreateOtp(userId, purpose);

        // Assert
        DateTime expectedExpiration = beforeCreation.AddMinutes(UserConstants.OtpExpirationMinutes);
        otp.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void CreateOtp_WithPasswordResetPurpose_ShouldCreateOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.PasswordReset;

        // Act
        OtpEntity otp = _sut.CreateOtp(userId, purpose);

        // Assert
        otp.Purpose.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    [Fact]
    public void CreateOtp_ShouldGenerateUniqueIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        // Act
        OtpEntity otp1 = _sut.CreateOtp(userId, purpose);
        OtpEntity otp2 = _sut.CreateOtp(userId, purpose);

        // Assert
        otp1.Id.Should().NotBe(otp2.Id);
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
