# Defects found while reading

None of these is a test-data placement question. They were found because auditing the factory layer
means reading what the factories actually do, and five of them do not do what they say.

## D1 — `VideoFactory.CreateWithCategory` ignores its own argument and attaches nothing

`Content.TestData/Factories/VideoFactory.cs:114`

```csharp
/// <summary>
/// Creates a free video filed under the given category. The category name reaches a
/// projection through the resolved lookups, not through the video.
/// </summary>
public static VideoEntity CreateWithCategory(Guid categoryId, CategoryEntity category) => Create(category.Id);
```

`categoryId` is never read, and the `Category` navigation is never populated — while
`CreateManyWithCategory` (line 119) inherits the same body and its doc comment claims "the Category
navigation property loaded via reflection." **12 files call one of the two.**

Because it does not work, two tests wrote private local helpers doing what it was supposed to do:

- `Content.Unit.Tests/Application/Editorial/Factories/VideoDtoFactoryTests.cs:62-67` — `CreateVideoWithCategory()` → `new VideoBuilder(CategoryId).WithCategory(category).Build()`
- `Content.Unit.Tests/.../AttachYoutubeVideoUrl/AdminAttachYoutubeVideoUrlHandlerTests.cs:73-83` — the same chain plus an optional `WithShootingScheduledAt`

**Fix (as implemented):** `VideoEntity` has **no** `Category` navigation — stage 15 deleted it, and
`VideoBuilder.WithCategory` only assigns the foreign key, so the prescribed builder chain is
byte-for-byte what the factory already did. The real defect is the dead first parameter: it is gone
from the signature and from all 20 call sites, `CreateManyWithCategory`'s doc no longer claims a
navigation "loaded via reflection", both local helpers are deleted, and
`CreateWithShootingScheduledAt` was added for the second one. Behaviour-preserving throughout.

## D2 — three builders promise to attach a navigation and only assign the foreign key

| Builder | Line | Symptom |
| --- | --- | --- |
| `ContentOrderBuilder.WithCustomer` | `:75-79` | Doc says "Attaches the Customer navigation EF Core populates through `.Include(o => o.Customer)`". Body is `_customerId = customer.Id;`. There is no `_customer` field |
| `PackageSlotBuilder.WithCategory` | `:60-65` | Same false doc; `_categoryId = category.Id;` **written twice on consecutive lines** |
| `ContentOrderItemBuilder.WithCategory` | `:65-70` | Same false doc; `_categoryId = category.Id;` **written twice** |

The duplicated line is the tell: a navigation assignment was removed and the foreign-key line left
doubled. Verified by reading — `PackageSlotBuilder.cs:62` and `:63` are byte-identical.

> **Measured wider on implementation: seven builders, not three.** Sweeping every builder for the
> phrase found the same false doc on `ContentPaymentBuilder.WithOrder`, `PlaylistVideoBuilder.WithVideo`,
> `ArticleBuilder.WithCategory` and `CategoryBuilder.WithContentType`, two of which also carry the
> doubled assignment. All seven now say what they do. The three Identity builders that mention a
> navigation (`SessionBuilder.WithUser`, `UserRoleBuilder.WithRole`, `RolePermissionBuilder.WithPermission`)
> really do attach one and were left alone.

**Consequence:** every test that reads `WithCustomer`/`WithCategory` as "the `Include` is populated" is
not testing that. Fix the doc comments, or attach the navigation as `CategoryBuilder.WithContentType`
does. Until then the entity-overload and id-overload factories are indistinguishable, which is what
makes the substitutions in [10-missing-factories.md](10-missing-factories.md) M1 safe.

## D3 — a test discards the entity it just arranged, so its named scenario is never exercised

`Content.Integration.Tests/.../GetLyricsBySlug/V1/PublicGetLyricsBySlugEndpointV1Tests.cs:143-144`

```csharp
LyricsEntity entity = LyricsFactory.CreateForVideo(categoryId, staleVideoId);
entity = LyricsFactory.CreateWithSlug(categoryId, slug);
```

The first entity is overwritten on the next line, so `staleVideoId` never reaches the row. The stale-
video path the test is named for is not covered.

> **On implementation:** fixing the discarded entity made the test fail, which is the point — but not
> with the arrangement assumed here. `fk_lyrics_videos_video_id` rejects a lyrics row pointing at a
> video id that does not exist, so the scenario cannot be arranged with a random Guid at all. The test
> now seeds a real video, deletes it, and lets `OnDelete(SetNull)` produce the stale link, mirroring
> the deleted-artist test three methods below it.

## D4 — a state transition applied twice

`PublicGetLyricsBySlugEndpointV1Tests.cs:145-148, 170-176, 203-207, 233-237, 270-274` and
`PublicGetPublishedLyricsEndpointV1Tests.cs:120-124` each call `MarkPendingReview(); Approve();`
**twice** before `Publish(...)`, re-entering `PendingReview` from `Approved`. A copy-paste artefact; all
six collapse into the `CreatePublishedWithSlug` factory proposed in
[10-missing-factories.md](10-missing-factories.md).

## D5 — `AdminUpdateCategoryHandlerTests` sets up the same mock twice

`AdminUpdateCategoryHandlerTests.cs:85-88` repeats the `GetByIdOrThrowAsync(category.Id)` setup made
three lines earlier at `:82` via `SetupGetByIdOrThrow(category)`. The only difference is `.Verifiable()`,
which nothing then verifies. Delete `:85-88`.

## Adjacent: two structural gaps that are not bugs but block work

- **`tests/EndToEnd.Tests/EndToEnd.Tests.csproj` does not reference `Mailer.TestData`** (it references
  Content, Identity and Storage). Confirmed. Any use of Mailer builders in the workflow tests needs the
  reference added first — which is what blocks the one genuine `OutboxEmailFactory` case.
- **Unused `using` blocks throughout the suites.** A mechanical pass left them: a *Storage* repository
  test opens with four *Identity* `.TestData` usings
  (`Storage.Integration.Tests/Infrastructure/Repositories/FileRepositoryTests.cs:1-4`), and nearly every
  Content commerce file carries the same 19-line preamble regardless of what it uses
  (`AdminAddItemTierHandlerTests.cs:5-22`, `ArticleInteractionRepositoryTests.cs:7-21`,
  `CustomerMapperTests.cs:5-18`). They compile, but they make a file's real test-data dependencies
  unreadable — which is one reason the pattern-matching pass was unreliable. Clean with Roslyn's
  IDE0005, never with a regex: an extension method's namespace looks unused and is not.
