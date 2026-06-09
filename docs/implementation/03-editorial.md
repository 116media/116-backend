# Editorial Sub-Module — Implementation Plan

> Depends on: Catalog (categories must exist). This is the core product of the platform.

## Scope

| Entity | SQL Table | Repository |
|---|---|---|
| `ArticleEntity` | `content.articles` | `IArticleRepository` |
| `ArticleImageEntity` | `content.article_images` | `IArticleRepository` |
| `VideoEntity` | `content.videos` | `IVideoRepository` |
| `ShortVideoEntity` | `content.short_videos` | `IShortVideoRepository` |
| `LyricsEntity` | `content.lyrics` | `ILyricsRepository` |

---

## Image Handling Architecture

> Read this entire section before implementing any article endpoint.

### Article creation — two frontend steps, two API calls

The frontend form is split into two steps. Each step maps to a distinct API call:

| Frontend step | Fields | API call | When |
|---|---|---|---|
| Step 1 | `categoryId`, `title`, `slug`, `customerId?`, `orderItemId?` | `POST /api/v1/admin/articles` | Admin clicks **"Save Draft"** |
| Step 2 | `headline`, `body`, `coverImageUrl?` | `PUT /api/v1/admin/articles/{id}` + `PATCH /{id}/submit` | Admin clicks **"Submit"** |

**"Save Draft"** creates the article shell in the DB and returns `{ articleId }`. The article is born as `Draft` with `body = ''` and `headline = ''`.

**Step 2** uses that `articleId` for all image uploads. When the admin clicks **"Submit"**, the frontend first calls `PUT` to save the content, then `PATCH /submit` to transition the status.

### Full article authoring flow

```
Step 1 — Admin fills: categoryId, title, slug, customerId?, orderItemId?
        │
        ▼
Admin clicks "Save Draft"
        │
        ▼
POST /api/v1/admin/articles
  → article created: body='', headline='', author_id=JWT userId, status=Draft
  → returns { articleId }
        │
        ▼
Step 2 — Admin writes headline, body. Inserts images in the rich-text editor.
        │
        ▼
On each image insert — editor upload handler fires immediately (before Submit):
  POST /api/v1/admin/articles/{articleId}/images
  { file: IFormFile, imageType: "cover" | "body" }
  → backend uploads to Cloudinary (folder: content/article-images/)
  → stores in article_images: { article_id, storage_key, url, image_type }
  → returns { url, storageKey }
  → editor replaces blob/base64 with the Cloudinary URL in-place
        │
        ▼
Admin clicks "Submit"
        │
        ▼
PUT /api/v1/admin/articles/{articleId}
  { headline, body, coverImageUrl? }
  → body already contains only Cloudinary URLs — no base64, no blobs
  → handler diffs old vs new image URLs, deletes removed images from Cloudinary after commit
        │
        ▼
PATCH /api/v1/admin/articles/{articleId}/submit
  → transitions: Draft → PendingPayment (paid) or Draft → PendingReview (free)
```

### Why backend-proxied upload (not client-direct to Cloudinary)

- Cloudinary API keys stay on the server at all times
- All uploads go: browser → backend endpoint → Cloudinary
- Full control over folder structure and public_id naming
- Consistent with the existing avatar upload infrastructure in the Core module

### `author_id` — auto-set from JWT, no FK

`author_id` stores the authenticated user's UUID from the JWT claims. It is **never** passed by the client — the handler reads it from `HttpContext.User`. No FK to `identity.users` by design: the content schema is intentionally cross-schema FK-free so it can be extracted as a microservice.

`created_by` (inherited audit field on all `Aggregate<T>`) also stores the user ID. The distinction:
- `author_id` — editorial byline, used publicly ("Written by...")
- `created_by` — internal system audit trail, set by infrastructure automatically

### `headline` field

Short teaser / aperçu displayed on article cards, feeds, and meta previews. 100–300 characters. Max 300 enforced by `VARCHAR(300)`. Min 100 enforced at application level only (validator on `PUT`, not on `POST` since draft starts with `headline = ''`).

### `social_boost`, `is_promoted`, `promoted_until` — never set by the article form

These are stamped automatically by the **commerce payment verification flow**:
- `social_boost = true` → order item includes `social_boost` pricing tier AND payment verified
- `is_promoted = true` → order includes a promotion level AND payment verified
- `promoted_until` → `payment.verified_at + promotion_level.duration_days`

The admin never touches these fields through any article endpoint. They are side-effects of payment verification in the Commerce sub-module.

### Update image diff (on every PUT)

```
Load article_images WHERE article_id = this id    → set B (currently tracked)
Extract all Cloudinary URLs from new body HTML    → set A (still in use)

set B − set A = removed images
  → after DB commit: ICloudinaryService.DeleteImagesAsync(removedPublicIds[])
  → DELETE FROM article_images WHERE id IN (removed rows)

Cover replacement (old coverImageUrl ≠ new coverImageUrl):
  → after DB commit: ICloudinaryService.DeleteImageAsync(oldCoverPublicId)
  → DELETE FROM article_images WHERE id = old cover row
```

Cloudinary deletion always happens **after** the DB commit. A lingering orphan is acceptable; a broken article is not.

### Abandoned draft cleanup — background job

If the admin completes step 1 ("Save Draft") but never comes back to complete step 2:

```sql
SELECT id FROM content.articles
WHERE status = 'draft'
  AND body = ''
  AND headline = ''
  AND created_at < now() - interval '24 hours';
```

For each result:
1. Load `article_images` WHERE `article_id = draft.id`
2. Call `ICloudinaryService.DeleteImagesAsync(storageKeys[])`
3. `DELETE FROM content.articles WHERE id = draft.id` — cascades to `article_images`

### Required changes to Core module (before any article endpoint)

- Add `DeleteImageAsync(string publicId, CancellationToken ct) : Task<bool>` to `ICloudinaryService` and `CloudinaryService`
- Add `DeleteImagesAsync(IEnumerable<string> publicIds, CancellationToken ct) : Task<bool>` to `ICloudinaryService` and `CloudinaryService` (uses Cloudinary's `DeleteResourcesAsync()`, max 100 per batch)

### Article deletion and Cloudinary — hard delete vs archive

These are two fundamentally different operations with opposite image handling:

**Archive (`PATCH /{id}/archive`) — reversible, images untouched**
- Sets `status = Archived` only
- Article is hidden from all public feeds
- `article_images` rows and Cloudinary resources are **not touched**
- Rationale: archive can be undone (admin restores to `Approved` → re-publishes). If Cloudinary images were deleted on archive, restoring the article would result in broken image references.

**Hard delete (`DELETE /{id}`) — permanent, Cloudinary cleaned up first**

The order of operations is critical:

```
1. Load article_images WHERE article_id = id
   → collect all storage_keys (both cover and body images)

2. ICloudinaryService.DeleteImagesAsync(storageKeys[])
   → bulk delete from Cloudinary

3. DELETE FROM content.articles WHERE id = id
   → ON DELETE CASCADE automatically removes article_images rows
```

> **Never delete the article from DB first.** `ON DELETE CASCADE` wipes `article_images`
> immediately, taking the `storage_key` values with it. If Cloudinary deletion
> then fails or is skipped, those images are permanently orphaned with no recovery path.

Hard delete is only permitted for articles in `Draft` or `Rejected` status. Published or approved articles must be archived instead — they may be referenced by external links, bookmarks, or order records.

### All cleanup mechanisms — summary

| Scenario | Cloudinary images | DB rows | Mechanism |
|---|---|---|---|
| Step 1 saved, step 2 never completed | Deleted | Deleted (cascade) | Background job after 24h |
| Image removed from body during edit | Deleted after commit | Deleted after commit | Update diff in handler |
| Cover image replaced | Old cover deleted after commit | Old cover row deleted | Update handler |
| Article **archived** | **Not touched** — reversible | `status = Archived` | `ArchiveArticleCommandHandler` |
| Article **hard deleted** | Deleted **before** DB delete | Cascade-deleted | `DeleteArticleCommandHandler` — load images → Cloudinary delete → DB delete |

### Videos — thumbnail storage key

Videos have a single optional thumbnail (`thumbnail_url`). No body images, no diff needed. The `thumbnail_storage_key` column is stored directly on the `videos` row — no separate tracking table needed.

- **Thumbnail upload**: `POST /api/v1/admin/videos/{id}/thumbnail` — uploads to media storage, returns `{ url, storageKey }`, handler calls `VideoEntity.UpdateThumbnail(url, storageKey)`
- **Thumbnail replacement**: on `PUT /api/v1/admin/videos/{id}`, if `thumbnailUrl` changed → delete old `thumbnail_storage_key` from media storage after commit
- **Hard delete**: load `thumbnail_storage_key`, call `ICloudinaryService.DeleteImageAsync(storageKey)` before DB delete
- **Archive**: thumbnail untouched — same rule as articles

### Short videos — video file + thumbnail storage keys

Short videos have both a video file (`video_url`, `video_storage_key`) and an optional thumbnail (`thumbnail_url`, `thumbnail_storage_key`). Both keys stored directly on the `short_videos` row.

- **Video file upload**: `POST /api/v1/admin/shorts` — `video_storage_key` set at creation, required
- **Thumbnail upload**: `POST /api/v1/admin/shorts/{id}/thumbnail` — optional, sets `thumbnail_storage_key`
- **Deactivate**: no Cloudinary deletion — deactivation is reversible
- **Hard delete**: load both `video_storage_key` and `thumbnail_storage_key`, call `ICloudinaryService.DeleteImagesAsync([videoKey, thumbnailKey])` before DB delete

### Storage key naming convention

All columns and properties tracking media storage identifiers are named `*_storage_key` (not `*_cloudinary_public_id`). This is intentional: the schema and domain model must remain meaningful if the CDN provider changes (e.g. from Cloudinary to AWS S3, Cloudflare R2, Bunny CDN). Each provider uses its own term (`public_id`, `key`, `path`) but all map cleanly to `storage_key`.

### DB changes already applied in `CONTENT_SCHEMA.sql`

| Change | Detail |
|---|---|
| `articles.author_name` → `author_id TEXT NOT NULL` | UUID from JWT, no FK |
| `articles.headline VARCHAR(300) NOT NULL DEFAULT ''` | Added after `slug` |
| `articles.body TEXT NOT NULL DEFAULT ''` | Added `DEFAULT ''` for empty draft shell |
| New `content.article_images` table | `article_id`, `storage_key`, `url`, `image_type`, `created_at` |
| `videos.author_id TEXT NOT NULL` | UUID from JWT, no FK — same pattern as articles |
| `videos.thumbnail_storage_key TEXT` | Provider-agnostic key, NULL until thumbnail uploaded |
| `short_videos.video_storage_key TEXT NOT NULL` | Required — video file must always be trackable |
| `short_videos.thumbnail_url VARCHAR(500)` | Optional thumbnail image |
| `short_videos.thumbnail_storage_key TEXT` | NULL until thumbnail uploaded |

---

## 🔴 CRUCIAL — Core content creation and the publishing workflow

---

### POST /api/v1/admin/articles  *(Step 1 — "Save Draft")*

> Creates the article shell when the admin clicks "Save Draft" at the end of step 1.
> Only identifiers are required at this point — body and headline are empty.
> Returns the `articleId` which the frontend uses for all image uploads during step 2.
> For paid content, both `CustomerId` and `OrderItemId` must be provided together.
> The article starts in Draft status and is never publicly visible until published.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateArticleCommand(CategoryId, Title, Slug, CustomerId?, OrderItemId?)` |
| **Response** | `201` + `ArticleDetailDto` |

> `author_id` is set automatically from the JWT — never passed by the client.
> `body` and `headline` default to `''` — filled in step 2 via `PUT /{id}`.
> `CustomerId` and `OrderItemId` are both null for free content. Both must be provided together for paid content.

**TODOs**
- [ ] `ArticleEntity.CreateFree(id, categoryId, title, slug, authorId)` — `body=''`, `headline=''`, status=`Draft`
- [ ] `ArticleEntity.CreatePaid(id, customerId, orderItemId, categoryId, title, slug, authorId)` — `body=''`, `headline=''`, status=`Draft`
- [ ] `CreateArticleCommand(Guid CategoryId, string Title, string Slug, Guid? CustomerId, Guid? OrderItemId) : ICommand<ArticleDetailDto>`
- [ ] `CreateArticleCommandValidator` — `Title` max 200, `Slug` max 220. If `CustomerId` set then `OrderItemId` must also be set (and vice versa)
- [ ] `CreateArticleCommandHandler`:
  - reads `authorId` from `HttpContext.User` JWT claims (never from request body)
  - verifies `CategoryId` exists and is active
  - checks slug not taken (`IArticleRepository.GetBySlugAsync()`)
  - calls correct factory based on `CustomerId` presence
  - calls `IArticleRepository.AddAsync()`, commits `IContentUnitOfWork`
- [ ] `ArticleRepository.AddAsync(article)` and `ArticleRepository.GetBySlugAsync(slug)`
- [ ] `CreateArticleEndpointV1` Carter module

---

### POST /api/v1/admin/articles/{id}/images  *(image upload — editor fires this on each image insert)*

> Uploads a single image (body image or cover) to Cloudinary and records it in `article_images`.
> The rich-text editor calls this endpoint the moment the admin inserts any image during step 2 —
> before "Submit" is ever clicked. The returned URL is immediately embedded in the editor body.
> Because the article already exists from step 1 ("Save Draft"), `article_id` is always known
> at upload time — no staging, no null article_id, no ambiguity.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `FileUpload` |
| **Command** | `UploadArticleImageCommand(ArticleId, File, ImageType)` |
| **Response** | `201` + `{ url, storageKey }` |

**TODOs**
- [ ] `ArticleImageEntity` — `Entity<Guid>`, properties: `ArticleId`, `StorageKey`, `Url`, `ImageType (ArticleImageType)`, `CreatedAt`
- [ ] `ArticleImageType` enum — `Cover = 0`, `Body = 1`
- [ ] `UploadArticleImageCommand(Guid ArticleId, IFormFile File, ArticleImageType ImageType) : ICommand<UploadArticleImageResult>`
- [ ] `UploadArticleImageCommandValidator` — file required, valid image extension + MIME type, max size (reuse `FileConstants` from Core module), `ImageType` valid enum value
- [ ] `UploadArticleImageCommandHandler`:
  - verifies article exists (`IArticleRepository.GetByIdAsync`)
  - generates `publicId = $"content/article-images/{Guid.NewGuid()}"`
  - calls `ICloudinaryService.UploadImageAsync(file, publicId, folder: "content/article-images")`
  - creates `ArticleImageEntity`, calls `IArticleRepository.AddImageAsync(image)`, commits UoW
  - returns `{ Url, StorageKey }`
- [ ] `IArticleRepository.AddImageAsync(ArticleImageEntity image, CancellationToken ct)`
- [ ] `ICloudinaryService.DeleteImageAsync(string publicId, CancellationToken ct)` — add to Core module `ICloudinaryService` + `CloudinaryService`
- [ ] `ICloudinaryService.DeleteImagesAsync(IEnumerable<string> publicIds, CancellationToken ct)` — add to Core module (Cloudinary batch delete, max 100 per call)
- [ ] `UploadArticleImageEndpointV1` Carter module — `multipart/form-data`

---

### POST /api/v1/admin/videos

> Creates a new video record for a commissioned show (116 Le Focus, FlexBeat, Music Video, BTS,
> etc.) or a standalone free video. For pre-booked productions, the client pays before the shoot
> takes place, so the video starts in Draft with a `ShootingScheduledAt` date that lets the
> production team calendar the filming. The video goes through the full editorial workflow — submit,
> approve, YouTube link attachment, then publish — before it appears on the public video feed.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateVideoCommand(CategoryId, Title, Slug, Description?, ThumbnailUrl?, CustomerId?, OrderItemId?, ShootingScheduledAt?)` |
| **Response** | `201` + `VideoDetailDto` |

**TODOs**
- [ ] `VideoEntity.CreateFree(id, categoryId, title, slug, authorId, description?)` — `authorId` from JWT, status=`Draft`, `ThumbnailUrl=null`, `ThumbnailStorageKey=null`
- [ ] `VideoEntity.CreatePaid(id, customerId, orderItemId, categoryId, title, slug, authorId, description?)` — same defaults
- [ ] `CreateVideoCommand(Guid CategoryId, string Title, string Slug, Guid? CustomerId, Guid? OrderItemId, string? Description, DateTimeOffset? ShootingScheduledAt) : ICommand<VideoDetailDto>`
- [ ] `CreateVideoCommandValidator` — title max 200, slug max 220. If `CustomerId` set then `OrderItemId` must also be set
- [ ] `CreateVideoCommandHandler`:
  - reads `authorId` from `HttpContext.User` JWT claims (never from request body)
  - verifies `CategoryId` exists and is active
  - checks slug not taken (`IVideoRepository.GetBySlugAsync()`)
  - calls correct factory based on `CustomerId` presence
  - optionally calls `VideoEntity.ScheduleShoot(scheduledAt)`
  - calls `IVideoRepository.AddAsync()`, commits `IContentUnitOfWork`
- [ ] `VideoRepository.AddAsync(video)` and `VideoRepository.GetBySlugAsync(slug)`
- [ ] `CreateVideoEndpointV1` Carter module

---

### PATCH /api/v1/admin/articles/{id}/submit

> Moves the article out of Draft and into the next stage. For paid articles (linked to a customer
> and order item), this transitions to `PendingPayment` — the customer has not yet paid and the
> article waits for payment verification. For free articles (no customer link), it goes straight
> to `PendingReview` — the editorial team can immediately review it for publishing. This signals
> to the team that the article content is complete and ready for the next step in the workflow.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `SubmitArticleCommand(Id)` |
| **Response** | `204 No Content` |

> Moves article from `Draft` → `PendingPayment` (paid) or `Draft` → `PendingReview` (free).
> The handler reads `article.CustomerId` to determine which transition to apply.

**TODOs**
- [ ] `SubmitArticleCommand(Guid Id) : ICommand`
- [ ] `SubmitArticleCommandHandler` — fetches article, calls `ArticleEntity.Submit()` if paid (has `CustomerId`), calls `ArticleEntity.MarkPendingReview()` if free, calls `IArticleRepository.UpdateAsync()`, commits UoW
- [ ] `ArticleRepository.GetByIdAsync(id)` and `ArticleRepository.UpdateAsync(article)`
- [ ] `SubmitArticleEndpointV1` Carter module

---

### PATCH /api/v1/admin/articles/{id}/approve

> Marks the article as editorially approved and cleared for publication. Only articles that are
> in `PendingReview` status can be approved — this ensures the workflow is respected and no
> article can be published without an editorial sign-off. For paid articles, `PendingReview` is
> reached after payment is verified. For free articles, it is reached directly after submit. This
> is the editorial team's confirmation that the article is accurate, on brand, and ready to go live.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ApproveArticleCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `ApproveArticleCommand(Guid Id) : ICommand`
- [ ] `ApproveArticleCommandHandler` — fetches article (must be in `PendingReview`), calls `ArticleEntity.Approve()`, calls `IArticleRepository.UpdateAsync()`, commits UoW
- [ ] `ApproveArticleEndpointV1` Carter module

---

### PATCH /api/v1/admin/articles/{id}/publish

> Makes the article publicly visible on the platform. Sets `PublishedAt` to the current time
> and changes the status to `Published`. This is the final step in the editorial workflow and
> the moment the content becomes live for all visitors. Only `Approved` articles can be published
> — the endpoint enforces this gate to prevent bypassing the review process.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `PublishArticleCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `PublishArticleCommand(Guid Id) : ICommand`
- [ ] `PublishArticleCommandHandler` — fetches article (must be in `Approved`), calls `ArticleEntity.Publish()` (sets `PublishedAt = UtcNow`, `Status = Published`), calls `IArticleRepository.UpdateAsync()`, commits UoW
- [ ] `PublishArticleEndpointV1` Carter module

---

### PATCH /api/v1/admin/articles/{id}/reject

> Rejects the article at the editorial review stage and records a mandatory reason. Useful when
> the article does not meet quality standards, contains factual errors, or is not on brand. The
> article moves to `Rejected` status and can be revised by the admin before being resubmitted.
> Using rejection instead of deletion preserves the article and its history so the team can
> iterate on it with the client rather than starting from scratch.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RejectArticleCommand(Id, Reason)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `RejectArticleCommand(Guid Id, string Reason) : ICommand`
- [ ] `RejectArticleCommandValidator` — `Reason` required, max `ContentConstants.MaxRejectionReasonLength`
- [ ] `RejectArticleCommandHandler` — fetches article, calls `ArticleEntity.Reject(reason)`, calls `IArticleRepository.UpdateAsync()`, commits UoW
- [ ] `RejectArticleEndpointV1` Carter module

---

### PATCH /api/v1/admin/videos/{id}/submit
### PATCH /api/v1/admin/videos/{id}/approve
### PATCH /api/v1/admin/videos/{id}/reject

> These three endpoints mirror the article workflow for videos. Submit moves the video out of
> Draft — paid videos go to `PendingPayment`, free videos to `PendingReview`. Approve confirms
> editorial sign-off (required before publication). Reject returns the video to the team with a
> recorded reason. All three must exist before any video can progress through the pipeline to the
> YouTube attachment and publish steps.

**TODOs**
- [ ] `SubmitVideoCommand(Guid Id) : ICommand` → `VideoEntity.Submit()` or `VideoEntity.MarkPendingReview()`
- [ ] `ApproveVideoCommand(Guid Id) : ICommand` → `VideoEntity.Approve()`
- [ ] `RejectVideoCommand(Guid Id, string Reason) : ICommand` → `VideoEntity.Reject(reason)`
- [ ] Handlers + `IVideoRepository.UpdateAsync(video)` + endpoints for each

---

### PATCH /api/v1/admin/videos/{id}/youtube

> Attaches the YouTube video ID to an approved video. This is a mandatory gate — a video cannot
> be published without a YouTube ID set. Separating this step from creation reflects the real
> production flow: the client pays and the video is approved editorially while it is still being
> filmed and edited. Only after the finished video is uploaded to YouTube and the ID is known can
> the admin attach it here. The embedded YouTube player on the public video page relies on this ID.
>
> In addition to attaching the YouTube ID, this handler automatically downloads the YouTube
> thumbnail and re-uploads it to the platform's media storage (Cloudinary). YouTube thumbnail
> URLs are never stored directly — linking to YouTube's CDN is unreliable long-term (video
> deleted, made private, or YouTube URL format changes). Owning the asset on our own CDN
> guarantees the thumbnail is always available regardless of the video's YouTube status.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `AttachYoutubeIdCommand(VideoId, YoutubeVideoId)` |
| **Response** | `204 No Content` |

> Required gate: a video cannot be published without a YouTube ID attached.
> Thumbnail is automatically downloaded from YouTube and re-uploaded to media storage —
> admin does not need to upload a thumbnail separately after attaching a YouTube ID.

**TODOs**
- [ ] `AttachYoutubeIdCommand(Guid VideoId, string YoutubeVideoId) : ICommand`
- [ ] `AttachYoutubeIdCommandValidator` — `YoutubeVideoId` required, max `ContentConstants.MaxYoutubeVideoIdLength`
- [ ] `AttachYoutubeIdCommandHandler`:
  - fetches video, validates it exists
  - calls `VideoEntity.AttachYoutubeId(youtubeVideoId)`
  - downloads YouTube thumbnail:
    - attempts `https://img.youtube.com/vi/{youtubeVideoId}/maxresdefault.jpg` first (1280×720)
    - falls back to `https://img.youtube.com/vi/{youtubeVideoId}/hqdefault.jpg` (480×360) if 404
    - uses `IFileService.DownloadFileAsync(url)` — already exists in Core module (same pattern used for social avatar downloads)
  - generates `storageKey = $"content/video-thumbnails/{Guid.NewGuid()}"`
  - calls `ICloudinaryService.UploadImageAsync(downloadedStream, storageKey, folder: "content/video-thumbnails")`
  - if video already had a previous `ThumbnailStorageKey` → enqueue old key for deletion after commit
  - calls `VideoEntity.UpdateThumbnail(newUrl, newStorageKey)`
  - calls `IVideoRepository.UpdateAsync()`, commits `IContentUnitOfWork`
  - **after commit**: if old `ThumbnailStorageKey` existed → calls `ICloudinaryService.DeleteImageAsync(oldStorageKey)`
- [ ] `VideoEntity.AttachYoutubeId(string youtubeVideoId)` — sets `YoutubeVideoId` only, thumbnail handled separately by handler
- [ ] `IVideoRepository.UpdateAsync(VideoEntity video, CancellationToken ct)`
- [ ] `AttachYoutubeIdEndpointV1` Carter module

---

### PATCH /api/v1/admin/videos/{id}/publish

> Makes the video publicly visible on the platform. The video must be in `Approved` status and
> must have a YouTube ID attached — the entity's `Publish()` method enforces the YouTube gate by
> throwing if `YoutubeVideoId` is null. Once published, the embedded player on the public video
> page becomes active and the video appears on the video feed for all visitors.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `PublishVideoCommand(VideoId)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `PublishVideoCommand(Guid VideoId) : ICommand`
- [ ] `PublishVideoCommandHandler` — fetches video (must be `Approved`), calls `VideoEntity.Publish()` which internally throws if `YoutubeVideoId` is null, calls `IVideoRepository.UpdateAsync()`, commits UoW
- [ ] `PublishVideoEndpointV1` Carter module

---

## 🟡 IMPORTANT — Admin listings and content editing

---

### GET /api/v1/admin/articles

> Returns the paginated list of all articles across every status so the admin team can manage
> the full editorial queue. Editors use this to find articles awaiting approval, production staff
> use it to check what is in Draft, and the admin uses it to audit the publishing pipeline.
> Filtering by status and category allows each team member to see only the slice of content
> relevant to their role.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllArticlesAdminQuery(Page, PageSize, Status?, CategoryId?)` |
| **Response** | `200` + `PagedResponse<ArticleSummaryDto>` |

**TODOs**
- [ ] `GetAllArticlesAdminQuery(int Page, int PageSize, EnumContentStatus? Status, Guid? CategoryId) : IQuery<PagedResponse<ArticleSummaryDto>>`
- [ ] `GetAllArticlesAdminQueryHandler` — calls `IArticleRepository.GetAllAsync(page, pageSize, status, categoryId)`
- [ ] `ArticleRepository.GetAllAsync(page, pageSize, status, categoryId)` — applies filters conditionally, ordered by `CreatedAt DESC`
- [ ] `GetAllArticlesAdminEndpointV1` Carter module

---

### GET /api/v1/admin/articles/{id}

> Returns the complete article including its full body text, cover image, author name, tags, and
> SEO metadata. Used by the editorial team when reviewing an article for approval or rejection —
> they need to read the entire piece before making a decision. Also used when a client requests
> a preview of their commissioned article before it goes to review.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetArticleByIdQuery(Id)` |
| **Response** | `200` + `ArticleDetailDto` |

**TODOs**
- [ ] `GetArticleByIdQuery(Guid Id) : IQuery<ArticleDetailDto>`
- [ ] `GetArticleByIdQueryHandler` — calls `IArticleRepository.GetByIdAsync(id)`, throws `ResourceNotFoundException` if null
- [ ] `ArticleRepository.GetByIdAsync(id)` — includes `Category`, `Tags` with `Tag` navigation
- [ ] `GetArticleByIdEndpointV1` Carter module

---

### PUT /api/v1/admin/articles/{id}

> Updates all editable fields of an article in a single call. This endpoint serves two purposes:
> (1) **step 2 of the creation flow** — after the admin clicks "Save Draft" (POST), this PUT call
> fills in the headline, body, and cover image before clicking "Submit"; (2) **any subsequent edit**
> while the article is still in a mutable status — for example correcting a typo in the title of a
> rejected article or updating the category of a pending article.
>
> Covers metadata (`title`, `slug`, `categoryId`), content (`headline`, `body`, `coverImageUrl`),
> commerce fields (`customerId`, `orderItemId`), promotion flags (`socialBoost`),
> and SEO metadata (`metaTitle`, `metaDescription`).
>
> On every call the handler diffs the new body's Cloudinary URLs against `article_images` to detect
> removed images and delete them from Cloudinary after commit.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateArticleCommand(Id, CategoryId, Title, Slug, Headline, Body, CoverImageUrl?, CustomerId?, OrderItemId?, SocialBoost, MetaTitle?, MetaDescription?)` |
| **Response** | `200` + `ArticleDetailDto` |

> Allowed when status is `Draft`, `PendingPayment`, `PendingReview`, or `Rejected`.
> Locked once the article reaches `Approved`, `Published`, or `Archived`.
> `customerId` and `orderItemId` must be provided together or both omitted.
> Slug must be unique — returns 409 Conflict if taken by a different article.
> Min 100 chars on `headline` enforced here (not on `POST` which creates the empty draft shell).

**TODOs**
- [ ] `UpdateArticleCommand(string Id, Guid CategoryId, string Title, string Slug, string Headline, string Body, string? CoverImageUrl, Guid? CustomerId, Guid? OrderItemId, bool SocialBoost, string? MetaTitle, string? MetaDescription) : ICommand<UpdateArticleResult>`
- [ ] `UpdateArticleValidator` — `CategoryId` valid guid, `Title` valid, `Slug` valid, `Headline` min 100 / max 300, `Body` not empty, `CustomerId`/`OrderItemId` pair rules, `MetaTitle` min 10 / max 70, `MetaDescription` min 50 / max 160
- [ ] `UpdateArticleHandler`:
  - fetches article, throws `InvalidStatusTransition` if `Approved`, `Published`, or `Archived`
  - verifies category exists via `ICategoryRepository.GetByIdOrThrowAsync`
  - if slug changed: checks uniqueness via `IArticleRepository.GetBySlugAsync`, throws `SlugAlreadyExists` if taken by another article
  - loads current `article_images` for this article (set B)
  - extracts all Cloudinary URLs from new `Body` HTML (set A)
  - computes `set B − set A` = removed body images; adds old cover to remove list if cover changed
  - calls `ArticleEntity.Update(categoryId, title, slug, headline, body, coverImageUrl, customerId, orderItemId, socialBoost, metaTitle, metaDescription)`
  - calls `IArticleRepository.Update()`, commits `IContentUnitOfWork`
  - **after commit**: calls `ICloudinaryService.DeleteImagesAsync(storageKeys)`, removes rows from `article_images`, commits again
- [ ] `ArticleEntity.Update(...)` — single method covering all editable fields
- [ ] `IArticleRepository.GetImagesByArticleIdAsync(Guid articleId, CancellationToken ct)`
- [ ] `IArticleRepository.RemoveImages(IEnumerable<ArticleImageEntity> images)`
- [ ] `IArticleRepository.Update(ArticleEntity article)`
- [ ] `UpdateArticleEndpointV1` Carter module

---

### GET /api/v1/admin/videos

> Returns all videos across every status so the production team can track what is in pre-production
> (Draft), what is awaiting editorial review (PendingReview), what is approved and waiting for a
> YouTube link (Approved), and what is live (Published). Filtering by status and category allows
> the team to focus on their immediate pipeline without scrolling through the entire catalogue.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllVideosAdminQuery(Page, PageSize, Status?, CategoryId?)` |
| **Response** | `200` + `PagedResponse<VideoSummaryDto>` |

**TODOs**
- [ ] `GetAllVideosAdminQuery(int Page, int PageSize, EnumContentStatus? Status, Guid? CategoryId) : IQuery<PagedResponse<VideoSummaryDto>>`
- [ ] `GetAllVideosAdminQueryHandler` — calls `IVideoRepository.GetAllAsync(page, pageSize, status, categoryId)`
- [ ] `VideoRepository.GetAllAsync(page, pageSize, status, categoryId)`
- [ ] `GetAllVideosAdminEndpointV1` Carter module

---

### GET /api/v1/admin/videos/{id}

> Returns the full video record including shoot schedule, YouTube ID, thumbnail, tags, and all
> status metadata. Used by the editorial team when reviewing a video for approval, by the
> production team when verifying shooting details, and by the admin when a client asks for a
> status update on their commissioned video.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetVideoByIdQuery(Id)` |
| **Response** | `200` + `VideoDetailDto` |

**TODOs**
- [ ] `GetVideoByIdQuery(Guid Id) : IQuery<VideoDetailDto>`
- [ ] `GetVideoByIdQueryHandler` — calls `IVideoRepository.GetByIdAsync(id)`, throws `ResourceNotFoundException` if null
- [ ] `VideoRepository.GetByIdAsync(id)` — includes `Category`, `Tags` with `Tag` navigation
- [ ] `GetVideoByIdEndpointV1` Carter module

---

### PUT /api/v1/admin/videos/{id}

> Updates the video's title, slug, or description. Only permitted when status is `Draft` or
> `Rejected`. Used to correct mistakes in the initial record or revise content before
> resubmitting after a rejection. Thumbnail is not updated here — it is set automatically
> when a YouTube ID is attached (`PATCH /{id}/youtube`) or via a dedicated thumbnail
> upload endpoint (`POST /{id}/thumbnail`) for custom overrides.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateVideoCommand(Id, Title, Slug, Description?)` |
| **Response** | `200` + `VideoDetailDto` |

> Only allowed when status is `Draft` or `Rejected`.

**TODOs**
- [ ] `UpdateVideoCommand(Guid Id, string Title, string Slug, string? Description) : ICommand<VideoDetailDto>`
- [ ] `UpdateVideoCommandValidator` — title max 200, slug max 220
- [ ] `UpdateVideoCommandHandler` — fetches video, validates status is `Draft` or `Rejected`, checks new slug not taken by another video, updates fields inline, calls `IVideoRepository.UpdateAsync()`, commits UoW
- [ ] `UpdateVideoEndpointV1` Carter module

---

### POST /api/v1/admin/videos/{id}/thumbnail

> Uploads a custom thumbnail for a video, overriding the one auto-generated from YouTube.
> When a YouTube ID is attached the thumbnail is automatically downloaded from YouTube and
> re-uploaded to media storage. This endpoint allows the admin to replace that auto-generated
> thumbnail with a custom image (e.g. a branded still from the shoot).
> On replacement the old `thumbnail_storage_key` is deleted from media storage after commit.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `FileUpload` |
| **Command** | `UploadVideoThumbnailCommand(VideoId, File)` |
| **Response** | `200` + `{ url, storageKey }` |

**TODOs**
- [ ] `UploadVideoThumbnailCommand(Guid VideoId, IFormFile File) : ICommand<UploadVideoThumbnailResult>`
- [ ] `UploadVideoThumbnailCommandValidator` — file required, valid image extension + MIME type, max size
- [ ] `UploadVideoThumbnailCommandHandler`:
  - verifies video exists
  - captures old `ThumbnailStorageKey` (if any) before update
  - generates `storageKey = $"content/video-thumbnails/{Guid.NewGuid()}"`
  - calls `ICloudinaryService.UploadImageAsync(file, storageKey, folder: "content/video-thumbnails")`
  - calls `VideoEntity.UpdateThumbnail(newUrl, newStorageKey)`
  - calls `IVideoRepository.UpdateAsync()`, commits UoW
  - **after commit**: if old `ThumbnailStorageKey` existed → `ICloudinaryService.DeleteImageAsync(oldStorageKey)`
- [ ] `UploadVideoThumbnailEndpointV1` Carter module — `multipart/form-data`

---

### PATCH /api/v1/admin/videos/{id}/shoot

> Records or updates the scheduled shooting date for a video. This is particularly important for
> pre-booked shows like 116 Le Focus or FlexBeat where the client pays before the shoot occurs.
> The scheduled date gives the production team a calendar reference so they can organize their
> filming schedule. The date must be in the future at the time of scheduling.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ScheduleShootCommand(VideoId, ShootingScheduledAt)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `ScheduleShootCommand(Guid VideoId, DateTimeOffset ShootingScheduledAt) : ICommand`
- [ ] `ScheduleShootCommandValidator` — `ShootingScheduledAt` must be in the future
- [ ] `ScheduleShootCommandHandler` — fetches video, calls `VideoEntity.ScheduleShoot(scheduledAt)`, calls `IVideoRepository.UpdateAsync()`, commits UoW
- [ ] `ScheduleShootEndpointV1` Carter module

---

### PUT /api/v1/admin/articles/{id}/tags
### PUT /api/v1/admin/videos/{id}/tags

> Replaces the complete tag set on an article or video. Tags are the primary discovery mechanism
> for public users — they let site visitors find all content about a specific artist (e.g. "Fally
> Ipupa"), genre (e.g. "Afrobeats"), or topic (e.g. "Kinshasa") without relying on categories.
> They also drive SEO through tag-based URLs and the public tag cloud. Full replacement is used
> instead of per-tag add/remove so the admin can reorder and refresh the full set in one call.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateArticleTagsCommand(ArticleId, TagIds[])` / `UpdateVideoTagsCommand(VideoId, TagIds[])` |
| **Response** | `204 No Content` |

> Full replacement — removes all existing tags and inserts the new set.

**TODOs**
- [ ] `UpdateArticleTagsCommand(Guid ArticleId, IReadOnlyList<Guid> TagIds) : ICommand`
- [ ] `UpdateArticleTagsCommandHandler` — verifies article exists, verifies all tag IDs exist (`ILookupRepository`), removes current tags (`IArticleRepository.RemoveTagAsync()` for each), adds new tags (`IArticleRepository.AddTagAsync()` for each), commits UoW
- [ ] `ArticleRepository.AddTagAsync(tag)` and `ArticleRepository.RemoveTagAsync(articleId, tagId)`
- [ ] `UpdateArticleTagsEndpointV1` and `UpdateVideoTagsEndpointV1` Carter modules

---

## 🟢 MODERATE — Public feeds, SEO, archive, short videos, and lyrics

---

### GET /api/v1/public/articles

> Returns all published articles to anonymous visitors. This is the main article feed powering
> the public website — the article listing page, category sub-pages, and tag filter pages all
> use this endpoint. Supports filtering by category ID and tag slug so a single endpoint serves
> all public listing contexts. Results are ordered by most recently published so fresh content
> always surfaces at the top.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetPublishedArticlesQuery(Page, PageSize, CategoryId?, TagSlug?)` |
| **Response** | `200` + `PagedResponse<ArticleSummaryDto>` |

**TODOs**
- [ ] `GetPublishedArticlesQuery(int Page, int PageSize, Guid? CategoryId, string? TagSlug) : IQuery<PagedResponse<ArticleSummaryDto>>`
- [ ] `GetPublishedArticlesQueryHandler` — calls `IArticleRepository.GetAllAsync()` with `Status = Published`, applies optional filters
- [ ] `GetPublishedArticlesEndpointV1` Carter module (`.AllowAnonymous()`)

---

### GET /api/v1/public/articles/{slug}

> Returns a single published article by its SEO-friendly URL slug. This is the article detail
> page — the core reading experience on the platform. Slug-based URLs are human-readable and
> shareable on social media (e.g. `/articles/fally-ipupa-album-review-2025`). The endpoint
> returns 404 if the article does not exist or is not published, so draft or rejected articles
> are never accidentally exposed to the public.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetArticleBySlugQuery(Slug)` |
| **Response** | `200` + `ArticleDetailDto` |

**TODOs**
- [ ] `GetArticleBySlugQuery(string Slug) : IQuery<ArticleDetailDto>`
- [ ] `GetArticleBySlugQueryHandler` — calls `IArticleRepository.GetBySlugAsync(slug)`, throws `ResourceNotFoundException` if null or not published
- [ ] `GetArticleBySlugEndpointV1` Carter module (`.AllowAnonymous()`)

---

### GET /api/v1/public/articles/promoted

> Returns articles that are currently promoted (À la Une) — their `PromotedUntil`
> date is in the future and their status is `Published`. This list powers the homepage promoted
> article grid (the 10-slot layout) and the "À la Une" top story slot. Only paid clients who
> purchased a promotion level appear here. When a promotion expires, the article
> automatically falls out of this list without any manual intervention.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetPromotedArticlesQuery` |
| **Response** | `200` + `IReadOnlyList<ArticleSummaryDto>` |

**TODOs**
- [ ] `GetPromotedArticlesQuery : IQuery<IReadOnlyList<ArticleSummaryDto>>`
- [ ] `GetPromotedArticlesQueryHandler` — calls `IArticleRepository.GetPromotedAsync()`
- [ ] `ArticleRepository.GetPromotedAsync()` — `WHERE is_promoted = true AND promoted_until > now() AND status = 'published'`
- [ ] `GetPromotedArticlesEndpointV1` Carter module (`.AllowAnonymous()`)

---

### GET /api/v1/public/videos
### GET /api/v1/public/videos/{slug}
### GET /api/v1/public/videos/promoted

> These three endpoints mirror the article public endpoints for videos. The videos list feeds the
> category tab navigation on the video feed page (116 Music Video, 116 Interview, FlexBeat, etc.).
> The slug endpoint powers the individual video page with the embedded YouTube player, star ratings,
> and related videos. The promoted list powers the homepage video spotlight showing currently
> promoted videos whose `PromotedUntil` date is still active.

**TODOs**
- [ ] `GetPublishedVideosQuery`, `GetVideoBySlugQuery`, `GetPromotedVideosQuery`
- [ ] Respective handlers calling `IVideoRepository.GetAllAsync()`, `GetBySlugAsync()`, `GetPromotedAsync()`
- [ ] `VideoRepository.GetPromotedAsync()` — `WHERE is_promoted = true AND promoted_until > now() AND status = 'published'`
- [ ] Three Carter endpoints (`.AllowAnonymous()`)

---

### PATCH /api/v1/admin/articles/{id}/seo
### PATCH /api/v1/admin/videos/{id}/seo

> Sets a custom meta title (max 70 chars) and meta description (max 160 chars) for search engine
> indexing. Without this, Google falls back to the display title and may truncate the description
> arbitrarily. A well-crafted meta title and description improve the click-through rate from search
> results, which drives organic traffic — critical for lyrics pages and popular artist profiles
> that are likely to rank on Google searches.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateArticleSeoCommand(Id, MetaTitle?, MetaDescription?)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `UpdateArticleSeoCommand(Guid Id, string? MetaTitle, string? MetaDescription) : ICommand`
- [ ] `UpdateArticleSeoCommandValidator` — `MetaTitle` max `ContentConstants.MaxMetaTitleLength`, `MetaDescription` max `ContentConstants.MaxMetaDescriptionLength`
- [ ] `UpdateArticleSeoCommandHandler` — fetches article, calls `ArticleEntity.UpdateSeo(metaTitle, metaDescription)`, calls `IArticleRepository.UpdateAsync()`, commits UoW
- [ ] `UpdateVideoSeoCommand(Guid Id, string? MetaTitle, string? MetaDescription) : ICommand` → calls `VideoEntity.UpdateSeo()`
- [ ] Endpoints for each

---

### PATCH /api/v1/admin/articles/{id}/archive
### PATCH /api/v1/admin/videos/{id}/archive

> Soft-removes the content from all public feeds without permanently deleting it. Useful for
> taking down outdated articles (e.g. an event preview after the event has passed) or pulling a
> video that is being reuploaded to YouTube with corrections. Archived content remains accessible
> in the admin dashboard for reference and can be restored to Published if needed by going through
> the approve workflow again.
> **Cloudinary images are NOT deleted on archive** — the article is reversible and restoring
> it must produce intact images. Deleting images on archive would break the article on restore.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ArchiveArticleCommand(Id)` / `ArchiveVideoCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `ArchiveArticleCommand(Guid Id) : ICommand` → calls `ArticleEntity.Archive()` — no Cloudinary call
- [ ] `ArchiveVideoCommand(Guid Id) : ICommand` → calls `VideoEntity.Archive()` — no Cloudinary call
- [ ] Handlers + endpoints for each

---

### DELETE /api/v1/admin/articles/{id}

> Permanently and irreversibly deletes an article and all its associated Cloudinary images
> (cover + all body images). Only permitted for articles in `Draft` or `Rejected` status —
> articles that have been submitted, approved, or published must be archived instead, as they
> may be referenced by external links, bookmarks, or order records.
> **The handler must load and delete all Cloudinary images before issuing the DB delete.**
> `ON DELETE CASCADE` will wipe `article_images` rows on DB delete — if Cloudinary cleanup
> runs after, the `storage_key` values are gone and orphans can never be recovered.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `DeleteArticleCommand(Id)` |
| **Response** | `204 No Content` |

> Only allowed when status is `Draft` or `Rejected`.

**TODOs**
- [ ] `DeleteArticleCommand(Guid Id) : ICommand`
- [ ] `DeleteArticleCommandHandler`:
  - fetches article, validates status is `Draft` or `Rejected`
  - loads all `article_images` for this article (`IArticleRepository.GetImagesByArticleIdAsync()`)
  - calls `ICloudinaryService.DeleteImagesAsync(storageKeys[])` — **before** DB delete
  - hard deletes article (`IArticleRepository.RemoveAsync(article)`) — `ON DELETE CASCADE` removes `article_images` rows automatically
  - commits `IContentUnitOfWork`
- [ ] `IArticleRepository.RemoveAsync(ArticleEntity article, CancellationToken ct)`
- [ ] `DeleteArticleEndpointV1` Carter module

---

### DELETE /api/v1/admin/videos/{id}

> Permanently and irreversibly deletes a video and its thumbnail from media storage.
> Only permitted for videos in `Draft` or `Rejected` status — published or approved videos
> must be archived instead as they may be referenced by order records or external links.
> The handler must delete the thumbnail from media storage **before** the DB delete.
> Unlike articles there is no `video_images` table — the single `thumbnail_storage_key`
> column is read directly from the entity before deletion.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `DeleteVideoCommand(Id)` |
| **Response** | `204 No Content` |

> Only allowed when status is `Draft` or `Rejected`.

**TODOs**
- [ ] `DeleteVideoCommand(Guid Id) : ICommand`
- [ ] `DeleteVideoCommandHandler`:
  - fetches video, validates status is `Draft` or `Rejected`
  - if `ThumbnailStorageKey` is not null → calls `ICloudinaryService.DeleteImageAsync(thumbnailStorageKey)` **before** DB delete
  - hard deletes video (`IVideoRepository.RemoveAsync(video)`)
  - commits `IContentUnitOfWork`
- [ ] `IVideoRepository.RemoveAsync(VideoEntity video, CancellationToken ct)`
- [ ] `DeleteVideoEndpointV1` Carter module

---

### DELETE /api/v1/admin/shorts/{id}

> Permanently deletes a short video, its video file, and its thumbnail from media storage.
> Both `video_storage_key` and `thumbnail_storage_key` must be deleted from media storage
> **before** the DB delete. Deactivation (`PATCH /{id}/deactivate`) should be used instead
> when the intent is to temporarily hide the clip — hard delete is irreversible.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `DeleteShortVideoCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `DeleteShortVideoCommand(Guid Id) : ICommand`
- [ ] `DeleteShortVideoCommandHandler`:
  - fetches short video
  - collects storage keys: `videoStorageKey` (always set) + `thumbnailStorageKey` (if not null)
  - calls `ICloudinaryService.DeleteImagesAsync(storageKeys[])` **before** DB delete
  - hard deletes short video (`IShortVideoRepository.RemoveAsync(shortVideo)`)
  - commits `IContentUnitOfWork`
- [ ] `IShortVideoRepository.RemoveAsync(ShortVideoEntity shortVideo, CancellationToken ct)`
- [ ] `DeleteShortVideoEndpointV1` Carter module

---

### POST /api/v1/admin/shorts

> Creates a short vertical video clip (Reels-style) for the homepage discovery feed. Short videos
> are either standalone clips (scandals, gossip, buzz — no full video link) or teasers linked to a
> parent video (a dramatic moment from a Le Focus episode, the hook of a Music Video). The
> `has_full_video` flag on the entity controls whether the "View Full Video" CTA button appears on
> the clip card, driving traffic from the short feed into the full video catalogue.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `FileUpload` |
| **Command** | `CreateShortVideoCommand(Title, VideoUrl, VideoId?)` |
| **Response** | `201` + `ShortVideoDto(Id, Title, VideoUrl, VideoId, HasFullVideo, IsActive)` |

**TODOs**
- [ ] `ShortVideoEntity.CreateStandalone(id, title, videoUrl, videoStorageKey)` — for gossip/scandal clips
- [ ] `ShortVideoEntity.CreateTeaser(id, title, videoUrl, videoStorageKey, videoId)` — for show teasers linked to a parent video
- [ ] `CreateShortVideoCommand(string Title, IFormFile VideoFile, Guid? VideoId) : ICommand<ShortVideoDto>`
- [ ] `CreateShortVideoCommandValidator` — title max 200, video file required, valid video MIME type, if `VideoId` set verify it exists
- [ ] `CreateShortVideoCommandHandler`:
  - optionally verifies parent `VideoId` exists and is published
  - generates `videoStorageKey = $"content/short-videos/{Guid.NewGuid()}"`
  - calls `ICloudinaryService.UploadVideoAsync(file, videoStorageKey, folder: "content/short-videos")` — note: video upload, not image
  - calls correct factory with `videoUrl` + `videoStorageKey`
  - calls `IShortVideoRepository.AddAsync()`, commits UoW
- [ ] `ShortVideoRepository.AddAsync(shortVideo)`
- [ ] `CreateShortVideoEndpointV1` Carter module — `multipart/form-data`, `FileUpload` rate limit

---

### POST /api/v1/admin/shorts/{id}/thumbnail

> Uploads an optional thumbnail image for a short video. Used as a preview frame in the feed
> before the clip autoplays. On replacement the old `thumbnail_storage_key` is deleted from
> media storage after commit. Deactivating a short video does NOT delete its thumbnail —
> deactivation is reversible.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `FileUpload` |
| **Command** | `UploadShortVideoThumbnailCommand(ShortVideoId, File)` |
| **Response** | `200` + `{ url, storageKey }` |

**TODOs**
- [ ] `UploadShortVideoThumbnailCommand(Guid ShortVideoId, IFormFile File) : ICommand<UploadShortVideoThumbnailResult>`
- [ ] `UploadShortVideoThumbnailCommandValidator` — file required, valid image extension + MIME type, max size
- [ ] `UploadShortVideoThumbnailCommandHandler`:
  - verifies short video exists
  - captures old `ThumbnailStorageKey` (if any)
  - generates `storageKey = $"content/short-video-thumbnails/{Guid.NewGuid()}"`
  - calls `ICloudinaryService.UploadImageAsync(file, storageKey, folder: "content/short-video-thumbnails")`
  - calls `ShortVideoEntity.UpdateThumbnail(newUrl, newStorageKey)`
  - calls `IShortVideoRepository.UpdateAsync()`, commits UoW
  - **after commit**: if old `ThumbnailStorageKey` existed → `ICloudinaryService.DeleteImageAsync(oldStorageKey)`
- [ ] `UploadShortVideoThumbnailEndpointV1` Carter module — `multipart/form-data`

---

### GET /api/v1/public/shorts

> Returns the paginated list of active short videos for the public discovery feed. This is the
> endpoint that powers the homepage's vertical scroll experience — the frontend requests 10 clips
> at a time and preloads them to avoid buffering interruptions. Short videos are served in
> full-screen immersive mode ("the universe") and are the primary hook that draws casual visitors
> deeper into the platform's full article and video catalogue.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetActiveShortVideosQuery(Page, PageSize)` |
| **Response** | `200` + `PagedResponse<ShortVideoDto>` |

**TODOs**
- [ ] `GetActiveShortVideosQuery(int Page, int PageSize) : IQuery<PagedResponse<ShortVideoDto>>`
- [ ] `GetActiveShortVideosQueryHandler` — calls `IShortVideoRepository.GetAllAsync(page, pageSize, isActive: true)`
- [ ] `ShortVideoRepository.GetAllAsync(page, pageSize, isActive)` — includes `ParentVideo` navigation for `HasFullVideo`
- [ ] `GetActiveShortVideosEndpointV1` Carter module (`.AllowAnonymous()`)

---

### PATCH /api/v1/admin/shorts/{id}/activate
### PATCH /api/v1/admin/shorts/{id}/deactivate

> Controls whether a short video appears in the public discovery feed. Deactivating removes it
> from the feed immediately without deleting the record — useful for temporarily pulling a clip
> that is being revised, reuploaded, or is no longer appropriate to surface. Activating restores
> it to the feed without any re-upload required.

**TODOs**
- [ ] `ActivateShortVideoCommand(Guid Id) : ICommand` → calls `ShortVideoEntity.Activate()`
- [ ] `DeactivateShortVideoCommand(Guid Id) : ICommand` → calls `ShortVideoEntity.Deactivate()`
- [ ] `ShortVideoRepository.UpdateAsync(shortVideo)` — add to `IShortVideoRepository`
- [ ] `ActivateShortVideoEndpointV1` and `DeactivateShortVideoEndpointV1`

---

### POST /api/v1/admin/lyrics

> Creates a lyrics record that can be linked to a video (e.g. "116 Behind the Lyrics" or "116
> Lyric Video"), linked to an article (e.g. a Lyrics Page category article), or left standalone
> as an independent SEO-optimised page. Lyrics pages are organic traffic goldmines — Google indexes
> them for song-name searches like "Fally Ipupa Eloko Oyo lyrics", driving new visitors to the
> platform without ad spend. Exactly one of `VideoId` / `ArticleId` / neither can be set.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateLyricsCommand(SongTitle, ArtistName, LyricsText, Language, VideoId?, ArticleId?)` |
| **Response** | `201` + `LyricsDto(Id, SongTitle, ArtistName, Language, VideoId, ArticleId)` |

> Exactly one of `VideoId` / `ArticleId` / neither can be set. Both cannot be set simultaneously.

**TODOs**
- [ ] `LyricsEntity.CreateForVideo(id, videoId, songTitle, artistName, lyricsText, language)`
- [ ] `LyricsEntity.CreateForArticle(id, articleId, songTitle, artistName, lyricsText, language)`
- [ ] `LyricsEntity.CreateStandalone(id, songTitle, artistName, lyricsText, language)`
- [ ] `CreateLyricsCommand(string SongTitle, string ArtistName, string LyricsText, string Language, Guid? VideoId, Guid? ArticleId) : ICommand<LyricsDto>`
- [ ] `CreateLyricsCommandValidator` — song title max 200, artist max 100, language max 5, `VideoId` and `ArticleId` cannot both be set
- [ ] `CreateLyricsCommandHandler` — verifies parent exists if provided, calls correct factory, if `VideoId` provided also calls `VideoEntity.MarkHasLyrics()` and updates video, calls `ILyricsRepository.AddAsync()`, commits UoW
- [ ] `LyricsRepository.AddAsync(lyrics)`
- [ ] `CreateLyricsEndpointV1` Carter module

---

### GET /api/v1/public/lyrics/{id}

> Returns the full lyrics record including song title, artist name, full lyrics text, and all SEO
> metadata (meta title, meta description, keywords, and structured data JSON). The structured data
> field follows the schema.org `MusicRecording` format, enabling Google to display a rich snippet
> directly in search results — showing the song title and a preview of the lyrics without the user
> needing to click through. This is the highest-traffic potential endpoint on the platform for
> organic SEO.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetLyricsByIdQuery(Id)` |
| **Response** | `200` + `LyricsDetailDto(Id, SongTitle, ArtistName, LyricsText, Language, MetaTitle, MetaDescription, MetaKeywords, StructuredData, VideoId, ArticleId)` |

**TODOs**
- [ ] `GetLyricsByIdQuery(Guid Id) : IQuery<LyricsDetailDto>`
- [ ] `GetLyricsByIdQueryHandler` — calls `ILyricsRepository.GetByIdAsync(id)`, throws `ResourceNotFoundException` if null
- [ ] `LyricsRepository.GetByIdAsync(id)`
- [ ] `GetLyricsByIdEndpointV1` Carter module (`.AllowAnonymous()`)

---

## ⚪ TRIVIAL — Lyrics management and admin-only content views

---

### PUT /api/v1/admin/lyrics/{id}

> Updates the lyrics text when corrections are needed — typos fixed, missing lines added, or
> alternate versions substituted. Keeping lyrics accurate is important both for user trust and for
> the SEO value of the lyrics page: search engines crawl lyrics text and an accurate transcription
> ranks better than a partial or incorrect one.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateLyricsCommand(Id, LyricsText)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `UpdateLyricsCommand(Guid Id, string LyricsText) : ICommand`
- [ ] `UpdateLyricsCommandValidator` — `LyricsText` not empty
- [ ] `UpdateLyricsCommandHandler` — fetches lyrics, calls `LyricsEntity.UpdateLyrics(lyricsText)`, calls `ILyricsRepository.UpdateAsync()`, commits UoW
- [ ] `LyricsRepository.UpdateAsync(lyrics)`
- [ ] `UpdateLyricsEndpointV1` Carter module

---

### PATCH /api/v1/admin/lyrics/{id}/seo

> Updates all SEO fields for a lyrics page: meta title, meta description, meta keywords, and the
> structured data JSON (schema.org `MusicRecording`). These fields directly control how Google
> displays the lyrics page in search results. A correctly structured `MusicRecording` JSON-LD
> block enables rich snippet rendering — showing the artist, song name, and lyrics preview
> directly in the SERP without a click, which improves impressions even when the user does not
> visit the page.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateLyricsSeoCommand(Id, MetaTitle?, MetaDescription?, MetaKeywords?, StructuredData?)` |
| **Response** | `204 No Content` |

**TODOs**
- [ ] `UpdateLyricsSeoCommand(Guid Id, string? MetaTitle, string? MetaDescription, string? MetaKeywords, string? StructuredData) : ICommand`
- [ ] `UpdateLyricsSeoCommandValidator` — max lengths for each field, `StructuredData` must be valid JSON if provided
- [ ] `UpdateLyricsSeoCommandHandler` — fetches lyrics, calls `LyricsEntity.UpdateSeo(metaTitle, metaDescription, metaKeywords, structuredData)`, calls `ILyricsRepository.UpdateAsync()`, commits UoW
- [ ] `UpdateLyricsSeoEndpointV1` Carter module

---

### GET /api/v1/public/shorts/{id}

> Returns a single active short video by its ID. Used when a user shares a direct link to a
> specific short video clip, or when the frontend needs to deep-link into a particular clip in
> the immersive full-screen feed. Returns 404 if the short video does not exist or is inactive,
> preventing access to deactivated clips.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetShortVideoByIdQuery(Id)` |
| **Response** | `200` + `ShortVideoDto` |

**TODOs**
- [ ] `GetShortVideoByIdQuery(Guid Id) : IQuery<ShortVideoDto>`
- [ ] `GetShortVideoByIdQueryHandler` — calls `IShortVideoRepository.GetByIdAsync(id)`, throws if null or inactive
- [ ] `ShortVideoRepository.GetByIdAsync(id)`
- [ ] `GetShortVideoByIdEndpointV1` Carter module (`.AllowAnonymous()`)