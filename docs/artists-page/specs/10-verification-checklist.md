# Spec 10 — Verification Checklist

The final sweep across specs 01–09. **This is an audit, not a rubber stamp.** Every prior phase in
this module found stale-doc drift and prematurely-checked boxes; budget real verification time.

A box here is checked only after the behaviour is verified against the **current** code, not
against a spec's own claim that it was implemented.

## Rules this feature must not have broken

Read [`../../testing/00-unit-vs-integration-rules.md`](../../testing/00-unit-vs-integration-rules.md)
before writing anything in `tests/`. The two that get violated most often:

- An integration test reaches its target through **real HTTP** (`BaseApiTest`) or a **real
  repository from DI** (`BaseRepositoryTest`). Constructing a handler, validator, entity,
  specification or error factory inside `tests/Integration/` is a unit test in the wrong folder.
- **Coverage is a signal, not a target.** High unit coverage with near-zero integration coverage on
  the same file means the code is not wired into the application — a defect in `src/`, not a missing
  test. Wire it up or delete it; never close the gap by constructing the object in the integration
  folder.

## Cross-spec invariants

These are the assertions no single spec owns, and the ones a reviewer cannot catch by eye.

- [ ] **The directory and the profile agree.** Every artist returned by `GET /public/artists`
      returns 200 on `GET /public/artists/{slug}`. Asserted by iterating a seeded directory page,
      not by one hand-picked artist.
- [ ] **And in reverse.** An artist absent from the directory returns 404 on the profile.
- [ ] **`contentCount` equals what the profile renders.** For a seeded artist with a known mix
      across all five surfaces, the card's `contentCount` equals the sum of the profile's five
      totals.
- [ ] **The predicate and the counts are term-for-term aligned.** Compare
      `ArtistContentSpecifications.cs` against the projections in
      `ArtistRepository.GetPublicDirectoryAsync` and `GetTotalsAsync`: every surface in the
      predicate has a matching count term, and no term appears in one and not the other.
- [ ] **`EP` and `Single` are invisible end-to-end.** An artist whose only release is an `EP` is
      absent from the directory, 404s on the profile, and appears in neither release section.
- [ ] **A draft is never content.** Draft/archived songs, videos and articles count nowhere —
      directory filter, `contentCount`, totals, 404 rule, or any section.
- [ ] **No public response carries an artist `Id` or a `UserId`.** Assert against the raw response
      body of the directory, the profile, the releases endpoint and the articles endpoint.

## Performance

- [ ] **The directory is not N+1.** 30 seeded artists, real endpoint, command count asserted with a
      `DbCommandInterceptor` and bounded — not one query per row. This is a **release gate**, not a
      follow-up: the N+1 version passes every functional test and only fails in production.
- [ ] `availableLetters` does not add a query per letter.
- [ ] `GetTotalsAsync` is one round trip, not five.
- [ ] The video detail route's command count is unchanged by [spec 09](09-video-artist-slug.md).
- [ ] Every index in [`../ARTISTS_FEATURE_SCHEMA.sql`](../ARTISTS_FEATURE_SCHEMA.sql) exists in a
      generated migration.

## Correctness of the things that silently look right

- [ ] **A birthdate does not move.** `1986-10-24` round-trips as the same civil date with the test
      host's timezone forced to `Pacific/Auckland` **and** `America/Los_Angeles`.
- [ ] **Accent folding is consistent across all four readers.** `Élodie` sorts under `E`, buckets
      under `E`, matches `search=elodie`, and contributes `E` to `availableLetters`.
- [ ] **Renaming an artist recomputes both derived columns**, and the artist moves bucket.
- [ ] **Paging is stable.** For the directory, releases, and artist-articles: page 1 and page 2
      share no rows, including for rows with equal sort keys.
- [ ] **Null `ReleaseYear` sorts last**, not first.
- [ ] **Set-replace is a replace.** Tagging an article with `[A, B]` then `[B, C]` leaves exactly
      `B, C`.
- [ ] **Cascades fire.** Deleting an artist removes its social links and its article-artist rows;
      deleting an article removes its article-artist rows.
- [ ] **Upsert is idempotent.** Two upserts of one social platform leave one row.

## i18n

- [ ] Every new error message exists in the neutral, `.en` **and** `.fr` `.resx` files. A French
      catalog missing a key silently renders English to a French reader.
- [ ] No new user-facing string is hardcoded in a handler or endpoint.

## Contract

- [ ] The generated OpenAPI document contains all four new/changed public endpoints with correct
      response types and every `ProducesProblem` status.
- [ ] Enums serialise **by name** — `"Mixtape"`, `"Instagram"` — not as integers.
- [ ] `ArtistSummaryDto` carries no `Bio` and no `Id`.
- [ ] `aliases` and `socialLinks` serialise as `[]` when empty, never `null`.

## Build and suites

- [ ] `dotnet build` — clean, no warnings introduced.
- [ ] `dotnet csharpier --check .` — clean.
- [ ] `dotnet test tests/Unit` — green.
- [ ] `dotnet test tests/Integration` — green.
- [ ] The `AddArtistPageFeature` migration is generated, left unapplied, and carries the
      folded-name backfill in its `Up()`.
- [ ] Every `ArtistEntity.Create`/`Update` and `AlbumEntity.Create`/`Update` call site compiles —
      including seeds and test fixtures.

## Dead-code sweep

The signal that matters most, and the one that needs a deliberate pass:

- [ ] Every new repository method has at least one **non-test** caller. A method with only unit
      tests calling it is dead code with a green badge.
- [ ] Every new error factory member is thrown from somewhere in `src/`.
- [ ] Every new specification is used by a repository method.
- [ ] No `[ExcludeFromCodeCoverage]` was added to hide an untested path. If a line is genuinely
      unreachable by construction, say so in the PR with the reason.

## Spec sign-off

Check each only after re-verifying against current code, then flip the matching box in
[00-index.md](00-index.md).

- [ ] 01 — Artist identity fields
- [ ] 02 — Artist social links
- [ ] 03 — Release-type discriminator
- [ ] 04 — Artist-scoped release query
- [ ] 05 — Article → artist tagging
- [ ] 06 — Surfaceable-content predicate and `contentCount`
- [ ] 07 — Public artist list endpoint
- [ ] 08 — Profile payload, `isVerified` and surface totals
- [ ] 09 — Video artist slug
