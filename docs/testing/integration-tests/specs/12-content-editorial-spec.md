# Phase 10: Content Module — Editorial API Tests Spec

## Tasks

### Admin Article Commands
- [ ] `AdminCreateArticleEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_AsVisitor_ShouldReturn403
  - [ ] Post_WithInvalidData_ShouldReturn422
  - [ ] Post_WithNonExistentCategory_ShouldReturn404
- [ ] `AdminUpdateArticleEndpointTests.cs`
  - [ ] Put_AsAdmin_ExistingArticle_ShouldReturn200
  - [ ] Put_NonExistent_ShouldReturn404
- [ ] `AdminUpdateArticleSeoEndpointTests.cs`
- [ ] `AdminUpdateArticleTagsEndpointTests.cs`
  - [ ] Put_AsAdmin_ShouldReplaceAllTags
- [ ] `AdminUploadArticleImageEndpointTests.cs`
  - [ ] Post_WithValidImage_ShouldReturn200
- [ ] `AdminSubmitArticleEndpointTests.cs`
  - [ ] Patch_DraftArticle_ShouldReturn200
  - [ ] Patch_AlreadySubmitted_ShouldReturn409
- [ ] `AdminApproveArticleEndpointTests.cs`
  - [ ] Patch_SubmittedArticle_ShouldReturn200
  - [ ] Patch_DraftArticle_ShouldReturn409
- [ ] `AdminRejectArticleEndpointTests.cs`
  - [ ] Patch_SubmittedArticle_ShouldReturn200
- [ ] `AdminPublishArticleEndpointTests.cs`
  - [ ] Patch_ApprovedArticle_ShouldReturn200
  - [ ] Patch_DraftArticle_ShouldReturn409
- [ ] `AdminArchiveArticleEndpointTests.cs`
  - [ ] Patch_PublishedArticle_ShouldReturn200
- [ ] `AdminDeleteArticleEndpointTests.cs`
  - [ ] Delete_AsAdmin_ShouldReturn204
- [ ] `AdminForceUnpromoteArticleEndpointTests.cs`

### Admin Video Commands
- [ ] `AdminCreateVideoEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_AsVisitor_ShouldReturn403
  - [ ] Post_WithInvalidData_ShouldReturn422
- [ ] `AdminUpdateVideoEndpointTests.cs`
- [ ] `AdminUpdateVideoSeoEndpointTests.cs`
- [ ] `AdminUpdateVideoTagsEndpointTests.cs`
- [ ] `AdminUploadVideoThumbnailEndpointTests.cs`
- [ ] `AdminAttachYoutubeVideoUrlEndpointTests.cs`
  - [ ] Post_WithValidUrl_ShouldReturn200
  - [ ] Post_WithInvalidUrl_ShouldReturn422
- [ ] `AdminSubmitVideoEndpointTests.cs`
- [ ] `AdminApproveVideoEndpointTests.cs`
- [ ] `AdminRejectVideoEndpointTests.cs`
- [ ] `AdminPublishVideoEndpointTests.cs`
- [ ] `AdminArchiveVideoEndpointTests.cs`
- [ ] `AdminDeleteVideoEndpointTests.cs`
- [ ] `AdminScheduleShootEndpointTests.cs`
- [ ] `AdminForceUnpromoteVideoEndpointTests.cs`

### Admin Lyrics Commands
- [ ] `AdminCreateLyricsEndpointTests.cs`
  - [ ] Post_AsAdmin_ForExistingVideo_ShouldReturn201
  - [ ] Post_ForVideoWithExistingLyrics_ShouldReturn409
- [ ] `AdminUpdateLyricsEndpointTests.cs`
- [ ] `AdminDeleteLyricsEndpointTests.cs`

### Admin ShortVideo Commands
- [ ] `AdminCreateShortVideoEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
- [ ] `AdminUpdateShortVideoEndpointTests.cs`
- [ ] `AdminDeleteShortVideoEndpointTests.cs`
- [ ] `AdminActivateShortVideoEndpointTests.cs`
- [ ] `AdminDeactivateShortVideoEndpointTests.cs`
- [ ] `AdminUploadShortVideoThumbnailEndpointTests.cs`

### Public Editorial Queries
- [ ] `PublicGetLyricsByVideoIdEndpointTests.cs`
  - [ ] Get_WithExistingLyrics_ShouldReturn200
  - [ ] Get_WithNoLyrics_ShouldReturn404

## Content Status Lifecycle

```
Draft → Submitted → Approved → Published → Archived
                  → Rejected
```

Each state transition must verify:
1. Correct status code returned
2. Entity status updated in database
3. Invalid transitions return 409

## Seeding Requirements

Editorial tests need:
```
ContentType → Category → Article/Video/ShortVideo
Video → Lyrics (optional)
Article/Video → Tags (many-to-many)
```

## File Locations

```
tests/_116.Integration.Tests/Content/Api/Editorial/
├── Articles/
│   ├── AdminCreateArticleEndpointTests.cs
│   ├── AdminUpdateArticleEndpointTests.cs
│   └── ... (12 total)
├── Videos/
│   ├── AdminCreateVideoEndpointTests.cs
│   ├── AdminUpdateVideoEndpointTests.cs
│   └── ... (14 total)
├── Lyrics/
│   ├── AdminCreateLyricsEndpointTests.cs
│   ├── AdminUpdateLyricsEndpointTests.cs
│   ├── AdminDeleteLyricsEndpointTests.cs
│   └── PublicGetLyricsByVideoIdEndpointTests.cs
└── ShortVideos/
    ├── AdminCreateShortVideoEndpointTests.cs
    └── ... (6 total)
```

## Acceptance Criteria

1. Every editorial endpoint has integration tests
2. Full content lifecycle tested (Draft → Published → Archived)
3. Invalid state transitions return 409
4. Tag management verified (add, replace, remove)
5. `./scripts/run-tests-with-coverage.sh integration` passes
