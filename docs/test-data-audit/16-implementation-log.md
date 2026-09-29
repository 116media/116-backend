# Implementation log

What was applied from this audit, what it measured differently once the code was in front of the
compiler, and what was deliberately not done. Every batch below ends green: `dotnet build` 0/0,
`dotnet csharpier check .` clean over 4,100 files, and the suites named beside it.

Final state: **10,681 tests pass** — Content 3,741 + 1,525, Identity 2,896 + 393, Storage 354 + 18,
Mailer 333 + 47, Shared 1,120 + 58, Architecture 6, EndToEnd 190.

## Applied

| Batch | Change | Sites |
| --- | --- | --- |
| Defects | D1–D5 fixed, plus four more builders carrying the same false doc | 7 builders, 6 test files |
| Deletions | The 18 dead members in [11](11-dead-factories.md), plus the orphaned `ContentOrderItemBuilder.WithCategory` | 19 members |
| Mailer | `NewsletterSubscriberFactory` (3 members), `NotificationFactory` (2), `OutboxEmailFactory` (1); six byte-identical private `CreateNotification` helpers deleted; `Mailer.TestData` reference added to `EndToEnd.Tests` | 43 |
| Content interactions | Ten new factories: `ArticleLike`, `ArticleBookmark`, `ArticleShare`, `ArticleCommentLike`, `ArticleTag`, `ShortVideoLike`, `ShortVideoBookmark`, `ShortVideoShare`, `VideoShare`, `VideoTag` | 137 |
| Content existing | `ArticleCommentFactory.Create(…, body)` and `CreateReply` added; comment, rating, order, package-slot, article-artist, archived/rejected article and gossip category factories adopted | 95 |
| Content helpers | `ContentMapperFactory.Create()` replaces nine copies of the Mapster bootstrap and the `ContentMappingRegistration` alias; `SetupGetByIdOrThrow` adopted | 23 |
| Identity factories | `PermissionFactory.CreateDefault()`, `RoleFactory.CreateDefault()`, `UserFactory.CreateExternalWithSubject()` added and adopted | 77 |
| Identity mocks | F6 guard setups, F8 refresh-token and session lookups, F4 password service, F7 user lookup, F5 raw `new Mock<T>()` → `Mock*.Create()` | 290 |
| Identity entities | The `Infrastructure/` inline constructions routed through `UserRoleFactory`, `RolePermissionFactory`, `PermissionFactory`, `RoleFactory`, `UserFactory` | 37 |
| Shared | `CrossContextTransactionTests` routed through `TagFactory`; `OtpFactory.CreateUsed` adopted (F9) | 5 |

14 new factory files; 176 files modified.

## Where the code disagreed with the audit

**D1 has no navigation to populate.** `VideoEntity` carries no `Category` navigation at all — stage 15
deleted it, and `VideoBuilder.WithCategory` only assigns the foreign key. So the prescribed fix
(`new VideoBuilder(categoryId).WithCategory(category).Build()`) is byte-for-byte what the factory
already did. The real defect is narrower and now fixed as such: the first parameter of
`CreateWithCategory(Guid categoryId, CategoryEntity category)` was dead, so it is gone from the
signature and from all 20 call sites, and `CreateManyWithCategory`'s doc comment no longer claims a
navigation "loaded via reflection". The two local helpers still go, and `CreateWithShootingScheduledAt`
was added for the second one.

**D2 is seven builders, not three.** The audit read three files. Sweeping every builder for the phrase
found the same false doc on `ContentPaymentBuilder.WithOrder`, `PlaylistVideoBuilder.WithVideo`,
`ArticleBuilder.WithCategory` and `CategoryBuilder.WithContentType` — and the duplicated assignment
line on two more (`PlaylistVideoBuilder`, `ArticleBuilder`). All seven now say what they do. The three
Identity builders that mention a navigation (`SessionBuilder.WithUser`, `UserRoleBuilder.WithRole`,
`RolePermissionBuilder.WithPermission`) really do attach one and were left alone.

**D3's scenario could never have been arranged that way.** Fixing the discarded entity made the test
fail, which is the point of fixing it — but not with the assertion it was written for:
`fk_lyrics_videos_video_id` rejects a lyrics row pointing at a video id that does not exist. The test
now seeds a real video, deletes it, and lets `OnDelete(SetNull)` produce the stale link, mirroring the
deleted-artist test three methods below it. That is the first time the stale-video path has been
exercised.

**`PermissionFactory.CreateDefault` is 45 sites, not 72**, and `RoleFactory.CreateDefault` is 21 as
stated. The 72 counted the `Create()` and three-argument forms alongside the two-argument one.

**`SetupIsSessionValid` is 27 sites, not 4** — the audit's grep was scoped to one file.

## Deliberately not done

**`MockCommerceCustomerNotifier.Create()`** — proposed for 8 files. Read in full, those eight mocks
carry no setups at all and each test verifies a *different* member of the interface. A `Create()` that
only returns `new()` would name nothing, which is the same reason the audit itself rejected an in-module
`OutboxEmailFactory`. The eight `new()` declarations stay.

**`MockCommerceCustomerNotifier.Create()`** is the only proposal rejected outright. Step 5 and step 0
were carried out afterwards, on the owner's go-ahead — see below.

## Step 5 — the structural items, applied on the owner's go-ahead

| Item | Change | Sites |
| --- | --- | --- |
| `SessionBuilder` `CreatedAt` | Defaulted in the constructor, so `WithCreatedAt` is now an override rather than a requirement; the hand-stamped chains collapse into `SessionFactory` members | 18 |
| `ContentDbContext` bootstrap | `ContentDbContextFactory.CreateInMemory()` / `CreateInMemoryOptions()` in `Content.TestData`; the 6-line constructor body in each repository test becomes one line | 24 |
| `SeedCategoryAsync` | `ContentSeeder.AddCategory(context)` in `Content.TestData`; each suite keeps its own one-line `SeedCategoryAsync` wrapper, since `SeedAsync` is a fixture member | 17 |
| Editorial request builders | 30 new `*RequestBuilder` types for the Artist/Album/streaming-link/lyrics-revision slice, each defaulting to the modal argument its call sites already used | 214 |

The request-builder defaults were not invented: for every parameter, the default is the value that
already appeared most often at that position across the existing call sites, so a site that passed the
common value now passes nothing and a site that passed something else says so explicitly with a
`With…` call.

## Step 0 — the unused-`using` sweep

Run with Roslyn, never a regex, exactly as the plan required. Two things had to be enabled for
IDE0005 to report at all, both reverted afterwards: `GenerateDocumentationFile` (the compiler does not
emit the diagnostic without it) and `dotnet_diagnostic.IDE0005.severity = warning` (the SDK's default
analysis level silences it). Scoped to the 18 test projects; nothing under `src/` production code was
touched. CSharpier then restored the import grouping the formatter owns.

**~11,000 lines of dead `using` directives removed across 1,078 files** — including the four *Identity*
`.TestData` imports at the top of a *Storage* repository test, and the 19-line preamble every Content
commerce file carried regardless of what it used.

All 12 suites were re-run afterwards, because this is precisely where an extension method's namespace
looks unused and is not: 10,681 tests still pass.
