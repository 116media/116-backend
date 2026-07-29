using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter;

/// <summary>
/// Handles the <see cref="PublicUnsubscribeNewsletterCommand" />: resolves the
/// token and opts the subscriber out. No goodbye email is sent — emailing
/// someone who just opted out defeats the point.
/// </summary>
/// <param name="newsletterRepository">Repository for subscriber persistence.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
/// <param name="errors">Newsletter error factory for unknown tokens.</param>
public class PublicUnsubscribeNewsletterHandler(
    INewsletterRepository newsletterRepository,
    IMailerUnitOfWork unitOfWork,
    NewsletterErrors errors
) : ICommandHandler<PublicUnsubscribeNewsletterCommand, PublicUnsubscribeNewsletterResult>
{
    /// <summary>
    /// Handles the unsubscription idempotently.
    /// </summary>
    public async Task<PublicUnsubscribeNewsletterResult> Handle(
        PublicUnsubscribeNewsletterCommand command,
        CancellationToken cancellationToken
    )
    {
        NewsletterSubscriberEntity subscriber =
            await newsletterRepository.GetByUnsubscribeTokenAsync(command.Token, cancellationToken)
            ?? throw errors.TokenNotFound();

        bool changed = subscriber.Unsubscribe(now: DateTime.UtcNow);

        if (changed)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return new PublicUnsubscribeNewsletterResult(
            IsUnsubscribed: subscriber.Status == EnumNewsletterStatus.Unsubscribed
        );
    }
}
