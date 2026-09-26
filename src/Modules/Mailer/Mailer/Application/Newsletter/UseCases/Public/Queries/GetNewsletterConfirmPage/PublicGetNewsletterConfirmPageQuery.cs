using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterConfirmPage;

/// <summary>
/// Query for the page the confirm link opens, whose form posts the token back.
/// </summary>
/// <param name="Action">The route the rendered form posts to.</param>
public record PublicGetNewsletterConfirmPageQuery(string Action) : IQuery<PublicGetNewsletterConfirmPageResult>;

/// <summary>
/// Result of the <see cref="PublicGetNewsletterConfirmPageQuery" />.
/// </summary>
/// <param name="Html">The rendered page document.</param>
public record PublicGetNewsletterConfirmPageResult(string Html);
