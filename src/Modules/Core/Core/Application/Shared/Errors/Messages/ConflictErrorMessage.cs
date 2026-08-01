using Microsoft.Extensions.Localization;

namespace _116.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Provides conflict-related error messages for the <c>Core</c> domain.
/// These messages describe situations where operations cannot proceed due to conflicts.
/// </summary>
public class ConflictErrorMessage(IStringLocalizer<ConflictErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;
}
