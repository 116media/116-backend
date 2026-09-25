namespace _116.Mailer.Application.Newsletter.Constants;

/// <summary>
/// Contains route path constants for newsletter API endpoints. Centralising the segments keeps
/// the routes and the links embedded in newsletter emails from drifting apart.
/// </summary>
public static class NewsletterRouteConstants
{
    /// <summary>
    /// Route segment for the subscription collection.
    /// Example: /api/v1/public/newsletter/subscriptions.
    /// </summary>
    public const string Subscriptions = "subscriptions";

    /// <summary>
    /// Route segment for the double opt-in confirmation.
    /// Example: /api/v1/public/newsletter/confirm.
    /// </summary>
    public const string Confirm = "confirm";

    /// <summary>
    /// Route segment for the one-click opt-out.
    /// Example: /api/v1/public/newsletter/unsubscribe.
    /// </summary>
    public const string Unsubscribe = "unsubscribe";

    /// <summary>
    /// Route segment for the admin subscriber listing.
    /// Example: /api/v1/admin/newsletter/subscribers.
    /// </summary>
    public const string Subscribers = "subscribers";

    /// <summary>
    /// The confirmation route with its token parameter.
    /// </summary>
    public const string ConfirmWithToken = $"{Confirm}/{{token}}";

    /// <summary>
    /// The opt-out route with its token parameter.
    /// </summary>
    public const string UnsubscribeWithToken = $"{Unsubscribe}/{{token}}";
}
