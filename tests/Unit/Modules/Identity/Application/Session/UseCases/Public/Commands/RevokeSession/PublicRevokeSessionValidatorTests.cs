using _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;

/// <summary>
/// Unit tests for <see cref="PublicRevokeSessionValidator"/>.
/// </summary>
public class PublicRevokeSessionValidatorTests
{
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
    private readonly PublicRevokeSessionValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicRevokeSessionValidatorTests"/>.
    /// </summary>
    public PublicRevokeSessionValidatorTests()
    {
        _validator = new PublicRevokeSessionValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        PublicRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: Guid.NewGuid().ToString());

        // Act
        TestValidationResult<PublicRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: null!);

        // Act
        TestValidationResult<PublicRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.Localizer["SessionIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithEmptySessionId_ShouldHaveError()
    {
        // Arrange
        PublicRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: string.Empty);

        // Act
        TestValidationResult<PublicRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.Localizer["SessionIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithWhitespaceSessionId_ShouldHaveError()
    {
        // Arrange
        PublicRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: "   ");

        // Act
        TestValidationResult<PublicRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.Localizer["SessionIdRequired"].Value);
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        // Arrange
        PublicRevokeSessionCommand command = new(UserId: Guid.NewGuid(), SessionId: "not-a-guid");

        // Act
        TestValidationResult<PublicRevokeSessionCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(_i18n.Localizer["SessionIdInvalid"].Value);
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
        var validator = new PublicRevokeSessionValidator(i18n);
        var command = new PublicRevokeSessionCommand(UserId: Guid.NewGuid(), SessionId: null!);

        // Act
        TestValidationResult<PublicRevokeSessionCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.SessionId)
            .WithErrorMessage(i18n.Localizer["SessionIdRequired"].Value);
    }

    #endregion
}
