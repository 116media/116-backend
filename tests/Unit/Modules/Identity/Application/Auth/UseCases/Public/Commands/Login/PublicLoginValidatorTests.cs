using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Unit tests for <see cref="PublicLoginValidator"/>.
/// </summary>
public class PublicLoginValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly PublicLoginValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicLoginValidatorTests"/>.
    /// </summary>
    public PublicLoginValidatorTests()
    {
        _validator = new PublicLoginValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidEmailCredentials_ShouldNotHaveErrors()
    {
        // Arrange
        PublicLoginCommand command = new(
            Credentials: TestConstants.User.ValidEmail,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidUsernameCredentials_ShouldNotHaveErrors()
    {
        // Arrange
        PublicLoginCommand command = new(
            Credentials: TestConstants.User.ValidUserName,
            Password: TestConstants.User.ValidPassword
        );

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Credentials Validation Tests

    [Fact]
    public async Task Validate_WithNullCredentials_ShouldHaveError()
    {
        // Arrange
        PublicLoginCommand command = new(Credentials: null!, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Credentials)
            .WithErrorMessage(_i18n.User.Validation.EmailOrUsernameRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyCredentials_ShouldHaveError()
    {
        // Arrange
        PublicLoginCommand command = new(Credentials: string.Empty, Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Credentials)
            .WithErrorMessage(_i18n.User.Validation.EmailOrUsernameRequired());
    }

    [Fact]
    public async Task Validate_WithWhitespaceCredentials_ShouldHaveError()
    {
        // Arrange
        PublicLoginCommand command = new(Credentials: "   ", Password: TestConstants.User.ValidPassword);

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.Credentials)
            .WithErrorMessage(_i18n.User.Validation.EmailOrUsernameRequired());
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public async Task Validate_WithNullPassword_ShouldHaveError()
    {
        // Arrange
        PublicLoginCommand command = new(Credentials: TestConstants.User.ValidEmail, Password: null!);

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_i18n.User.Validation.PasswordRequired());
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldHaveError()
    {
        // Arrange
        PublicLoginCommand command = new(Credentials: TestConstants.User.ValidEmail, Password: string.Empty);

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_i18n.User.Validation.PasswordRequired());
    }

    [Fact]
    public async Task Validate_WithWhitespacePassword_ShouldHaveError()
    {
        // Arrange
        PublicLoginCommand command = new(Credentials: TestConstants.User.ValidEmail, Password: "   ");

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(_i18n.User.Validation.PasswordRequired());
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        PublicLoginCommand command = new(Credentials: string.Empty, Password: string.Empty);

        // Act
        TestValidationResult<PublicLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.ShouldHaveValidationErrorFor(x => x.Credentials);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
