using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
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
    private readonly ValidationErrorMessage _enMsg = LocalizerFactory.CreateMessage<ValidationErrorMessage>();

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
        public TestOtpCodeCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.OtpCode).ValidOtpCode(i18n);
        }
    }

    private class TestOtpPurposeCommandValidator : AbstractValidator<TestOtpPurposeCommand>
    {
        public TestOtpPurposeCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.OtpPurpose).ValidOtpPurpose(i18n);
        }
    }

    #region ValidOtpCode

    [Fact]
    public void ValidOtpCode_WithValidSixDigitCode_ShouldPass()
    {
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = "123456" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpCode);
    }

    [Fact]
    public void ValidOtpCode_WithAllZeros_ShouldPass()
    {
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = "000000" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpCode);
    }

    [Fact]
    public void ValidOtpCode_WithEmptyCode_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = string.Empty };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpCode).WithErrorMessage(_enMsg.OtpCodeRequired());
    }

    [Fact]
    public void ValidOtpCode_WithEmptyCode_ShouldStopCascading()
    {
        // CascadeMode.Stop: only the NotEmpty error fires, not length/format errors
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = string.Empty };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestOtpCodeCommand.OtpCode));
    }

    [Fact]
    public void ValidOtpCode_WithFiveDigits_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = "12345" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.OtpCode)
            .WithErrorMessage(_enMsg.OtpCodeWrongLength(UserConstants.OtpCodeLength));
    }

    [Fact]
    public void ValidOtpCode_WithSevenDigits_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = "1234567" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.OtpCode)
            .WithErrorMessage(_enMsg.OtpCodeWrongLength(UserConstants.OtpCodeLength));
    }

    [Fact]
    public void ValidOtpCode_WithLetters_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = "abc123" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpCode).WithErrorMessage(_enMsg.OtpCodeNotNumeric());
    }

    [Fact]
    public void ValidOtpCode_WithSpecialCharacters_ShouldFail()
    {
        var validator = new TestOtpCodeCommandValidator(_enMsg);
        var command = new TestOtpCodeCommand { OtpCode = "12!456" };

        TestValidationResult<TestOtpCodeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpCode).WithErrorMessage(_enMsg.OtpCodeNotNumeric());
    }

    #endregion

    #region ValidOtpPurpose

    [Fact]
    public void ValidOtpPurpose_WithEmailVerification_ShouldPass()
    {
        var validator = new TestOtpPurposeCommandValidator(_enMsg);
        var command = new TestOtpPurposeCommand { OtpPurpose = nameof(EnumOtpPurpose.EmailVerification) };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpPurpose);
    }

    [Fact]
    public void ValidOtpPurpose_WithPasswordReset_ShouldPass()
    {
        var validator = new TestOtpPurposeCommandValidator(_enMsg);
        var command = new TestOtpPurposeCommand { OtpPurpose = nameof(EnumOtpPurpose.PasswordReset) };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtpPurpose);
    }

    [Fact]
    public void ValidOtpPurpose_WithEmptyPurpose_ShouldFail()
    {
        var validator = new TestOtpPurposeCommandValidator(_enMsg);
        var command = new TestOtpPurposeCommand { OtpPurpose = string.Empty };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpPurpose).WithErrorMessage(_enMsg.OtpPurposeRequired());
    }

    [Fact]
    public void ValidOtpPurpose_WithEmptyPurpose_ShouldStopCascading()
    {
        var validator = new TestOtpPurposeCommandValidator(_enMsg);
        var command = new TestOtpPurposeCommand { OtpPurpose = string.Empty };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestOtpPurposeCommand.OtpPurpose));
    }

    [Fact]
    public void ValidOtpPurpose_WithInvalidPurpose_ShouldFail()
    {
        var validator = new TestOtpPurposeCommandValidator(_enMsg);
        var command = new TestOtpPurposeCommand { OtpPurpose = "InvalidPurpose" };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpPurpose).WithErrorMessage(_enMsg.OtpPurposeInvalid());
    }

    [Fact]
    public void ValidOtpPurpose_WithLowercasePurpose_ShouldFail()
    {
        var validator = new TestOtpPurposeCommandValidator(_enMsg);
        var command = new TestOtpPurposeCommand { OtpPurpose = "emailverification" };

        TestValidationResult<TestOtpPurposeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpPurpose).WithErrorMessage(_enMsg.OtpPurposeInvalid());
    }

    #endregion
}
