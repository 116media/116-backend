# Work plan

Ordered by sites fixed per unit of risk.

> **All steps are implemented** — see [16-implementation-log.md](16-implementation-log.md) for what
> landed, what measured differently, and the one proposal rejected on reading.

## 0. Before anything — clean the unused `using` blocks

A mechanical pass left them across the suites; a *Storage* repository test opens with four *Identity*
`.TestData` usings. Every later diff is easier to read without them, and they are why a file's real
test-data dependencies are currently unreadable.

Use Roslyn (`dotnet format --diagnostics IDE0005` or the IDE's Remove Unnecessary Usings), **never** a
regex: an extension method's namespace looks unused and is not. Run the suites afterwards — the risk is
exactly the extension-method case.

## 1. Free wins — an existing helper is hand-rolled, byte for byte

No new code, no judgment calls. Roughly **250 sites**, all in Identity `Application/`:

| Order | Item | Sites |
| --- | --- | --- |
| 1.1 | `IRefreshTokenService` / `GetByRefreshTokenHashAsync` setups → existing helpers | 82 |
| 1.2 | `IAuthRepository` guard setups → `SetupIsUserAccountActiveReturnsTrue` and friends | 69 |
| 1.3 | `IAuthRepository` lookup setups → `SetupGetUserWithRolesAndPermissionsById` and friends | 52 |
| 1.4 | `IPasswordService` setups → `SetupVerifySuccess` / `SetupVerifyFailure` / `SetupHash` | 53 |
| 1.5 | Raw `new Mock<T>()` → `Mock*.Create()` | 10 files |

1.2 is worth doing first regardless of volume: those 69 setups have no `.Returns(...)` and so express the
opposite of what the tests mean.

Then the same class in Content: `AbandonedDraftCleanupJobTests` (5 + 2), the `SetupGetAuthorInfosByIds`
sites, `VideoRatingFactory` (11), `ArticleCommentFactory` (12), `PackageSlotFactory` (5),
`ArticleArtistFactory.Link` (3), and the 46 Identity `Infrastructure/` entity sites.

**One caveat to carry into the PR text:** the entity swap is not inert for `UserRoleEntity` and
`RolePermissionEntity` — see [02](02-identity-infrastructure.md).

## 2. Defects — small, and they change behaviour

Each is independent and each needs the suites run:

1. `VideoFactory.CreateWithCategory` — fix the body, delete the two local workarounds (12 callers gain a populated navigation).
2. The three builders whose doc comments promise a navigation they never attach — fix the docs or attach it; delete the duplicated assignment lines.
3. `PublicGetLyricsBySlugEndpointV1Tests` — the discarded entity, so the stale-video scenario is actually covered.
4. The doubled `MarkPendingReview(); Approve();` at six sites.
5. `AdminUpdateCategoryHandlerTests.cs:85-88` — delete the redundant setup.

Details in [12-defects-found.md](12-defects-found.md).

## 3. Delete what is dead

18 members, each verified unused with no hand-rolled twin: 8 Content, 5 Storage, 5 Identity. Pure
subtraction — see [11-dead-factories.md](11-dead-factories.md). Do **not** touch the two "dead for a
reason" members.

## 4. Add the missing factories, biggest first

[10-missing-factories.md](10-missing-factories.md), in this order:

1. **Mailer's four** — the module has none, and six byte-identical private helpers disappear with them.
2. `PermissionFactory.CreateDefault()` (72) and `RoleFactory.CreateDefault()` (21) — two methods, 93 sites.
3. `LyricsFactory.CreatePublishedWithSlug` (22) — and it removes the doubled transitions from item 2.4.
4. The interaction-entity cluster (`ArticleLike`, `ArticleBookmark`, `ArticleShare`, `ShortVideoLike`, `ShortVideoBookmark`, `ShortVideoShare`, `VideoShare`, `ArticleCommentLike`, `ArticleTag`, `VideoTag`) — ~100 sites, all the same trivial shape.
5. `ContentOrderFactory.CreateForCustomer` (17) and the two order factories beside it.
6. The two DTO factories — `SessionExportDto` (14) and `AuthenticationDto`/`UserResponseDto` (21).
7. The remaining mock factories, and `MockAccountLockoutRepository` as a new class.

## 5. Structural, needs a decision first

- **`SessionBuilder`'s missing `CreatedAt` default** — unblocks ~20 sites but changes what the builder produces for every consumer.
- **`tests/EndToEnd.Tests` needs a `Mailer.TestData` project reference** before its four outbox sites can be fixed.
- **The request-builder gap in Editorial's Artist/Album/Streaming-link slice** — ~30 request types with no builder. Worth confirming the convention applies there before writing 30 builders.
- **The 25-file `ContentDbContext` bootstrap and the ~34 `SeedCategoryAsync` copies** — a base class or a factory; this is a test-infrastructure decision, not a test-data one.

## What to do about the thin coverage

[14-method-and-coverage.md](14-method-and-coverage.md) names three gaps. The one worth a second pass is
**`EndToEnd.Tests`** — 26 of 32 files were never opened, and its request-record inventory is grep-derived.
Everything else is either read or structurally excluded.
