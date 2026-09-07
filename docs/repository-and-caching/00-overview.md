# Repository & Caching Architecture

A study of how repositories and caching are structured in two reference codebases, what each gets
right, what each gets wrong, and the design that replaces the current caching in this backend.

## The question

Caching currently lives inside four query handlers, keyed by hand and evicted through a
process-local `CancellationTokenSource`. Where should it live instead?

**Answer: a Decorator over the CQRS handler — the fourth in a chain this codebase already has —
backed by `HybridCache`.** Both are established: one is a GoF pattern already applied three times in
`CqrsExtension`, the other is first-party .NET. Nothing about the caching layer is hand-written.

## The decision rule

```text
Can this query's result be stale for its TTL, and does it round-trip through JSON?
├─ YES → the query implements ICacheableRequest. Nothing else changes.
└─ NO  → it does not. Nothing else changes.
```

Per-user results, free-text search, and anything holding an aggregate answer NO. There is no second
mechanism and no memory-vs-distributed decision to make per query: `HybridCache` is L1 and L2 at
once.

## Verdict table

| | `dotnet-microservices` | `EShopModularMonoliths` | Target for this backend |
| --- | --- | --- | --- |
| Composition | inheritance (`CachedRepository<,,>` base) | Scrutor `Decorate` on the repository | **Scrutor `Decorate` on the CQRS handler** — the chain already exists |
| Cache store | `IMemoryCache` | `IDistributedCache` | **`HybridCache`** — L1 + L2 behind one API |
| Cached value | whole entity table | single aggregate | **the query result record** — never an aggregate |
| Key | `typeof(T).FullName` | natural key (`UserName`) | declared by the query itself |
| Invalidation | none (process lifetime) | write-through in the repository | **`RemoveByTagAsync`**, from the existing post-commit event handlers |
| Stampede protection | none | none | built in |

## Why not a repository decorator

`EShopModularMonoliths` decorates `IBasketRepository` directly, and that is the right shape *there*:
`ShoppingCartEntity` is two levels deep with a natural key, and the repository owns `SaveChangesAsync`
so it observes every commit.

Neither holds here. `IArticleRepository` returns `ArticleEntity` — a `Category` navigation to another
aggregate, `Images` and `Tags` join collections, domain events on `Aggregate<Guid>` — so caching it
means serializing an aggregate, which is what forced EShop into two hand-written `JsonConverter`s and
reflection into a private field. And commit lives in `IContentUnitOfWork`, so a repository decorator
never learns whether the transaction succeeded.

Decorating the **query handler** avoids both: the cached value is the result record the endpoint
already serializes, and invalidation stays where it is correct — the post-commit domain event
handlers this codebase already has, which is the one thing neither reference project does.

## Contents

| Doc | What it covers |
| --- | --- |
| [01-reference-microservices.md](01-reference-microservices.md) | Repository layering and the `ICacheable` inheritance model; what to take, what to reject |
| [02-reference-eshop.md](02-reference-eshop.md) | Scrutor decoration, aggregate serialization, write-through eviction |
| [03-current-state.md](03-current-state.md) | What this backend does today and the concrete problems with it |
| [04-target-design.md](04-target-design.md) | The design: `ICacheableRequest`, `CachingDecorator`, `HybridCache`, tag invalidation — with complete code |
| [05-migration-plan.md](05-migration-plan.md) | Ordered steps, tests, SOLID gates, and what belongs in which stage |
| [06-repository-contracts.md](06-repository-contracts.md) | Unrelated to caching: the generic repository contract and the `Query()` hydration seam, adopted from `dotnet-microservices` |
