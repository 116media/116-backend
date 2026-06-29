# Implementation Specs — Popular Articles Endpoint

Every file, type, method, and test to add. Full C# snippets with multiline XML docs, backend
conventions exact. Mirrors the `GetPopularTags` slice for endpoint + caching structure.

Do **not** run `dotnet test` / `dotnet build` as part of applying these specs — the user runs
tests themselves.

---

## Spec documents

| Spec | Content |
|------|---------|
| [01-endpoint-query-handler.md](01-endpoint-query-handler.md) | Route segment, query record, validator, meta fields, handler (with caching), response + endpoint, DI |
| [02-query-builder-scoring.md](02-query-builder-scoring.md) | Scoring constants, `IPopularArticlesQueryBuilder` + builder, repository port method + impl |
| [03-cache-invalidator.md](03-cache-invalidator.md) | `IPopularArticlesCacheInvalidator` + implementation, DI registration, invalidation wiring in mutation handlers |
| [04-tests.md](04-tests.md) | Unit + integration tests, mocks, seed helpers |
| [05-frontend-integration.md](05-frontend-integration.md) | Repo port + impl, use case, DI, hook, query key, client regen |

---

## Recommended order

1. Spec 02 — scoring constants, builder, repo method (no dependencies).
2. Spec 03 — invalidator + DI + mutation wiring.
3. Spec 01 — endpoint / query / handler (depends on 02 + 03).
4. Spec 04 — tests.
5. Spec 05 — frontend (after backend Swagger reflects the endpoint).

---

## File inventory (backend)

### New files

| # | File |
|---|------|
| 1 | `src/Modules/Content/Content/Application/Editorial/Constants/PopularArticlesScoring.cs` |
| 2 | `src/Modules/Content/Content/Application/Editorial/Builders/Contracts/IPopularArticlesQueryBuilder.cs` |
| 3 | `src/Modules/Content/Content/Application/Editorial/Builders/PopularArticlesQueryBuilder.cs` |
| 4 | `src/Modules/Content/Content/Application/Shared/Cache/IPopularArticlesCacheInvalidator.cs` |
| 5 | `src/Modules/Content/Content/Infrastructure/Cache/PopularArticlesCacheInvalidator.cs` |
| 6 | `.../UseCases/Public/Queries/GetPopularArticles/PublicGetPopularArticlesQuery.cs` |
| 7 | `.../GetPopularArticles/PublicGetPopularArticlesHandler.cs` |
| 8 | `.../GetPopularArticles/PublicGetPopularArticlesValidator.cs` |
| 9 | `.../GetPopularArticles/PublicGetPopularArticlesMetaField.cs` |
| 10 | `.../GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1.cs` |
| 11 | `tests/Unit/.../GetPopularArticles/PublicGetPopularArticlesHandlerTests.cs` |
| 12 | `tests/Unit/Common/Mocks/Infrastructure/MockPopularArticlesCacheInvalidator.cs` |
| 13 | `tests/Integration/.../GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1Tests.cs` |

(`.../` = `src/Modules/Content/Content/Application/Editorial`.)

### Modified files

| # | File | Change |
|---|------|--------|
| 1 | `src/Modules/Content/Content/Application/Editorial/Constants/EditorialRouteConstants.cs` | add `Popular` segment |
| 2 | `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs` | add `GetPopularArticlesAsync` |
| 3 | `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs` | implement `GetPopularArticlesAsync` |
| 4 | `src/Modules/Content/Content/ContentModule.cs` | register `IPopularArticlesCacheInvalidator` |
| 5–11 | engagement + publish/archive mutation handlers | call `Invalidate()` after commit |
| 12 | `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs` | add `SetupGetPopularArticlesAsync` |
| 13 | `tests/Integration/Common/Base/BaseApiTest.cs` | reset popular-articles cache in `InitializeAsync` |

### Frontend (see spec 05)

Repository port + impl, use case, DI registration, hook, query key, `yarn api:generate`.

---

## Global task checklist

- [x] Add scoring constants (spec 02)
- [x] Add query builder + contract (spec 02)
- [x] Add repo port method + impl (spec 02)
- [x] Add cache invalidator + impl + DI (spec 03)
- [x] Wire `Invalidate()` into engagement + publish/archive handlers (spec 03)
- [x] Add route constant, query, validator, meta field, handler, endpoint (spec 01)
- [x] Add unit + integration tests + mocks + cache reset (spec 04)
- [ ] Frontend repo/use case/hook + `yarn api:generate` (spec 05) — blocked until the
      article-detail page (and its `useArticleDetailPopular` hook) is implemented on the web
      frontend; the endpoint's consumer does not exist yet
