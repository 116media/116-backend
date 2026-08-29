using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Resolves the keyed <see cref="ISocialTokenVerifier" /> registered for a provider.
/// </summary>
/// <param name="serviceProvider">The container the keyed verifiers are registered in.</param>
public sealed class SocialTokenVerifierFactory(IServiceProvider serviceProvider) : ISocialTokenVerifierFactory
{
    /// <inheritdoc />
    public ISocialTokenVerifier For(EnumAuthProvider provider)
    {
        ISocialTokenVerifier? verifier = serviceProvider.GetKeyedService<ISocialTokenVerifier>(provider);

        if (verifier is not null)
        {
            return verifier;
        }
        else
        {
            throw new UnsupportedProviderException(provider);
        }
    }
}
