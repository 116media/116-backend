using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="OtpEntity"/>.
/// </summary>
public class OtpEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateOtp()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string code = TestConstants.Otp.ValidCode;
        var purpose = EnumOtpPurpose.EmailVerification;
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(TestConstants.Otp.ExpirationMinutes);

        // Act
        var otp = OtpEntity.Create(id, userId, code, purpose, expiresAt);

        // Assert
        otp.Id.Should().Be(id);
        otp.UserId.Should().Be(userId);
        otp.Code.Should().Be(code);
        otp.Purpose.Should().Be(purpose);
        otp.ExpiresAt.Should().Be(expiresAt);
        otp.IsUsed.Should().BeFalse();
        otp.UsedAt.Should().BeNull();
        otp.AttemptCount.Should().Be(0);
    }

    [Theory]
    [InlineData(EnumOtpPurpose.EmailVerification)]
    [InlineData(EnumOtpPurpose.PasswordReset)]
    public void Create_WithDifferentPurposes_ShouldSetCorrectPurpose(EnumOtpPurpose purpose)
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string code = TestConstants.Otp.ValidCode;
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var otp = OtpEntity.Create(id, userId, code, purpose, expiresAt);

        // Assert
        otp.Purpose.Should().Be(purpose);
    }

    #endregion

    #region MarkAsUsed Tests

    [Fact]
    public void MarkAsUsed_ShouldSetIsUsedAndUsedAt()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        otp.MarkAsUsed();

        // Assert
        otp.IsUsed.Should().BeTrue();
        otp.UsedAt.Should().NotBeNull();
        otp.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsUsed_WhenAlreadyUsed_ShouldUpdateUsedAt()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateUsed();
        DateTime? originalUsedAt = otp.UsedAt;

        // Act
        otp.MarkAsUsed();

        // Assert
        otp.IsUsed.Should().BeTrue();
        otp.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region IncrementAttemptCount Tests

    [Fact]
    public void IncrementAttemptCount_ShouldIncreaseCountByOne()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();
        int initialCount = otp.AttemptCount;

        // Act
        otp.IncrementAttemptCount();

        // Assert
        otp.AttemptCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void IncrementAttemptCount_MultipleTimes_ShouldAccumulateCorrectly()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        otp.IncrementAttemptCount();
        otp.IncrementAttemptCount();
        otp.IncrementAttemptCount();

        // Assert
        otp.AttemptCount.Should().Be(3);
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_WhenNotUsedNotExpiredAndBelowMaxAttempts_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        bool result = otp.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenUsed_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateUsed();

        // Act
        bool result = otp.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateExpired();

        // Act
        bool result = otp.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenMaxAttemptsReached_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateMaxAttemptsReached();

        // Act
        bool result = otp.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsExpired Tests

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        bool result = otp.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpired_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateExpired();

        // Act
        bool result = otp.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region HasMaxAttemptsReached Tests

    [Fact]
    public void HasMaxAttemptsReached_WhenBelowMax_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        bool result = otp.HasMaxAttemptsReached();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasMaxAttemptsReached_WhenAtMax_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateMaxAttemptsReached();

        // Act
        bool result = otp.HasMaxAttemptsReached();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasMaxAttemptsReached_AfterIncrementingToMax_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act - Increment to max attempts (5 by default)
        for (int i = 0; i < TestConstants.Otp.MaxAttempts; i++)
        {
            otp.IncrementAttemptCount();
        }

        // Assert
        otp.HasMaxAttemptsReached().Should().BeTrue();
    }

    #endregion

    #region OTP Purpose Tests

    [Fact]
    public void Create_ForEmailVerification_ShouldHaveCorrectPurpose()
    {
        // Arrange & Act
        OtpEntity otp = OtpFactory.CreateForEmailVerification(Guid.NewGuid());

        // Assert
        otp.Purpose.Should().Be(EnumOtpPurpose.EmailVerification);
    }

    [Fact]
    public void Create_ForPasswordReset_ShouldHaveCorrectPurpose()
    {
        // Arrange & Act
        OtpEntity otp = OtpFactory.CreateForPasswordReset(Guid.NewGuid());

        // Assert
        otp.Purpose.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    #endregion
}
