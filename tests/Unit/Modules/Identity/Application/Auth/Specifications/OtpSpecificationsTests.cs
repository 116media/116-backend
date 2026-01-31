using _116.Identity.Application.Auth.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
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
        Guid userId = Guid.NewGuid();
        OtpEntity otp = new OtpBuilder().WithUserId(userId).Build();
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
        OtpEntity otp = new OtpBuilder().WithUserId(Guid.NewGuid()).Build();
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
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = new OtpBuilder().WithPurpose(purpose).Build();
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
        OtpEntity otp = new OtpBuilder().WithPurpose(EnumOtpPurpose.EmailVerification).Build();
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
        OtpEntity otp = new OtpBuilder().WithCode(code).Build();
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
        OtpEntity otp = new OtpBuilder().WithCode("123456").Build();
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
        OtpEntity otp = new OtpBuilder().Build();
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
        OtpEntity otp = new OtpBuilder().AsUsed().Build();
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
        OtpEntity otp = new OtpBuilder().AsUsed().Build();
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
        OtpEntity otp = new OtpBuilder().Build();
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
        OtpEntity otp = new OtpBuilder().WithExpiresAt(DateTime.UtcNow.AddMinutes(10)).Build();
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
        OtpEntity otp = new OtpBuilder().WithExpiresAt(DateTime.UtcNow.AddMinutes(-1)).Build();
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
        OtpEntity otp = new OtpBuilder().WithExpiresAt(DateTime.UtcNow.AddMinutes(-1)).Build();
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
        OtpEntity otp = new OtpBuilder().WithExpiresAt(DateTime.UtcNow.AddMinutes(10)).Build();
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
        Guid userId = Guid.NewGuid();
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = new OtpBuilder()
            .WithUserId(userId)
            .WithPurpose(purpose)
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10))
            .Build();
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
        Guid userId = Guid.NewGuid();
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = new OtpBuilder()
            .WithUserId(userId)
            .WithPurpose(purpose)
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(-1))
            .Build();
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
        Guid userId = Guid.NewGuid();
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = new OtpBuilder()
            .WithUserId(userId)
            .WithPurpose(purpose)
            .AsUsed()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10))
            .Build();
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
        Guid userId = Guid.NewGuid();
        string code = "123456";
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).Build();
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
        Guid userId = Guid.NewGuid();
        string code = "123456";
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed().Build();
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
        Guid userId = Guid.NewGuid();
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;
        OtpEntity otp = new OtpBuilder().WithUserId(userId).WithCode("123456").WithPurpose(purpose).Build();
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
        Guid userId = Guid.NewGuid();
        EnumOtpPurpose purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = new OtpBuilder().WithUserId(userId).WithPurpose(purpose).Build();
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
        Guid userId = Guid.NewGuid();
        EnumOtpPurpose purpose = EnumOtpPurpose.PasswordReset;
        OtpEntity otp = new OtpBuilder().WithUserId(userId).WithPurpose(purpose).AsUsed().Build();
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
        Guid userId = Guid.NewGuid();
        string code = "123456";
        EnumOtpPurpose purpose = EnumOtpPurpose.TwoFactorAuthentication;
        OtpEntity otp = new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).AsUsed().Build();
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
        Guid userId = Guid.NewGuid();
        string code = "123456";
        EnumOtpPurpose purpose = EnumOtpPurpose.TwoFactorAuthentication;
        OtpEntity otp = new OtpBuilder().WithUserId(userId).WithCode(code).WithPurpose(purpose).Build();
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
        Guid userId = Guid.NewGuid();
        List<OtpEntity> otps =
        [
            new OtpBuilder().WithUserId(userId).Build(),
            new OtpBuilder().WithUserId(userId).Build(),
            new OtpBuilder().WithUserId(Guid.NewGuid()).Build(),
        ];

        OtpByUserIdSpecification spec = new(userId);

        // Act
        List<OtpEntity> filtered = otps.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(o => o.UserId == userId).Should().BeTrue();
    }

    [Fact]
    public void OtpIsValidForUserAndPurposeSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        EnumOtpPurpose purpose = EnumOtpPurpose.EmailVerification;

        OtpEntity validOtp = new OtpBuilder()
            .WithUserId(userId)
            .WithPurpose(purpose)
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10))
            .Build();

        OtpEntity expiredOtp = new OtpBuilder()
            .WithUserId(userId)
            .WithPurpose(purpose)
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(-1))
            .Build();

        OtpEntity usedOtp = new OtpBuilder()
            .WithUserId(userId)
            .WithPurpose(purpose)
            .AsUsed()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10))
            .Build();

        List<OtpEntity> otps = [validOtp, expiredOtp, usedOtp];
        OtpIsValidForUserAndPurposeSpecification spec = new(userId, purpose);

        // Act
        List<OtpEntity> filtered = otps.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].UserId.Should().Be(userId);
    }

    #endregion
}
