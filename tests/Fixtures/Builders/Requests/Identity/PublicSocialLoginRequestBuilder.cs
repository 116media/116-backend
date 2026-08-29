using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;
using _116.Identity.Domain.Enums;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicSocialLoginRequest"/> instances in tests with valid
/// defaults that satisfy the social-login validator (a valid auth provider enum member and a
/// non-empty token). The token is opaque to the API and is resolved by the (stubbed) verifier.
/// </summary>
public class PublicSocialLoginRequestBuilder
{
    private string _provider;
    private string _idToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicSocialLoginRequestBuilder"/> class with the
    /// Google provider and a unique non-empty token.
    /// </summary>
    public PublicSocialLoginRequestBuilder()
    {
        _provider = nameof(EnumAuthProvider.Google);
        _idToken = $"token-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Sets the social authentication provider.
    /// </summary>
    /// <param name="provider">The social authentication provider (Google or Facebook).</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSocialLoginRequestBuilder WithProvider(string provider)
    {
        _provider = provider;
        return this;
    }

    /// <summary>
    /// Sets the provider-issued token.
    /// </summary>
    /// <param name="idToken">The provider-issued token to verify.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSocialLoginRequestBuilder WithIdToken(string idToken)
    {
        _idToken = idToken;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicSocialLoginRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicSocialLoginRequest instance.</returns>
    public PublicSocialLoginRequest Build()
    {
        return new PublicSocialLoginRequest(Provider: _provider, IdToken: _idToken);
    }
}
