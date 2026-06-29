# Frontend Integration

How `apps/frontend` consumes each phase. The frontend article-detail comments design
already anticipates this projection — see
`apps/frontend/docs/article-detail/12-comments.md` and
`apps/frontend/docs/article-detail/19-open-questions.md` (the backend gap is tracked there),
with the API shape in `apps/frontend/docs/article-detail/03-backend-api-reference.md` §2.

---

## Phase 1 — author fills the existing UI slot

The comment row (`ArticleDetailComment`) is already designed around
`IArticleCommentEntity.author?` — an `IArticleAuthor`-shaped projection
(`{ userName, avatarUrl }`, the same shape the article author already exposes). Today the
row **degrades gracefully** because the DTO has no author: it renders a neutral avatar
derived from a `userId` prefix and a generic user reference
(`apps/frontend/docs/article-detail/12-comments.md` §"Resolving the author gap").

Once Phase 1 ships:

1. **Regenerate the OpenAPI client.** The backend now emits
   `ArticleCommentDto.author` (nullable), so the generated `ArticleCommentDto` type gains
   `author?: AuthorDto`. Run the frontend's client-generation step against the updated
   backend OpenAPI document.
2. **Populate `author` in `articleCommentFromDto`.** The DTO→entity mapper maps
   `dto.author` into `IArticleCommentEntity.author` (mapping `AuthorDto { userName,
   avatarUrl }` to the `IArticleAuthor` projection). When `dto.author` is null (deleted
   comment or unresolved user), `author` stays undefined and the row keeps its existing
   fallback.
3. **Remove the interim neutral-avatar fallback as the primary path.** The `userId`-prefix
   neutral avatar documented in `12-comments.md` becomes the *fallback only* — real
   `userName` + `avatarUrl` is now the normal state. The graceful-degradation branch stays
   as a safety net for null-author rows (deleted comments, unresolved users) and for the
   optimistic "freshly posted" row, which continues to stamp the current user from
   `useAuth()`.
4. **No layout change.** As `12-comments.md` notes, the row's structure does not change —
   only the *source* of `displayName`/`avatarUrl` shifts from the `userId` fallback to the
   resolved `author`.

Update `apps/frontend/docs/article-detail/12-comments.md` §"Resolving the author gap" to
mark the projection as shipped, and close the corresponding item in
`apps/frontend/docs/article-detail/19-open-questions.md`.

### Privacy on the client

`ArticleCommentDto.author` never contains an email on the public endpoint (the backend
projects `Email = null` — see [02-comment-author-projection.md](02-comment-author-projection.md)
§Privacy). The frontend must not assume an email is present for comment authors.

---

## Phase 2 — threading

When replies ship, the frontend extends the comment section:

- `IArticleCommentEntity` gains `parentCommentId?`, `replyCount`, and optional `replies`.
- The list renders top-level rows; each row with `replyCount > 0` shows a "view N replies"
  affordance that calls the replies endpoint
  (`GET /api/v1/public/articles/comments/{commentId}/replies`, paged) via a new
  `useArticleCommentReplies(commentId)` infinite query, following the same key/hook pattern
  as `useArticleComments` in `apps/frontend/docs/article-detail/specs/03-hooks-and-keys.md`.
- The composer gains a reply mode that posts to
  `POST /api/v1/public/articles/{id}/comments/{commentId}/replies`; author resolution is
  identical to top-level comments (Phase 1 batch projection), so reply rows render authors
  the same way with no extra frontend work.

---

## Phase 3 — likes

When per-comment likes ship:

- `IArticleCommentEntity` gains `likeCount` and `isLiked`.
- A like button on each row toggles via `useToggleCommentLike(commentId)`, mirroring the
  article like mutation with optimistic count bump + `isLiked` flip and rollback on error,
  gated by `useRequireAuth` exactly like the composer and the feed-card engagement controls
  described in `apps/frontend/docs/article-detail/09-interactions.md` and
  `12-comments.md`.
- `isLiked` is viewer-specific and `false` for anonymous users, matching the backend
  contract in [04-comment-likes.md](04-comment-likes.md).

---

## Client regeneration checklist (each phase)

- [ ] Rebuild the backend so the OpenAPI document reflects the new `ArticleCommentDto` shape.
- [ ] Regenerate the frontend API client.
- [ ] Update `articleCommentFromDto` to map the new fields into `IArticleCommentEntity`.
- [ ] Update the affected `apps/frontend/docs/article-detail/*` docs (12, 19, specs) to reflect shipped state.
