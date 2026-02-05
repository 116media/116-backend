using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="OtpValidation"/>.
/// </summary>
public class OtpValidationTests
{
    private class TestOtpCodeCommand
    {
        public string OtpCode { get; set; } = string.Empty;
    }

    private class TestOtpPurposeCommand
    {
        public string OtpPurpose { get; set; } = string.Empty;
    }

    private class TestOtpCodeCommandValidator : AbstractValidator<TestOtpCodeCommand>
    {
        public TestOtpCodeCommandValidator()
        {
            RuleFor(x => x.OtpCode).ValidOtpCode();
        }
    }

    private class TestOtpPurposeCommandValidator : AbstractValidator<TestOtpPurposeCommand>
    {
        public TestOtpPurposeCommandValidator()
        {
            RuleFor(x => x.OtpPurpose).ValidOtpPurpose();
        }
    }

    #region ValidOtpCode

    [Fact]
    public void ValidOtpCode_WithValidSixDigitCode_ShouldPass()
    {
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = "123456" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpCode);
    }

    [Fact]
    public void ValidOtpCode_WithAllZeros_ShouldPass()
    {
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = "000000" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpCode);
    }

    [Fact]
    public void ValidOtpCode_WithEmptyCode_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = string.Empty };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpCode).WithErrorMessage("Verification code is required");
    }

    [Fact]
    public void ValidOtpCode_WithEmptyCode_ShouldStopCascading()
    {
        // CascadeMode.Stop: only the NotEmpty error fires, not length/format errors
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = string.Empty };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestOtpCodeCommand.OtpCode));
    }

    [Fact]
    public void ValidOtpCode_WithFiveDigits_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = "12345" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.OtpCode)
            .WithErrorMessage($"Verification code must be exactly {UserConstants.OtpCodeLength} characters long");
    }

    [Fact]
    public void ValidOtpCode_WithSevenDigits_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = "1234567" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.OtpCode)
            .WithErrorMessage($"Verification code must be exactly {UserConstants.OtpCodeLength} characters long");
    }

    [Fact]
    public void ValidOtpCode_WithLetters_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = "abc123" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.OtpCode)
            .WithErrorMessage("Verification code must contain only numbers");
    }

    [Fact]
    public void ValidOtpCode_WithSpecialCharacters_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator();
        var command = new TestOtpCodeCommand { OtpCode = "12!456" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.OtpCode)
            .WithErrorMessage("Verification code must contain only numbers");
    }

    #endregion

    #region ValidOtpPurpose

    [Fact]
    public void ValidOtpPurpose_WithEmailVerification_ShouldPass()
    {
        var validator = new TestOtpPurposeCommandValidator();
        var command = new TestOtpPurposeCommand { OtpPurpose = nameof(EnumOtpPurpose.EmailVerification) };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpPurpose);
    }

    [Fact]
    public void ValidOtpPurpose_WithPasswordReset_ShouldPass()
    {
        var validator = new TestOtpPurposeCommandValidator();
        var command = new TestOtpPurposeCommand { OtpPurpose = nameof(EnumOtpPurpose.PasswordReset) };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpPurpose);
    }

    [Fact]
    public void ValidOtpPurpose_WithEmptyPurpose_ShouldFail()
    {
        var validator = new TestOtpPurposeCommandValidator();
        var command = new TestOtpPurposeCommand { OtpPurpose = string.Empty };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpPurpose).WithErrorMessage("OTP purpose is required.");
    }

    [Fact]
    public void ValidOtpPurpose_WithEmptyPurpose_ShouldStopCascading()
    {
        var validator = new TestOtpPurposeCommandValidator();
        var command = new TestOtpPurposeCommand { OtpPurpose = string.Empty };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestOtpPurposeCommand.OtpPurpose));
    }

    [Fact]
    public void ValidOtpPurpose_WithInvalidPurpose_ShouldFail()
    {
        var validator = new TestOtpPurposeCommandValidator();
        var command = new TestOtpPurposeCommand { OtpPurpose = "InvalidPurpose" };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpPurpose).WithErrorMessage("Invalid OTP purpose specified.");
    }

    [Fact]
    public void ValidOtpPurpose_WithLowercasePurpose_ShouldFail()
    {
        var validator = new TestOtpPurposeCommandValidator();
        var command = new TestOtpPurposeCommand { OtpPurpose = "emailverification" };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpPurpose).WithErrorMessage("Invalid OTP purpose specified.");
    }

    #endregion
}
