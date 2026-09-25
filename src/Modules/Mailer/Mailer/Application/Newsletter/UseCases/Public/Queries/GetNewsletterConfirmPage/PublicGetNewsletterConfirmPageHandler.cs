using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Newsletter.Pages;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterConfirmPage;

/// <summary>
/// Renders the confirm page in the request's culture. The page exists so the state change
/// happens on a POST: link scanners and mail previewers follow GETs, not forms.
/// </summary>
/// <param name="page">The localized page copy.</param>
public class PublicGetNewsletterConfirmPageHandler(NewsletterPageMessage page)
    : IQueryHandler<PublicGetNewsletterConfirmPageQuery, PublicGetNewsletterConfirmPageResult>
{
    /// <inheritdoc />
    public Task<PublicGetNewsletterConfirmPageResult> Handle(
        PublicGetNewsletterConfirmPageQuery query,
        CancellationToken cancellationToken = default
    )
    {
        string html = SubscriptionPage.Render(
            title: page.ConfirmTitle(),
            prompt: page.ConfirmPrompt(),
            buttonLabel: page.ConfirmButton(),
            action: query.Action
        );

        return Task.FromResult(new PublicGetNewsletterConfirmPageResult(Html: html));
    }
}
