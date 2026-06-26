# Test-Data Spec 02 — Typed request builders

## Problem
Endpoint tests build request bodies as anonymous objects with hardcoded values,
duplicating "what a valid request looks like" across hundreds of tests and
drifting from the validators.

## Before
```csharp
var slug = $"test-article-{Guid.NewGuid():N}"[..13];
var request = new
{
    CategoryId = category.Id,
    Title = "Test Article Title",
    Slug = slug,
};
var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Articles, request);
```

## After
Typed request builders in `tests/Fixtures/Requests/` (one per command),
valid-by-default, with `with`-overrides for the field under test:

```csharp
public sealed record CreateArticleRequest(Guid CategoryId, string Title, string Slug);

public static class CreateArticleRequestBuilder
{
    private static readonly Faker Faker = new();

    public static CreateArticleRequest Valid(Guid? categoryId = null) => new(
        CategoryId: categoryId ?? Guid.NewGuid(),
        Title: Faker.Commerce.ProductName(),
        Slug: $"{Faker.Lorem.Slug()}-{Guid.NewGuid():N}"[..Math.Min(...)]);
}
```

```csharp
// happy path
var request = CreateArticleRequestBuilder.Valid(category.Id);
// validation path
var bad = CreateArticleRequestBuilder.Valid(category.Id) with { Title = "" };
```

Tests then assert the **echoed** fields against the same `request` instance
(see assertions specs).

## TODO checklist
- [ ] Create `tests/Fixtures/Requests/` with builders for each create/update command
      (start with the most-used: Article, Video, ShortVideo, Lyrics, Category,
      Customer, Package, Order, ContentType, PricingTier, PromotionLevel, Tag,
      Role, Permission, Playlist, Comment, profile/password commands).
- [ ] Replace anonymous-object payloads in endpoint tests with these builders
      (done incrementally alongside the per-module assertion specs).

## Acceptance
- No `var request = new { ... }` anonymous payloads remain in endpoint tests
  (sweep: `grep -rn 'new {' tests/Integration | grep -i request`).
- Validation tests construct the invalid case via `with { … }` from a valid base.
