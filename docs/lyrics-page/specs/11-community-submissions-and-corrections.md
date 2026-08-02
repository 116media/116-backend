# Spec 11 — Community Submissions & Corrections

Depends on spec 01's status workflow (a submission's approval creates a real `LyricsEntity` that
starts in `Draft`/`PendingReview`, not a parallel status concept) and spec 08's `ArtistEntity`
(the verified-artist fast path). Three paths, one published table:

| Path | Who | Review | Result |
| --- | --- | --- | --- |
| Admin CRUD | Staff | None (already trusted) | `Draft` → normal editorial workflow (spec 01) |
| Verified-artist upload | A user with a claimed `ArtistEntity.UserId` | None for their own songs | `LyricsEntity` created directly, `Published` after their own submit/approve/publish cycle — or auto-published, a product decision (see below) |
| Community submission | Any signed-in user | Moderation queue | `LyricsSubmissionEntity`, promoted to a real `LyricsEntity` only on approval |
| Community correction | Any signed-in user, on **any** published song | Peer review + threshold, or moderator override | Merges into `LyricsEntity.LyricsText` on acceptance |

**No trust exemption for corrections based on who created the song.** The first three rows describe
who can *create* a lyrics record and how much review that creation gets — admin-entered content
skips review because staff are already trusted to create it correctly. That trust does not extend
to "therefore nobody can flag a typo in it later." The correction row applies uniformly to every
published `LyricsEntity`, regardless of origin: `ProposeLyricsRevisionCommand`/`LyricsRevisionEntity`
(below) take only a `lyricsId` — there is no check anywhere for how that record was created,
intentionally. A community member or a verified artist finding a mistake in an admin-transcribed
song goes through exactly the same propose → vote-threshold-or-moderator-decide flow as a mistake
in a community-submitted one.

## New-song submissions

```csharp
/// <summary>
/// A community-submitted new song, pending moderation before it becomes a real
/// <see cref="LyricsEntity" />. Distinct from the editorial <c>Draft</c> status — a
/// submission isn't a lyrics record yet, it's a proposal to create one.
/// </summary>
public class LyricsSubmissionEntity : Aggregate<Guid>
{
    public string SongTitle { get; private set; } = null!;
    public string ArtistName { get; private set; } = null!;
    public string LyricsText { get; private set; } = null!;
    public string Language { get; private set; } = null!;
    public Guid SubmittedByUserId { get; private set; }
    public EnumSubmissionStatus Status { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public string? ReviewNote { get; private set; }
    public Guid? PublishedLyricsId { get; private set; }

    private LyricsSubmissionEntity() { }

    public static LyricsSubmissionEntity Submit(
        Guid id, string songTitle, string artistName, string lyricsText, string language, Guid userId)
    {
        return new LyricsSubmissionEntity
        {
            Id = id, SongTitle = songTitle, ArtistName = artistName, LyricsText = lyricsText,
            Language = language, SubmittedByUserId = userId, Status = EnumSubmissionStatus.Pending,
        };
    }

    /// <summary>
    /// Marks this submission approved and links it to the newly created lyrics record.
    /// Called after the lyrics record itself is successfully created — see the handler note
    /// on why these are two separate, individually-safe steps.
    /// </summary>
    public void Approve(Guid reviewedByUserId, Guid publishedLyricsId)
    {
        Status = EnumSubmissionStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        PublishedLyricsId = publishedLyricsId;
    }

    public void Reject(Guid reviewedByUserId, string note)
    {
        Status = EnumSubmissionStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        ReviewNote = note;
    }

    public void RequestRevision(Guid reviewedByUserId, string note)
    {
        Status = EnumSubmissionStatus.NeedsRevision;
        ReviewedByUserId = reviewedByUserId;
        ReviewNote = note;
    }
}

public enum EnumSubmissionStatus { Pending, Approved, Rejected, NeedsRevision }
```

Approval handler — the two-step, individually-safe sequence this feature's ACID posture requires
(see [../00-overview.md](../00-overview.md)):

```csharp
public class AdminApproveLyricsSubmissionHandler(
    ILyricsSubmissionRepository submissionRepository, ILyricsRepository lyricsRepository,
    ICategoryRepository categoryRepository, IContentUnitOfWork unitOfWork, ContentI18n i18n
) : ICommandHandler<AdminApproveLyricsSubmissionCommand, AdminApproveLyricsSubmissionResult>
{
    public async Task<AdminApproveLyricsSubmissionResult> Handle(
        AdminApproveLyricsSubmissionCommand command, CancellationToken ct)
    {
        LyricsSubmissionEntity submission = await submissionRepository.GetByIdOrThrowAsync(command.Id, ct);

        string slug = GenerateSlugFrom(submission.ArtistName, submission.SongTitle); // spec 01 §1 formula

        LyricsEntity? existing = await lyricsRepository.GetBySlugAsync(slug, ct);
        if (existing is not null)
        {
            throw i18n.Lyrics.SlugAlreadyExists(slug);
        }

        // Community submissions never carry a customer/order — the submitter never picks a
        // category — so approval always assigns the seeded default free category (spec 12's
        // "Standard Lyrics"), via LyricsEntity.CreateFree, never CreatePaid.
        Guid defaultCategoryId = await categoryRepository.GetDefaultLyricsCategoryIdAsync(ct);

        var lyrics = LyricsEntity.CreateFree(
            id: Guid.NewGuid(), categoryId: defaultCategoryId, songTitle: submission.SongTitle,
            artistName: submission.ArtistName, slug: slug, lyricsText: submission.LyricsText,
            language: submission.Language, authorId: command.ReviewerId, videoId: null,
            errors: i18n.Lyrics);

        await lyricsRepository.AddAsync(lyrics, ct);
        await unitOfWork.CommitAsync(ct); // step 1 — safe to retry alone if step 2 never runs

        submission.Approve(reviewedByUserId: command.ReviewerId, publishedLyricsId: lyrics.Id);
        submissionRepository.Update(submission);
        await unitOfWork.CommitAsync(ct); // step 2

        return new AdminApproveLyricsSubmissionResult(IsSuccess: true, LyricsId: lyrics.Id);
    }
}
```

If the process is interrupted between the two `CommitAsync` calls, the created `LyricsEntity`
already exists correctly and the submission is left `Pending` with no `PublishedLyricsId` — a
detectable, repairable inconsistency (a reconciliation query:
`submissions WHERE Status = Pending AND a matching-slug LyricsEntity already exists`), not data
corruption. The submission is created in `Draft`, then goes through the normal spec-01 editorial
workflow (`Submit → Approve → Publish`) like any other lyrics record — no separate publish concept
is introduced for community-originated content.

## Corrections to existing songs

Same propose → vote-threshold-or-moderator-decide → apply shape as spec 10's translation revisions,
targeting `LyricsEntity.LyricsText` instead of a translation's text:

```csharp
/// <summary>
/// A proposed correction to an existing, published lyrics page's canonical text.
/// </summary>
public class LyricsRevisionEntity : Aggregate<Guid>
{
    public Guid LyricsId { get; private set; }
    public string ProposedText { get; private set; } = null!;
    public string? EditSummary { get; private set; }
    public Guid ProposedByUserId { get; private set; }
    public EnumRevisionStatus Status { get; private set; }
    public Guid? DecidedByUserId { get; private set; }

    private LyricsRevisionEntity() { }

    public static LyricsRevisionEntity Propose(
        Guid id, Guid lyricsId, string proposedText, string? editSummary, Guid userId)
    {
        return new LyricsRevisionEntity
        {
            Id = id, LyricsId = lyricsId, ProposedText = proposedText,
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
```

`LyricsRevisionVoteEntity` mirrors `LyricsTranslationVoteEntity` (spec 10) exactly, FK'd to this
entity instead — not repeated here field-by-field, same shape. `LyricsEntity` needs a
`ReplaceLyricsText(string text)` method (a narrow setter, distinct from the full `Update(...)`, so
an accepted correction doesn't have to re-supply every other field):

```csharp
/// <summary>
/// Replaces the canonical lyrics text — used when a community correction is accepted.
/// Distinct from the full <see cref="Update" /> call, which requires re-supplying every field.
/// </summary>
public void ReplaceLyricsText(string lyricsText) => LyricsText = lyricsText;
```

`VoteOnLyricsRevisionHandler` follows `VoteOnTranslationRevisionHandler`'s exact shape (spec 10):
insert the vote (unique `(RevisionId, UserId)` rejects a repeat), tally net approvals, auto-accept
at the same `AutoAcceptThreshold`, calling `revision.Accept(null)` then
`lyrics.ReplaceLyricsText(revision.ProposedText)`.

## Verified-artist fast path — gated by identity, not name matching

**Do not match on `ArtistName` text.** Artist names change over a career, get spelled multiple
ways, and can collide between unrelated people — exactly the class of bug spec 01 already fixed
once by moving off `songTitle + artistName` as a uniqueness key. The fast path must be gated by
**who the submitter is**, not by comparing strings.

`ArtistEntity.UserId` already encodes that identity directly: `GetByUserIdAsync(userId)`
returns a result if and only if this specific user owns a claimed artist profile — there is nothing
left to compare against free text. If a profile comes back, **its own `Name`/`Id` are used as the
song's authoritative artist identity**, not whatever the client happened to submit — a verified
artist doesn't need to (and shouldn't be able to) type a different artist name for their own
submission. If the artist's name later changes, `ArtistEntity.Update(...)` updates it once, and
every subsequent submission from that same `UserId` automatically carries the current name —
no stale comparison anywhere, ever.

```csharp
/// <summary>
/// Command to submit a new song. When the submitting user owns a claimed
/// <see cref="ArtistEntity" />, <paramref name="ArtistName" /> is informational only and is
/// ignored in favor of that profile's own name — see the identity-gated fast path.
/// </summary>
/// <param name="SongTitle">The song title.</param>
/// <param name="ArtistName">
/// The artist name as typed by the submitter. Required when the submitter owns no claimed
/// artist profile; ignored (in favor of the owned profile's name) otherwise.
/// </param>
/// <param name="LyricsText">The full lyrics text.</param>
/// <param name="Language">ISO 639-1 language code.</param>
/// <param name="UserId">The identity user UUID of the submitter, from JWT claims.</param>
public record SubmitLyricsCommand(
    string SongTitle, string? ArtistName, string LyricsText, string Language, Guid UserId
) : ICommand<SubmitLyricsResult>;
```

```csharp
public class SubmitLyricsHandler(
    IArtistRepository artistRepository, ILyricsSubmissionRepository submissionRepository,
    ILyricsRepository lyricsRepository, ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork, ContentI18n i18n
) : ICommandHandler<SubmitLyricsCommand, SubmitLyricsResult>
{
    public async Task<SubmitLyricsResult> Handle(SubmitLyricsCommand command, CancellationToken ct)
    {
        ArtistEntity? ownedArtist = await artistRepository.GetByUserIdAsync(command.UserId, ct);

        if (ownedArtist is not null)
        {
            string slug = GenerateSlugFrom(ownedArtist.Name, command.SongTitle);

            // A verified artist's own self-upload is still free by default — the fast path
            // skips the moderation queue, it doesn't imply a commercial transaction. A label
            // wanting to commission this same song as paid/promoted goes through
            // AdminCreateLyricsHandler's CreatePaid branch (spec 01/12) instead, separately.
            Guid defaultCategoryId = await categoryRepository.GetDefaultLyricsCategoryIdAsync(ct);

            var lyrics = LyricsEntity.CreateFree(
                Guid.NewGuid(), defaultCategoryId, command.SongTitle, ownedArtist.Name, slug,
                command.LyricsText, command.Language, command.UserId, videoId: null, i18n.Lyrics);
            lyrics.LinkArtist(ownedArtist.Id);

            await lyricsRepository.AddAsync(lyrics, ct);
            await unitOfWork.CommitAsync(ct);

            return new SubmitLyricsResult(IsSuccess: true, LyricsId: lyrics.Id, WentToQueue: false);
        }

        if (string.IsNullOrWhiteSpace(command.ArtistName))
        {
            throw i18n.Lyrics.ArtistNameRequired();
        }

        var submission = LyricsSubmissionEntity.Submit(
            Guid.NewGuid(), command.SongTitle, command.ArtistName, command.LyricsText,
            command.Language, command.UserId);

        await submissionRepository.AddAsync(submission, ct);
        await unitOfWork.CommitAsync(ct);

        return new SubmitLyricsResult(IsSuccess: true, LyricsId: null, WentToQueue: true);
    }
}
```

`ArtistName` on the command stays purely for the **unclaimed-submitter path** — a community member
crediting a song to an artist who has no profile at all yet, exactly like the free-text field the
rest of this doc set already treats as the fallback (spec 08). Once that artist claims a profile
later, an admin (or the artist themselves) can retroactively `LinkArtist` this record — the same
backfill relationship spec 08 already describes for existing catalog rows.

The fast-path record still lands in `Draft` (per spec 01) — "no queue for your own songs" means
skipping the community-submission table, not skipping editorial state entirely; the artist (or an
admin on their behalf) still has to move it through `Submit`/`Approve`/`Publish`, or a further
product decision could auto-publish verified-artist uploads directly. That decision is left open
here deliberately — auto-publishing bypasses the review-before-public principle spec 01 was just
built to enforce, so it should be a conscious call, not a side effect of "verified artists get a
fast path."

## Endpoints

| Method | Route | Auth |
| --- | --- | --- |
| POST | `/api/v1/lyrics/submissions` | Authenticated, `ContentContribution` |
| GET | `/api/v1/admin/lyrics/submissions` | Admin/Moderator — review queue, filterable by status |
| PUT | `/api/v1/admin/lyrics/submissions/{id}` | Admin/Moderator — approve/reject/needs-revision |
| POST | `/api/v1/lyrics/{id}/revisions` | Authenticated, `ContentContribution` |
| POST | `/api/v1/lyrics/revisions/{id}/votes` | Authenticated, `ContentContribution` |
| PUT | `/api/v1/admin/lyrics/revisions/{id}` | Admin/Moderator — direct decide |

## Migration

```bash
dotnet ef migrations add AddLyricsSubmissionsAndRevisions \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Task checklist

- [x] `LyricsSubmissionEntity`, `EnumSubmissionStatus` + configuration
- [x] `LyricsRevisionEntity`, `LyricsRevisionVoteEntity` + configurations (unique
  `(RevisionId, UserId)`)
- [x] `LyricsEntity.ReplaceLyricsText`
- [x] `SubmitLyricsCommand`/`Handler` (verified-artist fast path + queue path) — **stale-doc
  correction**: uses the real `ICategoryRepository.GetDefaultLyricsCategoryAsync()` (returning
  `CategoryEntity?`, not a `Guid` directly), not the `GetDefaultLyricsCategoryIdAsync` name this
  doc originally sketched, since Phase 4 shipped it following the existing `IsGossip`/
  `GetGossipCategoryAsync` precedent instead. Both self-uploads and approved submissions use
  `LyricsEntity.CreateFree`, never `CreatePaid` — becoming a paid/promoted product is a separate,
  later action (spec 12). Also: `Slug` turned out to be a required client-supplied field on both
  this command (fast-path only) and `AdminApproveLyricsSubmissionCommand`, matching every other
  lyrics-creation path in this codebase (slugs are always client-generated, server only validates
  format/uniqueness) — not server-generated via a `GenerateSlugFrom(...)` helper as this doc's
  original snippet showed.
- [x] `AdminApproveLyricsSubmissionCommand`/`Handler` (two-step create-then-link sequence, two
  separate `CommitAsync` calls — genuinely non-atomic across the two steps, unlike spec 10's
  vote-accept-and-apply which commits both mutations together),
  `AdminRejectLyricsSubmissionCommand`, `AdminRequestLyricsRevisionCommand` — shipped as three
  separate endpoints/routes (`PUT .../submissions/{id}`, `PATCH .../submissions/{id}/reject`,
  `PATCH .../submissions/{id}/request-revision`), matching this codebase's existing
  one-endpoint-per-action convention rather than a single action-flag endpoint
- [x] `ProposeLyricsRevisionCommand`, `VoteOnLyricsRevisionCommand` (threshold auto-accept, mirrors
  spec 10's structure exactly), admin override — takes only a `lyricsId`, no check anywhere for
  how the record was created, confirmed by an integration test proposing a correction against a
  plain admin-`CreateFree` record with zero submission history
- [x] All six endpoints + `ContentContribution` rate-limit policy (spec 10) applied throughout
- [x] Reconciliation query for approved-but-unlinked submissions
- [x] Migration `AddLyricsSubmissionsAndRevisions`
- [x] Integration tests: a submission from a user who owns a claimed `ArtistEntity` skips the
  queue entirely (no `LyricsSubmissionEntity` row created), starts in `Draft`, and is attributed to
  the owned profile's own `Name`/`Id` regardless of whatever `ArtistName` text the client sent (an
  intentionally mismatched `ArtistName` in the request must not change the result — proving the
  gate is identity-based, not string-based); a submission from a user who owns no claimed profile
  always queues, and is rejected if it omits `ArtistName`; community submission sits `Pending`
  until reviewed; approving creates the `LyricsEntity` and links `PublishedLyricsId`; a duplicate
  vote on a revision is rejected by the unique constraint; the threshold auto-accepts and updates
  `LyricsText`

**Bug caught and fixed during verification (shared with spec 10, since this workflow structurally
copies it)**: `VoteOnLyricsRevisionHandler` had the identical off-by-one tally bug as spec 10's
translation-revision handler — fixed the same way, tallying existing votes before the just-cast
one and adding its own contribution in memory.

**Verification, 2026-08-01**: `dotnet build` clean; full suite 6673/6676 unit (3 pre-existing
unrelated skips), 1673/1673 integration, zero failures.
