using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the module's shared value objects.
/// </summary>
public class ValueObjectErrorMessage(IStringLocalizer<ValueObjectErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a slug is not well-formed.
    /// </summary>
    /// <param name="slug">The rejected slug.</param>
    /// <returns>A formatted error message indicating the expected slug format.</returns>
    public string InvalidSlug(string slug)
    {
        return string.Format(localizer["InvalidSlug"], slug);
    }

    /// <summary>
    /// Gets an error message for when a monetary amount is negative.
    /// </summary>
    /// <param name="amount">The rejected amount.</param>
    /// <returns>A formatted error message indicating that the amount cannot be negative.</returns>
    public string NegativeMoneyAmount(string amount)
    {
        return string.Format(localizer["NegativeMoneyAmount"], amount);
    }
}
