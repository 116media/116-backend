# INSTRUCTION.md — How to Resume Editorial Unit Tests

## Purpose
This file helps restore context at the start of a new conversation. Read this file first, then read `PROGRESS.md`.

## Codebase Location
`/Users/coolbeatz/projects/116/116/apps/backend`

## What We Are Doing
Writing unit tests for the **Editorial submodule** of the Content module. This includes:
- Articles (CRUD, workflow: Draft→PendingPayment/PendingReview→Approved→Published/Rejected/Archived)
- Videos (same workflow + YouTube ID + thumbnail)
- Short Videos (no editorial workflow — Activate/Deactivate only)
- Lyrics (standalone SEO pages)

## Key Rules
1. **Never call a Builder directly in test cases** — use Factories.
2. **All created code must be fully reused** — no unused classes, constants, methods.
3. **No `[Fact(Skip=...)]` unless strictly necessary** (only for ILike/InMemory limitation).
4. **Do NOT test V1 endpoint `AddRoutes`** — those are integration tests only.
5. Test `*Response` and `*Request` record constructors in endpoint V1 tests.
6. Follow `PATTERNS.md` for test naming, AAA structure, and regions.
7. Use `AwesomeAssertions` (NOT FluentAssertions) — `using AwesomeAssertions;`
8. Use xUnit v3 (`Xunit` namespace), Moq 4.20+, Bogus for fake data.
9. Check if file exists before creating; check if constant already exists before adding.

## Files to Read First (After This + PROGRESS.md)
1. `projects/testing/TODO.md` — see what's done (`[x]`) vs pending (`[ ]`)
2. `projects/testing/PATTERNS.md` — naming, structure, regions
3. `tests/Unit/Common/BaseContentHandlerTest.cs` — base class for all Content handler tests
4. `tests/Fixtures/Constants/TestConstants.cs` — constants (check for existing Editorial section)
5. `tests/Fixtures/Builders/Entities/Content/CategoryBuilder.cs` — builder pattern
6. `tests/Fixtures/Factories/Content/CategoryFactory.cs` — factory pattern
7. `tests/Unit/Common/Mocks/Repositories/MockCategoryRepository.cs` — mock repo pattern
8. `tests/Unit/Common/Mocks/Infrastructure/MockContentUnitOfWork.cs` — mock UoW

## Infrastructure Already Created (Check These Files)
After Session 4 (2026-03-16) starts, these should exist:
- `tests/Fixtures/Builders/Entities/Content/ArticleBuilder.cs`
- `tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs`
- `tests/Fixtures/Builders/Entities/Content/ShortVideoBuilder.cs`
- `tests/Fixtures/Builders/Entities/Content/LyricsBuilder.cs`
- `tests/Fixtures/Builders/Entities/Content/ArticleImageBuilder.cs`
- `tests/Fixtures/Factories/Content/ArticleFactory.cs`
- `tests/Fixtures/Factories/Content/VideoFactory.cs`
- `tests/Fixtures/Factories/Content/ShortVideoFactory.cs`
- `tests/Fixtures/Factories/Content/LyricsFactory.cs`
- `tests/Fixtures/Factories/Content/ArticleImageFactory.cs`
- `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`
- `tests/Unit/Common/Mocks/Repositories/MockVideoRepository.cs`
- `tests/Unit/Common/Mocks/Repositories/MockShortVideoRepository.cs`
- `tests/Unit/Common/Mocks/Repositories/MockLyricsRepository.cs`
- `tests/Unit/Common/Mocks/Services/MockCloudinaryService.cs`
- `tests/Unit/Common/Mocks/Services/MockYoutubeThumbnailService.cs`

## Key Entity Methods (Domain)
### ArticleEntity
- `CreateFree(id, categoryId, title, slug, authorId)` → Draft
- `CreatePaid(id, customerId, orderItemId, categoryId, title, slug, authorId)` → Draft
- `Submit()` → bool (false if already PendingPayment)
- `MarkPendingReview()` → bool
- `Approve()` → bool
- `Publish()` → bool
- `Reject(reason)` → bool
- `Archive()` → bool
- `StampSocialBoost()`, `StampPromotion(until)`
- Counters: IncrementLikeCount, DecrementLikeCount, IncrementCommentCount, DecrementCommentCount, IncrementShareCount, IncrementBookmarkCount, DecrementBookmarkCount
- Throws TitleRequired/SlugRequired on empty

### VideoEntity
- Same factory methods + `description` optional param
- `Publish()` throws `VideoErrors.CannotPublishWithoutYoutubeId()` if no YoutubeVideoId
- `AttachYoutubeId(youtubeVideoId)`, `UpdateThumbnail(url, key)`, `ScheduleShoot(date)`, `MarkHasLyrics()`, `UpdateRating(avg, count)`, `IncrementShareCount()`

### ShortVideoEntity
- `CreateStandalone(id, title, slug, videoUrl, videoStorageKey, authorId)` — throws TitleRequired
- `CreateTeaser(id, title, slug, videoUrl, videoStorageKey, videoId, authorId)` — HasFullVideo=true
- `Activate()` → bool (false if already active), `Deactivate()` → bool
- `UpdateThumbnail(url, key)`
- Counters: View, Like (Increment/Decrement), Share, Bookmark (Increment/Decrement)

### LyricsEntity
- `CreateForVideo(id, videoId, songTitle, artistName, lyricsText, language, authorId)`
- `CreateForArticle(id, articleId, songTitle, artistName, lyricsText, language, authorId)`
- `CreateStandalone(id, songTitle, artistName, lyricsText, language, authorId)`
- All throw SongTitleRequired/ArtistNameRequired/LyricsTextRequired if empty
- `UpdateLyrics(text)` — throws LyricsTextRequired
- `UpdateSeo(metaTitle, metaDescription, metaKeywords, structuredData)`

### ArticleImageEntity
- `Create(id, articleId, storageKey, url, imageType)` — no validation

## Key Constants (ContentConstants.cs)
- `MaxTitleLength = 100` (articles/videos)
- `MaxSlugLength = 220`
- `MaxHeadlineLength = 300`, `MinHeadlineLength = 100`
- `MaxRejectionReasonLength = 500`
- `MaxYoutubeVideoIdLength = 20`
- `MaxShortVideoTitleLength = 200`
- `MaxSongTitleLength = 200`, `MaxArtistNameLength = 100`, `MaxLyricsLanguageLength = 5`
- `DefaultLyricsLanguage = "fr"`

## ICloudinaryService
- `UploadImageAsync(IFormFile, publicId, folder?, ct) → CloudinaryUploadResult`
- `DeleteImageAsync(string storageKey, ct) → Task<bool>`
- `DeleteImagesAsync(IEnumerable<string> storageKeys, ct) → Task<bool>`
- `CloudinaryUploadResult` is a positional record with 7 params: `(PublicId, SecureUrl, Format, Width, Height, Bytes, ResourceType)`

## IYoutubeThumbnailService
- `DownloadThumbnailAsync(string youtubeVideoId, ct) → Task<IFormFile>`

## Test Project Namespaces
- Unit tests: `_116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.*`
- Fixtures: `_116.Tests.Fixtures.Builders.Entities.Content` / `_116.Tests.Fixtures.Factories.Content`
- Mocks: `_116.Unit.Tests.Common.Mocks.Repositories` / `_116.Unit.Tests.Common.Mocks.Services`

## How Handlers Look (for Test Writing)
- All handlers are in `src/Modules/Content/Content/Application/Editorial/UseCases/`
- Pattern: primary constructor params, then `Handle(command/query, ct)` method
- After writing a command: `Add → Commit → reload → map → return`
- `GetByIdOrThrowAsync` throws `NotFoundException` if not found
- Error factory methods like `ArticleErrors.SlugAlreadyExists()` throw `ConflictException`
- Status gate errors like `AlreadyApproved` throw `ConflictException`
- `InvalidStatusTransition` throws `BadRequestException`

## Running Tests
```bash
dotnet build
dotnet test tests/Unit --no-build --filter "FullyQualifiedName~Editorial"
```
