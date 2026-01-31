using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Unit tests for <see cref="PublicVerifyOtpValidator"/>.
/// </summary>
public class PublicVerifyOtpValidatorTests
{
    private readonly PublicVerifyOtpValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
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
        PublicVerifyOtpCommand command = new(
            Email: null!,
            Code: TestConstants.Otp.ValidCode,
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

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
        PublicVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: null!,
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public async Task Validate_WithInvalidCodeLength_ShouldHaveError()
    {
        // Arrange
        PublicVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: "12345",
            Purpose: nameof(EnumOtpPurpose.EmailVerification)
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

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
        PublicVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            Purpose: null!
        );

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
        PublicVerifyOtpCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Code: TestConstants.Otp.ValidCode,
            Purpose: "InvalidPurpose"
        );

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
        PublicVerifyOtpCommand command = new(Email: "invalid", Code: "abc", Purpose: "invalid");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    #endregion
}
