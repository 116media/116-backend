# Alternative — A Dedicated Interaction-State Endpoint

The recommended design (docs 02–03) **embeds** `IsLiked` / `IsBookmarked` on the article
DTOs. This document records the alternative that was considered and rejected for Phase 1: a
separate endpoint that returns only the current user's interaction state for an article.

---

## Shape

```
GET /api/v1/public/articles/{id}/interaction-state
```

Response:

```json
{ "isLiked": true, "isBookmarked": false }
```

Sketch (Carter, mirroring the existing public interaction endpoints):

```csharp
group
    .MapGet(
        "/{id}/interaction-state",
        async (
            string id,
            ClaimsPrincipal user,
            IClaimsProvider claimsProvider,
            IDispatcher dispatcher
        ) =>
        {
            Guid articleId = Guid.Parse(id);
            Guid? userId = null;
            if (user.Identity?.IsAuthenticated == true)
            {
                userId = claimsProvider.GetUserIdFromClaims(user: user);
            }

            var query = new PublicGetArticleInteractionStateQuery(ArticleId: articleId, CurrentUserId: userId);
            PublicGetArticleInteractionStateResult result = await dispatcher.Send(request: query);

            return Results.Ok(new PublicGetArticleInteractionStateResponse(result.IsLiked, result.IsBookmarked));
        }
    )
    .AllowAnonymous()
    .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing);
```

The handler resolves the two flags with the same `HasLikedAsync` / `HasBookmarkedAsync`
existence checks used in Phase 1. Anonymous → `{ false, false }`.

A batch variant would cover the feed:

```
GET /api/v1/public/articles/interaction-state?ids=<id1>,<id2>,...
```

returning `{ "liked": [id1, ...], "bookmarked": [id2, ...] }`, backed by the same
`GetLikedArticleIdsAsync` / `GetBookmarkedArticleIdsAsync` batch methods from Phase 2.

---

## Pros

- **Article payloads stay user-agnostic.** `ArticleDetailDto` / `ArticleSummaryDto` never
  carry per-user data, so any caching of those payloads is trivially correct — the caching
  concern in [07-caching-and-rollout.md](07-caching-and-rollout.md) disappears for the main
  read path.
- **Clean separation.** Interaction state is a distinct, cheaply invalidated resource; the
  client can refetch just this endpoint after a like/unlike without re-reading the article.
- **CDN-friendly.** The article read can be edge-cached aggressively; only the small
  interaction-state call is per-user and uncacheable.

## Cons

- **A second round-trip.** The detail page must fire two requests (article + interaction
  state), and the feed must fire the list request plus a batch interaction-state request.
- **Toggle flicker.** The like/bookmark buttons paint `false` first, then correct themselves
  when the second response lands — the exact first-paint jitter this feature set out to
  remove.
- **More surface area.** A new query, handler, endpoint, response type, meta fields, and
  tests per variant (single + batch), versus a few added fields on existing DTOs.
- **Client orchestration.** The frontend must coordinate two calls, their loading states, and
  their error handling, instead of reading two booleans already present on the entity.

---

## Recommendation

**Embed the flags (Phase 1 / Phase 2). Do not build this endpoint for Phase 1.**

The embedded approach gives correct first paint in a single request, which is the whole point
of closing the gap. The only real cost — caching correctness for the list payloads — is
contained and solved in [07-caching-and-rollout.md](07-caching-and-rollout.md) by keeping the
cached payload user-agnostic and layering flags after the cache read.

Revisit this endpoint only if a future requirement makes the article payloads
heavily-cached/CDN-fronted and the per-user layering becomes a bottleneck. It is a clean
Phase-3 option, not a Phase-1 choice.
