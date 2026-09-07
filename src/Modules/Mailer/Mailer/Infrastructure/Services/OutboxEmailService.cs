using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Mailer.Domain.Entities;

namespace _116.Mailer.Infrastructure.Services;

/// <summary>
/// The <see cref="IEmailService" /> implementation: renders the template and persists a self-contained
/// pending outbox row in the Mailer module's own context.
/// </summary>
/// <param name="renderer">The template renderer.</param>
/// <param name="outboxRepository">The outbox persistence port.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
public class OutboxEmailService(
    IEmailTemplateRenderer renderer,
    IOutboxEmailRepository outboxRepository,
    IMailerUnitOfWork unitOfWork
) : IEmailService
{
    /// <inheritdoc />
    public async Task EnqueueAsync(
        EnumEmailTemplate template,
        EmailRecipientDto to,
        IReadOnlyDictionary<string, string> tokens,
        string culture,
        CancellationToken cancellationToken
    )
    {
        RenderedEmail rendered = renderer.Render(template, tokens, culture);

        OutboxEmailEntity email = OutboxEmailEntity.Enqueue(
            id: Guid.NewGuid(),
            recipientAddress: to.Address,
            recipientName: to.DisplayName,
            subject: rendered.Subject,
            htmlBody: rendered.HtmlBody,
            textBody: rendered.TextBody,
            template: template.ToString(),
            now: DateTime.UtcNow
        );

        await outboxRepository.AddAsync(email, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
