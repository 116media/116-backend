using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateUserValidator"/>.
/// </summary>
public class AdminDeactivateUserValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminDeactivateUserValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivateUserValidatorTests"/>.
    /// </summary>
    public AdminDeactivateUserValidatorTests()
    {
        _validator = new AdminDeactivateUserValidator(_i18n);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminDeactivateUserCommand command = new(UserId: Guid.NewGuid().ToString());

        // Act
        TestValidationResult<AdminDeactivateUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNullUserId_ShouldHaveError()
    {
        // Arrange
        AdminDeactivateUserCommand command = new(UserId: null!);

        // Act
        TestValidationResult<AdminDeactivateUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        AdminDeactivateUserCommand command = new(UserId: string.Empty);

        // Act
        TestValidationResult<AdminDeactivateUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithWhitespaceUserId_ShouldHaveError()
    {
        // Arrange
        AdminDeactivateUserCommand command = new(UserId: "   ");

        // Act
        TestValidationResult<AdminDeactivateUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        AdminDeactivateUserCommand command = new(UserId: "not-a-guid");

        // Act
        TestValidationResult<AdminDeactivateUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdInvalid"].Value);
    }
}
