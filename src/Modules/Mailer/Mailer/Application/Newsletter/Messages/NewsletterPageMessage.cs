using _116.Mailer.Application.Newsletter.Messages;
using Microsoft.Extensions.Localization;

namespace _116.Mailer.Application.Newsletter.Messages;

/// <summary>
/// Localized copy for the confirm and unsubscribe landing pages. The pages exist so the state
/// change rides a POST that link scanners do not follow.
/// </summary>
public class NewsletterPageMessage(IStringLocalizer<NewsletterPageMessage> localizer)
{
    /// <summary>
    /// Heading shown on the subscription confirmation page.
    /// </summary>
    public string ConfirmTitle() => localizer["ConfirmTitle"];

    /// <summary>
    /// Prompt asking the reader to confirm their subscription.
    /// </summary>
    public string ConfirmPrompt() => localizer["ConfirmPrompt"];

    /// <summary>
    /// Label on the button that confirms the subscription.
    /// </summary>
    public string ConfirmButton() => localizer["ConfirmButton"];

    /// <summary>
    /// Heading shown on the unsubscribe page.
    /// </summary>
    public string UnsubscribeTitle() => localizer["UnsubscribeTitle"];

    /// <summary>
    /// Prompt asking the reader to confirm they want to stop receiving the newsletter.
    /// </summary>
    public string UnsubscribePrompt() => localizer["UnsubscribePrompt"];

    /// <summary>
    /// Label on the button that performs the unsubscribe.
    /// </summary>
    public string UnsubscribeButton() => localizer["UnsubscribeButton"];
}
