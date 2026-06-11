# Phase 11: Content Module — Interactions API Tests Spec

## Tasks

### Public Article Interactions
- [ ] `PublicLikeArticleEndpointTests.cs`
  - [ ] Post_AsVisitor_ShouldReturn201
  - [ ] Post_AlreadyLiked_ShouldReturn409
  - [ ] Post_WithoutAuth_ShouldReturn401
- [ ] `PublicUnlikeArticleEndpointTests.cs`
  - [ ] Delete_AsVisitor_ShouldReturn204
  - [ ] Delete_NotLiked_ShouldReturn404
- [ ] `PublicBookmarkArticleEndpointTests.cs`
  - [ ] Post_AsVisitor_ShouldReturn201
  - [ ] Post_AlreadyBookmarked_ShouldReturn409
- [ ] `PublicUnbookmarkArticleEndpointTests.cs`
  - [ ] Delete_AsVisitor_ShouldReturn204
- [ ] `PublicShareArticleEndpointTests.cs`
  - [ ] Post_AsVisitor_ShouldReturn201
- [ ] `PublicAddArticleCommentEndpointTests.cs`
  - [ ] Post_AsVisitor_WithValidContent_ShouldReturn201
  - [ ] Post_WithEmptyContent_ShouldReturn422
- [ ] `PublicEditArticleCommentEndpointTests.cs`
  - [ ] Put_AsOwner_ShouldReturn200
  - [ ] Put_AsOtherUser_ShouldReturn403
- [ ] `PublicDeleteArticleCommentEndpointTests.cs`
  - [ ] Delete_AsOwner_ShouldReturn204
  - [ ] Delete_AsOtherUser_ShouldReturn403

### Public Video Interactions
- [ ] `PublicRateVideoEndpointTests.cs`
  - [ ] Post_AsVisitor_WithValidRating_ShouldReturn201
  - [ ] Post_WithInvalidRating_ShouldReturn422
  - [ ] Post_AlreadyRated_ShouldUpdateRating
- [ ] `PublicShareVideoEndpointTests.cs`
  - [ ] Post_AsVisitor_ShouldReturn201

### Public ShortVideo Interactions
- [ ] `PublicLikeShortVideoEndpointTests.cs`
  - [ ] Post_AsVisitor_ShouldReturn201
  - [ ] Post_AlreadyLiked_ShouldReturn409
- [ ] `PublicUnlikeShortVideoEndpointTests.cs`
- [ ] `PublicBookmarkShortVideoEndpointTests.cs`
- [ ] `PublicUnbookmarkShortVideoEndpointTests.cs`
- [ ] `PublicShareShortVideoEndpointTests.cs`
- [ ] `PublicRecordShortVideoViewEndpointTests.cs`
  - [ ] Post_ShouldIncrementViewCount

### Public Playlist Commands
- [ ] `PublicCreatePlaylistEndpointTests.cs`
  - [ ] Post_AsVisitor_WithValidName_ShouldReturn201
  - [ ] Post_WithDuplicateName_ShouldReturn409
- [ ] `PublicRenamePlaylistEndpointTests.cs`
  - [ ] Put_AsOwner_ShouldReturn200
  - [ ] Put_AsOtherUser_ShouldReturn403
- [ ] `PublicDeletePlaylistEndpointTests.cs`
  - [ ] Delete_AsOwner_ShouldReturn204
- [ ] `PublicAddVideoToPlaylistEndpointTests.cs`
  - [ ] Post_AsOwner_WithExistingVideo_ShouldReturn201
  - [ ] Post_WithAlreadyAddedVideo_ShouldReturn409
- [ ] `PublicRemoveVideoFromPlaylistEndpointTests.cs`
  - [ ] Delete_AsOwner_ShouldReturn204

### Public Interaction Queries
- [ ] `PublicGetArticleCommentsEndpointTests.cs`
  - [ ] Get_Anonymous_ShouldReturn200WithPaginatedComments
- [ ] `PublicGetMyArticleBookmarksEndpointTests.cs`
  - [ ] Get_AsVisitor_ShouldReturn200WithBookmarks
  - [ ] Get_WithoutAuth_ShouldReturn401
- [ ] `PublicGetMyPlaylistsEndpointTests.cs`
  - [ ] Get_AsVisitor_ShouldReturn200WithPlaylists
- [ ] `PublicGetPlaylistByIdEndpointTests.cs`
  - [ ] Get_AsOwner_ShouldReturn200WithVideos

### Admin Interaction Commands
- [ ] `AdminDeleteArticleCommentEndpointTests.cs`
  - [ ] Delete_AsAdmin_ShouldReturn204 (any comment)

## Seeding Requirements

Interactions need published content:
```
ContentType → Category → Article/Video/ShortVideo (status: Published)
User (via auth) → Like/Bookmark/Comment/Rate
```

For ownership tests, use `AuthenticateAs(userId, "Visitor")` to control the user ID:
```csharp
var userId = Guid.NewGuid();
Client.AuthenticateAs(userId, "Visitor");
// Create comment as userId

var otherUserId = Guid.NewGuid();
Client.AuthenticateAs(otherUserId, "Visitor");
// Try to edit comment → should fail
```

## File Locations

```
tests/_116.Integration.Tests/Content/Api/Interactions/
├── Articles/
│   ├── PublicLikeArticleEndpointTests.cs
│   ├── PublicUnlikeArticleEndpointTests.cs
│   ├── PublicBookmarkArticleEndpointTests.cs
│   ├── PublicUnbookmarkArticleEndpointTests.cs
│   ├── PublicShareArticleEndpointTests.cs
│   ├── PublicAddArticleCommentEndpointTests.cs
│   ├── PublicEditArticleCommentEndpointTests.cs
│   ├── PublicDeleteArticleCommentEndpointTests.cs
│   └── AdminDeleteArticleCommentEndpointTests.cs
├── Videos/
│   ├── PublicRateVideoEndpointTests.cs
│   └── PublicShareVideoEndpointTests.cs
├── ShortVideos/
│   ├── PublicLikeShortVideoEndpointTests.cs
│   ├── PublicUnlikeShortVideoEndpointTests.cs
│   ├── PublicBookmarkShortVideoEndpointTests.cs
│   ├── PublicUnbookmarkShortVideoEndpointTests.cs
│   ├── PublicShareShortVideoEndpointTests.cs
│   └── PublicRecordShortVideoViewEndpointTests.cs
├── Playlists/
│   ├── PublicCreatePlaylistEndpointTests.cs
│   ├── PublicRenamePlaylistEndpointTests.cs
│   ├── PublicDeletePlaylistEndpointTests.cs
│   ├── PublicAddVideoToPlaylistEndpointTests.cs
│   └── PublicRemoveVideoFromPlaylistEndpointTests.cs
└── Queries/
    ├── PublicGetArticleCommentsEndpointTests.cs
    ├── PublicGetMyArticleBookmarksEndpointTests.cs
    ├── PublicGetMyPlaylistsEndpointTests.cs
    └── PublicGetPlaylistByIdEndpointTests.cs
```

## Acceptance Criteria

1. Every interaction endpoint has integration tests
2. Ownership enforcement tested (own vs other user)
3. Idempotency/conflict tested (double like → 409)
4. Toggle operations tested (like/unlike, bookmark/unbookmark)
5. `./scripts/run-tests-with-coverage.sh integration` passes
