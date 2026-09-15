using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
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
        string codeHash = new PasswordService().Hash(TestConstants.Otp.ValidCode);
        var purpose = EnumOtpPurpose.EmailVerification;
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(TestConstants.Otp.ExpirationMinutes);

        // Act
        var otp = OtpEntity.Create(id, userId, codeHash, purpose, expiresAt);

        // Assert
        otp.Id.Should().Be(id);
        otp.UserId.Should().Be(userId);
        otp.CodeHash.Should().Be(codeHash);
        otp.CodeHash.Should().NotBe(TestConstants.Otp.ValidCode);
        otp.Purpose.Value.Should().Be(purpose);
        otp.ExpiresAt.Should().Be(expiresAt);
        otp.IsUsed.Should().BeFalse();
        otp.UsedAt.Should().BeNull();
        otp.ConsumedAt.Should().BeNull();
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
        string codeHash = new PasswordService().Hash(TestConstants.Otp.ValidCode);
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var otp = OtpEntity.Create(id, userId, codeHash, purpose, expiresAt);

        // Assert
        otp.Purpose.Value.Should().Be(purpose);
    }

    #endregion

    #region MarkAsUsed Tests

    [Fact]
    public void MarkAsUsed_ShouldSetIsUsedAndUsedAt()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        otp.MarkAsUsed(now: DateTime.UtcNow);

        // Assert
        otp.IsUsed.Should().BeTrue();
        otp.UsedAt.Should().NotBeNull();
        otp.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsUsed_WhenAlreadyUsed_ShouldReportFalseAndKeepTheFirstStamp()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateUsed();
        DateTime? originalUsedAt = otp.UsedAt;

        // Act
        bool transitioned = otp.MarkAsUsed(now: DateTime.UtcNow.AddMinutes(5));

        // Assert
        transitioned.Should().BeFalse();
        otp.IsUsed.Should().BeTrue();
        otp.UsedAt.Should().Be(originalUsedAt);
    }

    #endregion

    #region MarkAsConsumed Tests

    [Fact]
    public void MarkAsConsumed_ShouldSetConsumedAt()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        otp.MarkAsConsumed(now: DateTime.UtcNow);

        // Assert
        otp.ConsumedAt.Should().NotBeNull();
        otp.ConsumedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task MarkAsConsumed_WhenAlreadyConsumed_ShouldKeepTheFirstTimestamp()
    {
        // Arrange
        OtpEntity otp = new OtpBuilder().AsConsumed().Build();
        DateTime? firstConsumedAt = otp.ConsumedAt;

        // The clock has to advance, otherwise a second stamp would be indistinguishable from the first.
        await Task.Delay(TimeSpan.FromMilliseconds(20));

        // Act
        otp.MarkAsConsumed(now: DateTime.UtcNow);

        // Assert
        otp.ConsumedAt.Should().Be(firstConsumedAt);
    }

    [Fact]
    public void MarkAsConsumed_ShouldNotReportTheOtpAsVerifiedByItsOwner()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        otp.MarkAsConsumed(now: DateTime.UtcNow);

        // Assert
        otp.IsUsed.Should().BeFalse();
        otp.UsedAt.Should().BeNull();
    }

    #endregion

    #region ConsumedAt Tests

    [Fact]
    public void ConsumedAt_WhenNotConsumed_ShouldBeNull()
    {
        // Arrange & Act
        OtpEntity otp = OtpFactory.Create();

        // Assert
        otp.ConsumedAt.Should().BeNull();
    }

    [Fact]
    public void ConsumedAt_WhenConsumed_ShouldBeStamped()
    {
        // Arrange & Act
        OtpEntity otp = new OtpBuilder().AsConsumed().Build();

        // Assert
        otp.ConsumedAt.Should().NotBeNull();
    }

    [Fact]
    public void ConsumedAt_WhenOnlyMarkedAsUsed_ShouldStayNull()
    {
        // Arrange & Act — consumption is a separate terminal state from verification
        OtpEntity otp = OtpFactory.CreateUsed();

        // Assert
        otp.ConsumedAt.Should().BeNull();
    }

    #endregion

    #region Verify Tests

    [Fact]
    public void Verify_WithAMatchingCode_ShouldReportValidWithoutConsumingAnAttempt()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        EnumOtpVerificationStatus status = otp.Verify(suppliedCodeMatches: true, now: DateTime.UtcNow);

        // Assert
        status.Should().Be(EnumOtpVerificationStatus.Valid);
        otp.AttemptCount.Should().Be(0);
    }

    [Fact]
    public void Verify_WithAWrongCode_ShouldReportMismatchAndConsumeAnAttempt()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        EnumOtpVerificationStatus status = otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);

        // Assert
        status.Should().Be(EnumOtpVerificationStatus.Mismatch);
        otp.AttemptCount.Should().Be(1);
    }

    [Fact]
    public void Verify_WhenExpired_ShouldReportExpiredWithoutJudgingTheCode()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateExpired();

        // Act
        EnumOtpVerificationStatus status = otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);

        // Assert
        status.Should().Be(EnumOtpVerificationStatus.Expired);
        otp.AttemptCount.Should().Be(0);
    }

    [Fact]
    public void Verify_WhenExpiredWithACorrectCode_ShouldStillReportExpired()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateExpired();

        // Act
        EnumOtpVerificationStatus status = otp.Verify(suppliedCodeMatches: true, now: DateTime.UtcNow);

        // Assert
        status.Should().Be(EnumOtpVerificationStatus.Expired);
    }

    [Fact]
    public void Verify_WhenAttemptsAlreadyExhausted_ShouldReportExhaustedWithoutConsumingMore()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateMaxAttemptsReached();
        int attemptsBefore = otp.AttemptCount;

        // Act
        EnumOtpVerificationStatus status = otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);

        // Assert
        status.Should().Be(EnumOtpVerificationStatus.AttemptsExhausted);
        otp.AttemptCount.Should().Be(attemptsBefore);
    }

    [Fact]
    public void Verify_WhenAWrongCodeConsumesTheLastAttempt_ShouldReportExhausted()
    {
        // Arrange
        OtpEntity otp = new OtpBuilder().WithAttemptCount(TestConstants.Otp.MaxAttempts - 1).Build();

        // Act
        EnumOtpVerificationStatus status = otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);

        // Assert
        status.Should().Be(EnumOtpVerificationStatus.AttemptsExhausted);
        otp.AttemptCount.Should().Be(TestConstants.Otp.MaxAttempts);
    }

    [Fact]
    public void Verify_WithWrongCodes_ShouldAccumulateAttempts()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);
        otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);

        // Assert
        otp.AttemptCount.Should().Be(2);
    }

    #endregion

    #region IsExpired Tests

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();

        // Act
        bool result = otp.IsExpired(now: DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpired_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateExpired();

        // Act
        bool result = otp.IsExpired(now: DateTime.UtcNow);

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
    public void HasMaxAttemptsReached_OneAttemptBelowTheThreshold_ShouldReturnFalse()
    {
        OtpEntity otp = OtpFactory.Create();

        for (int i = 0; i < TestConstants.Otp.MaxAttempts - 1; i++)
        {
            otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);
        }

        otp.HasMaxAttemptsReached().Should().BeFalse();
    }

    [Fact]
    public void HasMaxAttemptsReached_AtTheThreshold_ShouldReturnTrue()
    {
        OtpEntity otp = OtpFactory.Create();

        for (int i = 0; i < TestConstants.Otp.MaxAttempts; i++)
        {
            otp.Verify(suppliedCodeMatches: false, now: DateTime.UtcNow);
        }

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
        otp.Purpose.Value.Should().Be(EnumOtpPurpose.EmailVerification);
    }

    [Fact]
    public void Create_ForPasswordReset_ShouldHaveCorrectPurpose()
    {
        // Arrange & Act
        OtpEntity otp = OtpFactory.CreateForPasswordReset(Guid.NewGuid());

        // Assert
        otp.Purpose.Value.Should().Be(EnumOtpPurpose.PasswordReset);
    }

    #endregion
}
