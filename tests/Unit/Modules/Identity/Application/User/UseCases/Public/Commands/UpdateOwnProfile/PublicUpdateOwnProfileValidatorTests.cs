using _116.BuildingBlocks.Constants;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

/// <summary>
/// Unit tests for <see cref="PublicUpdateOwnProfileValidator"/>.
/// </summary>
public class PublicUpdateOwnProfileValidatorTests
{
    private readonly PublicUpdateOwnProfileValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: TestConstants.User.ValidEmail,
            UserName: TestConstants.User.ValidUserName,
            CountryName: "United States",
            PartialPhoneNumber: "1234567890",
            CountryIsoCode: "US",
            CountryDialCode: "+1"
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithAllNullOptionalFields_ShouldNotHaveErrors()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: "notanemail",
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithTooLongEmail_ShouldHaveError()
    {
        // Arrange
        string longEmail = new string('a', UserConstants.MaxEmailLength) + "@test.com";
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: longEmail,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region UserName Validation Tests

    [Fact]
    public async Task Validate_WithTooShortUserName_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: "ab",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithTooLongUserName_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: new string('a', UserConstants.MaxUserNameLength + 1),
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public async Task Validate_WithInvalidUserNameCharacters_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: "user@name!",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: new string('a', UserConstants.MaxCountryNameLength + 1),
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.CountryName)
            .WithErrorMessage($"Country name cannot exceed {UserConstants.MaxCountryNameLength} characters");
    }

    #endregion

    #region CountryIsoCode Validation Tests

    [Fact]
    public async Task Validate_WithTooLongCountryIsoCode_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "USAA",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CountryIsoCode);
    }

    [Fact]
    public async Task Validate_WithLowercaseCountryIsoCode_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "us",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.CountryIsoCode)
            .WithErrorMessage("Country ISO code must contain only uppercase letters");
    }

    [Fact]
    public async Task Validate_WithValidTwoCharCountryIsoCode_ShouldNotHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "US",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithValidThreeCharCountryIsoCode_ShouldNotHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: "USA",
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region CountryDialCode Validation Tests

    [Fact]
    public async Task Validate_WithTooLongCountryDialCode_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: new string('1', UserConstants.MaxCountryDialCodeLength + 1)
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public async Task Validate_WithCountryDialCodeMissingPlus_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: "1"
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.CountryDialCode);
    }

    [Fact]
    public async Task Validate_WithValidCountryDialCode_ShouldNotHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: "+1"
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region PartialPhoneNumber Validation Tests

    [Fact]
    public async Task Validate_WithTooLongPartialPhoneNumber_ShouldHaveError()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: new string('1', UserConstants.MaxPartialPhoneNumberLength + 1),
            CountryIsoCode: null,
            CountryDialCode: null
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.PartialPhoneNumber)
            .WithErrorMessage(
                $"Partial phone number cannot exceed {UserConstants.MaxPartialPhoneNumberLength} characters"
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        PublicUpdateOwnProfileCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            Email: "invalid",
            UserName: "ab",
            CountryName: new string('a', UserConstants.MaxCountryNameLength + 1),
            PartialPhoneNumber: new string('1', UserConstants.MaxPartialPhoneNumberLength + 1),
            CountryIsoCode: "usaa",
            CountryDialCode: "1"
        );

        // Act
        TestValidationResult<PublicUpdateOwnProfileCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(6);
    }

    #endregion
}
