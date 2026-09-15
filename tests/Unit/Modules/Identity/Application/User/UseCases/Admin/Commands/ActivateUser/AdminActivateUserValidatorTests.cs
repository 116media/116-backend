using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.User.UseCases.Admin.Commands.ActivateUser;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.ActivateUser;

/// <summary>
/// Unit tests for <see cref="AdminActivateUserValidator"/>.
/// </summary>
public class AdminActivateUserValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminActivateUserValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivateUserValidatorTests"/>.
    /// </summary>
    public AdminActivateUserValidatorTests()
    {
        _validator = new AdminActivateUserValidator(_i18n);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminActivateUserCommand command = new(UserId: Guid.NewGuid().ToString());

        // Act
        TestValidationResult<AdminActivateUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNullUserId_ShouldHaveError()
    {
        // Arrange
        AdminActivateUserCommand command = new(UserId: null!);

        // Act
        TestValidationResult<AdminActivateUserCommand>? result = await _validator.TestValidateAsync(command);

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
        AdminActivateUserCommand command = new(UserId: string.Empty);

        // Act
        TestValidationResult<AdminActivateUserCommand>? result = await _validator.TestValidateAsync(command);

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
        AdminActivateUserCommand command = new(UserId: "   ");

        // Act
        TestValidationResult<AdminActivateUserCommand>? result = await _validator.TestValidateAsync(command);

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
        AdminActivateUserCommand command = new(UserId: "not-a-guid");

        // Act
        TestValidationResult<AdminActivateUserCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["UserIdInvalid"].Value);
    }
}
