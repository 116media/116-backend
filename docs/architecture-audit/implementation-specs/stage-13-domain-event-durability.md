# Stage 13 — Domain-event durability (identity, outbox, transaction boundary)

Closes **[01 §1.7]** (Critical), **[01 §1.8]**, **[02 §13]**, **[04 §7]** / **[06 §7]** (High),
**[02 §5]**, **[05 §12]**, **[05 §13]**.

Three load-bearing facts, all verified in the current tree:

- `IDomainEvent` declares `Guid EventId => Guid.NewGuid();` and `DateTime CreatedAt =>
  DateTime.Now;` as **default interface members** — every read mints a new id and a new local
  timestamp. No event can be deduplicated, correlated, or replayed.
- `DispatchDomainEventsInterceptor` buffers events at `SavingChanges` and dispatches them in
  `SavedChanges` — post-commit, in a fresh scope, with failures logged and dropped. A handler
  crash after commit silently loses the reaction (`[01 §1.7]` / `[02 §13]`).
- 11 handlers call `CommitAsync` more than once (`OrderPaidEffectsHandler` and
  `AdminUpdateCategoryHandler` three times each); a failure between commits strands
  half-applied state, and `AdminSetExclusiveCategoryHandler`'s two commits can empty the
  "exclusive category" mutex `[04 §7]` / `[06 §7]`.

Mailer has the same disease twice over: `OutboxEmailDispatcherJob` holds one Postgres
transaction **across every SMTP send in the batch** (`BeginTransactionAsync` → `DeliverAsync`
loop → `CommitAsync`), and enqueue is a second, separate transaction, so the verification email
can commit while the signup rolls back — or vice versa (`[05 §12]` / `[05 §13]`).

> Draft — finalized against the tree Stage 12 lands on.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Event identity | fix the default members in place, or a base record | **Base record.** Default interface members re-evaluate per read by design; only a stored property fixes it. `DomainEvent` stamps once at construction; the interface keeps only declarations. Every event record's declaration changes from `: IDomainEvent` to `: DomainEvent` — mechanical, ~40 records. |
| D2 | Durability model | replace in-process dispatch with an outbox, or add the outbox as the safety net | **Outbox as the guarantee, in-process dispatch as the fast path.** The interceptor already buffers the events at exactly the right moment (`SavingChanges`, same unit of work) — writing them to an outbox table there makes durability free. Post-commit dispatch marks the row done on success; the replay job only touches rows whose dispatch died. No latency added to the happy path. |
| D3 | Idempotency | processed-events table, or per-handler natural idempotency | **Processed-events table keyed on `(EventId, HandlerName)`.** Stage 8's counter deltas are *not* naturally idempotent (a replayed like event would double-increment); a uniform guard beats auditing every handler forever. |
| D4 | Transactions | wrap handlers ad hoc, or one seam on the unit of work | **One seam.** `IContentUnitOfWork` (and siblings) gain `ExecuteInTransactionAsync`, implemented through `CreateExecutionStrategy()` so it composes with Stage 10's `EnableRetryOnFailure` instead of fighting it. The 11 multi-commit handlers collapse to one commit inside it. |
| D5 | Cross-module file+content writes | distributed pattern, or same-DB transaction | **Same-DB transaction.** Both contexts target one Postgres; `TransactionScope`-style coordination via sharing the underlying connection at the unit-of-work seam makes the file row + content row atomic `[02 §5]`. The contract-level fix is Stage 14; this stage only stops the torn write. |
| D6 | Mailer dispatch | keep transactional claim, or claim-then-send | **Claim-then-send.** Short transaction sets `Claimed` + a lease expiry; sends happen with no transaction open; a second short write records `Sent`/`Failed`. A crashed dispatcher's leases expire and the rows are re-claimed. Enqueue stops opening its own transaction — the outbox row rides the caller's `SaveChanges` `[05 §13]`. |

---

## Checklist

- [x] 13.1 — `DomainEvent` base record; interface cleaned; all event records migrated
- [x] 13.2 — `domain_event_outbox` table per module schema + write in `SavingChanges`
- [x] 13.3 — Post-commit dispatch marks rows; clustered replay job re-dispatches dead rows
- [x] 13.4 — `processed_domain_events` idempotency guard in the event-handler pipeline
- [x] 13.5 — `ExecuteInTransactionAsync`; the multi-commit handlers collapse to one commit
- [x] 13.6 — File+content writes share one transaction
- [x] 13.7 — Mailer claim-then-send + transactional enqueue
- [x] 13.8 — Tests: kill-between-commit-and-dispatch → replay delivers exactly once
- [x] 13.9 — Verify (build 0/0, csharpier, unit, integration; migrations generated, unapplied)

---

## Part A — Event identity

`src/Shared/Shared/Domain/IDomainEvent.cs` today (the bug is the `=>`):

```csharp
public interface IDomainEvent
{
    Guid EventId => Guid.NewGuid();          // new value every read
    public DateTime CreatedAt => DateTime.Now; // local time, new every read
    public string EventType => GetType().AssemblyQualifiedName!;
}
```

After:

```csharp
namespace _116.Shared.Domain;

/// <summary>
/// A fact the domain records at the moment it happens. Identity and timestamp are stamped
/// once at construction so the event can be stored, deduplicated and replayed.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The event's stable unique identifier.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred, in UTC.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// The fully qualified name of the event type.
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base record for all domain events; stamps identity and time exactly once.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <inheritdoc />
    public string EventType => GetType().AssemblyQualifiedName!;
}
```

Every event record changes its base — e.g.
`public record ArticleEngagedEvent(Guid ArticleId, EnumEngagementKind Kind, int Delta) : DomainEvent;`.
Consumers of `CreatedAt` rename to `OccurredOn` (compiler-led sweep).

## Part B — The outbox

### 13.2 The table and the write

```csharp
namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// A domain event captured durably in the same transaction as the state change that raised it.
/// </summary>
public class OutboxEventEntity
{
    public Guid Id { get; private set; }                 // = the event's EventId
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!; // System.Text.Json of the record
    public DateTime OccurredOn { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    // Create/MarkDispatched/MarkFailed factory + mutators follow the house entity shape.
}
```

`DispatchDomainEventsInterceptor.CollectDomainEvents` already runs inside `SavingChanges` with
the change tracker live — the only addition is materializing the buffer into the same context:

```csharp
private static void CollectDomainEvents(DbContext? context)
{
    // …existing aggregate scan and buffer fill…

    foreach (IDomainEvent domainEvent in buffer)
    {
        context.Set<OutboxEventEntity>().Add(OutboxEventEntity.Create(domainEvent));
    }
}
```

Because the rows join the same `SaveChanges`, the event is durable **iff** the state change is —
no second transaction, no torn enqueue. Post-commit dispatch (unchanged mechanics) marks
`DispatchedAt` on success; the catch block that today only logs now records
`MarkFailed(exception.Message)`.

### 13.3 Replay

> **Deviation at implementation time — replay is a delivery path, not only a safety net.** The
> interceptor dispatches at save completion, which inside `ExecuteInTransactionAsync` (Part C)
> is *before* the commit: handlers would observe uncommitted state and a rollback would leave
> reactions behind — the hazard the interceptor's own remarks already documented. Since the
> outbox row is durable at that point, a save inside an explicit transaction now **skips**
> in-process dispatch and leaves delivery to the replay job, which by construction runs after
> the commit. Consequently the job ships **enabled** from the first deploy rather than dark for
> one release, and its cadence is one minute rather than five.


> **Two defects the 13.8 tests caught, both on the replay path only.** The replay job read its
> pending rows through contexts that default to no-tracking, so every `MarkDispatched` was
> applied to a detached row and dropped at save — rows replayed forever. And `DomainEvent`
> stamped `EventId` in a get-only property, which `JsonSerializer` cannot set, so rebuilding an
> event from its payload minted a **new** id on every replay and the `(EventId, HandlerName)`
> guard never matched: the one path the guard exists for was the one path it could not protect.
> Fixed by `AsTracking()` on the pending query and by making `EventId`/`OccurredOn` `init`-set so
> identity survives the round trip.

One Quartz job (clustered per Stage 10), per module context:

```csharp
[DisallowConcurrentExecution]
public class OutboxReplayJob(IServiceScopeFactory scopeFactory, ILogger<OutboxReplayJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // claim: DispatchedAt IS NULL AND AttemptCount < Max, oldest first, small batch
        // dispatch through the same IDomainEventDispatcher the interceptor uses
        // MarkDispatched / MarkFailed per row; dead rows (AttemptCount == Max) surface in a metric
    }
}
```

### 13.4 Idempotent consumers

The event-handler pipeline (where the interceptor resolves `IDomainEventHandler<T>`) gains one
guard around the invoke:

```csharp
bool fresh = await processedEvents.TryRecordAsync(
    eventId: domainEvent.EventId,
    handlerName: handler.GetType().Name,
    cancellationToken: cancellationToken
);

if (!fresh)
{
    return; // this handler already completed for this event — a replay, not a bug
}
```

`TryRecordAsync` is an `INSERT … ON CONFLICT DO NOTHING` on `(event_id, handler_name)` returning
the row count — the same set-based idiom Stage 8 established.

## Part C — One commit per handler

`IContentUnitOfWork` (and the Core/Identity/Mailer siblings) grow the seam:

```csharp
/// <summary>
/// Runs <paramref name="operation" /> inside one database transaction, composed with the
/// retrying execution strategy, committing once at the end.
/// </summary>
Task ExecuteInTransactionAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken = default
);
```

```csharp
/// <inheritdoc />
public Task ExecuteInTransactionAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken = default
)
{
    IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

    return strategy.ExecuteAsync(async ct =>
    {
        await using IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(ct);

        await operation(ct);
        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }, cancellationToken);
}
```

> **Census correction at implementation time.** A raw `CommitAsync` count over-reports: five of
> the named files commit twice on *mutually exclusive branches*, each followed by its own
> `return`, so no execution ever performs two commits — `PublicSubmitLyricsHandler`, the three
> `Upsert*Link` handlers, and `RefreshTokenFactory`. Six handlers were genuinely sequential and
> are the ones collapsed. Verification step 2 should therefore count sequential commits per
> path, not occurrences per file.

The multi-commit handlers (fresh census — the audit's 14 predates Stages 8–9):
`OrderPaidEffectsHandler` (3 commits), `AdminUpdateCategoryHandler` (3),
`AdminSetExclusiveCategoryHandler`, `AdminCreateCategoryHandler`, `AdminRemoveItemTierHandler`,
`AdminUpsertSingleStreamingLinkHandler`, `AdminUpsertAlbumStreamingLinkHandler`,
`AdminUpsertArtistSocialLinkHandler`, `AdminApproveLyricsSubmissionHandler`,
`AdminUploadArticleImageHandler`, `PublicSubmitLyricsHandler` — each becomes one
`ExecuteInTransactionAsync` body with a single implicit commit. The exclusive-category mutex can
no longer be observed empty between its clear and its set.

> **Deviation at implementation time — no cross-context transaction is used at all.**
>
> Two implementations were built and rejected before the current one. The first hot-swapped a
> secondary context's `DbConnection` mid-transaction; it skipped enlistment silently whenever
> that connection was already open, so the guarantee quietly degraded to best-effort. The second
> gave every module context one shared scoped connection and enlisted through `UseTransaction`;
> that worked, but it ruled out `AddDbContextPool` everywhere, made concurrent use of two module
> contexts a runtime error, and rested on ~200 lines of invented scaffolding
> (`IEnlistableContext`, `EnlistableContext<T>`, `SecondaryContextEnlistment`).
>
> Both were dropped. The failure they prevented was an orphaned row, never corruption — the file
> is always written before the row referencing it, so content can never point at a missing file.
> That does not justify taxing every transaction in the application.
>
> **D5 is now claim-on-use.** `core.files` gains `claimed_at`. An upload is created unclaimed;
> the handler calls `IFileRepository.ClaimAsync` once the referencing write has committed; and
> `UnclaimedFileReaperJob` soft-deletes uploads still unclaimed after a 24-hour grace period,
> which raises `FileSoftDeletedEvent` so the existing handler removes the remote asset too. This
> is the standard object-storage upload lifecycle, it needs no distributed transaction, and it
> also closes a gap none of the transactional designs touched: a Cloudinary upload that succeeds
> and then fails before any DB write previously orphaned a blob with no row at all.
>
> The residual risk is a crash in the ~1 ms between the referencing commit and the claim, which
> would reap a referenced file a day later. The grace period is five orders of magnitude larger
> than that window; making it airtight would mean raising a domain event per referencing entity,
> which was judged not worth the invasiveness.

## Part D — Mailer

`OutboxEmailDispatcherJob.Execute` today: one transaction wraps `ClaimDueBatchAsync`, the whole
`DeliverAsync` SMTP loop, then `SaveChangesAsync` + commit — up to 20 network calls holding row
locks `[05 §12]`. After:

```csharp
// 1. short transaction: claim the batch (set Status=Claimed, LeaseExpiresAt=now+2m), commit.
// 2. no transaction: foreach → DeliverAsync(email) records Sent/Failed on the entity.
// 3. short save: persist the outcomes.
// A dispatcher that dies mid-batch leaves leases to expire; the next run re-claims them.
```

> **Deviation at implementation time — `[05 §13]` is solved by domain events, not by a shared
> transaction.** `IMailer` writes to `MailerDbContext`, which Content and Identity cannot name:
> they reference `Mailer.Contracts` only. Rather than breach that boundary, the five inline
> enqueues (signup, forgot-password ×2, resend-OTP ×2 — every other email in the system was
> already event-driven) now raise `OtpIssuedEvent` on the OTP aggregate. The event row lands in
> Identity's own `domain_event_outbox` in the same `SaveChanges` as the OTP row, and
> `OtpIssuedEmailHandler` enqueues post-commit.
>
> This is **stronger** than the transaction it replaces: a rolled-back flow writes no event and
> sends nothing, and a committed flow is re-delivered by the replay job even if the process dies
> between commit and enqueue — which a shared transaction would not have covered. The event
> carries the culture explicitly, because delivery runs in a fresh scope that no longer has the
> request's culture.
>
> One caveat: the plaintext OTP now sits in `domain_event_outbox.payload`. Not a new exposure —
> `mailer.outbox_emails.html_body` already stores the rendered code at rest — but it is a second
> copy, so retention on dispatched outbox rows is worth revisiting.

---

## Tests

- **Unit:** `DomainEvent` stamps once (`e.EventId == e.EventId`); the idempotency guard skips a
  replayed `(EventId, Handler)`; `ExecuteInTransactionAsync` rolls back on operation failure.
- **Integration — the durability proof:** commit an interaction with the post-commit dispatcher
  replaced by a throwing stub → outbox row remains undispatched; run `OutboxReplayJob` → counter
  moves; run it again → counter unchanged (idempotency).
- **Integration — the mutex regression:** fail `AdminSetExclusiveCategoryHandler` after its first
  (former) commit point → the exclusive category is still set (one transaction now).
- **Integration — Mailer:** signup rolled back leaves no outbox email; a claimed-but-unsent row
  with an expired lease is re-claimed and sent exactly once.

---

## Rollout

Two migrations per module tree (`domain_event_outbox`, `processed_domain_events`) — generated,
left unapplied per house rule. The replay job ships **enabled** — see the 13.3 deviation: it is the delivery path for events
raised inside a transaction, not merely a retry for failed dispatches.

---

## Verification

1. Build/format/unit/integration green.
2. `python3` census: no path performing two `CommitAsync` calls in sequence. Counting
   occurrences per file over-reports — a commit followed by `return` or `throw` ends its path,
   and five files commit twice only on mutually exclusive branches.
3. `IDomainEvent` declares no member-bodied `=>` for `EventId`/`OccurredOn`: the base record
   stamps both once in a property initializer, and both are `init`-set so a replayed event keeps
   the identity it was raised with.
4. Kill-and-replay chaos test green twice in a row.

---

**PR:** `feat(platform): durable domain events with a per-module outbox and one-commit handlers`
