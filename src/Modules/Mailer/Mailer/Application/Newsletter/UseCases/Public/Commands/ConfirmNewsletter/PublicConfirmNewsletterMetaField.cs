using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter;

/// <summary>
/// Contains metadata information for the newsletter confirmation route.
/// </summary>
public static class PublicConfirmNewsletterMetaField
{
    public static readonly RouteMetadata ConfirmNewsletter = new(
        "PublicConfirmNewsletter",
        "Confirm a newsletter subscription from the emailed link",
        """
            Completes the double opt-in: the token from the confirmation email flips the\n
            pending subscriber to subscribed and triggers the welcome email carrying the\n
            unsubscribe link.\n
            \n
            **Behavior:**\n
            - First click: subscriber becomes subscribed, welcome email is sent\n
            - Re-click: succeeds with no side effects (idempotent)\n
            - Unknown or rotated token: 404 problem\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the subscription state\n
            - Returns 404 Not Found for an unknown token\n
            - Returns 400 Bad Request for a missing token.
        """
    );
}
