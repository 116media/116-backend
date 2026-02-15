using _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById;

/// <summary>
/// Unit tests for <see cref="PublicGetOwnSessionByIdValidator"/>.
/// </summary>
public class PublicGetOwnSessionByIdValidatorTests
{
    private readonly PublicGetOwnSessionByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidQuery_ShouldNotHaveErrors()
    {
        // Arrange
        PublicGetOwnSessionByIdQuery query = new(UserId: Guid.NewGuid(), SessionId: Guid.NewGuid().ToString());

        // Act
        TestValidationResult<PublicGetOwnSessionByIdQuery>? result = await _validator.TestValidateAsync(query);

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
        PublicGetOwnSessionByIdQuery query = new(UserId: Guid.NewGuid(), SessionId: null!);

        // Act
        TestValidationResult<PublicGetOwnSessionByIdQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is required.");
    }

    [Fact]
    public async Task Validate_WithEmptySessionId_ShouldHaveError()
    {
        // Arrange
        PublicGetOwnSessionByIdQuery query = new(UserId: Guid.NewGuid(), SessionId: string.Empty);

        // Act
        TestValidationResult<PublicGetOwnSessionByIdQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is required.");
    }

    [Fact]
    public async Task Validate_WithWhitespaceSessionId_ShouldHaveError()
    {
        // Arrange
        PublicGetOwnSessionByIdQuery query = new(UserId: Guid.NewGuid(), SessionId: "   ");

        // Act
        TestValidationResult<PublicGetOwnSessionByIdQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is required.");
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        PublicGetOwnSessionByIdQuery query = new(UserId: Guid.NewGuid(), SessionId: "not-a-guid");

        // Act
        TestValidationResult<PublicGetOwnSessionByIdQuery>? result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.SessionId).WithErrorMessage("Session ID is invalid.");
    }

    #endregion
}
