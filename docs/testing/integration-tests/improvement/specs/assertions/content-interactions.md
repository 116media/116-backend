# Assertions — Content / Interactions

Likes, bookmarks, comments, ratings, shares, views, playlists.

## After (like — currently status-only)
```csharp
await using var db = CreateDbContext<ContentDbContext>();
(await db.ArticleLikes.AnyAsync(l => l.ArticleId == articleId && l.UserId == TestUser.VisitorId))
    .Should().BeTrue();
// liking twice → ShouldBeProblem(Conflict)
// unlike without prior like → ShouldBeProblem(BadRequest)
```

## After (comment — echo + side-effect)
```csharp
var body = await response.ReadAsAsync<PublicAddArticleCommentResponse>();
body.Comment.Content.Should().Be(request.Content);
body.Comment.Id.Should().NotBeEmpty();
await using var db = CreateDbContext<ContentDbContext>();
(await db.ArticleComments.AnyAsync(c => c.Id == body.Comment.Id)).Should().BeTrue();
// edit/delete other user's comment → ShouldBeProblem
```

Playlists: create asserts returned id + DB row; add/remove video asserts the
join row; rename asserts new name; get-by-id / my-playlists assert contents.
Rate video asserts the stored rating value. All interaction toggles assert the
DB side-effect, and duplicate/missing toggles use `ShouldBeProblem`.

## TODO checklist
- [ ] AdminDeleteArticleCommentEndpointV1Tests.cs
- [ ] PublicAddArticleCommentEndpointV1Tests.cs
- [ ] PublicAddVideoToPlaylistEndpointV1Tests.cs
- [ ] PublicBookmarkArticleEndpointV1Tests.cs
- [ ] PublicBookmarkShortVideoEndpointV1Tests.cs
- [ ] PublicCreatePlaylistEndpointV1Tests.cs
- [ ] PublicDeleteArticleCommentEndpointV1Tests.cs
- [ ] PublicDeletePlaylistEndpointV1Tests.cs
- [ ] PublicEditArticleCommentEndpointV1Tests.cs
- [ ] PublicGetArticleCommentsEndpointV1Tests.cs
- [ ] PublicGetMyArticleBookmarksEndpointV1Tests.cs
- [ ] PublicGetMyPlaylistsEndpointV1Tests.cs
- [ ] PublicGetPlaylistByIdEndpointV1Tests.cs
- [ ] PublicLikeArticleEndpointV1Tests.cs
- [ ] PublicLikeShortVideoEndpointV1Tests.cs
- [ ] PublicRateVideoEndpointV1Tests.cs
- [ ] PublicRecordShortVideoViewEndpointV1Tests.cs
- [ ] PublicRemoveVideoFromPlaylistEndpointV1Tests.cs
- [ ] PublicRenamePlaylistEndpointV1Tests.cs
- [ ] PublicShareArticleEndpointV1Tests.cs
- [ ] PublicShareShortVideoEndpointV1Tests.cs
- [ ] PublicShareVideoEndpointV1Tests.cs
- [ ] PublicUnbookmarkArticleEndpointV1Tests.cs
- [ ] PublicUnbookmarkShortVideoEndpointV1Tests.cs
- [ ] PublicUnlikeArticleEndpointV1Tests.cs
- [ ] PublicUnlikeShortVideoEndpointV1Tests.cs

## Acceptance
- Every toggle/CRUD verifies the DB join/row; duplicate/missing use `ShouldBeProblem`.
