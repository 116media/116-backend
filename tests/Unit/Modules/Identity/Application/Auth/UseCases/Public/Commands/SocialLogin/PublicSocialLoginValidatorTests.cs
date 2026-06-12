using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Unit tests for <see cref="PublicSocialLoginValidator"/>.
/// </summary>
public class PublicSocialLoginValidatorTests
{
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
    private readonly PublicSocialLoginValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicSocialLoginValidatorTests"/>.
    /// </summary>
    public PublicSocialLoginValidatorTests()
    {
        _validator = new PublicSocialLoginValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: "https://example.com/avatar.jpg",
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNullAvatarUrl_ShouldNotHaveError()
    {
        // Arrange - AvatarUrl is optional
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: null,
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicSocialLoginCommand command = new(
            Email: null!,
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: null,
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: "notanemail",
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: null,
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region UserName Validation Tests

    [Fact]
    public async Task Validate_WithNullUserName_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: null!,
            AvatarUrl: null,
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithTooShortUserName_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: "ab",
            AvatarUrl: null,
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithTooLongUserName_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: new string('a', UserConstants.MaxUserNameLength + 1),
            AvatarUrl: null,
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region AvatarUrl Validation Tests

    [Fact]
    public async Task Validate_WithInvalidAvatarUrl_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: "not-a-url",
            Provider: nameof(EnumAuthProvider.Google)
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl).WithErrorMessage(_i18n.AvatarUrlInvalid());
    }

    #endregion

    #region Provider Validation Tests

    [Fact]
    public async Task Validate_WithNullProvider_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: null,
            Provider: null!
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Provider).WithErrorMessage(_i18n.AuthProviderRequired());
    }

    [Fact]
    public async Task Validate_WithInvalidProvider_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: null,
            Provider: "InvalidProvider"
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Provider).WithErrorMessage(_i18n.AuthProviderInvalid());
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Email: "invalid",
            UserName: "ab",
            AvatarUrl: "not-url",
            Provider: "invalid"
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
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
        var validator = new PublicSocialLoginValidator(i18n);
        var command = new PublicSocialLoginCommand(
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            AvatarUrl: null,
            Provider: null!
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Provider).WithErrorMessage(i18n.AuthProviderRequired());
    }

    #endregion
}
