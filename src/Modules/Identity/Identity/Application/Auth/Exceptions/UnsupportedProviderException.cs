using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.Exceptions;

/// <summary>
/// Raised when no verifier is registered for a social provider. Carries the provider so its strategy
/// handler can name it in the localized detail.
/// </summary>
/// <param name="provider">The provider that has no registered verifier.</param>
public class UnsupportedProviderException(EnumAuthProvider provider) : Exception
{
    /// <summary>
    /// The provider that has no registered verifier.
    /// </summary>
    public EnumAuthProvider Provider { get; } = provider;
}
