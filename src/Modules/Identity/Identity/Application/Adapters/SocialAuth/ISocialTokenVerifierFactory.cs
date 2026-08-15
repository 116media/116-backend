using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// Resolves the <see cref="ISocialTokenVerifier" /> for a provider, throwing
/// <see cref="UnsupportedProviderException" /> when no adapter is registered — the strategy handler
/// turns that into the user-facing "unsupported provider" error.
/// </summary>
public interface ISocialTokenVerifierFactory
{
    /// <summary>
    /// Returns the verifier registered for <paramref name="provider" />.
    /// </summary>
    /// <param name="provider">The provider whose verifier is requested.</param>
    /// <returns>The verifier for the provider.</returns>
    ISocialTokenVerifier For(EnumAuthProvider provider);
}
