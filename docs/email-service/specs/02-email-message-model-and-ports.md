# Spec 02 — Email Message Model and Ports

## Goal

Define the two seams that make the provider swappable, and the message model
they exchange. Everything else in the email service is an implementation
detail behind one of these two interfaces.

## The two ports — and why two

| Port | Layer that calls it | Responsibility |
| --- | --- | --- |
| `IMailer` | Business code (Identity handlers, newsletter use cases) | "Send template X to user Y with these tokens" — enqueues, never talks to a provider |
| `IEmailSender` | Outbox dispatcher only | "Deliver this fully rendered `EmailMessage` now" — one provider call |

Splitting them keeps provider swap *and* delivery policy independent:

- Swapping SMTP → Resend touches only `IEmailSender` adapters (spec 03).
- Changing delivery policy (retry, batching, throttling) touches only the
  dispatcher (spec 05).
- Business code sees neither; it cannot even express a provider-specific
  concept.

## Message model

Declared in `Application/Shared/Services/` **next to the port that exchanges
them** — the codebase has no `Models/` folder; its convention for in-process
carrier records is co-location with the owning port (`ImageColors` inside
`IImageColorService.cs`, `ArtistDirectoryRow`/`ArtistTotals` inside
`IArtistRepository.cs`, `AuthorInfo` beside `IUserLookupService`). These two
records live with `IEmailSender`:

```csharp
/// <summary>
/// A fully rendered, provider-agnostic email ready for transport.
/// </summary>
public record EmailMessage(
    EmailRecipient To,
    string Subject,
    string HtmlBody,
    string TextBody
);

/// <summary>
/// The destination of an email: address plus optional display name.
/// </summary>
public record EmailRecipient(string Address, string? DisplayName = null);
```

Rules:

- **No attachments, no CC/BCC, no custom headers in v1.** Every current use
  case (OTP, welcome, login alert, newsletter) is a single-recipient simple
  send. Fields are added when a use case needs them, not speculatively.
- `HtmlBody` and `TextBody` are both mandatory — every template renders both
  (spec 04); the text part keeps spam scores down and screen readers happy.
- The sender identity (`from` address/name) is **not** on the message: it is
  environment configuration (`EMAIL_FROM_ADDRESS` / `EMAIL_FROM_NAME`, spec
  08) applied by the adapter. Business code must not be able to spoof it.

## IEmailSender — the transport port

`Application/Shared/Services/IEmailSender.cs`:

```csharp
/// <summary>
/// Transport seam to a concrete email provider. Implementations perform one
/// delivery attempt and surface failures as <see cref="EmailDeliveryException"/>;
/// retry policy belongs to the caller, never to the adapter.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
```

Failure contract (mirrors `StreamingLinkResolutionException`):

```csharp
public class EmailDeliveryException(string message, bool isTransient = true)
    : Exception(message)
{
    /// <summary>
    /// Transient failures (timeouts, 5xx, connection refused) are retried by
    /// the dispatcher; permanent ones (invalid recipient, rejected sender)
    /// mark the outbox row failed immediately.
    /// </summary>
    public bool IsTransient { get; } = isTransient;
}
```

## IMailer — the application port

`Application/Shared/Services/IMailer.cs`:

```csharp
/// <summary>
/// Enqueues templated emails for reliable background delivery. The write is
/// transactional with the caller's unit of work: if the business change
/// rolls back, so does the email.
/// </summary>
public interface IMailer
{
    Task EnqueueAsync(
        EnumEmailTemplate template,
        EmailRecipient to,
        IReadOnlyDictionary<string, string> tokens,
        string culture,
        CancellationToken cancellationToken
    );
}
```

- `template` names an entry in the catalog (spec 04); rendering happens at
  enqueue time so the outbox row is self-contained and replayable even if a
  template changes later.
- `tokens` carries the dynamic values (`otpCode`, `userName`, `unsubscribeUrl`,
  …); the template defines which tokens it requires and rendering throws on a
  missing one — a missing token is a programming error, not a runtime state.
- `culture` is the two-letter request culture (`"en"`, `"fr"`); resolution
  falls back to the neutral resource exactly like the error-message localizers.

## Consumption from other modules

`IMailer` is registered by `MailerModule` and injected by any handler that
needs it (the DI container is shared across modules — same mechanism Identity
already uses when Content-hosted requests resolve Identity services). Identity
gains **no project reference** to Mailer's infrastructure; if solution layout
requires a compile-time contract, `IMailer` + models move to
`Shared.Contracts` following `ICommand`/`IQuery` precedent — decide at
implementation time based on the existing project graph, and record the choice
here.

## Checklist

- [x] `EmailMessage`, `EmailRecipient` records with XML docs
- [x] `IEmailSender` + `EmailDeliveryException` (transient flag)
- [x] `IMailer` port with template/tokens/culture signature
- [x] No provider type, header, or SDK concept leaks into any of these files
- [x] Unit tests: exception flag defaults, record equality (spec 09 lists them)

## Implementation notes

- The compile-time contract decision came out as its own project:
  `Mailer.Contracts` holds `IMailer`, `EmailRecipient`, `EnumEmailTemplate`
  and the `EmailCulture` helper, following the `Identity.Contracts`
  precedent. `IEmailSender`, `EmailMessage` and `EmailDeliveryException`
  stay inside the Mailer project — only the dispatcher and adapters see them.
- **Atomicity revision**: module DbContexts do not share connections or
  transactions in this monolith, so "enqueue joins the caller's unit of work"
  is not implementable across modules. `IMailer.EnqueueAsync` renders and
  commits immediately in the Mailer context; consumers call it **after**
  their own commit. A rolled-back operation therefore still sends nothing;
  the residual window is a crash between business commit and enqueue, which
  every OTP/notification flow tolerates (resend paths exist). Newsletter
  flows write subscriber and outbox rows through the same context and remain
  fully atomic.
