# Article Interaction State — Overview

Article DTOs currently expose only **global** interaction counters — `LikeCount`,
`BookmarkCount`, `CommentCount`, `ShareCount`. They carry **no per-user interaction
state**: `ArticleDetailDto` and `ArticleSummaryDto` have no `IsLiked` / `IsBookmarked`
flag telling the signed-in reader whether *they* have already liked or bookmarked the
article.

As a result, when an authenticated reader opens an article (or scrolls the feed), the
frontend cannot render the correct filled/unfilled like and bookmark state. It currently
seeds both toggles to `false` and relies on optimistic updates, reconciling a wrong guess
only when the backend returns `409 Conflict` on a redundant like/bookmark. This is a known
limitation tracked on the frontend side in
`apps/frontend/docs/article-detail/19-open-questions.md` (Q2) and
`apps/frontend/docs/article-detail/09-interactions.md`.

---

## Goal

Return accurate per-user interaction state for the current user on public article reads, so
the frontend can paint the correct like/bookmark toggle on first render.

- **Anonymous readers** have no user id → `IsLiked` and `IsBookmarked` are always `false`.
- **Authenticated readers** get the true value, resolved from the `article_likes` /
  `article_bookmarks` join tables.

---

## Scope

| Phase | Surface | DTO | Mechanism |
|-------|---------|-----|-----------|
| **Phase 1** | Single-article page (`GET /api/v1/public/articles/{slug}`) | `ArticleDetailDto` | Two existence checks per request |
| **Phase 2** | Feed / promoted / promotion-feed lists | `ArticleSummaryDto` | One **batch** lookup per request (avoid N+1) |

Phase 1 is small and self-contained; ship it first. Phase 2 touches every list handler and
introduces the caching correctness concern described below, so it is separated deliberately.

---

## Decisions

| # | Decision |
|---|----------|
| D1 | **Embed the flags on the DTO** (`bool IsLiked`, `bool IsBookmarked`) rather than expose a separate interaction-state endpoint. One round-trip, no client orchestration. The alternative endpoint is documented in [04-alternative-endpoint.md](04-alternative-endpoint.md) but not recommended. |
| D2 | **Optional current user, resolved at the endpoint.** The get-by-slug and feed endpoints are `.AllowAnonymous()`. Follow the existing optional-auth pattern (`PublicShareVideoEndpointV1`): read the `ClaimsPrincipal`, resolve `Guid? userId` when authenticated, and pass it into the query record. Anonymous → `null` → both flags `false`. See [01-current-state.md](01-current-state.md) §3. |
| D3 | **Phase 2 uses a batch lookup**, not per-item existence checks. Add `GetLikedArticleIdsAsync(userId, articleIds)` / `GetBookmarkedArticleIdsAsync(...)` returning the subset of ids the user has interacted with — one query each, regardless of page size. |
| D4 | **Per-user flags must never be served from a shared cache.** No article read handler is cached today, but the promoted/promotion-feed lists are cache candidates. The cached payload must stay **user-agnostic** (`IsLiked = IsBookmarked = false`); per-user flags are layered on **after** the cache read. See [07-caching-and-rollout.md](07-caching-and-rollout.md). |
| D5 | **No schema migration.** The join tables (`content.article_likes`, `content.article_bookmarks`), their unique `(user_id, article_id)` indexes, and the denormalized counters on `ArticleEntity` already exist. This change is read-path only — new repository query methods, DTO fields, and handler wiring. No EF migration. |

---

## Alternative designs considered

1. **Embed flags on the DTO (recommended, D1).** Accurate first paint in a single request.
   Cost: the flag values are per-user, which complicates any future caching of these
   payloads (addressed in Phase 2 / [07](07-caching-and-rollout.md)).
2. **Dedicated interaction-state endpoint** — `GET /api/v1/public/articles/{id}/interaction-state`
   returning `{ isLiked, isBookmarked }`. Keeps the article payload user-agnostic and
   trivially cacheable, but adds a second round-trip and a brief flicker before the toggle
   settles. Documented fully in [04-alternative-endpoint.md](04-alternative-endpoint.md);
   not chosen for Phase 1.
3. **A batch "my interactions" endpoint** for a set of article ids (feed variant of #2).
   Same trade-off as #2 at list scale; the embedded batch lookup in Phase 2 achieves the
   same efficiency without a second request.

---

## Documents in this folder

| Doc | Content |
|-----|---------|
| [00-overview.md](00-overview.md) | This document |
| [01-current-state.md](01-current-state.md) | How likes/bookmarks are stored, counted, and mapped today; the current-user accessor; exact file paths |
| [02-detail-interaction-state.md](02-detail-interaction-state.md) | Phase 1 — `IsLiked`/`IsBookmarked` on `ArticleDetailDto` for the single-article page |
| [03-feed-interaction-state.md](03-feed-interaction-state.md) | Phase 2 — the flags on `ArticleSummaryDto` via a batch lookup, across feed / promoted / promotion-feed |
| [04-alternative-endpoint.md](04-alternative-endpoint.md) | The dedicated interaction-state endpoint alternative, pros/cons |
| [05-frontend-integration.md](05-frontend-integration.md) | How `apps/frontend` consumes the new flags |
| [06-testing.md](06-testing.md) | Unit and integration test plan |
| [07-caching-and-rollout.md](07-caching-and-rollout.md) | The caching correctness concern in depth, rollout order, open questions |
| [specs/00-index.md](specs/00-index.md) | Implementation specs index |
| [specs/01-detail-flags.md](specs/01-detail-flags.md) | Phase 1 — DTO + handler + repo methods, with `## Tasks` checklist |
| [specs/02-feed-flags-batch.md](specs/02-feed-flags-batch.md) | Phase 2 — batch lookup + caching, with `## Tasks` checklist |
| [specs/03-tests.md](specs/03-tests.md) | Test specs, with `## Tasks` checklist |

---

## Execution order

1. **Phase 1** (docs 02, specs 01): add the two flags to `ArticleDetailDto`, two repository
   existence methods, wire the get-by-slug endpoint + handler to resolve the optional user.
2. **Phase 2** (docs 03, specs 02): add the flags to `ArticleSummaryDto`, two batch repository
   methods, wire the feed / promoted / promotion-feed handlers, address caching.
3. **Tests** (doc 06, specs 03): unit + integration for both phases.
4. **Frontend** (doc 05): regenerate the API client, seed the toggle hooks from the flags.
