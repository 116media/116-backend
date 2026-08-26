# Migration plan

Two independent tracks. Track A is the caching rewrite and ships without Redis. Track B is the
repository contract work, which is unrelated to caching and stands on its own merits. Neither
changes an endpoint contract, so no client coordination is needed.

## Track A — caching

| # | Change | Files |
| --- | --- | --- |
| A1 | `Microsoft.Extensions.Caching.Hybrid` package reference; `AddHybridCache` in `Program.cs` | 2 edits |
| A2 | `ICacheableRequest` + `IConditionallyCacheableRequest` in `Shared.Contracts` | 1 new |
| A3 | `ContentCacheTags` constants | 1 new |
| A4 | `CachingDecorator<,>` + registration in `CqrsExtension` | 1 new, 1 edit |
| A5 | The four cached queries implement the marker; their handlers drop `IMemoryCache`, the invalidator and the TTL | 8 edits |
| A6 | Three cache event handlers switch to `RemoveByTagAsync` | 3 edits |
| A7 | Delete `ICacheInvalidator`, `CacheInvalidator`, the three markers and two subclasses | 7 deleted |
| A8 | Lookup-table queries opt in; admin lookup mutations evict `ContentCacheTags.Lookups` | ~12 edits |
| A9 | Redis as L2: `AddStackExchangeRedisCache` + compose/CI/fixture | 1 edit + infra |

**A1 version check.** The 9.x line targets `net9.0`; 10.x targets .NET 10. Confirm the current 9.x
release before pinning rather than copying a version out of this doc.

**A5 before A7.** The marker conversion must land before the invalidator types are deleted, or the
four handlers lose their eviction mid-series. One commit per file keeps the tree green throughout.

**A9 is the only step needing infrastructure.** Everything before it runs L1-only and is already a
strict improvement on today: stampede protection and tag eviction arrive immediately, cross-instance
coherence arrives with Redis. That is why this plan no longer has a "wait for Stage 10" gate.

**A8 is new capability, not a refactor.** The lookup tables are read on every request and cached
nowhere today.

### Tests — Track A

- **Unit** — `CachingDecorator` against a real `HybridCache` built from `AddHybridCache` in a
  `ServiceCollection`: a non-cacheable request always reaches the inner handler; a cacheable one
  reaches it once across two calls; `IsCacheable: false` always reaches it.
- **Unit** — every cacheable query's `CacheKey`: two different parameter sets never collide, and the
  same parameters always produce the same string.
- **Unit** — every cached result type round-trips through `System.Text.Json` unchanged. This is the
  guard against someone later caching a type that holds an aggregate.
- **Integration** — the existing `CacheInvalidationRegressionTests` must pass **unchanged**: like an
  article, re-read the feed, assert the fresh count. It proves `RemoveByTagAsync` reaches the same
  entries the eviction token did.
- **Integration, after A9** — two app instances over one Testcontainers Redis: evict on A, assert B
  serves fresh. This is the test that cannot pass today and is the entire point of L2.

## Track B — repository contracts

Specified in [06-repository-contracts.md](06-repository-contracts.md) and specced as **Stage 10
Part D**. Independent of Track A — it addresses 33 repositories duplicating the same members and
hydration drift, not caching — and kept as its own commit series so it can split into a second PR.

| # | Change | Files |
| --- | --- | --- |
| B1 | Replace the empty `IRepository<T>` marker with `IReadRepository` + `IWriteRepository` + their union | 1 edit |
| B2 | Add `RepositoryBase<TContext, TEntity, TId>` | 1 new |
| B3 | Per-module bases: `ContentRepository<TEntity>`, `IdentityRepository<TEntity>`, `CoreRepository<TEntity>` | 3 new |
| B4 | Derive the 33 repositories; delete the inherited members from each | 33 edits |
| B5 | Add `Query()` overrides where an aggregate has a hydration graph | ~8 edits |

**B4 is mechanical but not blind.** Deleting `GetByIdOrThrowAsync` only works where the
specification is a plain id match — `ArticleByIdSpecification` and friends are, but check each.
Where it adds filtering, keep the local implementation.

**B5 changes generated SQL.** It is the one step here that is not behaviour-preserving: finders that
previously loaded a bare aggregate now load its navigations. Land it separately from B4 and read the
Stage 9 split-query notes before touching `ArticleRepository`.

### Tests — Track B

- **Existing integration suite, unchanged.** That is the whole verification for B1–B4: bodies move,
  behaviour does not. A failure means a specification did more than an id match.
- **Integration** — after B5, assert that a finder which previously returned a bare aggregate now has
  its navigation populated, so the hydration guarantee is pinned rather than incidental.

## Sequencing against the audit stages

| Track | Stage | Why |
| --- | --- | --- |
| A1–A8 | Stage 10.1–10.4 — may ship ahead of Redis | No infrastructure dependency, no contract change |
| A9 | Stage 10.5 | Needs Redis in compose, CI and the fixture |
| B1–B5 | Stage 10.12–10.15 (Part D) | Was routed to Stage 15, which never specified it — a planning bug, now corrected |

**Stage 10 now carries this work.** Its Part A was rewritten around this design — A1–A8 here map to
checklist items 10.1–10.4, and A9 maps to 10.5. The audit finding is
[`16-caching-architecture.md`](../architecture-audit/16-caching-architecture.md), which supersedes
the version-key remedy in `[04 §4.8]`.

## Verification checklist

```bash
dotnet build --no-incremental          # 0 errors, 0 warnings
dotnet csharpier check .
dotnet test tests/Unit
dotnet test tests/Integration
```

Three structural assertions, each of which must return nothing:

```bash
grep -rn --include='*.cs' "IMemoryCache" src/Modules/Content/Content/Application
grep -rn --include='*.cs' "CacheInvalidator" src
grep -rn --include='*.cs' "CancellationChangeToken" src
```

## SOLID gates

Acceptance criteria for the tracks above, not commentary.

| Principle | Gate | How to check |
| --- | --- | --- |
| **SRP** | No handler contains caching code | No `UseCases` file references a cache type |
| **OCP** | No handler was edited to *gain* caching | A5's diff removes lines from handlers and adds none |
| **LSP** | Only queries that tolerate staleness implement the marker | Review each `ICacheableRequest` implementer against the not-cached table in [04](04-target-design.md) |
| **ISP** | The marker is implemented only where used | No type implements `ICacheableRequest` without a real `CacheKey` |
| **DIP** | Application references only the cache *abstraction* | `grep -rn "Caching.Memory\|Caching.Distributed\|MemoryCacheEntryOptions\|CancellationChangeToken" src/Modules/*/*/Application` returns nothing. `Microsoft.Extensions.Caching.Hybrid` is permitted: `HybridCache` is an abstract class in `Microsoft.Extensions.Caching.Abstractions`, and the event handlers legitimately name it. |

The DIP gate is the one that fails on the code in the tree today. Note what it permits: the three
cache event handlers live in `Application/Shared/EventHandlers/` and inject `HybridCache` to call
`RemoveByTagAsync`. That is an abstraction — the same category `IMemoryCache` occupies — so the gate
bans the *store-specific* types (`MemoryCacheEntryOptions`, `CancellationChangeToken`,
`IDistributedCache`) rather than the word "cache".
