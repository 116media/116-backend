using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for share interactions across every content type.
/// </summary>
public class ShareErrorMessage(IStringLocalizer<ShareErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a share channel value is not a known channel.
    /// </summary>
    /// <param name="channel">The rejected channel value.</param>
    /// <returns>
    /// A formatted error message indicating that the share channel is invalid.
    /// </returns>
    public string InvalidShareChannel(string channel)
    {
        return string.Format(localizer["InvalidShareChannel"], channel);
    }
}
