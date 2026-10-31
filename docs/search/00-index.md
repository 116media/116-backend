# Site Search — Backend Spec Index

Implementation specs for `GET /api/v1/public/search`.

**Read [`../../../../docs/search/`](../../../../docs/search/README.md) first** — the
monorepo docs carry the product requirement, the API contract and the ranking design.
These specs are the backend implementation of that contract and do not restate it.

---

## Specs

| File | What it covers |
| --- | --- |
| [01-database-and-tsvector.md](01-database-and-tsvector.md) | The `immutable_unaccent` wrapper, generated `search_vector` columns, GIN indexes, raw SQL |
| [02-ef-configuration-and-migration.md](02-ef-configuration-and-migration.md) | EF mapping for a generated column, the migration, expected `Up()` / `Down()` |
| [03-contracts-and-dtos.md](03-contracts-and-dtos.md) | `EnumSearchScope`, `SearchHitDto`, `SearchCountsDto`, the result record |
| [04-repository-and-queries.md](04-repository-and-queries.md) | `ISearchRepository`, the scored queries per entity, the count query |
| [05-use-case-and-endpoint.md](05-use-case-and-endpoint.md) | Query, validator, handler, MetaField, Endpoint V1 |
| [06-logging-and-caching.md](06-logging-and-caching.md) | Why the response is not cached, and keeping `q` out of request logs |
| [07-tests.md](07-tests.md) | Unit tests for the validator and handler, integration tests through real HTTP |

---

## Architecture

Search reads **one projection table**, `content.search_documents`, holding one row per
searchable item across every type. That makes a page one query and the per-scope counts one
`GROUP BY`, whatever types are registered, instead of one query per type plus a merge in
application memory.

```
PublicSearchHandler            names no content type
   └─ ISearchCatalog           one page, one counts call
        └─ CachedSearchCatalog  caches only the counts
             └─ SearchDocumentCatalog  one table, two queries

ISearchDocumentProjector       the extension point: one per type
   ├─ ArticleSearchProjector
   ├─ VideoSearchProjector
   └─ ShortVideoSearchProjector
        ↑ driven by the domain events, routed through ISearchProjection
```

The source tables stay the record; the projection is derived and rebuildable.

---

## Searchable types

| Type | Scope | Visible when | Searched text | Projected on | Status |
| --- | --- | --- | --- | --- | --- |
| Article | `Articles` | `status = 'Published'` | title (A), headline (B), meta (C), body (D) | published / unpublished / deleted | shipped |
| Video | `Videos` | `status = 'Published'` | title (A), description (B), meta (C) | published / unpublished / deleted | shipped |
| Short | `Shorts` | `is_active` | title (A) | changed / deleted | shipped |
| Lyrics | — | — | — | — | **planned** |
| Artist | — | — | — | — | **planned** |

Adding a type is a projector, a DI line, an `EnumSearchScope` member and a projection
handler for its events. The table, the vector, the catalog, the cache, the handler and the
endpoint are untouched.

Two things to know before adding lyrics and artists, both established by reading the
domain rather than assumed:

- **Articles and videos freeze at publish.** `EnsureEditable` permits only `Draft`,
  `PendingPayment`, `PendingReview` and `Rejected`, so their searchable text cannot change
  once visible. Publish, unpublish and delete are the whole of what the projection needs.
- **Shorts, artists and lyrics stay mutable for life.** Shorts already raise
  `ShortVideoChangedEvent` on every revision. Artists raise `ArtistChangedEvent` from
  `ReviseProfile` and `SetAvatarFileId` but **not** from `Rename`. Lyrics have a full
  publication state machine but no `EnsureEditable` guard and **no change event at all** —
  both gaps have to close before either type can be projected reliably.

---

## Drift

A projection can fall out of step with its sources: a handler that throws leaves a document
stale or missing while the entity looks correct everywhere else. `ISearchProjection.RebuildAllAsync`
reprojects every type and is covered by an integration test that deletes the documents and
restores them. **Not yet built: an operator-facing trigger for it and a periodic
reconciliation that compares per-type counts and alerts.** Until those exist, drift is
recoverable but not detectable.

---

## Implementation order

1. Database objects and migration (specs 01, 02)
2. Contracts and DTOs (spec 03)
3. Repository (spec 04)
4. Use case, validator, endpoint (spec 05)
5. Logging and caching (spec 06)
6. Tests (spec 07)

---

## Where the code lives

The feature belongs to the **Content** module, Editorial area, because every searchable
entity is a Content aggregate:

```text
src/modules/Content/Content/src/
├── Content.Application/
│   ├── Editorial/
│   │   └── UseCases/Public/Queries/Search/
│   │       ├── PublicSearchQuery.cs
│   │       ├── PublicSearchValidator.cs
│   │       ├── PublicSearchHandler.cs
│   │       ├── PublicSearchMetaField.cs
│   │       └── V1/PublicSearchEndpointV1.cs
│   └── Shared/
│       ├── DTOs/SearchHitDto.cs
│       └── Repositories/ISearchRepository.cs
├── Content.Domain/Enums/EnumSearchScope.cs
└── Content.Infrastructure/
    ├── Persistence/Migrations/…AddSearchVectors.cs
    └── Repositories/SearchRepository.cs
```

A dedicated `Search` area was considered and rejected: it would own no entities and
would need a repository reaching into Editorial's tables anyway.

---

## Conventions these specs follow

- Use-case files and types are `Public`-prefixed, per `CLAUDE.md`.
- XML docs are multiline block form, 2–3 lines, no rationale (that lives here).
- Handlers never throw inline; i18n factories or repository guards own the throw.
- Tests follow `docs/testing/00-unit-vs-integration-rules.md`: integration reaches the
  endpoint through real HTTP or a real repository from DI, never by constructing a handler.
