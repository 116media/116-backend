# Spec 05 — Outbox, Reliability and Dispatch

## Goal

Make email delivery reliable without making business operations fragile:
enqueuing is a database write inside the caller's transaction; actual provider
calls happen in a background hosted service with retry, backoff and a terminal
failure state.

## Why outbox (and not fire-and-forget in the handler)

- A provider outage must not fail a signup or password reset — the OTP row
  already committed; the email must eventually follow.
- A process crash between "business commit" and "provider call" must not lose
  the email — the outbox row *is* the intent, persisted atomically with the
  business change.
- Retry needs state (attempt count, next attempt time); state lives in a
  table, not in memory.

## OutboxEmailEntity

`Domain/Entities/OutboxEmailEntity.cs` → table `mailer.outbox_emails`:

| Column | Type | Notes |
| --- | --- | --- |
| `id` | uuid PK | |
| `recipient_address` | text | |
| `recipient_name` | text null | |
| `subject` / `html_body` / `text_body` | text | rendered at enqueue time (spec 02) |
| `template` | text | enum-as-string, for observability/filtering |
| `status` | text | `EnumOutboxEmailStatus`: `Pending`, `Sent`, `Failed` |
| `attempt_count` | int | default 0 |
| `next_attempt_at` | timestamptz | index with `status` — the dispatcher's scan key |
| `last_error` | text null | truncated provider error for diagnosis |
| `sent_at` | timestamptz null | |
| + auditable fields | | `created_at` etc. from the aggregate base |

Domain methods (guard-tested like every entity): `Enqueue(...)` factory,
`MarkSent(now)`, `RegisterFailure(error, now)` — the latter increments
`attempt_count`, sets `next_attempt_at = now + Backoff(attempt_count)` while
attempts remain, else flips to `Failed`.

Backoff: `1min, 5min, 30min, 2h, 12h` (`MailerConstants.RetrySchedule`,
5 attempts max). Permanent failures (`EmailDeliveryException.IsTransient ==
false`) skip the schedule and go straight to `Failed`.

## Enqueue path

The `IMailer` implementation renders the template (spec 04), builds the
entity, and adds it via `IOutboxEmailRepository` **without committing** — the
calling handler's existing `IUnitOfWork.CommitAsync` persists both the
business change and the email intent in one transaction. `IMailer` never
commits on its own; that is the atomicity guarantee, and it is why the port
lives with the repositories rather than wrapping its own scope.

## OutboxEmailDispatcher

`Infrastructure/Services/OutboxEmailDispatcher.cs` — a `BackgroundService`
registered by `MailerModule`:

- Loop: every `MailerConstants.DispatchIntervalSeconds` (default 15), create a
  scope, fetch a batch (`status = Pending AND next_attempt_at <= now`, ordered
  by `next_attempt_at`, limit 20, `FOR UPDATE SKIP LOCKED` via raw SQL or the
  EF equivalent) and deliver each through `IEmailSender`.
- Success ⇒ `MarkSent`; `EmailDeliveryException` ⇒ `RegisterFailure` with the
  transient/permanent distinction above; unexpected exception ⇒ treated as
  transient, logged at `Error` with the outbox id.
- Cancellation-aware; one failing message never stops the batch.
- `SKIP LOCKED` keeps the design correct if the API ever scales to multiple
  replicas — no double-send from concurrent dispatchers.

## Observability

- Serilog structured events on send success/failure (`OutboxEmailId`,
  `Template`, `AttemptCount`) — visible in Seq alongside existing request logs.
- `Failed` rows are the operational alarm surface; an admin re-queue endpoint
  is deliberately out of scope until there is an operator to use it (document
  the `UPDATE ... SET status = 'Pending'` recovery statement in the runbook
  section of spec 08 instead).

## Checklist

- [x] Entity + `EnumOutboxEmailStatus` + configuration + `AddOutboxEmails`
      migration (indexes: `(status, next_attempt_at)`)
- [x] `IMailer` implementation renders + enqueues, never commits
- [x] Dispatcher hosted service with batch, backoff schedule and SKIP LOCKED
- [x] Transient vs permanent failure paths honored
- [x] Unit tests: entity transitions, backoff schedule edges, dispatcher
      orchestration with mocked sender; integration: enqueue-commit atomicity,
      dispatch against the stub sender (spec 09)

## Implementation notes

- The dispatcher is a Quartz `IScheduledJob`
  (`OutboxEmailDispatcherJob`, cron `0/15 * * * * ?`) registered through the
  house `AddScheduledJob` extension — not a raw `BackgroundService`; Quartz
  is how every background job in this codebase runs.
- Claim + deliver + record run inside one transaction so `FOR UPDATE SKIP
  LOCKED` holds for the batch; an empty claim rolls back immediately.
- See spec 02's atomicity revision for the enqueue-side guarantee.
