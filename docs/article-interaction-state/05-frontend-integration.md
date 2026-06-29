# Frontend Integration

How `apps/frontend` consumes the new per-user flags once the backend ships them. This closes
frontend open question **Q2** in `apps/frontend/docs/article-detail/19-open-questions.md` and
removes the "No per-user initial state" caveat in
`apps/frontend/docs/article-detail/09-interactions.md`.

> The article-detail feature is still in design on the frontend (its docs describe the intended
> `IArticleDetailEntity`; the interface is not yet in the codebase). The steps below apply
> when that entity lands, and immediately to the already-shipped `IArticleSummaryEntity`.

---

## 1. Regenerate the API client

The frontend consumes the backend via a generated client (the DTO types
`ArticleDetailDto` / `ArticleSummaryDto` are generated). After the backend adds `isLiked` /
`isBookmarked`:

1. Regenerate the API client so the generated `ArticleDetailDto` and `ArticleSummaryDto` gain
   the two optional `isLiked?: boolean` / `isBookmarked?: boolean` fields.
2. The fields are optional in TypeScript (they default to `false` server-side), so existing
   mapper code keeps compiling; the mappers below then read them with a `?? false` fallback.

---

## 2. Domain entities gain the flags

### `IArticleSummaryEntity`

**File:** `src/modules/articles/domain/entities/IArticleSummaryEntity.ts`

Add two properties beside the existing counters:

```ts
/**
 * @property {boolean} isLiked - Whether the current user has liked this article. False when anonymous.
 * @property {boolean} isBookmarked - Whether the current user has bookmarked this article. False when anonymous.
 */
export interface IArticleSummaryEntity {
    // ... existing properties ...
    isLiked: boolean;
    isBookmarked: boolean;
}
```

### `IArticleDetailEntity`

When the detail entity is introduced (per `apps/frontend/docs/article-detail/13-domain-entities-and-mappers.md`),
give it the same two properties. The detail-page notes already anticipate this — Q2's note in
`19-open-questions.md` says: *"If Q2 is answered, `useArticleDetail` seeds the toggles from
the entity; the engagement components already read the flags."*

---

## 3. Mappers read the flags

**File:** `src/modules/articles/infrastructure/mappers/articles.mapper.ts`

`articleSummaryFromDto` already maps the counters with a nullish fallback; add the two flags
the same way:

```ts
articleSummaryFromDto(dto: ArticleSummaryDto): IArticleSummaryEntity {
    return {
        // ... existing fields ...
        likeCount: dto.likeCount ?? 0,
        bookmarkCount: dto.bookmarkCount ?? 0,
        isLiked: dto.isLiked ?? false,
        isBookmarked: dto.isBookmarked ?? false,
    };
}
```

Because `articleSummaryFromDto` is reused by `articlePageFromDto` and `promotionFeedFromDto`,
the feed grid, mega-menu cards, and promotion feed all pick up the flags with this one change.
The detail mapper reads them identically when it lands.

---

## 4. Seed the toggle hooks from the flags

**Files:**
- `src/modules/articles/presentation/hooks/useToggleArticleLike.ts`
- `src/modules/articles/presentation/hooks/useToggleArticleBookmark.ts`

Today both hooks own the boolean client-side and start it at `false`
(`09-interactions.md` §"No per-user initial state"). The underlying `useToggle` takes an
initial count but not an initial `on` state:

```ts
export function useToggleArticleLike(articleId: string, initialCount: number) {
    const { on, count, toggle } = useToggle(initialCount, /* runInteraction */);
    return { liked: on, count, toggle };
}
```

Thread an `initialLiked` / `initialBookmarked` seed through so the toggle starts from the
server truth:

```ts
export function useToggleArticleLike(
    articleId: string,
    initialCount: number,
    initialLiked: boolean = false,
) {
    const { on, count, toggle } = useToggle(initialCount, /* runInteraction */, initialLiked);
    return { liked: on, count, toggle };
}
```

`useToggle` gains a corresponding optional `initialOn` argument used as its initial boolean
state. Call sites pass the entity flag:

```tsx
const like = useToggleArticleLike(article.id, article.likeCount, article.isLiked);
const bookmark = useToggleArticleBookmark(article.id, article.bookmarkCount, article.isBookmarked);
```

Now the first paint shows the filled/unfilled state correctly; the optimistic toggle and the
`409 Conflict` reconciliation remain as-is for the interaction itself.

---

## 5. Behavior after the change

| Reader | First paint (before) | First paint (after) |
|--------|----------------------|---------------------|
| Anonymous | unfilled (guess, correct by luck) | unfilled (server says `false`) |
| Signed-in, has liked | unfilled (**wrong**), corrects on `409` | filled (**correct**) |
| Signed-in, not liked | unfilled (correct) | unfilled (server says `false`) |

The auth gating, optimistic ±1 count delta, and rollback-on-failure described in
`09-interactions.md` are unchanged — only the **initial** state is now seeded from the entity
instead of hardcoded `false`.

---

## 6. Cross-references

- `apps/frontend/docs/article-detail/09-interactions.md` — the "No per-user initial state"
  section is the limitation this removes; the reused hooks are the seams touched above.
- `apps/frontend/docs/article-detail/19-open-questions.md` — Q2 (and its "Deferred to a later
  phase → Per-user initial interaction state" bullet) is resolved by this integration.
