using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterUnsubscribePage;

/// <summary>
/// Query for the page the unsubscribe link opens, whose form posts the token back.
/// </summary>
/// <param name="Action">The route the rendered form posts to.</param>
public record PublicGetNewsletterUnsubscribePageQuery(string Action) : IQuery<PublicGetNewsletterUnsubscribePageResult>;

/// <summary>
/// Result of the <see cref="PublicGetNewsletterUnsubscribePageQuery" />.
/// </summary>
/// <param name="Html">The rendered page document.</param>
public record PublicGetNewsletterUnsubscribePageResult(string Html);
