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
    private const string PasswordFieldName = "Password";
    private const string EmailDomain = "@example.com";

    private readonly ValidationErrorMessage _enMsg = LocalizerFactory.CreateMessage<ValidationErrorMessage>();

    /// <summary>
    /// The failure branch a rejected input is required to trigger. Rows carry the branch rather
    /// than a literal message so the assertion stays tied to the localizer, which is what makes an
    /// emptied resource entry fail the test.
    /// </summary>
    public enum FailureBranch
    {
        /// <summary>The email is absent.</summary>
        EmailRequired,

        /// <summary>The email exceeds <see cref="UserConstants.MaxEmailLength"/>.</summary>
        EmailTooLong,

        /// <summary>The email is not a well-formed address.</summary>
        EmailFormat,

        /// <summary>The password is absent.</summary>
        PasswordRequired,

        /// <summary>The password is shorter than <see cref="UserConstants.MinPasswordLength"/>.</summary>
        PasswordTooShort,

        /// <summary>The password lacks a lowercase, an uppercase, or a digit.</summary>
        PasswordComplexity,

        /// <summary>The username is absent.</summary>
        UsernameRequired,

        /// <summary>The username is shorter than <see cref="UserConstants.MinUserNameLength"/>.</summary>
        UsernameTooShort,

        /// <summary>The username exceeds <see cref="UserConstants.MaxUserNameLength"/>.</summary>
        UsernameTooLong,

        /// <summary>The username contains characters outside letters, digits, spaces, and hyphens.</summary>
        UsernameInvalidChars,

        /// <summary>The login credentials value is absent.</summary>
        CredentialsRequired,

        /// <summary>The current password is absent.</summary>
        CurrentPasswordRequired,
    }

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

    /// <summary>
    /// Builds a well-formed email address of exactly <paramref name="totalLength"/> characters so
    /// length boundaries are expressed against <see cref="UserConstants.MaxEmailLength"/> rather
    /// than against a literal.
    /// </summary>
    /// <param name="totalLength">The required total length of the address.</param>
    /// <returns>A syntactically valid address of the requested length.</returns>
    private static string EmailOfLength(int totalLength) =>
        new string('a', totalLength - EmailDomain.Length) + EmailDomain;

    /// <summary>
    /// Resolves the localized message the given failure branch must produce.
    /// </summary>
    /// <param name="branch">The branch the rule is expected to report.</param>
    /// <returns>The localized message for the branch.</returns>
    private string ExpectedMessage(FailureBranch branch) =>
        branch switch
        {
            FailureBranch.EmailRequired => _enMsg.EmailRequired(),
            FailureBranch.EmailTooLong => _enMsg.EmailTooLong(UserConstants.MaxEmailLength),
            FailureBranch.EmailFormat => _enMsg.InvalidEmailFormatMsg(),
            FailureBranch.PasswordRequired => _enMsg.PasswordRequired(),
            FailureBranch.PasswordTooShort => _enMsg.PasswordTooShort(
                PasswordFieldName,
                UserConstants.MinPasswordLength
            ),
            FailureBranch.PasswordComplexity => _enMsg.PasswordComplexity(PasswordFieldName),
            FailureBranch.UsernameRequired => _enMsg.UsernameRequired(),
            FailureBranch.UsernameTooShort => _enMsg.UsernameTooShort(UserConstants.MinUserNameLength),
            FailureBranch.UsernameTooLong => _enMsg.UsernameTooLong(UserConstants.MaxUserNameLength),
            FailureBranch.UsernameInvalidChars => _enMsg.UsernameInvalidChars(),
            FailureBranch.CredentialsRequired => _enMsg.EmailOrUsernameRequired(),
            FailureBranch.CurrentPasswordRequired => _enMsg.CurrentPasswordRequired(),
            _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, "Unmapped failure branch"),
        };

    #region ValidEmail — required (default)

    /// <summary>
    /// Inputs the required-email rule must reject, paired with the branch that must fire. The rule
    /// cascades NotEmpty, MaximumLength, then EmailAddress, so each row also pins the order.
    /// </summary>
    /// <returns>Candidate email and expected failure branch per row.</returns>
    public static TheoryData<string, FailureBranch> RejectedRequiredEmails() =>
        new()
        {
            { (string)null!, FailureBranch.EmailRequired },
            { string.Empty, FailureBranch.EmailRequired },
            { "   ", FailureBranch.EmailRequired },
            { EmailOfLength(UserConstants.MaxEmailLength + 1), FailureBranch.EmailTooLong },
            { EmailOfLength(UserConstants.MaxEmailLength + 12), FailureBranch.EmailTooLong },
            { "not-an-email", FailureBranch.EmailFormat },
            { "userexample.com", FailureBranch.EmailFormat },
            { "user@", FailureBranch.EmailFormat },
            { "@example.com", FailureBranch.EmailFormat },
            { "user@@example.com", FailureBranch.EmailFormat },
        };

    /// <summary>
    /// Inputs the required-email rule must accept, including both sides of the length boundary.
    /// </summary>
    /// <returns>Candidate email per row.</returns>
    public static TheoryData<string> AcceptedRequiredEmails() =>
        new()
        {
            "user@example.com",
            "a@b.co",
            EmailOfLength(UserConstants.MaxEmailLength - 1),
            EmailOfLength(UserConstants.MaxEmailLength),
        };

    [Theory]
    [MemberData(nameof(RejectedRequiredEmails))]
    public void ValidEmail_Required_ShouldRejectWithTheExpectedMessage(string? email, FailureBranch branch)
    {
        // Arrange
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = email };

        // Act
        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(ExpectedMessage(branch));
    }

    [Theory]
    [MemberData(nameof(AcceptedRequiredEmails))]
    public void ValidEmail_Required_ShouldAccept(string email)
    {
        // Arrange
        var validator = new TestEmailCommandValidator(_enMsg);
        var command = new TestEmailCommand { Email = email };

        // Act
        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region ValidEmail — optional (isRequired = false)

    /// <summary>
    /// Inputs the optional-email rule must still reject once the value is present.
    /// </summary>
    /// <returns>Candidate email and expected failure branch per row.</returns>
    public static TheoryData<string, FailureBranch> RejectedOptionalEmails() =>
        new()
        {
            { "not-an-email", FailureBranch.EmailFormat },
            { "user@", FailureBranch.EmailFormat },
            { EmailOfLength(UserConstants.MaxEmailLength + 1), FailureBranch.EmailTooLong },
        };

    /// <summary>
    /// Inputs the optional-email rule must accept, including the absent values its When() gate skips.
    /// </summary>
    /// <returns>Candidate email per row.</returns>
    public static TheoryData<string> AcceptedOptionalEmails() =>
        new() { "user@example.com", (string)null!, string.Empty, "   ", EmailOfLength(UserConstants.MaxEmailLength) };

    [Theory]
    [MemberData(nameof(RejectedOptionalEmails))]
    public void ValidEmail_Optional_ShouldRejectWithTheExpectedMessage(string? email, FailureBranch branch)
    {
        // Arrange
        var validator = new TestEmailCommandValidator(_enMsg, isRequired: false);
        var command = new TestEmailCommand { Email = email };

        // Act
        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(ExpectedMessage(branch));
    }

    [Theory]
    [MemberData(nameof(AcceptedOptionalEmails))]
    public void ValidEmail_Optional_ShouldAccept(string? email)
    {
        // Arrange
        var validator = new TestEmailCommandValidator(_enMsg, isRequired: false);
        var command = new TestEmailCommand { Email = email };

        // Act
        TestValidationResult<TestEmailCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region ValidPassword — strong (default)

    /// <summary>
    /// Inputs the strong-password rule must reject, paired with the branch that must fire.
    /// </summary>
    /// <returns>Candidate password and expected failure branch per row.</returns>
    public static TheoryData<string, FailureBranch> RejectedStrongPasswords() =>
        new()
        {
            { (string)null!, FailureBranch.PasswordRequired },
            { string.Empty, FailureBranch.PasswordRequired },
            { "      ", FailureBranch.PasswordRequired },
            { "Ab1", FailureBranch.PasswordTooShort },
            { new string('A', UserConstants.MinPasswordLength - 1), FailureBranch.PasswordTooShort },
            { "secure1password", FailureBranch.PasswordComplexity },
            { "SECURE1PASSWORD", FailureBranch.PasswordComplexity },
            { "SecurePassword", FailureBranch.PasswordComplexity },
            { "Abcdefghijkl", FailureBranch.PasswordComplexity },
        };

    /// <summary>
    /// Inputs the strong-password rule must accept, including the minimum-length boundary.
    /// </summary>
    /// <returns>Candidate password per row.</returns>
    public static TheoryData<string> AcceptedStrongPasswords() =>
        new() { "SecureP@ssw0rd", "Abc1defghijk", "Abc1defghijkl" };

    [Theory]
    [MemberData(nameof(RejectedStrongPasswords))]
    public void ValidPassword_ShouldRejectWithTheExpectedMessage(string? password, FailureBranch branch)
    {
        // Arrange
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = password! };

        // Act
        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(ExpectedMessage(branch));
    }

    [Theory]
    [MemberData(nameof(AcceptedStrongPasswords))]
    public void ValidPassword_ShouldAccept(string password)
    {
        // Arrange
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = password };

        // Act
        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_WithEmptyPassword_ShouldStopCascading()
    {
        // Arrange
        var validator = new TestPasswordCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = string.Empty };

        // Act
        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestPasswordCommand.Password));
    }

    #endregion

    #region ValidPassword — not strong (isStrong = false)

    /// <summary>
    /// Inputs the presence-only password rule must reject; the complexity branches must not fire.
    /// </summary>
    /// <returns>Candidate password per row.</returns>
    public static TheoryData<string> RejectedNonStrongPasswords() => new() { (string)null!, string.Empty, "   " };

    /// <summary>
    /// Inputs the presence-only password rule must accept, including values the strong rule rejects.
    /// </summary>
    /// <returns>Candidate password per row.</returns>
    public static TheoryData<string> AcceptedNonStrongPasswords() =>
        new() { "anypassword", "ab", "a", "nouppercase123" };

    [Theory]
    [MemberData(nameof(RejectedNonStrongPasswords))]
    public void ValidPassword_NotStrong_ShouldRejectWithTheRequiredMessage(string? password)
    {
        // Arrange
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = password! };

        // Act
        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(ExpectedMessage(FailureBranch.PasswordRequired));
    }

    [Theory]
    [MemberData(nameof(AcceptedNonStrongPasswords))]
    public void ValidPassword_NotStrong_ShouldAccept(string password)
    {
        // Arrange
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = password };

        // Act
        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_NotStrong_WithEmptyPassword_ShouldStopCascading()
    {
        // Arrange
        var validator = new TestPasswordNotStrongCommandValidator(_enMsg);
        var command = new TestPasswordCommand { Password = string.Empty };

        // Act
        TestValidationResult<TestPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestPasswordCommand.Password));
    }

    #endregion

    #region ValidUsername — required (default)

    /// <summary>
    /// Inputs the required-username rule must reject, paired with the branch that must fire.
    /// </summary>
    /// <returns>Candidate username and expected failure branch per row.</returns>
    public static TheoryData<string, FailureBranch> RejectedRequiredUsernames() =>
        new()
        {
            { (string)null!, FailureBranch.UsernameRequired },
            { string.Empty, FailureBranch.UsernameRequired },
            { "   ", FailureBranch.UsernameRequired },
            { new string('a', UserConstants.MinUserNameLength - 1), FailureBranch.UsernameTooShort },
            { new string('a', UserConstants.MaxUserNameLength + 1), FailureBranch.UsernameTooLong },
            { "john_doe!", FailureBranch.UsernameInvalidChars },
            { "john@doe", FailureBranch.UsernameInvalidChars },
            { "josé", FailureBranch.UsernameInvalidChars },
        };

    /// <summary>
    /// Inputs the required-username rule must accept, including both length boundaries.
    /// </summary>
    /// <returns>Candidate username per row.</returns>
    public static TheoryData<string> AcceptedRequiredUsernames() =>
        new()
        {
            "john-doe",
            "John Doe",
            "user123",
            new string('a', UserConstants.MinUserNameLength),
            new string('a', UserConstants.MaxUserNameLength - 1),
            new string('a', UserConstants.MaxUserNameLength),
        };

    [Theory]
    [MemberData(nameof(RejectedRequiredUsernames))]
    public void ValidUsername_Required_ShouldRejectWithTheExpectedMessage(string? userName, FailureBranch branch)
    {
        // Arrange
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = userName };

        // Act
        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage(ExpectedMessage(branch));
    }

    [Theory]
    [MemberData(nameof(AcceptedRequiredUsernames))]
    public void ValidUsername_Required_ShouldAccept(string userName)
    {
        // Arrange
        var validator = new TestUsernameCommandValidator(_enMsg);
        var command = new TestUsernameCommand { UserName = userName };

        // Act
        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region ValidUsername — optional (isRequired = false)

    /// <summary>
    /// Inputs the optional-username rule must still reject once the value is present. Every branch
    /// of the optional chain is represented, not only the length ceiling.
    /// </summary>
    /// <returns>Candidate username and expected failure branch per row.</returns>
    public static TheoryData<string, FailureBranch> RejectedOptionalUsernames() =>
        new()
        {
            { new string('a', UserConstants.MinUserNameLength - 1), FailureBranch.UsernameTooShort },
            { new string('a', UserConstants.MaxUserNameLength + 1), FailureBranch.UsernameTooLong },
            { "john_doe!", FailureBranch.UsernameInvalidChars },
        };

    /// <summary>
    /// Inputs the optional-username rule must accept, including the absent values its When() gate skips.
    /// </summary>
    /// <returns>Candidate username per row.</returns>
    public static TheoryData<string> AcceptedOptionalUsernames() =>
        new()
        {
            "john-doe",
            (string)null!,
            string.Empty,
            "   ",
            new string('a', UserConstants.MinUserNameLength),
            new string('a', UserConstants.MaxUserNameLength),
        };

    [Theory]
    [MemberData(nameof(RejectedOptionalUsernames))]
    public void ValidUsername_Optional_ShouldRejectWithTheExpectedMessage(string? userName, FailureBranch branch)
    {
        // Arrange
        var validator = new TestUsernameCommandValidator(_enMsg, isRequired: false);
        var command = new TestUsernameCommand { UserName = userName };

        // Act
        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage(ExpectedMessage(branch));
    }

    [Theory]
    [MemberData(nameof(AcceptedOptionalUsernames))]
    public void ValidUsername_Optional_ShouldAccept(string? userName)
    {
        // Arrange
        var validator = new TestUsernameCommandValidator(_enMsg, isRequired: false);
        var command = new TestUsernameCommand { UserName = userName };

        // Act
        TestValidationResult<TestUsernameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region ValidCredentials

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidCredentials_ShouldRejectWithTheRequiredMessage(string? credentials)
    {
        // Arrange
        var validator = new TestCredentialsCommandValidator(_enMsg);
        var command = new TestCredentialsCommand { Credentials = credentials! };

        // Act
        TestValidationResult<TestCredentialsCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Credentials)
            .WithErrorMessage(ExpectedMessage(FailureBranch.CredentialsRequired));
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("john-doe")]
    [InlineData("a")]
    public void ValidCredentials_ShouldAccept(string credentials)
    {
        // Arrange
        var validator = new TestCredentialsCommandValidator(_enMsg);
        var command = new TestCredentialsCommand { Credentials = credentials };

        // Act
        TestValidationResult<TestCredentialsCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Credentials);
    }

    #endregion

    #region ValidOldPassword

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidOldPassword_ShouldRejectWithTheRequiredMessage(string? oldPassword)
    {
        // Arrange
        var validator = new TestOldPasswordCommandValidator(_enMsg);
        var command = new TestOldPasswordCommand { OldPassword = oldPassword };

        // Act
        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.OldPassword)
            .WithErrorMessage(ExpectedMessage(FailureBranch.CurrentPasswordRequired));
    }

    [Theory]
    [InlineData("AnyPassword123")]
    [InlineData("x")]
    public void ValidOldPassword_ShouldAccept(string oldPassword)
    {
        // Arrange
        var validator = new TestOldPasswordCommandValidator(_enMsg);
        var command = new TestOldPasswordCommand { OldPassword = oldPassword };

        // Act
        TestValidationResult<TestOldPasswordCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OldPassword);
    }

    #endregion
}
