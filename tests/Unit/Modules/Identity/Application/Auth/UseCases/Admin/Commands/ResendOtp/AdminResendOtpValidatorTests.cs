using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;

/// <summary>
/// Unit tests for <see cref="AdminResendOtpValidator"/>.
/// </summary>
public class AdminResendOtpValidatorTests
{
    private readonly AdminResendOtpValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminResendOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

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
        AdminResendOtpCommand command = new(Email: null!, Purpose: nameof(EnumOtpPurpose.EmailVerification));

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        AdminResendOtpCommand command = new(Email: "notanemail", Purpose: nameof(EnumOtpPurpose.EmailVerification));

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Purpose Validation Tests

    [Fact]
    public async Task Validate_WithNullPurpose_ShouldHaveError()
    {
        // Arrange
        AdminResendOtpCommand command = new(Email: TestConstants.User.ValidEmail, Purpose: null!);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Purpose);
    }

    [Fact]
    public async Task Validate_WithInvalidPurpose_ShouldHaveError()
    {
        // Arrange
        AdminResendOtpCommand command = new(Email: TestConstants.User.ValidEmail, Purpose: "InvalidPurpose");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Purpose);
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminResendOtpCommand command = new(Email: "invalid", Purpose: "invalid");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion
}
