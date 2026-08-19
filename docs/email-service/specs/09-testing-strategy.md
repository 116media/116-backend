# Spec 09 — Testing Strategy

Everything here follows
[docs/testing/00-unit-vs-integration-rules.md](../../testing/00-unit-vs-integration-rules.md)
— unit tests prove methods work, integration tests prove they are wired, and
external-service stubs are the only permitted integration mocks.

## The stub — StubEmailSender

`tests/Integration/Common/Stubs/StubEmailSender.cs`, registered by the test
host exactly like `StubCloudinaryService`:

```csharp
/// <summary>
/// Records every message the dispatcher hands it instead of contacting a
/// provider. Integration tests assert on <see cref="Sent"/> and on the
/// outbox rows in Postgres.
/// </summary>
public class StubEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}
```

A settable `NextFailure` property (an `EmailDeliveryException?` thrown once)
lets integration tests drive the retry path without any provider.

## Unit tests (`tests/Unit/Modules/Mailer/`)

| Area | Cases |
| --- | --- |
| `OutboxEmailEntity` | `Enqueue` state, `MarkSent`, `RegisterFailure` backoff progression, terminal `Failed` after max attempts, permanent-failure short-circuit, idempotent guards |
| `NewsletterSubscriberEntity` | subscribe/confirm/unsubscribe transitions, idempotent re-clicks, re-subscribe token reissue |
| `EmailTemplateRenderer` | every template × {en, fr} renders with zero leftover `{{…}}`; missing token throws; HTML-encoding of token values |
| `Mailer` (the `IMailer` impl) | renders + adds to repository, never commits; culture passthrough |
| Dispatcher orchestration | batch loop with mocked `IEmailSender`/repository: success marks sent, transient failure schedules retry, permanent failure fails row, one throwing message doesn't stop the batch |
| Validators | newsletter email rules, token format rules |
| Adapters' pure parts | failure-classification mapping (status code → transient flag) extracted into a testable method |

## Integration tests (`tests/Integration/Modules/Mailer/`)

Real entry points only:

| Test class | Entry point | Asserts |
| --- | --- | --- |
| `PublicSubscribeNewsletterEndpointV1Tests` | real HTTP | 202 always; subscriber row + `NewsletterConfirm` outbox row; duplicate subscribe stays neutral; invalid email 400 |
| `PublicConfirmNewsletterEndpointV1Tests` | real HTTP | pending→subscribed; `NewsletterWelcome` outbox row; unknown token 404; re-click 200 idempotent |
| `PublicUnsubscribeNewsletterEndpointV1Tests` | real HTTP | subscribed→unsubscribed; idempotent; 404 unknown |
| `AdminGetNewsletterSubscribersEndpointV1Tests` | real HTTP | auth matrix (401/403), pagination, status filter |
| Identity flows (extend existing endpoint tests) | real HTTP | forgot-password/signup/resend enqueue the right template + tokens as outbox rows; **unknown-email forgot-password enqueues nothing and still 200** |
| `OutboxEmailRepositoryTests` | repository from DI | batch scan honors `(status, next_attempt_at)`; skip-locked claim |
| `SmtpEmailSenderTests` | loopback SMTP | see below |
| `ResendEmailSenderTests` | loopback HTTP | Odesli-pattern `HttpListener`: request body/bearer asserted; 429 ⇒ transient, 422 ⇒ permanent, 500 ⇒ transient, connection refused ⇒ transient |

### Loopback SMTP

Mirror the Odesli loopback approach at the TCP level: a minimal in-test
SMTP session (accept socket, speak `220/250/354/250`, capture the DATA blob)
on a random free port — enough to assert the wire contract (MAIL FROM from
config, RCPT TO, multipart body contains both parts) and the failure mapping
(reply `554` ⇒ permanent; refuse the connection ⇒ transient). If the raw
socket script proves brittle, the fallback is running these assertions against
Mailpit's API in a dev-only test category — decide during implementation and
record the choice here.

### Dispatcher end-to-end

One integration test drives the real hosted service once (`ExecuteAsync` with
a short-circuited interval or an internal `RunOnceAsync` seam): enqueue via a
real HTTP flow → run dispatcher → `StubEmailSender.Sent` has the message and
the row is `Sent`; with `NextFailure` set → row shows `attempt_count = 1` and
a future `next_attempt_at`.

## Coverage posture

Same as every module: unit owns guards/transitions exhaustively; integration
proves each piece is wired through real HTTP/repository/loopback. Anything
integration-unreachable follows the measured-proof process used for the
artist-page coverage rounds before being called unreachable.

## Checklist

- [x] `StubEmailSender` registered in the test host, with `NextFailure`
- [x] Unit suites above green
- [x] Endpoint + repository + loopback adapter integration suites green
- [x] Enumeration-safety and atomicity (rollback ⇒ no row) covered
- [x] Dispatcher end-to-end test green

## Implementation notes

- The loopback SMTP session is a raw `TcpListener` speaking
  220/250/354/250 with a scripted 554 recipient-rejection variant — the
  socket script proved stable, so the Mailpit fallback was not needed.
- The dispatcher end-to-end tests drive the real Quartz job's `Execute`
  directly with the host's scope factory and the singleton `StubEmailSender`
  (construction-of-infrastructure precedent: the Odesli loopback tests).
- Identity flow coverage lives in `Workflows/EmailDeliveryFlowTests`: signup
  persists the verification-OTP outbox row carrying the exact stored code,
  and unknown-email forgot-password stays a 200 with zero rows.
