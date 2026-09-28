# Content — Editorial and Domain suites

Area: `Content.Unit.Tests/Application/Editorial/`, `Content.Integration.Tests/Application/Editorial/`,
`Content.Unit.Tests/Domain/`. 370 files; 10 read in full, 19 in targeted windows, all 370 swept by
exhaustive grep per pattern.

**`Content.Unit.Tests/Domain/` is clean — all 50 files.** Every direct construction is of the entity
under test, and every collaborator comes from a factory (`CategoryFactory.Create` ×39,
`ContentOrderFactory.Create` ×16, `ContentOrderItemFactory.Create` ×10,
`PromotionLevelFactory.CreateDefault` ×9). Zero hand-rolled builder chains. This directory is the model
the Editorial application tests should follow, and the strongest evidence that those tests drifted
rather than never had a convention.

## Existing factory bypassed

| Factory | Sites | Where |
| --- | --- | --- |
| `ArticleCommentFactory.Create` | **12** | `ArticleSpecificationsTests.cs:343, 357, 382, 397, 412, 431, 445, 465, 479`; `ArticleInteractionSpecificationsTests.cs:193, 209, 246` — all `ArticleCommentEntity.Create(Guid.NewGuid(), userId, articleId, "body")`. Body text changes to `TestConstants.Interactions.ValidCommentBody`; no specification under test reads `Body` |
| `VideoRatingFactory.Create` | **5** | `VideoInteractionSpecificationsTests.cs:63, 74, 91, 112, 128`. Note the factory's argument order is videoId-first |
| `ArticleArtistFactory.Link` | **3** | `ArtistContentSpecificationsTests.cs:116, 129, 181`. `ArticleArtistBuilder.Build()` and `ArticleArtistFactory.Link` have byte-identical bodies, and the sibling `ArticleSpecificationsTests.cs:504, 517` already uses the factory |
| `ArticleFactory.CreateArchived` | **2** | `AdminArchiveArticleHandlerTests.cs:135`; `PromotionFeedSpecificationTests.cs:116` — glaring, because the other two arms of the same `switch` (115, 117) call `ArticleFactory.CreatePublished`/`Create` |
| `ArticleFactory.CreateRejected` | **2** | `AdminRejectArticleHandlerTests.cs:136`; `AdminDeleteArticleHandlerTests.cs:121` |
| `CategoryFactory.CreateGossip` | **2** | `PromotionFeedSpecificationTests.cs:205, 220` |

## Request builder bypassed by an anonymous object, in files that already import the builder

- `AdminRejectArticleEndpointV1Tests.cs:61, 74, 87` — `new { Reason = "test" }`, while `:97, 115, 134, 155` of the same file use `new AdminRejectArticleRequestBuilder().Build()`
- `AdminAttachYoutubeVideoUrlEndpointV1Tests.cs:65, 78` — `new { YoutubeVideoUrl = ValidYoutubeUrl }`, while `:88, 108, 135, 156` use the builder

Both are 401/403/404 tests that never reach validation, so the builder's different default `Reason` is
immaterial. Empty `new { }` bodies elsewhere are deliberate — see
[13-correctly-direct.md](13-correctly-direct.md).

## A factory result built and thrown away

`PublicGetArtistBySlugEndpointV1Tests.cs:214-232` calls `ArtistFactory.CreateWithIdentity(...)`, then at
`:222` re-creates the entity through `ArtistEntity.Create(artist.Id, artist.Name, slug, …)` purely to
substitute the slug, then calls `ClaimOwnership` at `:233`. One builder chain covers all of it:

```csharp
new ArtistBuilder().WithRealName(...).WithAliases([...]).WithBirthdate(birthdate)
    .WithHometown(...).WithSlug(slug).AsClaimedBy(Guid.NewGuid()).Build()
```

`AsClaimedBy` already calls `ClaimOwnership(userId, TestConstants.Clock.Instant)`. One difference:
`ArtistBuilder.Build()` stamps `CreatedAt`, which the discard-and-rebuild path loses; nothing here
asserts it. Side effect: `ArtistFactory.CreateWithIdentity` then has **no** callers and should go.

## Missing factories

Full list with every site in [10-missing-factories.md](10-missing-factories.md). The area's largest:

| Proposed | Sites | Shape |
| --- | --- | --- |
| `LyricsFactory.CreatePublishedWithSlug(Guid categoryId, string slug)` | **22** | `CreateWithSlug(...)` followed by `MarkPendingReview(); Approve(); Publish(TestConstants.Clock.Instant);` — which is exactly `LyricsBuilder.AsPublished()` (`LyricsBuilder.cs:260-264`) |
| `ArticleLikeFactory.Create` / `ArticleBookmarkFactory.Create` | 13 / 11 in this area | No factory for either, though `LyricsLikeFactory.Create(lyricsId, userId)` is the precedent |
| `LyricsFactory.CreatePublishedForVideo(Guid, Guid)` overload | 9 | An entity-taking overload exists with 1 caller; these need the `Guid` form |
| `ShortVideoLikeFactory` / `ShortVideoBookmarkFactory` | 9 / 4 | `ShortVideoInteractionSpecificationsTests.cs` is one of only three files in the area that touch the test-data layer **not at all** |
| `PromotionLevelFactory.CreateWithSpotPriority(int)` | 7 | `new PromotionLevelBuilder().WithSpotPriority(n).Build()` |
| `ShortVideoViewEventFactory.CreateCounted/Uncounted` | 7 | Via one private local helper at `ShortVideoSpecificationsTests.cs:270-285`; `LyricsViewEventFactory` is the precedent |
| `LyricsFactory.CreatePublishedWithTags(Guid, params Guid[])` | 6 | `CreateWithTags` + the same 3-line publish |

Plus `ShortVideoFactory.CreateWithTitle` (3), `VideoFactory.CreateWithTitleAndDescription` (3),
`LyricsFactory.CreateWithLanguage` (3), `VideoFactory.CreateWithShootingScheduledAt` (4),
`ArticleFactory.CreateWithCreatedAt` (3), `VideoTagFactory.Attach` (a verbatim-duplicated private helper
in two files), and `VideoFactory.CreatePendingPayment` — one site, but its Article and Lyrics twins both
have the method, so the triple is incomplete.

## The request-builder gap

`AdminRejectArticleRequestBuilder` and `AdminRejectVideoRequestBuilder` exist for the same
`record X(string Reason)` shape; **`AdminRejectLyricsRequest` has none** and is newed inline 3× plus 3
anonymous objects. Likewise `AdminForceUnpromoteArticle/VideoRequestBuilder` exist while
`AdminForceUnpromoteLyricsRequest` is newed inline 7×.

With no builder and 3+ sites each: `AdminCreateArtistRequest` (7 parameters, 11 sites),
`AdminUpdateArtistRequest` (6, 6), `AdminCreateAlbumRequest` (5, 6), `AdminUpdateAlbumRequest` (4, 6),
`AdminUpdateLyricsMetadataRequest` (5, 6), `PublicSubmitLyricsRequest` (5, 7).

**The Artist/Album/Streaming-link slice of Editorial has no request builders at all** — roughly 30
request types constructed inline. That is the area's largest structural gap, though it was measured by
grep across those files rather than by reading each of them.

## Defects

`VideoFactory.CreateWithCategory` is broken, one test discards its own arrangement, and a transition is
applied twice — all three in [12-defects-found.md](12-defects-found.md).
