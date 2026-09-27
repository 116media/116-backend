using System.Net.Http.Json;
using System.Text.Json.Serialization;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;
using Microsoft.Extensions.Options;

namespace _116.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Verifies a Facebook access token by calling the Graph API's <c>debug_token</c> endpoint with the
/// app's own <c>{app-id}|{app-secret}</c> token, then reads the profile. A token whose <c>is_valid</c>
/// is false or whose <c>app_id</c> is not ours throws <see cref="SocialTokenVerificationException" />.
/// Facebook only returns an email once the user has confirmed it, so a present email is treated as
/// verified. The strategy handler owns the user-facing error, so the adapter stays i18n-free.
/// </summary>
/// <param name="httpClient">The Graph API client (base address configured at registration).</param>
/// <param name="options">The social-auth provider credentials.</param>
public sealed class FacebookTokenVerifier(HttpClient httpClient, IOptions<SocialAuthOptions> options)
    : ISocialTokenVerifier
{
    private readonly SocialAuthOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        string appToken = $"{_options.FacebookAppId}|{_options.FacebookAppSecret}";

        DebugTokenResponse? debug = await httpClient.GetFromJsonAsync<DebugTokenResponse>(
            $"{SocialAuthConstants.FacebookDebugTokenEndpoint}"
                + $"?input_token={Uri.EscapeDataString(idToken)}&access_token={Uri.EscapeDataString(appToken)}",
            cancellationToken
        );

        if (debug?.Data is not { IsValid: true } data || data.AppId != _options.FacebookAppId)
        {
            throw new SocialTokenVerificationException();
        }

        ProfileResponse? profile = await httpClient.GetFromJsonAsync<ProfileResponse>(
            $"{SocialAuthConstants.FacebookProfileEndpoint}"
                + $"?fields={SocialAuthConstants.FacebookProfileFields}&access_token={Uri.EscapeDataString(idToken)}",
            cancellationToken
        );

        if (profile is null || string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new SocialTokenVerificationException();
        }

        return new SocialTokenPayload(
            Name: profile.Name,
            ProviderSubjectId: profile.Id,
            Email: profile.Email ?? string.Empty,
            EmailVerified: !string.IsNullOrWhiteSpace(profile.Email),
            PictureUrl: profile.Picture?.Data?.Url
        );
    }

    private sealed record DebugTokenResponse([property: JsonPropertyName("data")] DebugTokenData? Data);

    private sealed record DebugTokenData(
        [property: JsonPropertyName("is_valid")] bool IsValid,
        [property: JsonPropertyName("app_id")] string? AppId
    );

    private sealed record ProfileResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("picture")] PictureNode? Picture
    );

    private sealed record PictureNode([property: JsonPropertyName("data")] PictureData? Data);

    private sealed record PictureData([property: JsonPropertyName("url")] string? Url);
}
