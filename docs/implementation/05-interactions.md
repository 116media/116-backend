# Interactions Sub-Module — Implementation Plan

> Depends on: Editorial (articles, videos, short videos must be published before users interact).
> All user-facing features: likes, bookmarks, shares, comments, ratings, playlists.
> `user_id` in all interaction tables references `identity.users.id` — no FK, enforced at app level.
>
> **Scope note:** All endpoints in this file are `public` scope — they live under
> `/api/v1/public/...` and use `UseCases/Public/` folder structure.
> Auth is `UserRolePolicies.RequireVisitorOnly` for authenticated endpoints,
> `.AllowAnonymous()` for open ones.
> Admin monitoring and moderation endpoints are documented separately in `analytics/interactions.md`.

## Scope

| Entity | SQL Table | Repository |
|---|---|---|
| `ArticleLikeEntity` | `content.article_likes` | `IArticleRepository` |
| `ArticleBookmarkEntity` | `content.article_bookmarks` | `IArticleRepository` |
| `ArticleShareEntity` | `content.article_shares` | `IArticleRepository` |
| `ArticleCommentEntity` | `content.article_comments` | `IArticleRepository` |
| `VideoRatingEntity` | `content.video_ratings` | `IVideoRepository` |
| `VideoShareEntity` | `content.video_shares` | `IVideoRepository` |
| `PlaylistEntity` | `content.playlists` | `IPlaylistRepository` |
| `PlaylistVideoEntity` | `content.playlist_videos` | `IPlaylistRepository` |
| `ShortVideoLikeEntity` | `content.short_video_likes` | `IShortVideoRepository` |
| `ShortVideoBookmarkEntity` | `content.short_video_bookmarks` | `IShortVideoRepository` |
| `ShortVideoShareEntity` | `content.short_video_shares` | `IShortVideoRepository` |

---

## 🔴 CRUCIAL — Core engagement metrics that drive the platform

---

### POST /api/v1/public/articles/{id}/likes

> Records that the authenticated user has liked an article. Likes are the primary social proof
> signal on the article feed — the denormalized `LikeCount` on the article is displayed on every
> article card and drives the "most liked" sort filter. A high like count signals to new visitors
> that an article is worth reading, making this endpoint critical for organic content discovery
> and for validating to paid clients that their commissioned articles are generating engagement.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `LikeArticleCommand(ArticleId, UserId) : ICommand<LikeArticleResult>` |
| **Response** | `200` + `LikeArticleResult(bool IsSuccess)` |

> Toggle-style: throws `409 Conflict` if the user has already liked the article.

**TODOs**
- [ ] `ArticleLikeEntity` plain class with `UserId`, `ArticleId`, `DateTimeOffset CreatedAt`
- [ ] `LikeArticleCommand(Guid ArticleId, Guid UserId) : ICommand<LikeArticleResult>`
- [ ] `LikeArticleHandler` — calls `articleRepository.GetByIdOrThrowAsync()`, checks `articleRepository.HasLikedAsync()`, throws `ArticleInteractionErrors.AlreadyLiked()` if true, creates `ArticleLikeEntity`, calls `articleRepository.AddLikeAsync()`, calls `article.IncrementLikeCount()`, calls `articleRepository.Update(article)`, commits `IContentUnitOfWork`
- [ ] `ArticleRepository.HasLikedAsync(userId, articleId, ct)` — composite key lookup
- [ ] `ArticleRepository.AddLikeAsync(like, ct)`
- [ ] `LikeArticleEndpointV1` Carter module — `UseCases/Public/Commands/LikeArticle/`

---

### DELETE /api/v1/public/articles/{id}/likes

> Removes the authenticated user's like from an article. Keeps the like count accurate and
> lets users correct an accidental like. The `LikeCount` on the article is decremented
> immediately so the change is reflected on the article feed without a database recount.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UnlikeArticleCommand(ArticleId, UserId) : ICommand<UnlikeArticleResult>` |
| **Response** | `200` + `UnlikeArticleResult(bool IsSuccess)` |

**TODOs**
- [ ] `UnlikeArticleCommand(Guid ArticleId, Guid UserId) : ICommand<UnlikeArticleResult>`
- [ ] `UnlikeArticleHandler` — checks `articleRepository.HasLikedAsync()`, throws `ArticleInteractionErrors.LikeNotFound()` if false, calls `articleRepository.RemoveLikeAsync(userId, articleId)`, calls `article.DecrementLikeCount()`, calls `articleRepository.Update(article)`, commits UoW
- [ ] `ArticleRepository.RemoveLikeAsync(userId, articleId, ct)` — `context.ArticleLikes.Where(composite key).ExecuteDeleteAsync()`
- [ ] `UnlikeArticleEndpointV1` — `UseCases/Public/Commands/UnlikeArticle/`

---

### GET /api/v1/public/articles/{id}/interactions/me

> Returns whether the requesting visitor has already liked and/or bookmarked this article.
> Critical for the frontend to render the like and bookmark toggle buttons in the correct state
> on page load — without this, buttons always appear as "off" for returning users regardless
> of their previous interactions.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetArticleInteractionStatusQuery(ArticleId, UserId) : IQuery<GetArticleInteractionStatusResult>` |
| **Response** | `200` + `GetArticleInteractionStatusResult(bool IsLiked, bool IsBookmarked)` |

**TODOs**
- [ ] `GetArticleInteractionStatusQuery(Guid ArticleId, Guid UserId) : IQuery<GetArticleInteractionStatusResult>`
- [ ] `GetArticleInteractionStatusResult(bool IsLiked, bool IsBookmarked)`
- [ ] `GetArticleInteractionStatusHandler` — calls `articleRepository.HasLikedAsync()` and `articleRepository.HasBookmarkedAsync()` in parallel
- [ ] `GetArticleInteractionStatusEndpointV1` — `UseCases/Public/Queries/GetArticleInteractionStatus/`

---

### POST /api/v1/public/videos/{id}/ratings

> Lets authenticated users rate a video 1–5 stars. Ratings are the primary discovery and quality
> signal for the video catalogue — the denormalized `RatingAverage` and `RatingCount` appear on
> every video card and power the "highest rated" sort on the video feed. An upsert design is used
> so users can revise their rating rather than being locked in, which encourages honest feedback
> over time. After each submission the average is recomputed from all ratings for that video.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RateVideoCommand(VideoId, UserId, Stars) : ICommand<RateVideoResult>` |
| **Response** | `200` + `RateVideoResult(bool IsSuccess)` |

> One rating per user per video. If a rating already exists, updates it (upsert).

**TODOs**
- [ ] `VideoRatingEntity.Create(id, userId, videoId, stars)` — validate `1 <= stars <= 5`
- [ ] `RateVideoCommand(Guid VideoId, Guid UserId, short Stars) : ICommand<RateVideoResult>`
- [ ] `RateVideoValidator` — `Stars` between 1 and 5
- [ ] `RateVideoHandler`:
  - Calls `videoRepository.GetByIdOrThrowAsync()`
  - Calls `videoRepository.GetRatingAsync(userId, videoId)`
  - If exists: calls `rating.UpdateStars(stars)`, calls `videoRepository.UpdateRating(rating)`
  - If not: creates `VideoRatingEntity`, calls `videoRepository.AddRatingAsync(rating)`
  - Fetches all ratings, recomputes average and count
  - Calls `video.UpdateRating(average, count)`, calls `videoRepository.Update(video)`
  - Commits UoW
- [ ] `VideoRepository.GetRatingAsync(userId, videoId, ct)`
- [ ] `VideoRepository.AddRatingAsync(rating, ct)` and `VideoRepository.UpdateRating(rating)`
- [ ] `RateVideoEndpointV1` — `UseCases/Public/Commands/RateVideo/`

---

### GET /api/v1/public/videos/{id}/ratings/me

> Returns the star rating the requesting visitor previously gave this video, so the star UI
> component can pre-fill the correct number of stars on page load. Returns `null Stars` if
> the user has never rated this video.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetMyVideoRatingQuery(VideoId, UserId) : IQuery<GetMyVideoRatingResult>` |
| **Response** | `200` + `GetMyVideoRatingResult(short? Stars)` |

**TODOs**
- [ ] `GetMyVideoRatingQuery(Guid VideoId, Guid UserId) : IQuery<GetMyVideoRatingResult>`
- [ ] `GetMyVideoRatingResult(short? Stars)`
- [ ] `GetMyVideoRatingHandler` — calls `videoRepository.GetRatingAsync(userId, videoId)`, returns `Stars` or `null`
- [ ] `GetMyVideoRatingEndpointV1` — `UseCases/Public/Queries/GetMyVideoRating/`

---

### POST /api/v1/public/articles/{id}/comments

> Allows authenticated users to post a comment on a published article. Comments are the main
> community engagement tool on the platform — a busy comment section drives return visits,
> signals to the editorial team which topics are resonating, and gives paid clients proof that
> their commissioned content is generating conversation. The `CommentCount` is incremented
> immediately and shown on the article card on the feed.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `AddArticleCommentCommand(ArticleId, UserId, Body) : ICommand<AddArticleCommentResult>` |
| **Response** | `201` + `AddArticleCommentResult(ArticleCommentDto Comment)` |

**DTOs**
```
ArticleCommentDto(Guid Id, Guid UserId, string? Body, bool IsDeleted, DateTime? CreatedAt)
```

**TODOs**
- [ ] `ArticleCommentEntity.Create(id, userId, articleId, body)`
- [ ] `AddArticleCommentCommand(Guid ArticleId, Guid UserId, string Body) : ICommand<AddArticleCommentResult>`
- [ ] `AddArticleCommentValidator` — `Body` required, max `ContentConstants.MaxCommentBodyLength`
- [ ] `AddArticleCommentHandler` — calls `articleRepository.GetByIdOrThrowAsync()`, creates entity, calls `articleRepository.AddCommentAsync()`, calls `article.IncrementCommentCount()`, calls `articleRepository.Update(article)`, commits UoW
- [ ] `ArticleRepository.AddCommentAsync(comment, ct)`
- [ ] `AddArticleCommentEndpointV1` — `UseCases/Public/Commands/AddArticleComment/`

---

## 🟡 IMPORTANT — Bookmarks, comment management, and playlists

---

### POST /api/v1/public/articles/{id}/bookmarks
### DELETE /api/v1/public/articles/{id}/bookmarks

> Bookmarking saves an article to the user's personal library for later reading. Bookmarks appear
> in the user's profile under "My Library → Saved Articles" and allow users to build their own
> reading list. The `BookmarkCount` on the article is a secondary engagement metric visible to
> the admin team — a high bookmark count indicates an article has lasting reference value, which
> is useful intel when deciding which topics to prioritise in the editorial calendar.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `BookmarkArticleCommand(ArticleId, UserId)` / `UnbookmarkArticleCommand(ArticleId, UserId)` |
| **Response** | `200` + `{ IsSuccess: true }` |

**TODOs**
- [ ] `ArticleBookmarkEntity` plain class with `UserId`, `ArticleId`, `DateTimeOffset CreatedAt`
- [ ] `BookmarkArticleCommand(Guid ArticleId, Guid UserId) : ICommand<BookmarkArticleResult>`
- [ ] `BookmarkArticleHandler` — checks `articleRepository.HasBookmarkedAsync()`, throws `ArticleInteractionErrors.AlreadyBookmarked()` if true, creates `ArticleBookmarkEntity`, calls `articleRepository.AddBookmarkAsync()`, calls `article.IncrementBookmarkCount()`, calls `articleRepository.Update(article)`, commits UoW
- [ ] `UnbookmarkArticleCommand(Guid ArticleId, Guid UserId) : ICommand<UnbookmarkArticleResult>`
- [ ] `UnbookmarkArticleHandler` — checks `articleRepository.HasBookmarkedAsync()`, throws `ArticleInteractionErrors.BookmarkNotFound()` if false, calls `articleRepository.RemoveBookmarkAsync()`, calls `article.DecrementBookmarkCount()`, calls `articleRepository.Update(article)`, commits UoW
- [ ] `ArticleRepository.HasBookmarkedAsync(userId, articleId, ct)`, `AddBookmarkAsync(bookmark, ct)`, `RemoveBookmarkAsync(userId, articleId, ct)`
- [ ] `BookmarkArticleEndpointV1` — `UseCases/Public/Commands/BookmarkArticle/`
- [ ] `UnbookmarkArticleEndpointV1` — `UseCases/Public/Commands/UnbookmarkArticle/`

---

### GET /api/v1/public/articles/{id}/comments

> Returns the paginated comment thread for an article. Available to anonymous users so the public
> can read the conversation below an article without signing in — lowering the barrier to
> engagement. Soft-deleted comments are returned with `IsDeleted = true` and `Body = null` to
> preserve the visual continuity of the thread without showing the removed content.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetArticleCommentsQuery(ArticleId, PaginatedRequest) : IQuery<GetArticleCommentsResult>` |
| **Response** | `200` + `GetArticleCommentsResult(PaginatedResult<ArticleCommentDto> Comments)` |

**TODOs**
- [ ] `GetArticleCommentsQuery(Guid ArticleId, PaginatedRequest PaginatedRequest) : IQuery<GetArticleCommentsResult>`
- [ ] `GetArticleCommentsHandler` — calls `articleRepository.GetCommentsAsync(articleId, paginatedRequest)`
- [ ] `ArticleRepository.GetCommentsAsync(articleId, paginatedRequest, ct)` — ordered by `created_at`
- [ ] `GetArticleCommentsEndpointV1` (`.AllowAnonymous()`) — `UseCases/Public/Queries/GetArticleComments/`

---

### PUT /api/v1/public/articles/{id}/comments/{commentId}

> Allows a user to edit their own comment after posting it. Ownership is strictly enforced —
> the requesting user's ID must match the comment's `UserId`.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` (own comment only) |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `EditArticleCommentCommand(ArticleId, CommentId, UserId, Body) : ICommand<EditArticleCommentResult>` |
| **Response** | `200` + `EditArticleCommentResult(bool IsSuccess)` |

**TODOs**
- [ ] `EditArticleCommentCommand(Guid ArticleId, Guid CommentId, Guid UserId, string Body) : ICommand<EditArticleCommentResult>`
- [ ] `EditArticleCommentValidator` — `Body` required, max `ContentConstants.MaxCommentBodyLength`
- [ ] `EditArticleCommentHandler` — calls `articleRepository.GetCommentByIdAsync()`, throws `ArticleInteractionErrors.CommentNotFound()` if null, verifies `comment.UserId == command.UserId`, throws `ArticleInteractionErrors.NotCommentOwner()` if not, calls `comment.Edit(body)`, calls `articleRepository.UpdateComment(comment)`, commits UoW
- [ ] `ArticleRepository.GetCommentByIdAsync(commentId, ct)` and `ArticleRepository.UpdateComment(comment)`
- [ ] `EditArticleCommentEndpointV1` — `UseCases/Public/Commands/EditArticleComment/`

---

### DELETE /api/v1/public/articles/{id}/comments/{commentId}

> Soft-deletes a comment from the article thread. Visitors can delete their own comments;
> admins (identified by the `IsAdmin` flag resolved from the JWT role claim in the endpoint)
> can delete any comment for moderation. The `CommentCount` is decremented so the article's
> engagement counter stays accurate.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` (own) — `IsAdmin` flag passed from endpoint |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `DeleteArticleCommentCommand(ArticleId, CommentId, RequestingUserId, IsAdmin) : ICommand<DeleteArticleCommentResult>` |
| **Response** | `200` + `DeleteArticleCommentResult(bool IsSuccess)` |

**TODOs**
- [ ] `DeleteArticleCommentCommand(Guid ArticleId, Guid CommentId, Guid RequestingUserId, bool IsAdmin) : ICommand<DeleteArticleCommentResult>`
- [ ] `DeleteArticleCommentHandler` — calls `articleRepository.GetCommentByIdAsync()`, throws `ArticleInteractionErrors.CommentNotFound()` if null, if `!IsAdmin` verifies ownership and throws `ArticleInteractionErrors.NotCommentOwner()`, calls `comment.SoftDelete()`, calls `articleRepository.UpdateComment(comment)`, calls `article.DecrementCommentCount()`, calls `articleRepository.Update(article)`, commits UoW
- [ ] `DeleteArticleCommentEndpointV1` — `UseCases/Public/Commands/DeleteArticleComment/`

---

### POST /api/v1/public/playlists

> Creates a personal video playlist for the authenticated user. Playlists appear in the user's
> "My Library" profile section and power the "Add to Playlist" dropdown button on every video page.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreatePlaylistCommand(UserId, Name) : ICommand<CreatePlaylistResult>` |
| **Response** | `201` + `CreatePlaylistResult(PlaylistDto Playlist)` |

**DTOs**
```
PlaylistDto(Guid Id, string Name, int VideoCount)
```

**TODOs**
- [ ] `PlaylistEntity.Create(id, userId, name)` — validate name max `ContentConstants.MaxPlaylistNameLength`
- [ ] `CreatePlaylistCommand(Guid UserId, string Name) : ICommand<CreatePlaylistResult>`
- [ ] `CreatePlaylistValidator` — `Name` required, max `ContentConstants.MaxPlaylistNameLength`
- [ ] `CreatePlaylistHandler` — creates entity, calls `playlistRepository.AddAsync()`, commits UoW
- [ ] `PlaylistRepository.AddAsync(playlist, ct)`
- [ ] `CreatePlaylistEndpointV1` — `UseCases/Public/Commands/CreatePlaylist/`

---

### POST /api/v1/public/playlists/{id}/videos

> Adds a published video to the user's playlist at a specified sort position. Ownership is
> enforced. Duplicate prevention ensures the same video cannot appear twice in the same playlist.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` (playlist owner only) |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `AddVideoToPlaylistCommand(PlaylistId, VideoId, UserId, SortOrder) : ICommand<AddVideoToPlaylistResult>` |
| **Response** | `200` + `AddVideoToPlaylistResult(bool IsSuccess)` |

**TODOs**
- [ ] `PlaylistVideoEntity` plain class with `PlaylistId`, `VideoId`, `int SortOrder`
- [ ] `AddVideoToPlaylistCommand(Guid PlaylistId, Guid VideoId, Guid UserId, int SortOrder) : ICommand<AddVideoToPlaylistResult>`
- [ ] `AddVideoToPlaylistHandler` — fetches playlist, verifies `playlist.UserId == command.UserId`, throws `PlaylistErrors.NotOwner()` if not, verifies video exists and is published, calls `playlistRepository.VideoExistsInPlaylistAsync()`, throws `PlaylistErrors.VideoAlreadyInPlaylist()` if true, creates `PlaylistVideoEntity`, calls `playlistRepository.AddVideoAsync()`, commits UoW
- [ ] `PlaylistRepository.VideoExistsInPlaylistAsync(playlistId, videoId, ct)` and `PlaylistRepository.AddVideoAsync(playlistVideo, ct)`
- [ ] `AddVideoToPlaylistEndpointV1` — `UseCases/Public/Commands/AddVideoToPlaylist/`

---

### DELETE /api/v1/public/playlists/{id}/videos/{videoId}

> Removes a video from a playlist. Ownership is enforced. Hard delete of the playlist-video
> link only — the video itself is unaffected.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` (playlist owner only) |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RemoveVideoFromPlaylistCommand(PlaylistId, VideoId, UserId) : ICommand<RemoveVideoFromPlaylistResult>` |
| **Response** | `200` + `RemoveVideoFromPlaylistResult(bool IsSuccess)` |

**TODOs**
- [ ] `RemoveVideoFromPlaylistCommand(Guid PlaylistId, Guid VideoId, Guid UserId) : ICommand<RemoveVideoFromPlaylistResult>`
- [ ] `RemoveVideoFromPlaylistHandler` — fetches playlist, verifies ownership, throws `PlaylistErrors.NotOwner()` if not, calls `playlistRepository.RemoveVideoAsync(playlistId, videoId)`, commits UoW
- [ ] `PlaylistRepository.RemoveVideoAsync(playlistId, videoId, ct)` — `context.PlaylistVideos.Where(composite key).ExecuteDeleteAsync()`
- [ ] `RemoveVideoFromPlaylistEndpointV1` — `UseCases/Public/Commands/RemoveVideoFromPlaylist/`

---

## 🟢 MODERATE — Shares, queries, and short video interactions

---

### POST /api/v1/public/articles/{id}/shares
### POST /api/v1/public/videos/{id}/shares

> Records that a user (or anonymous visitor) has shared an article or video to an external
> platform. `UserId` is optional because the vast majority of social sharing happens without
> a logged-in account. The share count is denormalized on the content record.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ShareArticleCommand(ArticleId, UserId?)` / `ShareVideoCommand(VideoId, UserId?)` |
| **Response** | `200` + `{ IsSuccess: true }` |

**TODOs**
- [ ] `ArticleShareEntity.Create(id, userId, articleId)` — `userId` nullable
- [ ] `ShareArticleCommand(Guid ArticleId, Guid? UserId) : ICommand<ShareArticleResult>`
- [ ] `ShareArticleHandler` — calls `articleRepository.GetByIdOrThrowAsync()`, creates `ArticleShareEntity`, calls `articleRepository.AddShareAsync()`, calls `article.IncrementShareCount()`, calls `articleRepository.Update(article)`, commits UoW
- [ ] `ArticleRepository.AddShareAsync(share, ct)`
- [ ] `VideoShareEntity.Create(id, userId, videoId)` — same pattern
- [ ] `ShareVideoCommand(Guid VideoId, Guid? UserId) : ICommand<ShareVideoResult>`
- [ ] `ShareVideoHandler` → `video.IncrementShareCount()`
- [ ] `VideoRepository.AddShareAsync(share, ct)`
- [ ] `ShareArticleEndpointV1` (`.AllowAnonymous()`) — `UseCases/Public/Commands/ShareArticle/`
- [ ] `ShareVideoEndpointV1` (`.AllowAnonymous()`) — `UseCases/Public/Commands/ShareVideo/`

---

### GET /api/v1/public/playlists

> Returns all playlists owned by the requesting user. Powers the "My Library → Playlists"
> section and the "Add to Playlist" dropdown on the video page.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetMyPlaylistsQuery(UserId) : IQuery<GetMyPlaylistsResult>` |
| **Response** | `200` + `GetMyPlaylistsResult(IReadOnlyList<PlaylistDto> Playlists)` |

**TODOs**
- [ ] `GetMyPlaylistsQuery(Guid UserId) : IQuery<GetMyPlaylistsResult>`
- [ ] `GetMyPlaylistsHandler` — calls `playlistRepository.GetByUserIdAsync(userId)`
- [ ] `PlaylistRepository.GetByUserIdAsync(userId, ct)`
- [ ] `GetMyPlaylistsEndpointV1` — `UseCases/Public/Queries/GetMyPlaylists/`

---

### GET /api/v1/public/playlists/{id}

> Returns a single playlist with all its videos ordered by `SortOrder`. Ownership enforced.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` (owner only) |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetPlaylistByIdQuery(Id, UserId) : IQuery<GetPlaylistByIdResult>` |
| **Response** | `200` + `GetPlaylistByIdResult(PlaylistDetailDto Playlist)` |

**DTOs**
```
PlaylistDetailDto(Guid Id, string Name, IReadOnlyList<VideoInPlaylistDto> Videos)
VideoInPlaylistDto(Guid VideoId, string Title, string? ThumbnailUrl, decimal RatingAverage, int RatingCount, int SortOrder)
```

**TODOs**
- [ ] `GetPlaylistByIdQuery(Guid Id, Guid UserId) : IQuery<GetPlaylistByIdResult>`
- [ ] `GetPlaylistByIdHandler` — calls `playlistRepository.GetByIdWithVideosAsync(id)`, verifies `playlist.UserId == command.UserId`, throws `PlaylistErrors.NotFound()` if null or ownership fails
- [ ] `PlaylistRepository.GetByIdWithVideosAsync(id, ct)` — includes `PlaylistVideos` → `Video`, ordered by `SortOrder`
- [ ] `GetPlaylistByIdEndpointV1` — `UseCases/Public/Queries/GetPlaylistById/`

---

### POST /api/v1/public/shorts/{id}/likes
### DELETE /api/v1/public/shorts/{id}/likes

> Records or removes a like on a short video. The like count is one of the four counters
> displayed on every short video card in the discovery feed.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `LikeShortVideoCommand(ShortVideoId, UserId)` / `UnlikeShortVideoCommand(ShortVideoId, UserId)` |
| **Response** | `200` + `{ IsSuccess: true }` |

**TODOs**
- [ ] `ShortVideoLikeEntity` plain class with `UserId`, `ShortVideoId`, `DateTimeOffset CreatedAt`
- [ ] `LikeShortVideoCommand(Guid ShortVideoId, Guid UserId) : ICommand<LikeShortVideoResult>`
- [ ] `LikeShortVideoHandler` — calls `shortVideoRepository.GetByIdOrThrowAsync()`, checks `shortVideoRepository.HasLikedAsync()`, throws `ShortVideoInteractionErrors.AlreadyLiked()` if true, creates entity, calls `shortVideoRepository.AddLikeAsync()`, calls `shortVideo.IncrementLikeCount()`, calls `shortVideoRepository.Update(shortVideo)`, commits UoW
- [ ] `UnlikeShortVideoCommand(Guid ShortVideoId, Guid UserId) : ICommand<UnlikeShortVideoResult>`
- [ ] `UnlikeShortVideoHandler` → `shortVideo.DecrementLikeCount()`
- [ ] `ShortVideoRepository.HasLikedAsync(userId, shortVideoId, ct)`, `AddLikeAsync(like, ct)`, `RemoveLikeAsync(userId, shortVideoId, ct)`
- [ ] `LikeShortVideoEndpointV1` — `UseCases/Public/Commands/LikeShortVideo/`
- [ ] `UnlikeShortVideoEndpointV1` — `UseCases/Public/Commands/UnlikeShortVideo/`

---

### GET /api/v1/public/shorts/{id}/interactions/me

> Returns whether the requesting visitor has already liked and/or bookmarked this short video.
> Critical for the frontend to render the like and bookmark toggles in the correct state when
> the clip enters the viewport in the discovery feed scroll.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetShortVideoInteractionStatusQuery(ShortVideoId, UserId) : IQuery<GetShortVideoInteractionStatusResult>` |
| **Response** | `200` + `GetShortVideoInteractionStatusResult(bool IsLiked, bool IsBookmarked)` |

**TODOs**
- [ ] `GetShortVideoInteractionStatusQuery(Guid ShortVideoId, Guid UserId) : IQuery<GetShortVideoInteractionStatusResult>`
- [ ] `GetShortVideoInteractionStatusResult(bool IsLiked, bool IsBookmarked)`
- [ ] `GetShortVideoInteractionStatusHandler` — calls `shortVideoRepository.HasLikedAsync()` and `shortVideoRepository.HasBookmarkedAsync()` in parallel
- [ ] `GetShortVideoInteractionStatusEndpointV1` — `UseCases/Public/Queries/GetShortVideoInteractionStatus/`

---

### POST /api/v1/public/shorts/{id}/bookmarks
### DELETE /api/v1/public/shorts/{id}/bookmarks

> Lets users save a short video to their personal library. Same pattern as short video likes.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `BookmarkShortVideoCommand(ShortVideoId, UserId)` / `UnbookmarkShortVideoCommand(ShortVideoId, UserId)` |
| **Response** | `200` + `{ IsSuccess: true }` |

**TODOs**
- [ ] `ShortVideoBookmarkEntity` plain class with `UserId`, `ShortVideoId`, `DateTimeOffset CreatedAt`
- [ ] `BookmarkShortVideoCommand(Guid ShortVideoId, Guid UserId) : ICommand<BookmarkShortVideoResult>` → `shortVideo.IncrementBookmarkCount()`
- [ ] `UnbookmarkShortVideoCommand(Guid ShortVideoId, Guid UserId) : ICommand<UnbookmarkShortVideoResult>` → `shortVideo.DecrementBookmarkCount()`
- [ ] `ShortVideoRepository.HasBookmarkedAsync(userId, shortVideoId, ct)`, `AddBookmarkAsync(bookmark, ct)`, `RemoveBookmarkAsync(userId, shortVideoId, ct)`
- [ ] `BookmarkShortVideoEndpointV1` — `UseCases/Public/Commands/BookmarkShortVideo/`
- [ ] `UnbookmarkShortVideoEndpointV1` — `UseCases/Public/Commands/UnbookmarkShortVideo/`

---

### POST /api/v1/public/shorts/{id}/shares

> Records a share event for a short video. `UserId` is optional — anonymous shares are tracked.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ShareShortVideoCommand(ShortVideoId, UserId?) : ICommand<ShareShortVideoResult>` |
| **Response** | `200` + `ShareShortVideoResult(bool IsSuccess)` |

**TODOs**
- [ ] `ShortVideoShareEntity.Create(id, userId, shortVideoId)` — `userId` nullable
- [ ] `ShareShortVideoCommand(Guid ShortVideoId, Guid? UserId) : ICommand<ShareShortVideoResult>`
- [ ] `ShareShortVideoHandler` → `shortVideo.IncrementShareCount()`
- [ ] `ShortVideoRepository.AddShareAsync(share, ct)`
- [ ] `ShareShortVideoEndpointV1` (`.AllowAnonymous()`) — `UseCases/Public/Commands/ShareShortVideo/`

---

### POST /api/v1/public/shorts/{id}/views

> Records a view event each time a short video becomes visible in the scroll feed. The frontend
> calls this as soon as a clip enters the viewport — no explicit play action required.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RecordShortVideoViewCommand(ShortVideoId) : ICommand<RecordShortVideoViewResult>` |
| **Response** | `200` + `RecordShortVideoViewResult(bool IsSuccess)` |

**TODOs**
- [ ] `RecordShortVideoViewCommand(Guid ShortVideoId) : ICommand<RecordShortVideoViewResult>`
- [ ] `RecordShortVideoViewHandler` — calls `shortVideoRepository.GetByIdOrThrowAsync()`, calls `shortVideo.IncrementViewCount()`, calls `shortVideoRepository.Update(shortVideo)`, commits UoW
- [ ] `RecordShortVideoViewEndpointV1` (`.AllowAnonymous()`) — `UseCases/Public/Commands/RecordShortVideoView/`

---

## ⚪ TRIVIAL — User library history and playlist management

---

### DELETE /api/v1/public/playlists/{id}

> Permanently deletes a playlist and all its video entries (cascaded). Ownership enforced.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` (owner only) |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `DeletePlaylistCommand(Id, UserId) : ICommand<DeletePlaylistResult>` |
| **Response** | `200` + `DeletePlaylistResult(bool IsSuccess)` |

**TODOs**
- [ ] `DeletePlaylistCommand(Guid Id, Guid UserId) : ICommand<DeletePlaylistResult>`
- [ ] `DeletePlaylistHandler` — fetches playlist, verifies `playlist.UserId == command.UserId`, throws `PlaylistErrors.NotOwner()` if not, calls `playlistRepository.Delete(playlist)`, commits UoW
- [ ] `PlaylistRepository.Delete(playlist)` — hard delete (cascade removes `PlaylistVideoEntity` rows)
- [ ] `DeletePlaylistEndpointV1` — `UseCases/Public/Commands/DeletePlaylist/`

---

### PUT /api/v1/public/playlists/{id}

> Renames a playlist. Ownership enforced.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` (owner only) |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RenamePlaylistCommand(Id, UserId, Name) : ICommand<RenamePlaylistResult>` |
| **Response** | `200` + `RenamePlaylistResult(bool IsSuccess)` |

**TODOs**
- [ ] `RenamePlaylistCommand(Guid Id, Guid UserId, string Name) : ICommand<RenamePlaylistResult>`
- [ ] `RenamePlaylistValidator` — `Name` required, max `ContentConstants.MaxPlaylistNameLength`
- [ ] `RenamePlaylistHandler` — fetches playlist, verifies ownership, throws `PlaylistErrors.NotOwner()` if not, calls `playlist.Rename(name)`, calls `playlistRepository.Update(playlist)`, commits UoW
- [ ] `PlaylistRepository.Update(playlist)`
- [ ] `RenamePlaylistEndpointV1` — `UseCases/Public/Commands/RenamePlaylist/`

---

### GET /api/v1/public/me/articles/bookmarks

> Returns the paginated list of articles bookmarked by the requesting user. Powers
> "My Library → Saved Articles". Scoped strictly to the requesting user.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetMyArticleBookmarksQuery(UserId, PaginatedRequest) : IQuery<GetMyArticleBookmarksResult>` |
| **Response** | `200` + `GetMyArticleBookmarksResult(PaginatedResult<ArticleSummaryDto> Articles)` |

**TODOs**
- [ ] `GetMyArticleBookmarksQuery(Guid UserId, PaginatedRequest PaginatedRequest) : IQuery<GetMyArticleBookmarksResult>`
- [ ] `GetMyArticleBookmarksHandler` — queries `context.ArticleBookmarks.Where(b => b.UserId == userId)`, includes `Article`, maps to `ArticleSummaryDto`, paginates
- [ ] `GetMyArticleBookmarksEndpointV1` — `UseCases/Public/Queries/GetMyArticleBookmarks/`

---

### GET /api/v1/public/me/articles/liked

> Returns the paginated list of articles liked by the requesting user. Powers
> "My Library → Liked Articles". Scoped strictly to the requesting user.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetMyLikedArticlesQuery(UserId, PaginatedRequest) : IQuery<GetMyLikedArticlesResult>` |
| **Response** | `200` + `GetMyLikedArticlesResult(PaginatedResult<ArticleSummaryDto> Articles)` |

**TODOs**
- [ ] `GetMyLikedArticlesQuery(Guid UserId, PaginatedRequest PaginatedRequest) : IQuery<GetMyLikedArticlesResult>`
- [ ] `GetMyLikedArticlesHandler` — queries `context.ArticleLikes.Where(l => l.UserId == userId)`, includes `Article`, maps to `ArticleSummaryDto`, paginates
- [ ] `ArticleRepository.GetLikedArticlesAsync(userId, paginatedRequest, ct)`
- [ ] `GetMyLikedArticlesEndpointV1` — `UseCases/Public/Queries/GetMyLikedArticles/`

---

### GET /api/v1/public/me/shorts/bookmarks

> Returns the paginated list of short videos bookmarked by the requesting user. Powers
> "My Library → Saved Shorts". Scoped strictly to the requesting user.

| | |
|---|---|
| **Auth** | `RequireVisitorOnly` |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetMyShortVideoBookmarksQuery(UserId, PaginatedRequest) : IQuery<GetMyShortVideoBookmarksResult>` |
| **Response** | `200` + `GetMyShortVideoBookmarksResult(PaginatedResult<ShortVideoDto> ShortVideos)` |

**TODOs**
- [ ] `GetMyShortVideoBookmarksQuery(Guid UserId, PaginatedRequest PaginatedRequest) : IQuery<GetMyShortVideoBookmarksResult>`
- [ ] `GetMyShortVideoBookmarksHandler` — queries `context.ShortVideoBookmarks.Where(b => b.UserId == userId)`, includes `ShortVideo`, maps to `ShortVideoDto`, paginates
- [ ] `ShortVideoRepository.GetBookmarkedShortVideosAsync(userId, paginatedRequest, ct)`
- [ ] `GetMyShortVideoBookmarksEndpointV1` — `UseCases/Public/Queries/GetMyShortVideoBookmarks/`