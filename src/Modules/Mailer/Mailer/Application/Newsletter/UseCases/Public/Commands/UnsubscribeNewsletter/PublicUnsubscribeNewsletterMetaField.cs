using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter;

/// <summary>
/// Contains metadata information for the newsletter unsubscription route.
/// </summary>
public static class PublicUnsubscribeNewsletterMetaField
{
    public static readonly RouteMetadata UnsubscribeNewsletter = new(
        "PublicUnsubscribeNewsletter",
        "Opt out of the newsletter from the emailed link",
        """
            One-click unsubscribe: the token from any newsletter email opts the\n
            subscriber out immediately. No further email is sent.\n
            \n
            **Behavior:**\n
            - First click: subscriber becomes unsubscribed\n
            - Re-click: succeeds with no side effects (idempotent)\n
            - Unknown token: 404 problem\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the opt-out state\n
            - Returns 404 Not Found for an unknown token\n
            - Returns 400 Bad Request for a missing token.
        """
    );
}
