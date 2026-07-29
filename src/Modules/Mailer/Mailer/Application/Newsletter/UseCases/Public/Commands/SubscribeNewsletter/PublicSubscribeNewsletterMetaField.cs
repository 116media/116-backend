using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter;

/// <summary>
/// Contains metadata information for the newsletter subscription route.
/// </summary>
public static class PublicSubscribeNewsletterMetaField
{
    public static readonly RouteMetadata SubscribeNewsletter = new(
        "PublicSubscribeNewsletter",
        "Sign an email address up to the newsletter with double opt-in",
        """
            Starts a double opt-in newsletter subscription for the given email address.\n
            \n
            Behavior by current state:\n
            - Unknown address: a pending subscriber is created and a confirmation email is sent\n
            - Pending or unsubscribed address: a fresh confirmation email is issued\n
            - Already subscribed address: nothing changes and no email is sent\n
            \n
            **Security Features:**\n
            - Always answers 202 with a neutral body, so responses never reveal\n
              whether an address is already subscribed (no enumeration)\n
            - Only confirmed subscribers ever receive newsletter content\n
            \n
            **Response Codes:**\n
            - Returns 202 Accepted with the email address for client reference\n
            - Returns 400 Bad Request for a missing or malformed email address.
        """
    );
}
