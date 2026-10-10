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
