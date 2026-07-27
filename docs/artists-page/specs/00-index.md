# Artist Page — Backend Implementation Specs

Read [../00-overview.md](../00-overview.md) first for the *why* and the current-state audit. This
index is the *how* — work in the order below, which is a real dependency chain. For the full SQL
shape of every table and column these specs introduce, see
[../ARTISTS_FEATURE_SCHEMA.sql](../ARTISTS_FEATURE_SCHEMA.sql).

Frontend source of truth: [`../../../../frontend/docs/artists-page/`](../../../../frontend/docs/artists-page/) —
especially [14 (data requirements)](../../../../frontend/docs/artists-page/14-data-requirements.md)
and [16 (gaps)](../../../../frontend/docs/artists-page/16-backend-gaps-and-contracts.md).

| # | File | Covers | Frontend gap |
| --- | --- | --- | :---: |
| 01 | [01-artist-identity-fields.md](01-artist-identity-fields.md) | `RealName`, `Aliases`, `Birthdate`, `Hometown` on `ArtistEntity` + admin surface | 7 |
| 02 | [02-artist-social-links.md](02-artist-social-links.md) | `ArtistSocialLinkEntity`, `EnumSocialPlatform`, upsert/remove admin commands | 8 |
| 03 | [03-release-type-discriminator.md](03-release-type-discriminator.md) | `EnumReleaseType` on `AlbumEntity`, admin create/update, backfill | 4 |
| 04 | [04-artist-scoped-release-query.md](04-artist-scoped-release-query.md) | `GetByArtistAsync`, public releases endpoint filtered by type | 3 |
| 05 | [05-article-artist-tagging.md](05-article-artist-tagging.md) | `ArticleArtistEntity` join, admin set-artists, public artist-articles endpoint | 5 |
| 06 | [06-surfaceable-content.md](06-surfaceable-content.md) | `ArtistHasContentSpecification` + `contentCount` — one definition, three uses, no N+1 | 1a |
| 07 | [07-public-artist-list-endpoint.md](07-public-artist-list-endpoint.md) | `GET /public/artists` — letter bucket, search, `availableLetters`, accent folding | 1, 1b, 1c |
| 08 | [08-profile-payload-and-totals.md](08-profile-payload-and-totals.md) | `isVerified`, identity/socials on the profile response, every surface total, 404 rule | 2 |
| 09 | [09-video-artist-slug.md](09-video-artist-slug.md) | `ArtistSlug` on the video detail response | 6 |
| 10 | [10-verification-checklist.md](10-verification-checklist.md) | Full backend test and verification sweep across every spec above | — |
| 11 | [11-odesli-streaming-link-resolution.md](11-odesli-streaming-link-resolution.md) | Paste one platform URL, Odesli fills the rest; `Deezer` platform; resolve endpoints | enhancement |

## Why this order

01–03 and 05 are **schema-first and independent** of one another — four migrations that can land in
any order. They come first because everything downstream reads columns they add.

04 needs 03 (the release-type filter has nothing to filter without the discriminator).

06 needs 03, 04 and 05: the content predicate counts albums and tagged articles, so it cannot be
written correctly until those surfaces exist. Writing it earlier means writing it twice.

07 and 08 both consume 06. They are the two endpoints the frontend actually calls, and they must
agree on the predicate or a listed artist 404s when clicked.

09 is independent of all of it and could land any time; it is smallest.

10 is a genuine audit, not a rubber stamp — see the note at the bottom of that file.

11 is an enhancement on the lyrics feature's streaming-link machinery, independent of specs 01–09.
It removes the per-platform manual entry from curation and adds Deezer; it can land before or
after the audit without affecting it.

## Global progress

- [ ] 01 — Artist identity fields
- [ ] 02 — Artist social links
- [ ] 03 — Release-type discriminator
- [ ] 04 — Artist-scoped release query
- [ ] 05 — Article → artist tagging
- [ ] 06 — Surfaceable-content predicate and `contentCount`
- [ ] 07 — Public artist list endpoint
- [ ] 08 — Profile payload, `isVerified` and surface totals
- [ ] 09 — Video artist slug
- [ ] 10 — Verification
- [x] 11 — Odesli streaming-link resolution

Mark a box `- [x]` only once that spec's own checklist is fully implemented, its tests pass, and
`dotnet build` plus the module's test suites are clean. Boxes have been checked prematurely in this
repo before; do not repeat it.

## Conventions every spec assumes

These are not restated per spec. They come from [`../../../CLAUDE.md`](../../../CLAUDE.md) and from
the existing module, and deviating from them is a bug in the implementation, not a new convention.

- **CQRS**: `ICommand<T>`/`IQuery<T>` + `ICommandHandler`/`IQueryHandler`, dispatched via
  `IDispatcher`. Validators are FluentValidation and run through `ValidationDecorator`.
- **Use-case files and types are scope-prefixed** — `Admin` or `Public` — on every file in the
  folder: Command/Query, Handler, MetaField, Validator, Endpoint, Result, Request, Response. The
  *folder* name is never prefixed.
- **Endpoints are Carter modules** named `<UseCase>EndpointV1.cs`, under a `V1/` folder, mapped
  through `MapApiVersionGroup(1)`, with `.WithName`/`.WithSummary`/`.WithDescription` fed from the
  use case's `MetaField`, an explicit rate-limit policy, and `Produces`/`ProducesProblem` for every
  status code the handler can produce.
- **Errors are the three-layer pattern**: an `XxxErrors` factory returning typed exceptions, an
  `XxxErrorMessage` localizer, and three `.resx` files (neutral, `.en`, `.fr`). Every new message
  lands in all three in the same commit.
- **Repositories** expose intent-named methods and use `Specification<T>` via `ApplySpecification`.
  Specifications are never referenced directly from a test.
- **XML docs are multiline block form** on every public type and member, `<inheritdoc />` on
  overrides.
- **Testing** follows [`../../testing/00-unit-vs-integration-rules.md`](../../testing/00-unit-vs-integration-rules.md)
  without exception. Unit tests prove a method works; integration tests prove it is *used*, and only
  ever reach their target through real HTTP (`BaseApiTest`) or a real repository from DI
  (`BaseRepositoryTest`). Constructing a handler, validator, entity or specification inside
  `tests/Integration/` is forbidden.
- **Migrations are generated and left unapplied**, matching every prior phase.
