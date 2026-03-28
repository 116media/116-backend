using _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;

/// <summary>
/// Unit tests for <see cref="AdminRevokeSessionValidator"/>.
/// </summary>
public class AdminRevokeSessionValidatorTests
{
    private readonly AdminRevokeSessionValidator _validator = new();

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
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is required.");
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
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is required.");
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
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is required.");
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
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is invalid.");
    }

    #endregion
}
