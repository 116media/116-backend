# Admin Analytics — Content Interactions

> Read-only monitoring endpoints for the admin to track how visitors engage with articles,
> videos, and short videos. This is the admin-facing counterpart to the visitor-facing
> Interactions sub-module (`05-interactions.md`).
>
> All endpoints here are `admin` scope — they require `Admin` or `SuperAdmin` role.
> They do not duplicate the visitor interaction endpoints; they aggregate and surface
> the data those interactions produce.

---

## Why this exists

The platform has two groups of people using the Interactions data:

| Group | What they do | Where |
|---|---|---|
| **Visitors** | Like, bookmark, share, comment, rate, create playlists | `public` scope — `05-interactions.md` |
| **Admin / Editorial team** | Monitor engagement, moderate comments, report to clients | `admin` scope — this file |

The admin never needs to "like" an article on behalf of a visitor. What the admin needs
is to see the aggregated signal those interactions produce, use it to make editorial
decisions, and report engagement metrics to paying clients who commissioned content.

---

## Priority Legend

| Symbol | Level | Meaning |
|---|---|---|
| 🔴 | CRUCIAL | Client reporting and platform health — needed immediately |
| 🟡 | IMPORTANT | Moderation and editorial decisions |
| 🟢 | MODERATE | Supporting metrics, trend analysis |
| ⚪ | TRIVIAL | Nice-to-have operational visibility |

---

## 🔴 CRUCIAL — Content Performance (Client Reporting)

Paying clients commission articles and videos. After publication, they expect proof that
their content generated real engagement. These endpoints power that reporting.

---

### GET /api/v1/admin/articles/top

> Returns the top-ranked articles sorted by a chosen engagement metric. Used by the
> editorial team to identify what is working and by account managers to pull engagement
> numbers for client reports (e.g. "your article ranked #1 by likes this week").

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `GetTopArticlesQuery(SortBy, Limit, DateFrom?, DateTo?)` |
| **Response** | `200` + `IReadOnlyList<ArticleEngagementDto>` |

**Query parameters**

| Parameter | Values | Description |
|---|---|---|
| `sortBy` | `likes`, `shares`, `bookmarks`, `comments` | Engagement metric to rank by |
| `limit` | 1–50, default 10 | Number of results |
| `dateFrom` | ISO date | Optional: filter interactions created after this date |
| `dateTo` | ISO date | Optional: filter interactions created before this date |

**Response DTO**

```
ArticleEngagementDto(
    Guid Id,
    string Title,
    string Slug,
    string? CoverImageUrl,
    int LikeCount,
    int BookmarkCount,
    int ShareCount,
    int CommentCount,
    DateTime PublishedAt
)
```

---

### GET /api/v1/admin/videos/top

> Returns the top-ranked videos sorted by rating average or share count. Used to identify
> which episodes the audience rates most highly — a strong signal for the production team
> on which show formats to prioritise.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `GetTopVideosQuery(SortBy, Limit, DateFrom?, DateTo?)` |
| **Response** | `200` + `IReadOnlyList<VideoEngagementDto>` |

**Query parameters**

| Parameter | Values | Description |
|---|---|---|
| `sortBy` | `rating`, `shares` | Engagement metric to rank by |
| `limit` | 1–50, default 10 | Number of results |
| `dateFrom` / `dateTo` | ISO date | Optional date range filter |

**Response DTO**

```
VideoEngagementDto(
    Guid Id,
    string Title,
    string Slug,
    string? ThumbnailUrl,
    decimal RatingAverage,
    int RatingCount,
    int ShareCount,
    DateTime PublishedAt
)
```

---

### GET /api/v1/admin/shorts/top

> Returns the top-ranked short videos sorted by views, likes, or shares. View count is
> the primary reach metric for short videos — it fires as soon as a clip enters the
> scroll viewport, giving the most honest measure of how many people actually saw the clip.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `GetTopShortVideosQuery(SortBy, Limit, DateFrom?, DateTo?)` |
| **Response** | `200` + `IReadOnlyList<ShortVideoEngagementDto>` |

**Query parameters**

| Parameter | Values | Description |
|---|---|---|
| `sortBy` | `views`, `likes`, `shares`, `bookmarks` | Engagement metric to rank by |
| `limit` | 1–50, default 10 | Number of results |
| `dateFrom` / `dateTo` | ISO date | Optional date range filter |

**Response DTO**

```
ShortVideoEngagementDto(
    Guid Id,
    string Title,
    string Url,
    int ViewCount,
    int LikeCount,
    int BookmarkCount,
    int ShareCount,
    DateTime ActivatedAt
)
```

---

### GET /api/v1/admin/articles/{id}/engagement

> Returns the full engagement breakdown for a single article. The primary endpoint for
> account managers pulling a one-pager for a client meeting: "here are the exact numbers
> for your article — likes, shares, bookmarks, and comments since publication."

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `GetArticleEngagementQuery(ArticleId)` |
| **Response** | `200` + `ArticleEngagementDto` |

> Same DTO as `GetTopArticles` — includes all four counters plus article metadata.

---

## 🟡 IMPORTANT — Comment Moderation

Comments are the only user-generated text on the platform. The admin needs full visibility
and the ability to remove harmful, spam, or off-brand content.

---

### GET /api/v1/admin/articles/{id}/comments

> Returns all comments on an article for the admin, including soft-deleted ones (visible
> to admin with `IsDeleted = true` and `Body = null`). This is the moderation view —
> distinct from the public endpoint which hides the deleted state differently.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `AdminGetArticleCommentsQuery(ArticleId, Page, PageSize, IncludeDeleted?)` |
| **Response** | `200` + `PagedResponse<ArticleCommentDto>` |

**Query parameters**

| Parameter | Default | Description |
|---|---|---|
| `page` | 1 | Page index |
| `pageSize` | 20 | Results per page |
| `includeDeleted` | `true` | Whether to show soft-deleted comments (admin only) |

---

### DELETE /api/v1/admin/articles/{id}/comments/{commentId}

> Permanently soft-deletes any comment, regardless of who authored it. This is the
> moderation tool — admins do not need to own a comment to remove it. The comment stays
> in the database (audit trail) but its body is nulled out and `IsDeleted = true`.
> The article `CommentCount` is decremented.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Command** | `AdminDeleteArticleCommentCommand(ArticleId, CommentId)` |
| **Response** | `200` + `{ IsSuccess: true }` |

> Note: the visitor-facing delete endpoint (`public/`) enforces ownership.
> This admin endpoint bypasses ownership — it is purely for moderation.

---

## 🟢 MODERATE — Engagement Health

These help the editorial team spot problems before they become business issues.

---

### GET /api/v1/admin/articles/zero-engagement

> Returns published articles that have received no likes, no shares, and no comments
> after a configurable number of days since publication. These are candidates for
> promotion (featured slots, push notifications) or re-evaluation.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `GetZeroEngagementArticlesQuery(DaysSincePublication, Page, PageSize)` |
| **Response** | `200` + `PagedResponse<ArticleEngagementDto>` |

---

### GET /api/v1/admin/articles/{id}/engagement/trend

> Returns a time-bucketed engagement count for one article (daily or weekly buckets).
> Shows whether an article's engagement is growing, steady, or fading — useful for
> deciding whether to resurface it in a newsletter or featured section.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `GetArticleEngagementTrendQuery(ArticleId, Bucket, DateFrom, DateTo)` |
| **Response** | `200` + `IReadOnlyList<EngagementBucketDto>` |

**Response DTO**

```
EngagementBucketDto(
    DateTime BucketStart,
    int Likes,
    int Shares,
    int Bookmarks,
    int Comments
)
```

---

## ⚪ TRIVIAL — Operational Visibility

---

### GET /api/v1/admin/interactions/summary

> Returns platform-wide interaction totals for a given date range. A single-number
> health check: are visitors engaging more or less than last week?

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `AdminMetrics` |
| **Query** | `GetInteractionsSummaryQuery(DateFrom, DateTo)` |
| **Response** | `200` + `InteractionsSummaryDto` |

**Response DTO**

```
InteractionsSummaryDto(
    int TotalArticleLikes,
    int TotalArticleBookmarks,
    int TotalArticleShares,
    int TotalArticleComments,
    int TotalVideoRatings,
    int TotalVideoShares,
    int TotalShortVideoViews,
    int TotalShortVideoLikes,
    int TotalShortVideoShares,
    int TotalShortVideoBookmarks
)
```

---

## Implementation Notes

- All queries read from the same tables populated by the visitor `public/` interactions.
  No new tables are needed — this is purely a read layer on top of existing data.
- `GetTopArticles`, `GetTopVideos`, `GetTopShorts` can be served directly from the
  denormalized counter columns (`LikeCount`, `ViewCount`, etc.) already on the entity —
  no join to interaction tables needed for basic ranking.
- `GetArticleEngagementTrend` requires querying `article_likes`, `article_shares`,
  `article_bookmarks`, `article_comments` with a `GROUP BY DATE_TRUNC(bucket, created_at)`
  — this is the only query that hits the interaction tables directly.
- `GetZeroEngagementArticles` filters on `articles WHERE like_count = 0 AND share_count = 0
  AND comment_count = 0 AND published_at < NOW() - INTERVAL '{days} days'`.