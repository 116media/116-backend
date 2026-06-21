# Standalone File Upload Pattern

How content entities that own uploaded files (cover images, posters, video
files, thumbnails) are created and updated in the Content module.

---

## The rule

**Entity create/update endpoints accept JSON only. Every file is uploaded
through its own dedicated single-file endpoint, as a separate request.**

A create/update request never carries a file. The file field still appears in
the admin form, but the form submits it via a separate call to the asset
endpoint — exactly how an article cover image works (`UploadArticleImage` is
decoupled from `CreateArticle`/`UpdateArticle`).

```
POST   /api/v1/admin/{resource}                 # JSON  → create entity
PUT    /api/v1/admin/{resource}/{id}            # JSON  → update entity
PUT|POST /api/v1/admin/{resource}/{id}/{asset}  # multipart → upload one file
```

---

## Why

A multipart body that mixes scalar fields with a file breaks both API client
generators the project relies on:

- **Dashboard** (`swagger-typescript-api`) nests a `[FromForm]` record body as
  `{ request: {...} }`, producing broken `FormData` where the file is lost.
- **Mobile** (`swagger_dart_code_generator` / chopper) only handles a single
  file in the multipart body plus scalar query params — it cannot model
  multiple form fields alongside a file.

Keeping create/update as pure JSON and isolating each file in its own
single-file endpoint sidesteps both. Scalars travel in a typed JSON body; the
file endpoint has exactly one `IFormFile` parameter, which both generators
handle cleanly.

This is enforced by convention, not tooling — the codebase has **zero**
`[FromForm]` usage. New file-bearing entities must follow the same shape.

---

## Endpoint shape

### JSON create/update

```csharp
.MapPut("/{id}", async (string id, AdminUpdateCategoryRequest request, IDispatcher dispatcher) => { ... })
    .RequireRateLimiting(RateLimitPolicies.ContentBrowsing)   // NOT FileUpload
    // no .DisableAntiforgery()
```

### Dedicated single-file endpoint

```csharp
.MapPost("/{id}/video", async (string id, IFormFile file, IDispatcher dispatcher) => { ... })
    .RequireRateLimiting(RateLimitPolicies.FileUpload)
    .DisableAntiforgery()
```

`FileUpload` rate limiting and `DisableAntiforgery()` belong **only** on the
file endpoint. A JSON endpoint that still carries either is a leftover from an
older multipart version and should be corrected.

The handler resolves the current file id from the entity and calls the
matching `IFileRepository` replace method, which soft-deletes the previous
`FileEntity` (and removes it from cloud storage) before uploading the new one:

| Asset type | Repository method        |
| ---------- | ------------------------ |
| Image      | `ReplaceImageFileAsync`  |
| Video      | `ReplaceVideoFileAsync`  |

---

## Current implementations

### Category poster

- Create/update: `AdminCreateCategoryEndpointV1`, `AdminUpdateCategoryEndpointV1`
  — JSON (`Name`, `Slug`, `Description`, `IsFree`/`IsGossip`/`IsExclusive`).
- Poster upload: `PUT /api/v1/admin/categories/{id}/poster`
  (`AdminUploadCategoryPosterEndpointV1` → `ReplaceImageFileAsync` →
  `SetPosterFileId`).

### Short video file — draft model

`ShortVideoEntity.VideoFileId` is nullable. A short video is created as an
**inactive, file-less draft**; the video file is attached afterwards.

1. `POST /shorts` (JSON) → creates a draft (`IsActive = false`,
   `VideoFileId = null`). Teaser vs. standalone is decided by `VideoId`.
2. `POST /shorts/{id}/video` (multipart) →
   `AdminUploadShortVideoFileEndpointV1` → `ReplaceVideoFileAsync` →
   `ReplaceVideoFile(fileId)`.
3. `PUT /shorts/{id}` (JSON) → updates `Title` / `VideoId` only.
4. Activation: `ShortVideoEntity.Activate()` throws `VideoFileRequired` when
   `VideoFileId is null`.

**Invariant:** an active short video always has a video file. File-less drafts
are `IsActive = false`, so they never surface in the public feed, and they
cannot be activated until a file is uploaded.

EF migration: `MakeShortVideoFileIdNullable` (`video_file_id` → nullable).

---

## Checklist for a new file-bearing entity

1. Create/update command + endpoint accept JSON. No `IFormFile`, no
   `[FromForm]`, `ContentBrowsing` rate limit, no `DisableAntiforgery()`.
2. The file column is nullable if the entity can exist before its file (draft
   model); otherwise enforce presence at the point it is required.
3. Add a dedicated `Upload{Entity}{Asset}` use case: command, validator
   (validate id + file), handler (resolve current file id → `Replace*FileAsync`
   → setter), metafield, and a `POST|PUT /{id}/{asset}` endpoint with
   `FileUpload` + `DisableAntiforgery()`.
4. Add the asset route segment to the relevant `*RouteConstants`.
5. Map the file URL in the entity's mapper, guarding the nullable file id.
6. Cover it: entity tests (nullable + any activation guard), JSON create/update
   tests, and dedicated upload-endpoint tests (unit + integration).
