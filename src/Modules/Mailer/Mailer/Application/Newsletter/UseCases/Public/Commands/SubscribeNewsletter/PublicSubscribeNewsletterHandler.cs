using System.Globalization;
using _116.BuildingBlocks.Constants;
using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Newsletter.Services;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Domain.Entities;
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
/// <param name="messageDispatcher">Dispatcher routing the confirmation to its recipient.</param>
public class PublicSubscribeNewsletterHandler(
    INewsletterRepository newsletterRepository,
    IMailerUnitOfWork unitOfWork,
    IMessageDispatcher messageDispatcher
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

        NewsletterSubscriberEntity subscriber;

        if (existing is null)
        {
            subscriber = NewsletterSubscriberEntity.Subscribe(id: Guid.NewGuid(), email: command.Email);
            await newsletterRepository.AddAsync(subscriber, cancellationToken);
        }
        else
        {
            subscriber = existing;

            if (!subscriber.ReissueConfirmation())
            {
                return new PublicSubscribeNewsletterResult(IsSuccess: true, Email: command.Email);
            }
        }

        await unitOfWork.CommitAsync(cancellationToken);

        var message = new NewsletterConfirmMessage(
            Subscriber: new MessageRecipient(
                UserId: null,
                Address: subscriber.Email,
                DisplayName: null,
                Locale: UserConstants.DefaultLocale
            ),
            ConfirmUrl: NewsletterLinkBuilder.ConfirmUrl(subscriber.ConfirmationToken)
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);

        return new PublicSubscribeNewsletterResult(IsSuccess: true, Email: command.Email);
    }
}
