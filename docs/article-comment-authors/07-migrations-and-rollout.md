# Migrations and Rollout

---

## Phase 1 — no migration, no backfill

The comment author is **resolved at read time** from the Identity module, never stored on
the comment row. Therefore Phase 1:

- adds **no column** to `article_comments`,
- needs **no EF migration**,
- needs **no backfill**.

The only persistence-adjacent change is the read query inside `UserLookupService`
(`WHERE id IN (...)`), which is a query change, not a schema change.

---

## Phase 2 — threading migration

**Module:** Content (`ContentDbContext`).

```bash
dotnet ef migrations add AddArticleCommentParent \
  --project src/Modules/Content/Content.Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

Schema effect:
- Add `parent_comment_id uuid NULL` to `content.article_comments`.
- Add FK `article_comments.parent_comment_id → article_comments.id` (`ON DELETE RESTRICT`).
- Add index `ix_article_comments_parent`.

Backfill: none. Existing rows get `parent_comment_id = NULL` and are correctly treated as
top-level comments.

> Adjust the `--project` path to the actual Content infrastructure project name if it
> differs; the migration commands in `apps/backend/CLAUDE.md` show the Identity/Core
> equivalents, and the file-entity-migration doc set contains a Content-module migration
> precedent.

---

## Phase 3 — likes migration

**Module:** Content (`ContentDbContext`).

```bash
dotnet ef migrations add AddArticleCommentLikes \
  --project src/Modules/Content/Content.Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

Schema effect:
- Create `content.article_comment_likes` (`id`, `user_id`, `comment_id`, auditable cols),
  FK `comment_id → article_comments.id` (`ON DELETE CASCADE`), unique index
  `ix_article_comment_likes_comment_user (comment_id, user_id)`.
- Add `like_count int NOT NULL DEFAULT 0` to `content.article_comments`.

Backfill: none required at introduction (no likes exist yet, all counts start at 0). If
likes are ever seeded/imported, reconcile `like_count` from the join table with a one-off
`UPDATE ... SET like_count = (SELECT COUNT(*) ...)`.

---

## Rollout order

1. **Phase 1 first, independently.** It is additive (nullable `author` field), unblocks the
   frontend, and carries no schema risk. Ship: backend change → regenerate frontend client
   → frontend maps `author`.
2. **Phase 2** only after Phase 1 is live: migration → entity/DTO/query → replies endpoint →
   frontend replies UI.
3. **Phase 3** last: migration → join entity + counts → like endpoints → frontend like
   button.

Each phase is deployable on its own; none blocks the previous one's release.

---

## Open questions and decisions

1. **Denormalize author onto the comment row vs resolve at read time?**
   **Decision: resolve at read time**, reusing the article resolver (`IUserLookupService`).
   Denormalizing `userName`/`avatarUrl`/`role` onto `article_comments` would need a
   migration, a backfill of every historical comment, and ongoing sync whenever a user
   renames or changes avatar (stale copies otherwise). Resolve-at-read keeps a single source
   of truth, matches how articles already work, and makes Phase 1 migration-free. The batch
   lookup keeps it O(1) queries per page. Revisit only if profiling shows the cross-module
   lookup is a hotspot; a read model / cache would be the escalation, not denormalized
   columns.

2. **Should the public comments endpoint expose the commenter's email?**
   **Decision: never.** The handler projects `AuthorDto.Email = null` on the public
   endpoint, asserted by an integration test. Only admin article reads surface email.

3. **Include `Role` in the public comment author?**
   **Decision: yes, for now** — it is non-sensitive and lets the UI badge staff/author
   comments. Drop it if product decides otherwise; it is a one-line change in the handler's
   `AuthorDto` construction.

4. **Threading depth?** **Decision: single level** (comment → replies). Deeper trees add
   pagination and rendering complexity without clear product need; revisit if demanded.

5. **Reply and like counting toward `article.CommentCount`?** Replies **do** count as
   comments (they increment `CommentCount`, matching the current add flow). Likes do **not**
   touch `CommentCount`; they maintain the comment's own `LikeCount`.

6. **Batch avatar resolution on `IFileRepository`.** Phase 1 prefers a batch
   `GetStorageUrlsByIdsAsync`. If that method is deferred, the handler may loop
   `GetByIdAsync` over the de-duplicated avatar file IDs — small N in practice, but the
   batch method is the intended end state.
