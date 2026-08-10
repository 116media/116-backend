using _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;

/// <summary>
/// Unit tests for <see cref="AdminRevokeSessionValidator"/>.
/// </summary>
public class AdminRevokeSessionValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly AdminRevokeSessionValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminRevokeSessionValidatorTests"/>.
    /// </summary>
    public AdminRevokeSessionValidatorTests()
    {
        _validator = new AdminRevokeSessionValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: Guid.NewGuid().ToString());

        // Act
        TestValidationResult<AdminRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region SessionId Validation Tests

    [Fact]
    public async Task Validate_WithNullSessionId_ShouldHaveError()
    {
        // Arrange
        AdminRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: null!);

        // Act
        TestValidationResult<AdminRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["SessionIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithEmptySessionId_ShouldHaveError()
    {
        // Arrange
        AdminRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: string.Empty);

        // Act
        TestValidationResult<AdminRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["SessionIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithWhitespaceSessionId_ShouldHaveError()
    {
        // Arrange
        AdminRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: "   ");

        // Act
        TestValidationResult<AdminRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["SessionIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        AdminRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: "not-a-guid");

        // Act
        TestValidationResult<AdminRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.User.Validation.Localizer["SessionIdInvalid"].Value);
    }

    #endregion
}
