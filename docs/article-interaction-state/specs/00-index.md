# Implementation Specs — Article Interaction State

Executable specs: every file, property, method, and test to add or change, with full C#
(multiline XML docs, backend conventions exact) and per-spec `## Tasks` `- [ ]` checklists.

| Spec | Scope |
|------|-------|
| [01-detail-flags.md](01-detail-flags.md) | Phase 1 — `IsLiked`/`IsBookmarked` on `ArticleDetailDto`; get-by-slug query/handler/endpoint; reuse existing existence repo methods |
| [02-feed-flags-batch.md](02-feed-flags-batch.md) | Phase 2 — flags on `ArticleSummaryDto`; batch repo methods; batch mapper overload; feed/promoted/promotion-feed handlers + endpoints; caching correctness |
| [03-tests.md](03-tests.md) | Unit + integration tests for both phases; mock helper additions |

## Conventions (all specs)

- Use-case files/types are `Public`-prefixed; the folder name is not.
- One class per file (record + its handler/endpoint co-located per the existing slice layout).
- Multiline XML doc comments on every added member; `/// <inheritdoc />` on interface impls.
- New optional DTO/record parameters go **last** and default to `false` / `null` so existing
  positional constructions keep compiling.
- No EF migration — the join tables, indexes, and counters already exist.
- Do not run `dotnet build` / `dotnet test` as part of implementing these; the user runs tests.

## Paths (relative to `apps/backend`)

| Thing | Path |
|-------|------|
| Detail DTO | `src/Modules/Content/Content/Application/Shared/DTOs/ArticleDetailDto.cs` |
| Summary DTO | `src/Modules/Content/Content/Application/Shared/DTOs/ArticleSummaryDto.cs` |
| Mapper | `src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs` |
| Repo interface | `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs` |
| Repo impl | `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs` |
| Get-by-slug slice | `src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetArticleBySlug/` |
| Published feed slice | `.../GetPublishedArticles/` |
| Promoted slice | `.../GetPromotedArticles/` |
| Promotion feed slice | `.../GetArticlePromotionFeed/` |
| Current-user resolver | `src/Modules/Identity/Identity.Contracts/Application/IClaimsProvider.cs` |
