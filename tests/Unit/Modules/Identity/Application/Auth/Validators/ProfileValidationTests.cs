using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="ProfileValidation"/>.
/// </summary>
public class ProfileValidationTests
{
    private readonly ValidationErrorMessage _enMsg = LocalizerFactory.CreateMessage<ValidationErrorMessage>();

    // Property names must match what the When() conditions check via reflection
    private class TestCountryNameCommand
    {
        public string? CountryName { get; set; }
    }

    private class TestCountryIsoCodeCommand
    {
        public string? CountryIsoCode { get; set; }
    }

    private class TestCountryDialCodeCommand
    {
        public string? CountryDialCode { get; set; }
    }

    private class TestPartialPhoneNumberCommand
    {
        public string? PartialPhoneNumber { get; set; }
    }

    private class TestCountryNameCommandValidator : AbstractValidator<TestCountryNameCommand>
    {
        public TestCountryNameCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.CountryName).ValidCountryName(i18n);
        }
    }

    private class TestCountryIsoCodeCommandValidator : AbstractValidator<TestCountryIsoCodeCommand>
    {
        public TestCountryIsoCodeCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.CountryIsoCode).ValidCountryIsoCode(i18n);
        }
    }

    private class TestCountryDialCodeCommandValidator : AbstractValidator<TestCountryDialCodeCommand>
    {
        public TestCountryDialCodeCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.CountryDialCode).ValidCountryDialCode(i18n);
        }
    }

    private class TestPartialPhoneNumberCommandValidator : AbstractValidator<TestPartialPhoneNumberCommand>
    {
        public TestPartialPhoneNumberCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.PartialPhoneNumber).ValidPartialPhoneNumber(i18n);
        }
    }

    #region ValidCountryName

    [Fact]
    public void ValidCountryName_WithValidName_ShouldPass()
    {
        var validator = new TestCountryNameCommandValidator(_enMsg);
        var command = new TestCountryNameCommand { CountryName = "United States" };

        TestValidationResult<TestCountryNameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryName);
    }

    [Fact]
    public void ValidCountryName_WithMaxLengthName_ShouldPass()
    {
        var validator = new TestCountryNameCommandValidator(_enMsg);
        var command = new TestCountryNameCommand { CountryName = new string('a', UserConstants.MaxCountryNameLength) };

        TestValidationResult<TestCountryNameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryName);
    }

    [Fact]
    public void ValidCountryName_WithNullName_ShouldPass()
    {
        // When condition: skipped when null
        var validator = new TestCountryNameCommandValidator(_enMsg);
        var command = new TestCountryNameCommand { CountryName = null };

        TestValidationResult<TestCountryNameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryName);
    }

    [Fact]
    public void ValidCountryName_WithEmptyName_ShouldPass()
    {
        // When condition: skipped when empty
        var validator = new TestCountryNameCommandValidator(_enMsg);
        var command = new TestCountryNameCommand { CountryName = string.Empty };

        TestValidationResult<TestCountryNameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryName);
    }

    [Fact]
    public void ValidCountryName_WithWhitespaceName_ShouldPass()
    {
        // When condition: skipped when whitespace
        var validator = new TestCountryNameCommandValidator(_enMsg);
        var command = new TestCountryNameCommand { CountryName = "   " };

        TestValidationResult<TestCountryNameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryName);
    }

    [Fact]
    public void ValidCountryName_WithNameExceedingMaxLength_ShouldFail()
    {
        var validator = new TestCountryNameCommandValidator(_enMsg);
        var command = new TestCountryNameCommand
        {
            CountryName = new string('a', UserConstants.MaxCountryNameLength + 1),
        };

        TestValidationResult<TestCountryNameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.CountryName)
            .WithErrorMessage(_enMsg.CountryNameTooLong(UserConstants.MaxCountryNameLength));
    }

    #endregion

    #region ValidCountryIsoCode

    [Fact]
    public void ValidCountryIsoCode_WithTwoLetterCode_ShouldPass()
    {
        var validator = new TestCountryIsoCodeCommandValidator(_enMsg);
        var command = new TestCountryIsoCodeCommand { CountryIsoCode = "US" };

        TestValidationResult<TestCountryIsoCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryIsoCode);
    }

    [Fact]
    public void ValidCountryIsoCode_WithThreeLetterCode_ShouldPass()
    {
        var validator = new TestCountryIsoCodeCommandValidator(_enMsg);
        var command = new TestCountryIsoCodeCommand { CountryIsoCode = "USA" };

        TestValidationResult<TestCountryIsoCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryIsoCode);
    }

    [Fact]
    public void ValidCountryIsoCode_WithNullCode_ShouldPass()
    {
        var validator = new TestCountryIsoCodeCommandValidator(_enMsg);
        var command = new TestCountryIsoCodeCommand { CountryIsoCode = null };

        TestValidationResult<TestCountryIsoCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryIsoCode);
    }

    [Fact]
    public void ValidCountryIsoCode_WithEmptyCode_ShouldPass()
    {
        var validator = new TestCountryIsoCodeCommandValidator(_enMsg);
        var command = new TestCountryIsoCodeCommand { CountryIsoCode = string.Empty };

        TestValidationResult<TestCountryIsoCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryIsoCode);
    }

    [Fact]
    public void ValidCountryIsoCode_WithLowercaseCode_ShouldFail()
    {
        var validator = new TestCountryIsoCodeCommandValidator(_enMsg);
        var command = new TestCountryIsoCodeCommand { CountryIsoCode = "us" };

        TestValidationResult<TestCountryIsoCodeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CountryIsoCode).WithErrorMessage(_enMsg.CountryIsoCodeInvalid());
    }

    [Fact]
    public void ValidCountryIsoCode_WithCodeExceedingMaxLength_ShouldFail()
    {
        var validator = new TestCountryIsoCodeCommandValidator(_enMsg);
        var command = new TestCountryIsoCodeCommand { CountryIsoCode = "USAA" };

        TestValidationResult<TestCountryIsoCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.CountryIsoCode)
            .WithErrorMessage(_enMsg.CountryIsoCodeTooLong(UserConstants.MaxCountryIsoCodeLength));
    }

    [Fact]
    public void ValidCountryIsoCode_WithSingleLetter_ShouldFail()
    {
        var validator = new TestCountryIsoCodeCommandValidator(_enMsg);
        var command = new TestCountryIsoCodeCommand { CountryIsoCode = "U" };

        TestValidationResult<TestCountryIsoCodeCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CountryIsoCode).WithErrorMessage(_enMsg.CountryIsoCodeInvalid());
    }

    #endregion

    #region ValidCountryDialCode

    [Fact]
    public void ValidCountryDialCode_WithValidSingleDigitCode_ShouldPass()
    {
        var validator = new TestCountryDialCodeCommandValidator(_enMsg);
        var command = new TestCountryDialCodeCommand { CountryDialCode = "+1" };

        TestValidationResult<TestCountryDialCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public void ValidCountryDialCode_WithValidMultiDigitCode_ShouldPass()
    {
        var validator = new TestCountryDialCodeCommandValidator(_enMsg);
        var command = new TestCountryDialCodeCommand { CountryDialCode = "+255" };

        TestValidationResult<TestCountryDialCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public void ValidCountryDialCode_WithNullCode_ShouldPass()
    {
        var validator = new TestCountryDialCodeCommandValidator(_enMsg);
        var command = new TestCountryDialCodeCommand { CountryDialCode = null };

        TestValidationResult<TestCountryDialCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public void ValidCountryDialCode_WithEmptyCode_ShouldPass()
    {
        var validator = new TestCountryDialCodeCommandValidator(_enMsg);
        var command = new TestCountryDialCodeCommand { CountryDialCode = string.Empty };

        TestValidationResult<TestCountryDialCodeCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public void ValidCountryDialCode_WithoutPlusPrefix_ShouldFail()
    {
        var validator = new TestCountryDialCodeCommandValidator(_enMsg);
        var command = new TestCountryDialCodeCommand { CountryDialCode = "255" };

        TestValidationResult<TestCountryDialCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.CountryDialCode)
            .WithErrorMessage(
                $"Country dial code must start with + followed by 1-{UserConstants.MaxCountryDialCodeLength} digits."
            );
    }

    [Fact]
    public void ValidCountryDialCode_WithLettersAfterPlus_ShouldFail()
    {
        var validator = new TestCountryDialCodeCommandValidator(_enMsg);
        var command = new TestCountryDialCodeCommand { CountryDialCode = "+ABC" };

        TestValidationResult<TestCountryDialCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.CountryDialCode)
            .WithErrorMessage(
                $"Country dial code must start with + followed by 1-{UserConstants.MaxCountryDialCodeLength} digits."
            );
    }

    [Fact]
    public void ValidCountryDialCode_WithPlusOnly_ShouldFail()
    {
        var validator = new TestCountryDialCodeCommandValidator(_enMsg);
        var command = new TestCountryDialCodeCommand { CountryDialCode = "+" };

        TestValidationResult<TestCountryDialCodeCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.CountryDialCode)
            .WithErrorMessage(
                $"Country dial code must start with + followed by 1-{UserConstants.MaxCountryDialCodeLength} digits."
            );
    }

    #endregion

    #region ValidPartialPhoneNumber

    [Fact]
    public void ValidPartialPhoneNumber_WithValidNumber_ShouldPass()
    {
        var validator = new TestPartialPhoneNumberCommandValidator(_enMsg);
        var command = new TestPartialPhoneNumberCommand { PartialPhoneNumber = "712345678" };

        TestValidationResult<TestPartialPhoneNumberCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PartialPhoneNumber);
    }

    [Fact]
    public void ValidPartialPhoneNumber_WithMaxLengthNumber_ShouldPass()
    {
        var validator = new TestPartialPhoneNumberCommandValidator(_enMsg);
        var command = new TestPartialPhoneNumberCommand
        {
            PartialPhoneNumber = new string('1', UserConstants.MaxPartialPhoneNumberLength),
        };

        TestValidationResult<TestPartialPhoneNumberCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PartialPhoneNumber);
    }

    [Fact]
    public void ValidPartialPhoneNumber_WithNullNumber_ShouldPass()
    {
        var validator = new TestPartialPhoneNumberCommandValidator(_enMsg);
        var command = new TestPartialPhoneNumberCommand { PartialPhoneNumber = null };

        TestValidationResult<TestPartialPhoneNumberCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PartialPhoneNumber);
    }

    [Fact]
    public void ValidPartialPhoneNumber_WithEmptyNumber_ShouldPass()
    {
        var validator = new TestPartialPhoneNumberCommandValidator(_enMsg);
        var command = new TestPartialPhoneNumberCommand { PartialPhoneNumber = string.Empty };

        TestValidationResult<TestPartialPhoneNumberCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PartialPhoneNumber);
    }

    [Fact]
    public void ValidPartialPhoneNumber_WithWhitespaceNumber_ShouldPass()
    {
        var validator = new TestPartialPhoneNumberCommandValidator(_enMsg);
        var command = new TestPartialPhoneNumberCommand { PartialPhoneNumber = "   " };

        TestValidationResult<TestPartialPhoneNumberCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PartialPhoneNumber);
    }

    [Fact]
    public void ValidPartialPhoneNumber_WithNumberExceedingMaxLength_ShouldFail()
    {
        var validator = new TestPartialPhoneNumberCommandValidator(_enMsg);
        var command = new TestPartialPhoneNumberCommand
        {
            PartialPhoneNumber = new string('1', UserConstants.MaxPartialPhoneNumberLength + 1),
        };

        TestValidationResult<TestPartialPhoneNumberCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.PartialPhoneNumber)
            .WithErrorMessage(
                $"Partial phone number cannot exceed {UserConstants.MaxPartialPhoneNumberLength} characters."
            );
    }

    #endregion
}
