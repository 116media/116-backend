using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Unit tests for <see cref="PublicSocialLoginValidator"/>. The validator only checks what the client
/// is trusted to send: a supported provider and a non-empty token.
/// </summary>
public class PublicSocialLoginValidatorTests
{
    private readonly IdentityI18n _i18n = TestErrorsFactory.CreateIdentityI18n();
    private readonly PublicSocialLoginValidator _validator;

    public PublicSocialLoginValidatorTests()
    {
        _validator = new PublicSocialLoginValidator(_i18n);
    }

    [Theory]
    [InlineData(nameof(EnumAuthProvider.Google))]
    [InlineData(nameof(EnumAuthProvider.Facebook))]
    public async Task Validate_WithSupportedProviderAndToken_ShouldNotHaveErrors(string provider)
    {
        // Arrange
        PublicSocialLoginCommand command = new(Provider: provider, IdToken: TestConstants.Auth.SocialLoginIdToken);

        // Act
        TestValidationResult<PublicSocialLoginCommand> result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyToken_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(Provider: TestConstants.Auth.ProviderGoogle, IdToken: string.Empty);

        // Act
        TestValidationResult<PublicSocialLoginCommand> result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IdToken);
    }

    [Fact]
    public async Task Validate_WithUnsupportedProvider_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(
            Provider: TestConstants.Auth.ProviderGitHub,
            IdToken: TestConstants.Auth.SocialLoginIdToken
        );

        // Act
        TestValidationResult<PublicSocialLoginCommand> result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Provider);
    }

    [Fact]
    public async Task Validate_WithEmptyProvider_ShouldHaveError()
    {
        // Arrange
        PublicSocialLoginCommand command = new(Provider: string.Empty, IdToken: TestConstants.Auth.SocialLoginIdToken);

        // Act
        TestValidationResult<PublicSocialLoginCommand> result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Provider);
    }
}
