using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="SessionValidation"/>.
/// </summary>
public class SessionValidationTests
{
    private readonly ValidationErrorMessage _enMsg = LocalizerFactory.CreateMessage<ValidationErrorMessage>();

    private class TestRefreshTokenCommand
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    private class TestRefreshTokenCommandValidator : AbstractValidator<TestRefreshTokenCommand>
    {
        public TestRefreshTokenCommandValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.RefreshToken).ValidRefreshToken(i18n);
        }
    }

    #region ValidRefreshToken

    [Fact]
    public void ValidRefreshToken_WithValidToken_ShouldPass()
    {
        var validator = new TestRefreshTokenCommandValidator(_enMsg);
        var command = new TestRefreshTokenCommand { RefreshToken = "valid-refresh-token-base64" };

        TestValidationResult<TestRefreshTokenCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void ValidRefreshToken_WithEmptyToken_ShouldFail()
    {
        var validator = new TestRefreshTokenCommandValidator(_enMsg);
        var command = new TestRefreshTokenCommand { RefreshToken = string.Empty };

        TestValidationResult<TestRefreshTokenCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_enMsg.RefreshTokenRequired());
    }

    [Fact]
    public void ValidRefreshToken_WithNullToken_ShouldFail()
    {
        var validator = new TestRefreshTokenCommandValidator(_enMsg);
        var command = new TestRefreshTokenCommand { RefreshToken = null! };

        TestValidationResult<TestRefreshTokenCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_enMsg.RefreshTokenRequired());
    }

    [Fact]
    public void ValidRefreshToken_WithWhitespaceToken_ShouldFail()
    {
        var validator = new TestRefreshTokenCommandValidator(_enMsg);
        var command = new TestRefreshTokenCommand { RefreshToken = "   " };

        TestValidationResult<TestRefreshTokenCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken).WithErrorMessage(_enMsg.RefreshTokenRequired());
    }

    #endregion
}
