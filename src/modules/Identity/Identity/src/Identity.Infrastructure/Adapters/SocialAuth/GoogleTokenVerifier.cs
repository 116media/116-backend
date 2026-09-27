using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace _116.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Verifies a Google ID token via <see cref="GoogleJsonWebSignature" />, pinning the audience to the
/// configured client id so a token minted for another app is rejected. Maps Google's payload to
/// <see cref="SocialTokenPayload" />; a token that does not verify throws
/// <see cref="SocialTokenVerificationException" /> so the strategy handler owns the user-facing error
/// and the adapter stays i18n-free.
/// </summary>
/// <param name="options">The social-auth provider credentials.</param>
public sealed class GoogleTokenVerifier(IOptions<SocialAuthOptions> options) : ISocialTokenVerifier
{
    private readonly SocialAuthOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [_options.GoogleClientId] };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new SocialTokenPayload(
                Name: payload.Name,
                Email: payload.Email,
                EmailVerified: payload.EmailVerified,
                ProviderSubjectId: payload.Subject,
                PictureUrl: payload.Picture
            );
        }
        catch (Exception ex) when (ex is InvalidJwtException or ArgumentException)
        {
            // A token that is malformed (bad segments) or fails signature/claim validation.
            throw new SocialTokenVerificationException();
        }
    }
}
