using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;

namespace _116.Mailer.Infrastructure.Services;

/// <summary>
/// The <see cref="IMailer" /> implementation: renders the template and
/// persists a self-contained pending outbox row in the Mailer module's own
/// context. The write commits immediately — callers enqueue after their own
/// business change has committed, so a rolled-back operation never leaves an
/// email behind.
/// </summary>
/// <param name="renderer">The template renderer.</param>
/// <param name="outboxRepository">The outbox persistence port.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
public class OutboxMailer(
    IEmailTemplateRenderer renderer,
    IOutboxEmailRepository outboxRepository,
    IMailerUnitOfWork unitOfWork
) : IMailer
{
    /// <inheritdoc />
    public async Task EnqueueAsync(
        EnumEmailTemplate template,
        EmailRecipient to,
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
