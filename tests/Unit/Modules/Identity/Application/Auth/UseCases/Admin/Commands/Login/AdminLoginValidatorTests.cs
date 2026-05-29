using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Unit tests for <see cref="AdminLoginValidator"/>.
/// </summary>
public class AdminLoginValidatorTests
{
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
    private readonly AdminLoginValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminLoginValidatorTests"/>.
    /// </summary>
    public AdminLoginValidatorTests()
    {
        _validator = new AdminLoginValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMaxLengthEmail_ShouldNotHaveErrors()
    {
        // Arrange
        string maxEmail = new string('a', UserConstants.MaxEmailLength - "@test.com".Length) + "@test.com";
        AdminLoginCommand command = new(Email: maxEmail, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

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
        AdminLoginCommand command = new(Email: null!, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_i18n.EmailRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: string.Empty, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_i18n.EmailRequired());
    }

    [Fact]
    public async Task Validate_WithWhitespaceEmail_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: "   ", Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_i18n.EmailRequired());
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: "notanemail", Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(_i18n.InvalidEmailFormatMsg());
    }

    [Fact]
    public async Task Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        string longEmail = new string('a', UserConstants.MaxEmailLength + 1) + "@test.com";
        AdminLoginCommand command = new(Email: longEmail, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(_i18n.EmailTooLong(UserConstants.MaxEmailLength));
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public async Task Validate_WithNullPassword_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: TestConstants.User.ValidEmail, Password: null!);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_i18n.PasswordRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: TestConstants.User.ValidEmail, Password: string.Empty);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_i18n.PasswordRequired());
    }

    [Fact]
    public async Task Validate_WithWhitespacePassword_ShouldHaveError()
    {
        // Arrange
        AdminLoginCommand command = new(Email: TestConstants.User.ValidEmail, Password: "   ");

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_i18n.PasswordRequired());
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminLoginCommand command = new(Email: string.Empty, Password: string.Empty);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>(culture);
        var validator = new AdminLoginValidator(i18n);
        var command = new AdminLoginCommand(Email: null!, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<AdminLoginCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(i18n.EmailRequired());
    }

    #endregion
}
