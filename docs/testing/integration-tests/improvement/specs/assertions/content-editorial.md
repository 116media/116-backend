# Assertions — Content / Editorial

Articles, videos, shorts, lyrics: create/update, status workflow
(submit→approve→publish/reject/archive), SEO/tags updates, YouTube URL, schedule
shoot, thumbnail/image uploads, promotion feeds, public reads.

## Key response types
- Lists: `AdminGetAllArticlesResponse` / `...Videos` / `...Shorts` / `...Lyrics`
  (`PaginatedResult<…Dto>`); public slug reads return the full content DTO.

## After (status workflow — re-query state)
```csharp
// PATCH .../{id}/publish on an approved article
await using var db = CreateDbContext<ContentDbContext>();
var article = await db.Articles.FindAsync(seeded.Id);
article!.Status.Should().Be(EnumContentStatus.Published);
article.PublishedAt.Should().NotBeNull();
```

## After (public slug read)
```csharp
var body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
body.Article.Slug.Should().Be(seeded.Slug);
body.Article.Title.Should().Be(seeded.Title);
// non-existent slug → ShouldBeProblem(NotFound)
```

Invalid transitions (publish from Draft, already-published, delete published,
youtube-before-shoot) → `ShouldBeProblem`. Uploads (image/thumbnail) assert the
stubbed Cloudinary URL and the persisted column. Tag/SEO updates re-query and
assert the new tags/meta. Promotion-feed/promoted/active lists assert filtering.

## TODO checklist
- [ ] AdminActivateShortVideoEndpointV1Tests.cs
- [ ] AdminApproveArticleEndpointV1Tests.cs
- [ ] AdminApproveVideoEndpointV1Tests.cs
- [ ] AdminArchiveArticleEndpointV1Tests.cs
- [ ] AdminArchiveVideoEndpointV1Tests.cs
- [ ] AdminAttachYoutubeVideoUrlEndpointV1Tests.cs
- [ ] AdminCreateArticleEndpointV1Tests.cs
- [ ] AdminCreateLyricsEndpointV1Tests.cs
- [ ] AdminCreateShortVideoEndpointV1Tests.cs
- [ ] AdminCreateVideoEndpointV1Tests.cs
- [ ] AdminDeactivateShortVideoEndpointV1Tests.cs
- [ ] AdminDeleteArticleEndpointV1Tests.cs
- [ ] AdminDeleteLyricsEndpointV1Tests.cs
- [ ] AdminDeleteShortVideoEndpointV1Tests.cs
- [ ] AdminDeleteVideoEndpointV1Tests.cs
- [ ] AdminForceUnpromoteArticleEndpointV1Tests.cs
- [ ] AdminForceUnpromoteVideoEndpointV1Tests.cs
- [ ] AdminGetActiveVideosEndpointV1Tests.cs
- [ ] AdminGetAllArticlesEndpointV1Tests.cs
- [ ] AdminGetAllLyricsEndpointV1Tests.cs
- [ ] AdminGetAllShortsEndpointV1Tests.cs
- [ ] AdminGetAllVideosEndpointV1Tests.cs
- [ ] AdminGetArticleByIdEndpointV1Tests.cs
- [ ] AdminGetShortByIdEndpointV1Tests.cs
- [ ] AdminGetVideoByIdEndpointV1Tests.cs
- [ ] AdminPublishArticleEndpointV1Tests.cs
- [ ] AdminPublishVideoEndpointV1Tests.cs
- [ ] AdminRejectArticleEndpointV1Tests.cs
- [ ] AdminRejectVideoEndpointV1Tests.cs
- [ ] AdminScheduleShootEndpointV1Tests.cs
- [ ] AdminSubmitArticleEndpointV1Tests.cs
- [ ] AdminSubmitVideoEndpointV1Tests.cs
- [ ] AdminUpdateArticleEndpointV1Tests.cs
- [ ] AdminUpdateArticleSeoEndpointV1Tests.cs
- [ ] AdminUpdateArticleTagsEndpointV1Tests.cs
- [ ] AdminUpdateLyricsEndpointV1Tests.cs
- [ ] AdminUpdateLyricsSeoEndpointV1Tests.cs
- [ ] AdminUpdateShortVideoEndpointV1Tests.cs
- [ ] AdminUpdateVideoEndpointV1Tests.cs
- [ ] AdminUpdateVideoSeoEndpointV1Tests.cs
- [ ] AdminUpdateVideoTagsEndpointV1Tests.cs
- [ ] AdminUploadArticleImageEndpointV1Tests.cs
- [ ] AdminUploadShortVideoThumbnailEndpointV1Tests.cs
- [ ] AdminUploadVideoThumbnailEndpointV1Tests.cs
- [ ] PublicGetArticleBySlugEndpointV1Tests.cs
- [ ] PublicGetArticlePromotionFeedEndpointV1Tests.cs
- [ ] PublicGetLyricsBySlugEndpointV1Tests.cs
- [ ] PublicGetLyricsByVideoIdEndpointV1Tests.cs
- [ ] PublicGetPromotedArticlesEndpointV1Tests.cs
- [ ] PublicGetPromotedVideosEndpointV1Tests.cs
- [ ] PublicGetPublicShortBySlugEndpointV1Tests.cs
- [ ] PublicGetPublicShortsEndpointV1Tests.cs
- [ ] PublicGetPublishedArticlesEndpointV1Tests.cs
- [ ] PublicGetPublishedVideosEndpointV1Tests.cs
- [ ] PublicGetVideoBySlugEndpointV1Tests.cs
- [ ] PublicGetVideoPromotionFeedEndpointV1Tests.cs

## Acceptance
- Status transitions verify persisted status + timestamps; public reads assert
  DTO fields; invalid transitions use `ShouldBeProblem`.
