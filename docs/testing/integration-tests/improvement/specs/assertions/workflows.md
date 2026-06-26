# Assertions — Workflows (end-to-end)

The 4 e2e flows already chain real endpoints; upgrade them to typed body
assertions, route helpers, and cross-step DB verification.

## Guidance
- Deserialize each step's response into its real DTO; carry returned IDs forward
  (don't re-query just to get an id you were already returned).
- Assert the **final** persisted state in the DB at the end of the flow.
- Replace hardcoded interaction/action segments with `Routes.*`.

## After (InteractionFlow excerpt)
```csharp
var like = await Client.PostAsync(Routes.Public.Articles.Likes(articleId), null);
like.StatusCode.Should().Be(HttpStatusCode.Created);
await using var db = CreateDbContext<ContentDbContext>();
(await db.ArticleLikes.CountAsync(l => l.ArticleId == articleId)).Should().Be(1);
```

## TODO checklist
- [ ] AuthenticationFlowTests.cs — assert tokens/user on login; duplicate-email & bad-creds via `ShouldBeProblem`.
- [ ] ContentPublicationFlowTests.cs — assert published content is publicly visible (typed) and draft is not (NotFound problem); verify DB status.
- [ ] InteractionFlowTests.cs — assert each interaction's DB side-effect; use `Routes.*` for likes/bookmarks/comments/ratings/shares.
- [ ] OrderLifecycleTests.cs — assert each status transition in the DB and the final totals.

## Acceptance
- Each flow asserts typed bodies + final DB state; no literal route segments.
