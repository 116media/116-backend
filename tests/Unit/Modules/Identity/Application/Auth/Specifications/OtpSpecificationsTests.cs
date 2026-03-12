using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Specifications;

/// <summary>
/// Unit tests for OTP specifications.
/// </summary>
public class OtpSpecificationsTests
{
    #region OtpByUserIdSpecification Tests

    [Fact]
    public void OtpByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        OtpEntity otp = OtpFactory.Create(userId);
        OtpByUserIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create(Guid.NewGuid());
        OtpByUserIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpByPurposeSpecification Tests

    [Fact]
    public void OtpByPurposeSpecification_WithMatchingPurpose_ShouldReturnTrue()
    {
        // Arrange
        var purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.CreateWithPurpose(purpose);
        OtpByPurposeSpecification spec = new(purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpByPurposeSpecification_WithDifferentPurpose_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateWithPurpose(EnumOtpPurpose.EmailVerification);
        OtpByPurposeSpecification spec = new(EnumOtpPurpose.PasswordReset);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpByCodeSpecification Tests

    [Fact]
    public void OtpByCodeSpecification_WithMatchingCode_ShouldReturnTrue()
    {
        // Arrange
        string code = "123456";
        OtpEntity otp = OtpFactory.CreateWithCode(code);
        OtpByCodeSpecification spec = new(code);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpByCodeSpecification_WithDifferentCode_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateWithCode("123456");
        OtpByCodeSpecification spec = new("654321");

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpIsNotUsedSpecification Tests

    [Fact]
    public void OtpIsNotUsedSpecification_WithUnusedOtp_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();
        OtpIsNotUsedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpIsNotUsedSpecification_WithUsedOtp_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateUsed();
        OtpIsNotUsedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpIsUsedSpecification Tests

    [Fact]
    public void OtpIsUsedSpecification_WithUsedOtp_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateUsed();
        OtpIsUsedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpIsUsedSpecification_WithUnusedOtp_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.Create();
        OtpIsUsedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpIsNotExpiredSpecification Tests

    [Fact]
    public void OtpIsNotExpiredSpecification_WithNonExpiredOtp_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateWithExpiresAt(DateTime.UtcNow.AddMinutes(10));
        OtpIsNotExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpIsNotExpiredSpecification_WithExpiredOtp_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateWithExpiresAt(DateTime.UtcNow.AddMinutes(-1));
        OtpIsNotExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpIsExpiredSpecification Tests

    [Fact]
    public void OtpIsExpiredSpecification_WithExpiredOtp_ShouldReturnTrue()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateWithExpiresAt(DateTime.UtcNow.AddMinutes(-1));
        OtpIsExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpIsExpiredSpecification_WithNonExpiredOtp_ShouldReturnFalse()
    {
        // Arrange
        OtpEntity otp = OtpFactory.CreateWithExpiresAt(DateTime.UtcNow.AddMinutes(10));
        OtpIsExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpIsValidForUserAndPurposeSpecification Tests

    [Fact]
    public void OtpIsValidForUserAndPurposeSpecification_WithValidOtp_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.Create(userId, purpose);
        OtpIsValidForUserAndPurposeSpecification spec = new(userId, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpIsValidForUserAndPurposeSpecification_WithExpiredOtp_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.CreateExpired(userId, purpose);
        OtpIsValidForUserAndPurposeSpecification spec = new(userId, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void OtpIsValidForUserAndPurposeSpecification_WithUsedOtp_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.CreateUsed(userId, purpose);
        OtpIsValidForUserAndPurposeSpecification spec = new(userId, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpForValidationSpecification Tests

    [Fact]
    public void OtpForValidationSpecification_WithValidOtp_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.Create(userId, code, purpose);
        OtpForValidationSpecification spec = new(userId, code, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpForValidationSpecification_WithUsedOtp_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string code = "123456";
        var purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.Create(userId, code, purpose);
        otp.MarkAsUsed();
        OtpForValidationSpecification spec = new(userId, code, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void OtpForValidationSpecification_WithWrongCode_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = OtpFactory.Create(userId, "123456", purpose);
        OtpForValidationSpecification spec = new(userId, "654321", purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpForInvalidationSpecification Tests

    [Fact]
    public void OtpForInvalidationSpecification_WithMatchingOtp_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);
        OtpForInvalidationSpecification spec = new(userId, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpForInvalidationSpecification_WithUsedOtp_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = OtpFactory.CreateForPasswordReset(userId);
        otp.MarkAsUsed();
        OtpForInvalidationSpecification spec = new(userId, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OtpForUsedValidationSpecification Tests

    [Fact]
    public void OtpForUsedValidationSpecification_WithUsedOtp_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string code = "123456";
        var purpose = EnumOtpPurpose.TwoFactorAuthentication;
        OtpEntity otp = OtpFactory.Create(userId, code, purpose);
        otp.MarkAsUsed();
        OtpForUsedValidationSpecification spec = new(userId, code, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void OtpForUsedValidationSpecification_WithUnusedOtp_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string code = "123456";
        var purpose = EnumOtpPurpose.TwoFactorAuthentication;
        OtpEntity otp = OtpFactory.Create(userId, code, purpose);
        OtpForUsedValidationSpecification spec = new(userId, code, purpose);

        // Act
        bool result = spec.IsSatisfiedBy(otp);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void OtpByUserIdSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        List<OtpEntity> otps =
        [
            OtpFactory.Create(userId),
            OtpFactory.Create(userId),
            OtpFactory.Create(Guid.NewGuid()),
        ];

        OtpByUserIdSpecification spec = new(userId);

        // Act
        List<OtpEntity> filtered = otps.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(o => o.UserId == userId);
    }

    [Fact]
    public void OtpIsValidForUserAndPurposeSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var purpose = EnumOtpPurpose.EmailVerification;

        OtpEntity validOtp = OtpFactory.Create(userId, purpose);

        OtpEntity expiredOtp = OtpFactory.CreateExpired(userId, purpose);

        OtpEntity usedOtp = OtpFactory.CreateUsed(userId, purpose);

        List<OtpEntity> otps = [validOtp, expiredOtp, usedOtp];
        OtpIsValidForUserAndPurposeSpecification spec = new(userId, purpose);

        // Act
        List<OtpEntity> filtered = otps.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].UserId.Should().Be(userId);
    }

    #endregion
}
