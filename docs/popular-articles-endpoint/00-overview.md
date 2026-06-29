# Popular Articles Endpoint — Overview

This feature adds a **true popularity-ranked public endpoint** for articles. Today the
article-detail page's "popular articles" sidebar is sourced from `getPromotedArticles`
(an editorial/paid boost signal) or, as a fallback, from `getPublishedArticles` (recency).
Neither reflects actual reader engagement. This document set designs and documents a proper
popularity endpoint backed by the engagement counters already persisted on the article
entity.

---

## The Gap

| Concern | Today | After |
|---------|-------|-------|
| "Popular" primary source | `GET /api/v1/public/articles/promoted` (paid promotion) | `GET /api/v1/public/articles/popular` (engagement ranked) |
| Ranking signal | Editorial promotion, then `CreatedAt` desc | Weighted engagement score, then `PublishedAt` desc |
| Exclude current article | Filtered client-side after fetch | `excludeId` query parameter, dropped in SQL |
| Caching | None dedicated | `IMemoryCache` + eviction token, mirrors popular-tags |

The published-articles slice orders by `CreatedAt` descending — pure recency. The promoted
slice orders by paid promotion state. There is **no endpoint that ranks by reader
engagement**. The frontend open question **Q3** (`apps/frontend/docs/article-detail/19-open-questions.md`)
explicitly records this: the sidebar approximates popularity with promoted articles as a
proxy, "when a real popularity-sorted endpoint (by engagement/views) lands, only the primary
source swaps."

---

## Goal

Add a single, cacheable, anonymous, rate-limited public endpoint:

```
GET /api/v1/public/articles/popular?limit=&categoryId=&excludeId=
```

that returns `ArticleSummaryDto[]` for **Published** articles ordered by a **weighted
engagement score** (descending), tie-broken by `PublishedAt` (descending). It mirrors the
`GetPopularTags` slice exactly for its endpoint shape and its caching + invalidation design.

---

## Scope

**In scope:**

- New CQRS vertical slice under
  `Application/Editorial/UseCases/Public/Queries/GetPopularArticles/`.
- New query builder `PopularArticlesQueryBuilder` computing and ordering by the score.
- New repository method `GetPopularArticlesAsync` on `IArticleRepository`.
- `IMemoryCache` caching keyed by `(limit, categoryId)` plus an eviction token.
- A new `IPopularArticlesCacheInvalidator` (mirrors `IPopularTagsCacheInvalidator`),
  invalidated on the engagement mutations that move the ranking.
- Unit + integration tests.
- Frontend: `getPopularArticles` repository method, use case, and a rewired
  `useArticleDetailPopular` hook.

**Out of scope:**

- Personalization or per-user ranking.
- Time-series analytics or a trending window backed by event history.
- Any change to the existing published/promoted slices (they remain as-is).

---

## Key Design Decision — How to define "popularity"

There is no single canonical "popularity" number on the article. The entity persists four
engagement counters (confirmed in `01-current-state.md`): `LikeCount`, `CommentCount`,
`ShareCount`, `BookmarkCount`. So popularity must be *derived* from these four counters.

### Option A — Simple v1: order by `LikeCount` desc, then `PublishedAt` desc

- **Pros:** Trivial to translate to SQL (`ORDER BY like_count DESC, published_at DESC`),
  trivially indexable, easy to reason about.
- **Cons:** Likes are only one dimension of engagement. An article with 0 likes but 50
  shares and 30 comments (highly discussed/viral) would rank below an article with 1 like.
  It ignores three of the four signals we already persist.

### Option B — Recommended: weighted engagement score

Compute a single score from all four counters with documented, tunable weights:

```
score = (w_like      * LikeCount)
      + (w_comment   * CommentCount)
      + (w_share     * ShareCount)
      + (w_bookmark  * BookmarkCount)
```

Order by `score` descending, tie-broken by `PublishedAt` descending.

**Recommended weight constants** (documented as named constants, tunable in one place):

| Signal | Constant | Weight | Rationale |
|--------|----------|--------|-----------|
| Like | `LikeWeight` | `4` | Highest weight — the most direct expression of reader approval, the primary popularity signal. |
| Comment | `CommentWeight` | `3` | High effort — the reader wrote something. Indicates discussion/controversy. |
| Share | `ShareWeight` | `2` | The reader redistributes the article to others. |
| Bookmark | `BookmarkWeight` | `1` | Private intent to return — meaningful but not amplifying. The baseline unit. |

These weights are ordinal (`like > comment > share > bookmark`) rather than statistically
tuned — there is no analytics history to fit them against yet. They live as `const int`
values on a single `PopularArticlesScoring` constants type so a product decision can retune
them without touching handler or builder logic. Because all four counters are non-negative
integers and the weights are integers, the whole score is an integer expression that
PostgreSQL evaluates directly in the `ORDER BY`.

### Recency / freshness (documented, defaulted OFF for v1)

Pure all-time engagement favors older articles that have accumulated counts over months.
Two freshness strategies are documented so the product can opt in later without a schema
change:

1. **Freshness window (hard filter):** only rank articles published within the last `N`
   days (e.g. 90). Simple, SQL-friendly (`WHERE published_at >= now() - interval`), but
   binary — an article at day `N+1` drops off a cliff.
2. **Time-decay factor (soft weighting):** multiply the score by a decay term based on age,
   e.g. `score / (age_in_days + 2)^gravity` (Hacker-News-style gravity). Smooth, but the
   division/power is harder to translate to an EF `ORDER BY` and to index.

**v1 recommendation:** ship the weighted score **without** decay (all-time popularity),
ordered `score DESC, PublishedAt DESC`. The `PublishedAt` tie-breaker already nudges fresher
articles ahead among equal-score ties. A freshness window is the cheapest future upgrade and
is documented as an open question in `06-caching-and-rollout.md`. Do not build the decay
factor until there is a clear product signal to justify it.

### Decision

Ship **Option B (weighted score)** with the constants above, ordered by
`score DESC, PublishedAt DESC`, filtered to `Status == Published`, with no recency decay in
v1. The weights are named constants and trivially retunable.

---

## Documents in This Folder

| Doc | Content |
|-----|---------|
| [01-current-state.md](01-current-state.md) | Published-articles slice, the engagement counters, and the GetPopularTags caching pattern — exact file paths |
| [02-endpoint-design.md](02-endpoint-design.md) | The new endpoint: route, query, handler, response, caching, `excludeId` |
| [03-query-and-scoring.md](03-query-and-scoring.md) | `PopularArticlesQueryBuilder`, the SQL `ORDER BY` scoring expression, filter composition |
| [04-frontend-integration.md](04-frontend-integration.md) | Frontend repo method, use case, rewired `useArticleDetailPopular` hook, client regeneration |
| [05-testing.md](05-testing.md) | Unit + integration test plan mirroring existing conventions |
| [06-caching-and-rollout.md](06-caching-and-rollout.md) | Caching + invalidation in depth, indexing, rollout, open questions |
| [specs/00-index.md](specs/00-index.md) | Implementation specs index (full C# snippets + task checklists) |

---

## Execution Order

1. **Scoring constants + query builder** (spec 02): `PopularArticlesScoring`,
   `PopularArticlesQueryBuilder`, `IArticleRepository.GetPopularArticlesAsync`.
2. **Cache invalidator** (spec 03): `IPopularArticlesCacheInvalidator` + implementation +
   DI registration.
3. **Endpoint + query + handler** (spec 01): the vertical slice with caching.
4. **Invalidator wiring** (spec 03): call `Invalidate()` from the engagement mutation
   handlers (like/unlike, comment add/delete, share, bookmark/unbookmark) and publish/archive.
5. **Tests** (spec 04): unit + integration.
6. **Frontend** (spec 05): repo method, use case, hook, `yarn api:generate`.
