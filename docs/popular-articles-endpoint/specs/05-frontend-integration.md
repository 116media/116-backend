# Spec 05 — Frontend Integration

Apply after the backend endpoint is live and Swagger reflects it. Layering:
generated client → repository → use case → hook. All new code mirrors the existing
`getPromotedArticles` / `getPublishedArticles` siblings.

Reference: `apps/frontend/docs/article-detail/11-popular-articles-sidebar.md`,
`apps/frontend/docs/article-detail/19-open-questions.md` (Q3 / D4 — this endpoint is the "real
popularity-sorted endpoint" those docs anticipate).

---

## 1. Regenerate the API client

```
yarn api:generate
```

(swagger-typescript-api against `http://localhost:5025/swagger/v1/swagger.json`, output
`src/shared/infrastructure/api/generated/116.api.ts`, then `scripts/format-api-docs.js`.)

This adds `getPopularArticles(query, params)` and a `PublicGetPopularArticlesResponse` type to
the generated client. Do not hand-edit generated files.

---

## 2. Repository port

**File:** `src/modules/articles/application/repositories/articles.repository.port.ts`

Add the query interface and method (mirror `IPublishedArticlesQuery` /
`getPublishedArticles`):

```typescript
export interface IPopularArticlesQuery {
    limit?: number;
    categoryId?: string;
    excludeId?: string;
}
```

```typescript
/**
 * Fetches the most popular published articles, ranked by engagement.
 *
 * @param query - Limit plus optional category and exclude-id filters.
 * @returns `ok(IArticleSummaryEntity[])` on success, `err(Failure)` on failure.
 */
getPopularArticles(query: IPopularArticlesQuery): Promise<Result<IArticleSummaryEntity[]>>;
```

---

## 3. Repository implementation

**File:** `src/modules/articles/infrastructure/repositories/articles.repository.impl.ts`

Add (mirror `getPromotedArticles`, which returns `response.data.articles`):

```typescript
async getPopularArticles(query: IPopularArticlesQuery): Promise<Result<IArticleSummaryEntity[]>> {
    try {
        const response = await this.api.getPopularArticles({
            limit: query.limit,
            categoryId: query.categoryId,
            excludeId: query.excludeId
        });
        return ok(response.data.articles.map(ArticlesMapper.articleSummaryFromDto));
    } catch (error) {
        return err(ProblemMapper.toFailure(error));
    }
}
```

No new mapper — `ArticlesMapper.articleSummaryFromDto` already maps `ArticleSummaryDto`.

---

## 4. Use case

**File:** `src/modules/articles/application/usecases/getpopulararticles.usecase.ts`

```typescript
import type {
    IArticlesRepositoryPort,
    IPopularArticlesQuery
} from "@/modules/articles/application/repositories/articles.repository.port";
import type { IArticleSummaryEntity } from "@/modules/articles/domain/entities/IArticleSummaryEntity";
import type { IResultUseCase } from "@/shared/application/usecases/IUseCase";
import type { Result } from "@/shared/domain/results/result";

/**
 * @interface IGetPopularArticlesUseCase
 * @extends {IResultUseCase<IPopularArticlesQuery, IArticleSummaryEntity[]>}
 */
interface IGetPopularArticlesUseCase
    extends IResultUseCase<IPopularArticlesQuery, IArticleSummaryEntity[]> {}

/**
 * Use case for fetching the most popular published articles.
 *
 * @class GetPopularArticlesUseCase
 * @implements {IGetPopularArticlesUseCase}
 *
 * @description
 * Fetches the engagement-ranked popular articles. Delegates to the articles repository and
 * returns its `Result<IArticleSummaryEntity[]>` unchanged.
 */
export class GetPopularArticlesUseCase implements IGetPopularArticlesUseCase {
    private readonly articlesRepository: IArticlesRepositoryPort;

    /**
     * @param {IArticlesRepositoryPort} articlesRepository - Repository for articles operations (injected)
     */
    constructor({ articlesRepository }: { articlesRepository: IArticlesRepositoryPort }) {
        this.articlesRepository = articlesRepository;
    }

    /**
     * Executes the get popular articles use case.
     *
     * @param {IPopularArticlesQuery} query - Limit plus optional filters
     * @returns {Promise<Result<IArticleSummaryEntity[]>>} `ok(list)` on success, `err(Failure)` on failure
     */
    async execute(query: IPopularArticlesQuery): Promise<Result<IArticleSummaryEntity[]>> {
        return this.articlesRepository.getPopularArticles(query);
    }
}
```

---

## 5. DI registration

**File:** `src/modules/articles/infrastructure/dependencies/articles.dependencies.ts`

Add next to `getPublishedArticlesUseCase`:

```typescript
getPopularArticlesUseCase: asClass(GetPopularArticlesUseCase).transient(),
```

---

## 6. Query key

**File:** `src/modules/articles/presentation/constants/articleKeys.ts`

Add:

```typescript
popular: (articleId: string, categoryId?: string) =>
    [...articleKeys.all, "popular", articleId, categoryId ?? null] as const,
```

---

## 7. Hook

**File:** `src/modules/articles/presentation/hooks/useArticleDetailPopular.ts`

Replace the promoted-then-published composition with a single call to the real endpoint,
passing `excludeId` so the server drops the current article:

```typescript
export function useArticleDetailPopular(currentArticleId: string, categoryId?: string) {
    return useQuery({
        queryKey: articleKeys.popular(currentArticleId, categoryId),
        queryFn: async () => {
            const result = await container.cradle.getPopularArticlesUseCase.execute({
                limit: 5,
                categoryId,
                excludeId: currentArticleId
            });
            if (!result.ok) throw result.error;
            return result.value;
        }
    });
}
```

Optional cold-start fallback (kept inside the hook — UI/mapping unchanged because both return
`IArticleSummaryEntity[]`): if `result.value` is empty, fall back to
`getPublishedArticlesUseCase.execute({ pageIndex: 0, pageSize: 6 })` and drop
`currentArticleId`.

---

## Tasks

- [ ] `yarn api:generate` (after backend Swagger includes the endpoint)
- [ ] Add `IPopularArticlesQuery` + `getPopularArticles` to the repository port
- [ ] Implement `getPopularArticles` in the repository impl
- [ ] Create `getpopulararticles.usecase.ts`
- [ ] Register `getPopularArticlesUseCase` in dependencies
- [ ] Add the `popular` query key
- [ ] Rewire `useArticleDetailPopular` to call the real endpoint with `excludeId`
- [ ] (Optional) keep an empty-result fallback to published articles inside the hook
