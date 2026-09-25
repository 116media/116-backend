using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterUnsubscribePage;

/// <summary>
/// Contains metadata information for the newsletter unsubscribe page route.
/// </summary>
public static class PublicGetNewsletterUnsubscribePageMetaField
{
    public static readonly RouteMetadata GetNewsletterUnsubscribePage = new(
        "PublicGetNewsletterUnsubscribePage",
        "Render the page the unsubscribe link opens",
        """
            Returns a small HTML page whose single button posts the token back to the same\n
            route. The GET changes nothing, so a mail client preview or a link scanner\n
            fetching the URL cannot unsubscribe on the recipient's behalf.\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the page document.
        """
    );
}
