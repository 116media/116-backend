using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Newsletter.Pages;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterUnsubscribePage;

/// <summary>
/// Renders the unsubscribe page in the request's culture. The page exists so the state change
/// happens on a POST: link scanners and mail previewers follow GETs, not forms.
/// </summary>
/// <param name="page">The localized page copy.</param>
public class PublicGetNewsletterUnsubscribePageHandler(NewsletterPageMessage page)
    : IQueryHandler<PublicGetNewsletterUnsubscribePageQuery, PublicGetNewsletterUnsubscribePageResult>
{
    /// <inheritdoc />
    public Task<PublicGetNewsletterUnsubscribePageResult> Handle(
        PublicGetNewsletterUnsubscribePageQuery query,
        CancellationToken cancellationToken = default
    )
    {
        string html = SubscriptionPage.Render(
            title: page.UnsubscribeTitle(),
            prompt: page.UnsubscribePrompt(),
            buttonLabel: page.UnsubscribeButton(),
            action: query.Action
        );

        return Task.FromResult(new PublicGetNewsletterUnsubscribePageResult(Html: html));
    }
}
