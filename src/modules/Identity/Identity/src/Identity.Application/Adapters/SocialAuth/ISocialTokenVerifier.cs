using _116.Identity.Application.Auth.Exceptions;

namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// The identity a social provider asserts once its token has been cryptographically verified. Every
/// field is taken from the verified token, never from the client request.
/// </summary>
/// <param name="ProviderSubjectId">
/// The provider's stable, opaque user identifier (Google <c>sub</c>, Facebook user id). Immutable for
/// the life of the account and the primary match key.
/// </param>
/// <param name="Email">The provider-asserted email address.</param>
/// <param name="EmailVerified">Whether the provider vouches the email is verified.</param>
/// <param name="Name">The display name, when the provider supplies one.</param>
/// <param name="PictureUrl">The avatar URL, when the provider supplies one.</param>
public sealed record SocialTokenPayload(
    string ProviderSubjectId,
    string Email,
    bool EmailVerified,
    string? Name,
    string? PictureUrl
);

/// <summary>
/// Verifies a social provider's identity token and translates the provider's model into a
/// <see cref="SocialTokenPayload" />. Throws <see cref="SocialTokenVerificationException" /> when the
/// token cannot be verified (bad signature, expired, wrong audience or app) — the strategy handler
/// turns that into the user-facing error, so the adapter never touches i18n.
/// </summary>
public interface ISocialTokenVerifier
{
    /// <summary>
    /// Verifies <paramref name="idToken" /> with the provider and returns the asserted identity.
    /// </summary>
    /// <param name="idToken">The provider-issued token to verify.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The asserted identity.</returns>
    Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken);
}
