# Correctly direct — read before any bulk change

Sites that look like the defects catalogued elsewhere and are not. A mechanical pass over the patterns
in this audit would break or degrade every one of them.

## The stated exception: a factory's own guard tests

A domain guard test **must** call `XEntity.Create(...)` directly, because that factory is the subject.
Verified clean on this basis:

- **`Content.Unit.Tests/Domain/` — all 50 files.** Every direct construction is of the entity under test; every collaborator comes from a factory.
- `Identity.Unit.Tests/Domain/Entities/UserEntityTests.cs:46, 69, 91, 113, 137, 155, 174, 194` — the reference implementation of the exception.
- `SessionEntityTests.cs:44, 88, 361`, `RoleEntityTests.cs:40, 62, 79`, `PermissionEntityTests.cs:41, 64, 89, 111`, `OtpEntityTests.cs:37, 64`.
- `Storage.Unit.Tests/Domain/Entities/FileEntityTests.cs:43, 61, 79, 107, 131, 155, 179, 203` — including six guard cases asserting `StorageRuleException.Code`. The same file uses `FileFactory` for the `Delete`/`MarkReplaced` tests that merely need *a* file (`:224-418`), which is exactly the right split.
- `Mailer.Unit.Tests/Domain/Entities/{NewsletterSubscriber,Notification,OutboxEmail}EntityTests.cs` — the factory and its transitions are the subject.

**But not the whole file.** Seven arrange blocks inside `*EntityTests.cs` use the domain factory
incidentally and should use the module factory — `RoleEntityTests.cs:320, 334, 348` and
`PermissionEntityTests.cs:400, 414`, where `ClearDomainEvents()` immediately discards whatever `Create`
raised, and `PermissionEntityTests.cs:432` already does it the right way in the same file.

## Where a builder cannot express the case

| Site | Why |
| --- | --- |
| `UserEntityTests.cs:208` | `CreateExternal(…, providerSubjectId: null!)`. `UserBuilder.WithProviderSubjectId` takes a non-nullable `string` with a non-null default, so the builder cannot express an un-linked account |
| `SessionEntityTests.cs:361` | Asserts `SessionCreatedEvent.IsNewDevice`; `SessionBuilder` has no `WithIsNewDevice` |
| `AccountStatusRequirementHandlerTests.cs:167-170` | `.ReturnsAsync((UserEntity?)null)`; `SetupFindUserByIdOrThrowNotFound` throws, and the test needs the null path |
| `PermissionSpecificationsTests.cs:237-239`, `RoleSpecificationsTests.cs:237-239` | `Create(); Activate(); SoftDelete(now)`. `CreateDeleted()` looks like the fix but `AsDeleted()` also sets `_isActive = false`, destroying the *deleted-but-active* case these tests exist to pin |
| `PermissionSpecificationsTests.cs:341-345`, `RoleSpecificationsTests.cs:353-357` | A `CreateInactive()` swap would drop the resource or name the surrounding assertion reads. Needs an overload first |
| `AdminLoginHandlerTests.cs:180-186` | Inline `SessionResult` with captured expiry locals it then asserts on; the helper's timestamps are unreachable |
| `SessionMapperTests.cs:315-325` | `new SessionDto(...)` — the DTO *is* the subject (a `with`-expression check) |
| `UserRoleEntityTests.cs:41`, `RolePermissionEntityTests.cs:42` | Assert `Id.Should().Be(Guid.Empty)`, which the builders deliberately break by stamping a new Guid. Same for `SuperAdminEntityFactoryTests.cs:294, 312, 340` |
| `AccountLockoutRepositoryTests.cs:58, 76`, `UserTokenStateRepositoryTests.cs:45, 71, 88, 120` | `UserLoginStateEntity` / `UserOtpStateEntity` / `UserTokenStateEntity` have no builder, and each takes one id with no configurable state — a builder would be a pure rename |

## Values that *are* the assertion

- `Identity` validator-extension tests — all 9 files (`CredentialValidationTests`, `FileValidationTests`, `OtpValidationTests`, `PermissionValidationTests`, `ProfileValidationTests`, `RoleValidationTests`, `SessionValidationTests`, `SocialLoginValidationTests`, `ValidationUtilsTests`) declare private command shapes and feed them `null!`, `"   "`, `new string('a', MaxEmailLength + 1)`, `"user@@example.com"`, `"josé"`, `"avatar.exe"`. Only the *file mock* in `FileValidationTests` is a defect.
- All `*ValidatorTests.cs` command construction across Identity (~35 files) — deliberately invalid values; no Auth command builder exists, and one would be wrong.
- `ContentOrderQueryBuilderTests.cs:171-180`, `ContentPaymentQueryBuilderTests.cs:61-65`, `ContentOrderSpecificationTests.cs:42-46` — `new CustomerBuilder().WithFullName("Acme Corp").WithEmail("grace@acme.io")…`: the literals are the search-match assertion. Same for `ContentOrderRepositoryTests.cs:175, 176, 177, 233`.
- `LookupSpecificationsTests.cs:178, 285` — `WithName("Gold"/"Featured")`: the name under test.
- `VideoRepositoryTests.cs:382, 383` — `WithSpotPriority(1)/(2)`: ordering is the assertion.

## Conventions that look like gaps

- **Content has no `*RequestFactory` at all.** Request shapes are deliberately kept at the call site through `*RequestBuilder` chains — `AdminCreatePromotionLevelRequestBuilder` (8 sites), `PublicEditArticleCommentRequestBuilder` (7), `AdminUpdateTagRequestBuilder` (6), and a dozen more. The pattern-matching pass flagged these; they are the convention.
- **Identity command records built inline in handler tests.** `Identity.TestData/Builders/Commands/` covers only the four Roles commands, and those *are* reached through `CommandFactory`. The rest vary a field per test by design; 20 new command builders would be wrong.
- **Empty `new { }` request bodies** in Content endpoint tests — `UpdateArticleSeo:38, 51, 64`, `UpdateArticleTags:38, 51`, `UpdateVideoTags:52, 65`, `UpdateVideoSeo:52, 65, 78`, `UpdateVideo:57, 68`, `UpdateLyrics:49, 60`, `CreateVideo:51`. These are 401/403/404 tests where the payload is deliberately irrelevant; a builder would imply it matters.
- **Production query builders are the subject, not test data** — `ArticleQueryBuilder`, `VideoQueryBuilder`, `LyricsQueryBuilder`, `ShortVideoQueryBuilder`, `PopularArticles/PopularVideosQueryBuilder`: ~68 sites in `Editorial/Builders/`. Likewise `new DbContextOptionsBuilder<ContentDbContext>()`.
- **Per-fixture `SeedCategoryAsync` / `SeedVideoAsync` / `SeedArticleAsync` helpers** wrap `DbContext` persistence, not entity shaping, and several already take a factory delegate. Their *duplication* is worth fixing; their existence is not a defect.
- **`EmailDispatcherTests.cs:60-74`** — `Subscribed()`/`Unsubscribed()` derive from `Pending()` by calling the real transitions, deliberately proving the state was reached legitimately. Only `:57` needs the factory.
- **`StreamingLinkFactoryTests.cs`** tests the *production* `StreamingLinkFactory.GenerateSearchUrl`; the name collision with the test-data layer is incidental.

## Whole suites with nothing to report

`Identity.Integration.Tests/Roles/**` (22 files, uniformly factory + request builder);
`Content.Unit.Tests/Application/Lookup` (39 files, zero hits for any misuse pattern);
`Content.Integration.Tests/Infrastructure/Mappers/*` (all entity arrangement via factories; only the
mapper bootstrap repeats); `Shared.Unit.Tests` (68 files — its project cannot see any module
`.TestData`, so a finding of this kind cannot exist there); the Like/Unlike/Bookmark/Unbookmark/Share/Rate
handler families in Content (~14 files, all through the existing extension methods — `PublicLikeArticleHandlerTests`
and `PublicUnlikeArticleHandlerTests` read as the reference standard).
