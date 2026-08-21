using System.Globalization;
using _116.Mailer.Application.Newsletter.Services;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter;

/// <summary>
/// Handles the <see cref="PublicConfirmNewsletterCommand" />: resolves the
/// token, flips the pending subscriber to subscribed, and sends the welcome
/// email carrying the unsubscribe link. Re-clicks are no-ops without a second
/// welcome email.
/// </summary>
/// <param name="newsletterRepository">Repository for subscriber persistence.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
/// <param name="mailer">The outbox mailer used to send the welcome email.</param>
/// <param name="errors">Newsletter error factory for unknown tokens.</param>
public class PublicConfirmNewsletterHandler(
    INewsletterRepository newsletterRepository,
    IMailerUnitOfWork unitOfWork,
    IMailer mailer,
    NewsletterErrors errors
) : ICommandHandler<PublicConfirmNewsletterCommand, PublicConfirmNewsletterResult>
{
    /// <summary>
    /// Handles the confirmation and sends the welcome email on the first click.
    /// </summary>
    public async Task<PublicConfirmNewsletterResult> Handle(
        PublicConfirmNewsletterCommand command,
        CancellationToken cancellationToken
    )
    {
        NewsletterSubscriberEntity subscriber =
            await newsletterRepository.GetByConfirmationTokenAsync(command.Token, cancellationToken)
            ?? throw errors.TokenNotFound();

        bool confirmed = subscriber.Confirm(now: DateTime.UtcNow);

        if (confirmed)
        {
            await unitOfWork.CommitAsync(cancellationToken);

            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.NewsletterWelcome,
                to: new EmailRecipient(subscriber.Email),
                tokens: new Dictionary<string, string>
                {
                    ["unsubscribeUrl"] = NewsletterLinkBuilder.UnsubscribeUrl(subscriber.UnsubscribeToken),
                },
                culture: CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                cancellationToken: cancellationToken
            );
        }

        return new PublicConfirmNewsletterResult(IsSubscribed: subscriber.Status == EnumNewsletterStatus.Subscribed);
    }
}
