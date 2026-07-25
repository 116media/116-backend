# Spec 10 — AI Translations & Community Review

A language switcher on the detail page needs translated lyrics text. AI generates the first draft;
a community can propose corrections, reviewed Wikipedia-style (propose → vote → threshold-accept,
or moderator override).

## New rate-limit policy

`RateLimitPolicies` (`BuildingBlocks/Constants/RateLimit/RateLimitPolicies.cs`) gains:

```csharp
/// <summary>
/// Policy name for authenticated content-contribution endpoints (translations, revisions,
/// votes, submissions). Algorithm: Fixed Window. Stricter than ContentBrowsing since these
/// are write paths open to any signed-in user, not just read traffic.
/// </summary>
public const string ContentContribution = "ContentContribution";
```

Used by every endpoint in this spec and specs 11 (submissions/corrections).

## Entities

```csharp
public enum EnumTranslationSource { Ai, Community }
public enum EnumRevisionStatus { Pending, Accepted, Rejected }
public enum EnumVote { Approve, Reject }

/// <summary>
/// A published translation of a lyrics page into a given language. One row per
/// (LyricsId, Language) pair — corrections update this row's text via an accepted
/// <see cref="LyricsTranslationRevisionEntity" />, they do not create a second row.
/// </summary>
public class LyricsTranslationEntity : Aggregate<Guid>
{
    public Guid LyricsId { get; private set; }
    public string Language { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    public EnumTranslationSource Source { get; private set; }

    private LyricsTranslationEntity() { }

    public static LyricsTranslationEntity CreateAi(Guid id, Guid lyricsId, string language, string text)
    {
        return new LyricsTranslationEntity
        {
            Id = id, LyricsId = lyricsId, Language = language, Text = text,
            Source = EnumTranslationSource.Ai,
        };
    }

    /// <summary>
    /// Applies an accepted community revision's text as the new published translation.
    /// </summary>
    public void ApplyAcceptedRevision(string newText)
    {
        Text = newText;
        Source = EnumTranslationSource.Community;
    }
}

/// <summary>
/// A proposed correction to a published translation. Never mutates the translation directly —
/// only <see cref="LyricsTranslationEntity.ApplyAcceptedRevision" /> does, once this revision
/// is accepted.
/// </summary>
public class LyricsTranslationRevisionEntity : Aggregate<Guid>
{
    public Guid TranslationId { get; private set; }
    public string ProposedText { get; private set; } = null!;
    public string? EditSummary { get; private set; }
    public Guid ProposedByUserId { get; private set; }
    public EnumRevisionStatus Status { get; private set; }
    public Guid? DecidedByUserId { get; private set; }

    private LyricsTranslationRevisionEntity() { }

    public static LyricsTranslationRevisionEntity Propose(
        Guid id, Guid translationId, string proposedText, string? editSummary, Guid userId)
    {
        return new LyricsTranslationRevisionEntity
        {
            Id = id, TranslationId = translationId, ProposedText = proposedText,
            EditSummary = editSummary, ProposedByUserId = userId, Status = EnumRevisionStatus.Pending,
        };
    }

    public void Accept(Guid? decidedByUserId)
    {
        Status = EnumRevisionStatus.Accepted;
        DecidedByUserId = decidedByUserId;
    }

    public void Reject(Guid decidedByUserId)
    {
        Status = EnumRevisionStatus.Rejected;
        DecidedByUserId = decidedByUserId;
    }
}

/// <summary>
/// A single user's vote on a pending translation revision.
/// </summary>
public class LyricsTranslationVoteEntity : Aggregate<Guid>
{
    public Guid RevisionId { get; private set; }
    public Guid UserId { get; private set; }
    public EnumVote Vote { get; private set; }
    public string? Comment { get; private set; }

    private LyricsTranslationVoteEntity() { }

    public static LyricsTranslationVoteEntity Create(Guid id, Guid revisionId, Guid userId, EnumVote vote, string? comment)
    {
        return new LyricsTranslationVoteEntity
        {
            Id = id, RevisionId = revisionId, UserId = userId, Vote = vote, Comment = comment,
        };
    }
}
```

## Configurations

```csharp
builder.HasIndex(x => new { x.LyricsId, x.Language }).IsUnique(); // LyricsTranslationConfiguration
builder.HasIndex(x => new { x.RevisionId, x.UserId }).IsUnique(); // LyricsTranslationVoteConfiguration
```

The vote unique constraint is the actual enforcement of "one vote per user per revision" — not
application-level dedup logic (per this feature's ACID posture — see
[../00-overview.md](../00-overview.md)).

## AI generation — idempotent

```csharp
public record RequestLyricsTranslationCommand(Guid LyricsId, string Language) : ICommand<RequestLyricsTranslationResult>;

public class RequestLyricsTranslationHandler(
    ILyricsRepository lyricsRepository, ITranslationRepository translationRepository,
    ITranslationService translationService, IContentUnitOfWork unitOfWork
) : ICommandHandler<RequestLyricsTranslationCommand, RequestLyricsTranslationResult>
{
    public async Task<RequestLyricsTranslationResult> Handle(RequestLyricsTranslationCommand command, CancellationToken ct)
    {
        LyricsTranslationEntity? existing = await translationRepository.GetByLyricsAndLanguageAsync(
            command.LyricsId, command.Language, ct);

        if (existing is not null)
        {
            return new RequestLyricsTranslationResult(existing.Text, existing.Source.ToString());
        }

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(command.LyricsId, ct);

        string translatedText = await translationService.TranslateAsync(
            lyrics.LyricsText, targetLanguage: command.Language, ct);

        var translation = LyricsTranslationEntity.CreateAi(Guid.NewGuid(), command.LyricsId, command.Language, translatedText);
        await translationRepository.AddAsync(translation, ct);
        await unitOfWork.CommitAsync(ct);

        return new RequestLyricsTranslationResult(translatedText, nameof(EnumTranslationSource.Ai));
    }
}
```

`ITranslationService` is a new port (`Application/Shared/Services/ITranslationService.cs`), kept
behind an interface so the concrete LLM provider is swappable and mockable in tests — same
dependency-inversion shape as `IUserLookupService`/`IFileRepository` elsewhere in this module. The
concrete implementation (which provider, API key config) is an infrastructure concern outside this
spec's scope; only the port and its consumption are specced here.

## Review workflow

```csharp
public record ProposeTranslationRevisionCommand(Guid TranslationId, string ProposedText, string? EditSummary)
    : ICommand<ProposeTranslationRevisionResult>;

public record VoteOnTranslationRevisionCommand(Guid RevisionId, EnumVote Vote, string? Comment)
    : ICommand<VoteOnTranslationRevisionResult>;
```

```csharp
public class VoteOnTranslationRevisionHandler(
    ITranslationRevisionRepository revisionRepository, ITranslationVoteRepository voteRepository,
    ITranslationRepository translationRepository, IContentUnitOfWork unitOfWork, ContentI18n i18n
) : ICommandHandler<VoteOnTranslationRevisionCommand, VoteOnTranslationRevisionResult>
{
    private const int AutoAcceptThreshold = 3;

    public async Task<VoteOnTranslationRevisionResult> Handle(VoteOnTranslationRevisionCommand command, CancellationToken ct)
    {
        LyricsTranslationRevisionEntity revision = await revisionRepository.GetByIdOrThrowAsync(command.RevisionId, ct);

        var vote = LyricsTranslationVoteEntity.Create(
            Guid.NewGuid(), command.RevisionId, command.UserId, command.Vote, command.Comment);
        await voteRepository.AddAsync(vote, ct); // unique (RevisionId, UserId) rejects a repeat vote

        int netApprovals = await voteRepository.GetNetApprovalsAsync(command.RevisionId, ct);

        if (netApprovals >= AutoAcceptThreshold && revision.Status == EnumRevisionStatus.Pending)
        {
            revision.Accept(decidedByUserId: null);
            revisionRepository.Update(revision);

            LyricsTranslationEntity translation = await translationRepository.GetByIdOrThrowAsync(revision.TranslationId, ct);
            translation.ApplyAcceptedRevision(revision.ProposedText);
            translationRepository.Update(translation);
        }

        await unitOfWork.CommitAsync(ct);
        return new VoteOnTranslationRevisionResult(IsSuccess: true);
    }
}
```

`AutoAcceptThreshold` should live in a named constants class
(`TranslationConstants.AutoAcceptThreshold`), not inline, for the same tunability reason as
`LyricsViewCountingConstants` in spec 05. `DecideTranslationRevisionCommand` (admin/moderator
endpoint) calls `revision.Accept(decidedByUserId: currentUserId)`/`Reject(...)` directly, bypassing
the vote tally — same two-step apply sequence as above.

### Why this two-step apply is safe without a transaction

Per [../00-overview.md](../00-overview.md)'s ACID posture: if `revisionRepository.Update` commits
but the process crashes before `translationRepository.Update` also commits (both are in the same
`unitOfWork.CommitAsync` call here, so this specific case is already atomic — but the same shape
recurs in spec 11 where the two updates are *not* always in the same call), the revision sits
`Accepted` with its text not yet applied — a detectable, repairable state (a periodic reconciliation
query `WHERE Status = Accepted AND translation.Text != revision.ProposedText` finds and fixes it),
not silent corruption.

## Endpoints

| Method | Route | Auth |
| --- | --- | --- |
| GET | `/api/v1/public/lyrics/{id}/translations` | Anonymous |
| POST | `/api/v1/public/lyrics/{id}/translations` | Authenticated, `ContentContribution` |
| POST | `/api/v1/translations/{id}/revisions` | Authenticated, `ContentContribution` |
| POST | `/api/v1/translations/revisions/{id}/votes` | Authenticated, `ContentContribution` |
| PUT | `/api/v1/admin/translations/revisions/{id}` | Admin/Moderator |
| GET | `/api/v1/translations/{id}/revisions` | Anonymous — full history |

## Migration

```bash
dotnet ef migrations add AddLyricsTranslationsAndReview \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Task checklist

- [x] `RateLimitPolicies.ContentContribution` — the constant existed briefly without being wired
  into the actual rate limiter; a real bug (see verification note below), now fixed
- [x] `LyricsTranslationEntity`, `LyricsTranslationRevisionEntity`, `LyricsTranslationVoteEntity`
  + configurations (both unique indexes)
- [x] `ITranslationService` port + a placeholder implementation (`PlaceholderTranslationService`,
  a deliberate no-op echoing the source text unchanged) — a concrete LLM provider integration was
  explicitly out of this spec's scope and remains unimplemented pending a real provider decision.
  Remaining work once a provider is chosen: register API key/config, add the provider's HTTP
  client, implement `ITranslationService.TranslateAsync` against it, then swap the DI registration
  in `ContentModule.cs` from `PlaceholderTranslationService` to the real implementation — nothing
  else in this spec (entities, review workflow, endpoints) needs to change
- [x] `RequestLyricsTranslationCommand`/`Handler` — idempotent on an existing translation
- [x] `ProposeTranslationRevisionCommand`, `VoteOnTranslationRevisionCommand` (threshold
  auto-accept), `DecideTranslationRevisionCommand` (admin override)
- [x] `TranslationConstants.AutoAcceptThreshold`
- [x] All six endpoints
- [x] Reconciliation query for `Accepted`-but-unapplied revisions (`GetAcceptedButUnappliedAsync`)
- [x] Migration `AddLyricsTranslationsAndReview`
- [x] Integration tests: requesting an existing translation returns it without a second AI call;
  a duplicate vote is rejected by the unique constraint; the threshold auto-accepts and updates the
  published text; an admin override bypasses the tally in either direction

**Bug caught and fixed during verification**: `VoteOnTranslationRevisionHandler`'s auto-accept
tally had a real off-by-one — it queried `GetNetApprovalsAsync` against the database immediately
after adding the just-cast vote to the change tracker, but EF Core doesn't reflect an unflushed
insert in a fresh query, so the tally was always one vote behind and never actually crossed the
threshold on the deciding vote. Fixed by tallying existing votes first, then adding the current
vote's own +1/-1 contribution in memory.

**Second bug caught and fixed**: `RateLimitPolicies.ContentContribution` was defined but never
registered with real limits in `RateLimitingExtension.ConfigureFixedWindowPolicies`, and separately
the integration test fixture's `ApiFixture.DisableRateLimiting` hardcoded policy list didn't
include it either — every write endpoint in this spec returned a bare 500 until both were fixed.

**Verification, 2026-08-01**: `dotnet build` clean; full suite 6673/6676 unit (3 pre-existing
unrelated skips), 1673/1673 integration, zero failures. Migrations
`AddLyricsTranslationsAndReview`/`AddLyricsSubmissionsAndRevisions` generated but **not applied**
to any database.
