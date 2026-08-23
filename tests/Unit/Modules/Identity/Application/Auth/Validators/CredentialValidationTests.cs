using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="CredentialValidation"/>.
/// </summary>
public class CredentialValidationTests
{
    private readonly ValidationErrorMessage _enMsg = LocalizerFactory.CreateMessage<ValidationErrorMessage>();

    // Property names must match what the optional When() conditions check via reflection
    private class TestEmailCommand
    {
        public string? Email { get; set; }
    }

    private class TestPasswordCommand
    {
        public string Password { get; set; } = string.Empty;
    }

    private class TestUsernameCommand
    {
        public string? UserName { get; set; }
    }

    private class TestOldPasswordCommand
    {
        public string? OldPassword { get; set; }
    }

    private class TestCredentialsCommand
    {
        public string Credentials { get; set; } = string.Empty;
    }

    private class TestEmailCommandValidator : AbstractValidator<TestEmailCommand>
    {
        public TestEmailCommandValidator(ValidationErrorMessage i18n, bool isRequired = true)
        {
            RuleFor(x => x.Email).ValidEmail(i18n, isRequired: isRequired);
        }
    }

    private class TestPasswordCommandValidator : AbstractValidator<TestPasswordCommand>
    {
        public TestPasswordCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.Password).ValidPassword(i18n);
        }
    }

    private class TestUsernameCommandValidator : AbstractValidator<TestUsernameCommand>
    {
        public TestUsernameCommandValidator(ValidationErrorMessage i18n, bool isRequired = true)
        {
            RuleFor(x => x.UserName).ValidUsername(i18n, isRequired: isRequired);
        }
    }

    private class TestPasswordNotStrongCommandValidator : AbstractValidator<TestPasswordCommand>
    {
        public TestPasswordNotStrongCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.Password).ValidPassword(i18n, isStrong: false);
        }
    }

    private class TestCredentialsCommandValidator : AbstractValidator<TestCredentialsCommand>
    {
        public TestCredentialsCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.Credentials).ValidCredentials(i18n);
        }
    }

    private class TestOldPasswordCommandValidator : AbstractValidator<TestOldPasswordCommand>
    {
        public TestOldPasswordCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.OldPassword).ValidOldPassword(i18n);
        }
    }

    #region ValidEmail — required (default)

    [Fact]
    public void ValidEmail_WithValidEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = "user@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_WithMaxLengthEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        // Build a valid email at exactly MaxEmailLength (254) chars
        string local = new('a', UserConstants.MaxEmailLength - "@example.com".Length);
        var command = new TestEmailCommand { Email = $"{local}@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_WithNullEmail_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = null };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.EmailRequired());
    }

    [Fact]
    public void ValidEmail_WithEmptyEmail_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = string.Empty };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.EmailRequired());
    }

    [Fact]
    public void ValidEmail_WithWhitespaceEmail_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = "   " };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.EmailRequired());
    }

    [Fact]
    public void ValidEmail_WithEmailExceedingMaxLength_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        string local = new('a', UserConstants.MaxEmailLength);
        var command = new TestEmailCommand { Email = $"{local}@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(_enMsg.EmailTooLong(UserConstants.MaxEmailLength));
    }

    [Fact]
    public void ValidEmail_WithInvalidFormat_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = "not-an-email" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.InvalidEmailFormatMsg());
    }

    [Fact]
    public void ValidEmail_WithMissingAtSign_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = "userexample.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.InvalidEmailFormatMsg());
    }

    #endregion

    #region ValidEmail — optional (isRequired = false)

    [Fact]
    public void ValidEmail_Optional_WithValidEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(_enMsg, isRequired: false);
        var command = new TestEmailCommand { Email = "user@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithNullEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(_enMsg, isRequired: false);
        var command = new TestEmailCommand { Email = null };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithEmptyEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(_enMsg, isRequired: false);
        var command = new TestEmailCommand { Email = string.Empty };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithWhitespaceEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(_enMsg, isRequired: false);
        var command = new TestEmailCommand { Email = "   " };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithInvalidFormat_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(_enMsg, isRequired: false);
        var command = new TestEmailCommand { Email = "not-an-email" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_enMsg.InvalidEmailFormatMsg());
    }

    #endregion

    #region ValidPassword

    [Fact]
    public void ValidPassword_WithValidPassword_ShouldPass()
    {
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "SecureP@ss1" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_WithMinLengthPassword_ShouldPass()
    {
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "Abc1de" }; // 6 chars

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_WithEmptyPassword_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = string.Empty };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_enMsg.PasswordRequired());
    }

    [Fact]
    public void ValidPassword_WithEmptyPassword_ShouldStopCascading()
    {
        // CascadeMode.Stop: only the NotEmpty error fires
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = string.Empty };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestPasswordCommand.Password));
    }

    [Fact]
    public void ValidPassword_WithTooShortPassword_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "Ab1" }; // too short

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(_enMsg.PasswordTooShort("Password", UserConstants.MinPasswordLength));
    }

    [Fact]
    public void ValidPassword_WithNoUppercase_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "secure1pass" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_enMsg.PasswordComplexity("Password"));
    }

    [Fact]
    public void ValidPassword_WithNoLowercase_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "SECURE1PASS" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_enMsg.PasswordComplexity("Password"));
    }

    [Fact]
    public void ValidPassword_WithNoNumber_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "SecurePass" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_enMsg.PasswordComplexity("Password"));
    }

    #endregion

    #region ValidUsername — required (default)

    [Fact]
    public void ValidUsername_WithValidUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = "john-doe" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_WithMaxLengthUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = new string('a', UserConstants.MaxUserNameLength) };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_WithNullUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = null };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage(_enMsg.UsernameRequired());
    }

    [Fact]
    public void ValidUsername_WithEmptyUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = string.Empty };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage(_enMsg.UsernameRequired());
    }

    [Fact]
    public void ValidUsername_WithTooShortUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = "ab" }; // below MinUserNameLength

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(_enMsg.UsernameTooShort(UserConstants.MinUserNameLength));
    }

    [Fact]
    public void ValidUsername_WithTooLongUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = new string('a', UserConstants.MaxUserNameLength + 1) };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(_enMsg.UsernameTooLong(UserConstants.MaxUserNameLength));
    }

    [Fact]
    public void ValidUsername_WithSpecialCharacters_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = "john_doe!" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage(_enMsg.UsernameInvalidChars());
    }

    [Fact]
    public void ValidUsername_WithHyphenAndSpaces_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = "John Doe" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region ValidUsername — optional (isRequired = false)

    [Fact]
    public void ValidUsername_Optional_WithValidUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(_enMsg, isRequired: false);
        var command = new TestUsernameCommand { UserName = "john-doe" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithNullUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(_enMsg, isRequired: false);
        var command = new TestUsernameCommand { UserName = null };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithEmptyUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(_enMsg, isRequired: false);
        var command = new TestUsernameCommand { UserName = string.Empty };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithWhitespaceUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(_enMsg, isRequired: false);
        var command = new TestUsernameCommand { UserName = "   " };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithTooLongUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator(_enMsg, isRequired: false);
        var command = new TestUsernameCommand { UserName = new string('a', UserConstants.MaxUserNameLength + 1) };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(_enMsg.UsernameTooLong(UserConstants.MaxUserNameLength));
    }

    #endregion

    #region ValidPassword — not strong (isStrong = false)

    [Fact]
    public void ValidPassword_NotStrong_WithValidPassword_ShouldPass()
    {
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "anypassword" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_NotStrong_WithShortPassword_ShouldPass()
    {
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "ab" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_NotStrong_WithNoUppercase_ShouldPass()
    {
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = "nouppercase123" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_NotStrong_WithEmptyPassword_ShouldFail()
    {
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = string.Empty };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_enMsg.PasswordRequired());
    }

    [Fact]
    public void ValidPassword_NotStrong_WithEmptyPassword_ShouldStopCascading()
    {
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = string.Empty };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestPasswordCommand.Password));
    }

    #endregion

    #region ValidCredentials

    [Fact]
    public void ValidCredentials_WithValidEmail_ShouldPass()
    {
        var validator = new TestCredentialsCommandValidator(_enMsg);
        var command = new TestCredentialsCommand { Credentials = "user@example.com" };

        TestValidationResult<TestCredentialsCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Credentials);
    }

    [Fact]
    public void ValidCredentials_WithValidUsername_ShouldPass()
    {
        var validator = new TestCredentialsCommandValidator(_enMsg);
        var command = new TestCredentialsCommand { Credentials = "john-doe" };

        TestValidationResult<TestCredentialsCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Credentials);
    }

    [Fact]
    public void ValidCredentials_WithEmptyCredentials_ShouldFail()
    {
        var validator = new TestCredentialsCommandValidator(_enMsg);
        var command = new TestCredentialsCommand { Credentials = string.Empty };

        TestValidationResult<TestCredentialsCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Credentials).WithErrorMessage(_enMsg.EmailOrUsernameRequired());
    }

    [Fact]
    public void ValidCredentials_WithNullCredentials_ShouldFail()
    {
        var validator = new TestCredentialsCommandValidator(_enMsg);
        var command = new TestCredentialsCommand { Credentials = null! };

        TestValidationResult<TestCredentialsCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Credentials).WithErrorMessage(_enMsg.EmailOrUsernameRequired());
    }

    [Fact]
    public void ValidCredentials_WithWhitespaceCredentials_ShouldFail()
    {
        var validator = new TestCredentialsCommandValidator(_enMsg);
        var command = new TestCredentialsCommand { Credentials = "   " };

        TestValidationResult<TestCredentialsCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Credentials).WithErrorMessage(_enMsg.EmailOrUsernameRequired());
    }

    #endregion

    #region ValidOldPassword

    [Fact]
    public void ValidOldPassword_WithValidPassword_ShouldPass()
    {
        var validator = new TestOldPasswordCommandValidator(_enMsg);
        var command = new TestOldPasswordCommand { OldPassword = "AnyPassword1" };

        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OldPassword);
    }

    [Fact]
    public void ValidOldPassword_WithNullPassword_ShouldFail()
    {
        var validator = new TestOldPasswordCommandValidator(_enMsg);
        var command = new TestOldPasswordCommand { OldPassword = null };

        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OldPassword).WithErrorMessage(_enMsg.CurrentPasswordRequired());
    }

    [Fact]
    public void ValidOldPassword_WithEmptyPassword_ShouldFail()
    {
        var validator = new TestOldPasswordCommandValidator(_enMsg);
        var command = new TestOldPasswordCommand { OldPassword = string.Empty };

        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OldPassword).WithErrorMessage(_enMsg.CurrentPasswordRequired());
    }

    #endregion
}
