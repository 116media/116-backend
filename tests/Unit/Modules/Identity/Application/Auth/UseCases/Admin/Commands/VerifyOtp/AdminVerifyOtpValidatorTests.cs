using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Unit tests for <see cref="AdminVerifyOtpValidator"/>.
/// </summary>
public class AdminVerifyOtpValidatorTests
{
    private readonly AdminVerifyOtpValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_WithNullEmail_ShouldHaveError()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: null!,
            Code: TestConstants.Otp.ValidCode,
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: "notanemail",
            Code: TestConstants.Otp.ValidCode,
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Code Validation Tests

    [Fact]
    public async Task Validate_WithNullCode_ShouldHaveError()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: null!,
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_WithInvalidCodeLength_ShouldHaveError()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "12345",
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage($"Verification code must be exactly {UserConstants.OtpCodeLength} characters long");
    }

    [Fact]
    public async Task Validate_WithNonNumericCode_ShouldHaveError()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "ABC123",
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    #endregion

    #region Purpose Validation Tests

    [Fact]
    public async Task Validate_WithNullPurpose_ShouldHaveError()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            Purpose: null!
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Purpose).WithErrorMessage("OTP purpose is required.");
    }

    [Fact]
    public async Task Validate_WithInvalidPurpose_ShouldHaveError()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            Purpose: "InvalidPurpose"
        );

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Purpose).WithErrorMessage("Invalid OTP purpose specified.");
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminVerifyOtpCommand command = new(Email: "invalid", Code: "abc", Purpose: "invalid");

        // Act
        TestValidationResult<AdminVerifyOtpCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion
}
