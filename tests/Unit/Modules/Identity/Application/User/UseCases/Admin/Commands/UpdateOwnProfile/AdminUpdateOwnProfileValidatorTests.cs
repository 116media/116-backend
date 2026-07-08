using System.Globalization;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Unit tests for <see cref="AdminUpdateOwnProfileValidator"/>.
/// </summary>
public class AdminUpdateOwnProfileValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminUpdateOwnProfileValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdateOwnProfileValidatorTests"/>.
    /// </summary>
    public AdminUpdateOwnProfileValidatorTests()
    {
        _validator = new AdminUpdateOwnProfileValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: TestConstants.User.ValidUserName,
            CountryName: "United States",
            PartialPhoneNumber: "1234567890",
            CountryIsoCode: "US",
            CountryDialCode: "+1"
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithAllNullOptionalFields_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region UserName Validation Tests

    [Fact]
    public async Task Validate_WithTooShortUserName_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: "ab",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithTooLongUserName_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: new string('a', UserConstants.MaxUserNameLength + 1),
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithInvalidUserNameCharacters_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: "user@name!",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region CountryName Validation Tests

    [Fact]
    public async Task Validate_WithTooLongCountryName_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: new string('a', UserConstants.MaxCountryNameLength + 1),
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.CountryName)
            .WithErrorMessage(_i18n.User.Validation.CountryNameTooLong(UserConstants.MaxCountryNameLength));
    }

    #endregion

    #region CountryIsoCode Validation Tests

    [Fact]
    public async Task Validate_WithTooLongCountryIsoCode_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "USAA",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CountryIsoCode);
    }

    [Fact]
    public async Task Validate_WithLowercaseCountryIsoCode_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "us",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.CountryIsoCode)
            .WithErrorMessage(_i18n.User.Validation.CountryIsoCodeInvalid());
    }

    [Fact]
    public async Task Validate_WithValidTwoCharCountryIsoCode_ShouldNotHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "US",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithValidThreeCharCountryIsoCode_ShouldNotHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "USA",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region CountryDialCode Validation Tests

    [Fact]
    public async Task Validate_WithTooLongCountryDialCode_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: new string('1', UserConstants.MaxCountryDialCodeLength + 1)
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public async Task Validate_WithCountryDialCodeMissingPlus_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: "1"
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public async Task Validate_WithValidCountryDialCode_ShouldNotHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: "+1"
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region PartialPhoneNumber Validation Tests

    [Fact]
    public async Task Validate_WithTooLongPartialPhoneNumber_ShouldHaveError()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: new string('1', UserConstants.MaxPartialPhoneNumberLength + 1),
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PartialPhoneNumber)
            .WithErrorMessage(
                _i18n.User.Validation.PartialPhoneNumberTooLong(UserConstants.MaxPartialPhoneNumberLength)
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: "ab",
            CountryName: new string('a', UserConstants.MaxCountryNameLength + 1),
            PartialPhoneNumber: new string('1', UserConstants.MaxPartialPhoneNumberLength + 1),
            CountryIsoCode: "usaa",
            CountryDialCode: "1"
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var i18n = TestErrorsFactory.CreateIdentityI18n();
        var validator = new AdminUpdateOwnProfileValidator(i18n);
        var command = new AdminUpdateOwnProfileCommand(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            UserName: null,
            CountryName: new string('a', UserConstants.MaxCountryNameLength + 1),
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<AdminUpdateOwnProfileCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.CountryName)
            .WithErrorMessage(i18n.User.Validation.CountryNameTooLong(UserConstants.MaxCountryNameLength));
    }

    #endregion
}
