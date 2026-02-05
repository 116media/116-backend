using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
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

    private class TestEmailCommandValidator : AbstractValidator<TestEmailCommand>
    {
        public TestEmailCommandValidator(bool isRequired = true)
        {
            RuleFor(x => x.Email).ValidEmail(isRequired);
        }
    }

    private class TestPasswordCommandValidator : AbstractValidator<TestPasswordCommand>
    {
        public TestPasswordCommandValidator(string fieldName = "Password")
        {
            RuleFor(x => x.Password).ValidPassword(fieldName);
        }
    }

    private class TestUsernameCommandValidator : AbstractValidator<TestUsernameCommand>
    {
        public TestUsernameCommandValidator(bool isRequired = true)
        {
            RuleFor(x => x.UserName).ValidUsername(isRequired);
        }
    }

    private class TestOldPasswordCommandValidator : AbstractValidator<TestOldPasswordCommand>
    {
        public TestOldPasswordCommandValidator(string fieldName = "Current password")
        {
            RuleFor(x => x.OldPassword).ValidOldPassword(fieldName);
        }
    }

    #region ValidEmail — required (default)

    [Fact]
    public void ValidEmail_WithValidEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator();
        var command = new TestEmailCommand { Email = "user@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_WithMaxLengthEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator();
        // Build a valid email at exactly MaxEmailLength (254) chars
        string local = new string('a', UserConstants.MaxEmailLength - "@example.com".Length);
        var command = new TestEmailCommand { Email = $"{local}@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_WithNullEmail_ShouldFail()
    {
        var validator = new TestEmailCommandValidator();
        var command = new TestEmailCommand { Email = null };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public void ValidEmail_WithEmptyEmail_ShouldFail()
    {
        var validator = new TestEmailCommandValidator();
        var command = new TestEmailCommand { Email = string.Empty };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public void ValidEmail_WithWhitespaceEmail_ShouldFail()
    {
        var validator = new TestEmailCommandValidator();
        var command = new TestEmailCommand { Email = "   " };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required");
    }

    [Fact]
    public void ValidEmail_WithEmailExceedingMaxLength_ShouldFail()
    {
        var validator = new TestEmailCommandValidator();
        string local = new string('a', UserConstants.MaxEmailLength);
        var command = new TestEmailCommand { Email = $"{local}@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage($"Email cannot exceed {UserConstants.MaxEmailLength} characters");
    }

    [Fact]
    public void ValidEmail_WithInvalidFormat_ShouldFail()
    {
        var validator = new TestEmailCommandValidator();
        var command = new TestEmailCommand { Email = "not-an-email" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void ValidEmail_WithMissingAtSign_ShouldFail()
    {
        var validator = new TestEmailCommandValidator();
        var command = new TestEmailCommand { Email = "userexample.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    #endregion

    #region ValidEmail — optional (isRequired = false)

    [Fact]
    public void ValidEmail_Optional_WithValidEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(isRequired: false);
        var command = new TestEmailCommand { Email = "user@example.com" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithNullEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(isRequired: false);
        var command = new TestEmailCommand { Email = null };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithEmptyEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(isRequired: false);
        var command = new TestEmailCommand { Email = string.Empty };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithWhitespaceEmail_ShouldPass()
    {
        var validator = new TestEmailCommandValidator(isRequired: false);
        var command = new TestEmailCommand { Email = "   " };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_Optional_WithInvalidFormat_ShouldFail()
    {
        var validator = new TestEmailCommandValidator(isRequired: false);
        var command = new TestEmailCommand { Email = "not-an-email" };

        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format");
    }

    #endregion

    #region ValidPassword

    [Fact]
    public void ValidPassword_WithValidPassword_ShouldPass()
    {
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = "SecureP@ss1" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_WithMinLengthPassword_ShouldPass()
    {
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = "Abc1de" }; // 6 chars

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_WithEmptyPassword_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = string.Empty };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required");
    }

    [Fact]
    public void ValidPassword_WithEmptyPassword_ShouldStopCascading()
    {
        // CascadeMode.Stop: only the NotEmpty error fires
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = string.Empty };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestPasswordCommand.Password));
    }

    [Fact]
    public void ValidPassword_WithTooShortPassword_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = "Ab1" }; // too short

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage($"Password must be at least {UserConstants.MinPasswordLength} characters long");
    }

    [Fact]
    public void ValidPassword_WithNoUppercase_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = "secure1pass" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(
                "Password must contain at least one lowercase letter, one uppercase letter, and one number"
            );
    }

    [Fact]
    public void ValidPassword_WithNoLowercase_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = "SECURE1PASS" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(
                "Password must contain at least one lowercase letter, one uppercase letter, and one number"
            );
    }

    [Fact]
    public void ValidPassword_WithNoNumber_ShouldFail()
    {
        var validator = new TestPasswordCommandValidator();
        var command = new TestPasswordCommand { Password = "SecurePass" };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(
                "Password must contain at least one lowercase letter, one uppercase letter, and one number"
            );
    }

    [Fact]
    public void ValidPassword_WithCustomFieldName_ShouldUseFieldNameInMessage()
    {
        var validator = new TestPasswordCommandValidator(fieldName: "New password");
        var command = new TestPasswordCommand { Password = string.Empty };

        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("New password is required");
    }

    #endregion

    #region ValidUsername — required (default)

    [Fact]
    public void ValidUsername_WithValidUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = "john-doe" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_WithMaxLengthUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = new string('a', UserConstants.MaxUserNameLength) };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_WithNullUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = null };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("Username is required");
    }

    [Fact]
    public void ValidUsername_WithEmptyUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = string.Empty };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("Username is required");
    }

    [Fact]
    public void ValidUsername_WithTooShortUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = "ab" }; // below MinUserNameLength

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage($"Username must be at least {UserConstants.MinUserNameLength} characters long");
    }

    [Fact]
    public void ValidUsername_WithTooLongUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = new string('a', UserConstants.MaxUserNameLength + 1) };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters");
    }

    [Fact]
    public void ValidUsername_WithSpecialCharacters_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = "john_doe!" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("Username can only contain letters, numbers, spaces, and hyphens");
    }

    [Fact]
    public void ValidUsername_WithHyphenAndSpaces_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator();
        var command = new TestUsernameCommand { UserName = "John Doe" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region ValidUsername — optional (isRequired = false)

    [Fact]
    public void ValidUsername_Optional_WithValidUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(isRequired: false);
        var command = new TestUsernameCommand { UserName = "john-doe" };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithNullUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(isRequired: false);
        var command = new TestUsernameCommand { UserName = null };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithEmptyUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(isRequired: false);
        var command = new TestUsernameCommand { UserName = string.Empty };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithWhitespaceUsername_ShouldPass()
    {
        var validator = new TestUsernameCommandValidator(isRequired: false);
        var command = new TestUsernameCommand { UserName = "   " };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void ValidUsername_Optional_WithTooLongUsername_ShouldFail()
    {
        var validator = new TestUsernameCommandValidator(isRequired: false);
        var command = new TestUsernameCommand { UserName = new string('a', UserConstants.MaxUserNameLength + 1) };

        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage($"Username cannot exceed {UserConstants.MaxUserNameLength} characters");
    }

    #endregion

    #region ValidOldPassword

    [Fact]
    public void ValidOldPassword_WithValidPassword_ShouldPass()
    {
        var validator = new TestOldPasswordCommandValidator();
        var command = new TestOldPasswordCommand { OldPassword = "AnyPassword1" };

        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OldPassword);
    }

    [Fact]
    public void ValidOldPassword_WithNullPassword_ShouldFail()
    {
        var validator = new TestOldPasswordCommandValidator();
        var command = new TestOldPasswordCommand { OldPassword = null };

        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OldPassword).WithErrorMessage("Current password is required");
    }

    [Fact]
    public void ValidOldPassword_WithEmptyPassword_ShouldFail()
    {
        var validator = new TestOldPasswordCommandValidator();
        var command = new TestOldPasswordCommand { OldPassword = string.Empty };

        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OldPassword).WithErrorMessage("Current password is required");
    }

    [Fact]
    public void ValidOldPassword_WithCustomFieldName_ShouldUseFieldNameInMessage()
    {
        var validator = new TestOldPasswordCommandValidator(fieldName: "Old password");
        var command = new TestOldPasswordCommand { OldPassword = null };

        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OldPassword).WithErrorMessage("Old password is required");
    }

    #endregion
}
