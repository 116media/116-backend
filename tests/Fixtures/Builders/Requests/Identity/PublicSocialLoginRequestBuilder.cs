using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicSocialLoginRequest"/> instances in tests
/// with valid defaults that satisfy the social-login validator (email format, username length
/// and allowed characters, optional avatar URL, valid auth provider enum member).
/// </summary>
public class PublicSocialLoginRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _userName;
    private string? _avatarUrl;
    private string _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicSocialLoginRequestBuilder"/> class
    /// with a unique valid email, an alphanumeric username within the allowed length range,
    /// a null avatar URL (to avoid a real provider image download), and the Google provider.
    /// </summary>
    public PublicSocialLoginRequestBuilder()
    {
        _email = $"social-{Guid.NewGuid():N}@test.com";
        _userName = $"u{Guid.NewGuid():N}"[..10];
        _avatarUrl = null;
        _provider = nameof(EnumAuthProvider.Google);
    }

    /// <summary>
    /// Sets the social provider email address.
    /// </summary>
    /// <param name="email">The user's email address from the social provider.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSocialLoginRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the display name from the social provider.
    /// </summary>
    /// <param name="userName">The user's display name from the social provider.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSocialLoginRequestBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    /// <summary>
    /// Sets the optional avatar URL.
    /// </summary>
    /// <param name="avatarUrl">Optional avatar URL from the social provider.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSocialLoginRequestBuilder WithAvatarUrl(string? avatarUrl)
    {
        _avatarUrl = avatarUrl;
        return this;
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
    /// Builds the <see cref="PublicSocialLoginRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicSocialLoginRequest instance.</returns>
    public PublicSocialLoginRequest Build()
    {
        return new PublicSocialLoginRequest(
            Email: _email,
            UserName: _userName,
            AvatarUrl: _avatarUrl,
            Provider: _provider
        );
    }
}
