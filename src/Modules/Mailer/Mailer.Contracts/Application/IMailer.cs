namespace _116.Mailer.Contracts.Application;

/// <summary>
/// Enqueues templated emails for reliable background delivery through the
/// Mailer module's outbox. Enqueuing renders the template immediately and
/// persists a self-contained outbox row; a background dispatcher performs the
/// provider call with retry, so callers never wait on — or fail because of —
/// an email provider.
/// </summary>
public interface IMailer
{
    /// <summary>
    /// Renders the given template in the given culture and persists it as a
    /// pending outbox email. The write is committed immediately in the Mailer
    /// module's own context; call it after the triggering business change has
    /// been committed. A missing required token throws — that is a programming
    /// error, never a runtime state.
    /// </summary>
    /// <param name="template">The template to render, from the catalog.</param>
    /// <param name="to">The recipient of the email.</param>
    /// <param name="tokens">The dynamic values the template requires.</param>
    /// <param name="culture">The two-letter request culture (e.g. "en", "fr").</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task EnqueueAsync(
        EnumEmailTemplate template,
        EmailRecipient to,
        IReadOnlyDictionary<string, string> tokens,
        string culture,
        CancellationToken cancellationToken
    );
}
