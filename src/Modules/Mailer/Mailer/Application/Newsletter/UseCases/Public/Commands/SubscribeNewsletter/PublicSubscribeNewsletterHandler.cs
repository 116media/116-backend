using System.Globalization;
using _116.Mailer.Application.Newsletter.Services;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter;

/// <summary>
/// Handles the <see cref="PublicSubscribeNewsletterCommand" /> with double opt-in:
/// a new address gets a pending row and a confirmation email; a pending or
/// unsubscribed address gets a fresh confirmation; an already subscribed
/// address changes nothing. Every path reports success so the response never
/// reveals whether an address was known.
/// </summary>
/// <param name="newsletterRepository">Repository for subscriber persistence.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
/// <param name="mailer">The outbox mailer used to send the confirmation email.</param>
public class PublicSubscribeNewsletterHandler(
    INewsletterRepository newsletterRepository,
    IMailerUnitOfWork unitOfWork,
    IMailer mailer
) : ICommandHandler<PublicSubscribeNewsletterCommand, PublicSubscribeNewsletterResult>
{
    /// <summary>
    /// Handles the subscription request and (re)issues the confirmation email
    /// when the address is not already subscribed.
    /// </summary>
    public async Task<PublicSubscribeNewsletterResult> Handle(
        PublicSubscribeNewsletterCommand command,
        CancellationToken cancellationToken
    )
    {
        NewsletterSubscriberEntity? existing = await newsletterRepository.GetByEmailAsync(
            command.Email,
            cancellationToken
        );

        if (existing is { Status: EnumNewsletterStatus.Subscribed })
        {
            return new PublicSubscribeNewsletterResult(IsSuccess: true, Email: command.Email);
        }

        NewsletterSubscriberEntity subscriber;

        if (existing is null)
        {
            subscriber = NewsletterSubscriberEntity.Subscribe(id: Guid.NewGuid(), email: command.Email);
            await newsletterRepository.AddAsync(subscriber, cancellationToken);
        }
        else
        {
            subscriber = existing;
            subscriber.ReissueConfirmation();
        }

        await unitOfWork.CommitAsync(cancellationToken);

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.NewsletterConfirm,
            to: new EmailRecipient(subscriber.Email),
            tokens: new Dictionary<string, string>
            {
                ["confirmUrl"] = NewsletterLinkBuilder.ConfirmUrl(subscriber.ConfirmationToken),
            },
            culture: CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            cancellationToken: cancellationToken
        );

        return new PublicSubscribeNewsletterResult(IsSuccess: true, Email: command.Email);
    }
}
