# Test Coverage Plan

## Phase 1: Foundation (Setup)

Establish the integration test infrastructure before writing any tests.

| Task | File | Status |
|------|------|--------|
| Create `_116.Integration.Tests.csproj` | `tests/Integration/` | |
| Add to solution file | `116_backend.sln` | |
| Add `partial class Program` to API entry point | `src/Api/Program.cs` | |
| Implement `PostgresFixture` | `Common/Fixtures/PostgresFixture.cs` | |
| Implement `DatabaseCollection` | `Common/Fixtures/DatabaseCollection.cs` | |
| Implement `ApiFixture` | `Common/Fixtures/ApiFixture.cs` | |
| Implement `BaseRepositoryTest` | `Common/Abstractions/BaseRepositoryTest.cs` | |
| Implement `BaseApiTest` | `Common/Abstractions/BaseApiTest.cs` | |
| Implement `HttpClientExtensions` | `Common/Extensions/HttpClientExtensions.cs` | |
| Implement `HttpResponseExtensions` | `Common/Extensions/HttpResponseExtensions.cs` | |
| Implement `TestDataSeeder` | `Common/Seeders/TestDataSeeder.cs` | |
| Implement `StubCloudinaryService` | `Common/Stubs/StubCloudinaryService.cs` | |
| Implement `StubYoutubeThumbnailService` | `Common/Stubs/StubYoutubeThumbnailService.cs` | |
| Verify: `dotnet test tests/Integration` runs (0 tests, green) | CI | |

## Phase 2: Repository Tests (ILike + Constraints)

Cover the tests skipped in unit tests and critical query paths.

### Content Module

| Test Class | Tests | Priority |
|-----------|-------|----------|
| `CategoryRepositoryTests` | GetBySlugAsync (ILike), GetExclusiveCategoryAsync, GetAllAsync (pagination), unique slug constraint | High |
| `VideoRepositoryTests` | GetAllAsync with status/category filters, pagination, ordering by CreatedAt | High |
| `ArticleRepositoryTests` | GetBySlugAsync (ILike), GetAllAsync with filters | High |
| `CustomerRepositoryTests` | GetByEmailAsync (ILike) | High |
| `TagRepositoryTests` | Search with ILike, popular tags query | Medium |
| `LookupRepositoryTests` | ContentType/PricingTier/PromotionLevel by name (ILike) | Medium |
| `PackageRepositoryTests` | GetBySlugAsync (ILike) | Medium |
| `PlaylistRepositoryTests` | GetAllAsync with filters | Low |
| `LyricsRepositoryTests` | GetBySlugAsync (ILike) | Low |
| `ShortVideoRepositoryTests` | GetAllAsync with filters | Low |

### Identity Module

| Test Class | Tests | Priority |
|-----------|-------|----------|
| `AuthRepositoryTests` | GetByEmailAsync (ILike), unique email constraint | High |
| `SessionRepositoryTests` | Active session queries, expiry filtering | Medium |
| `RoleRepositoryTests` | GetByNameAsync (ILike), unique name constraint | Medium |
| `PermissionRepositoryTests` | Unique resource+action constraint | Low |

### Core Module

| Test Class | Tests | Priority |
|-----------|-------|----------|
| `FileRepositoryTests` | GetByFileNameAsync, unique filename constraint | Low |

## Phase 3: API Endpoint Tests (High Priority)

Cover all 211 Carter `AddRoutes()` methods that are at 0% coverage.

### Authentication Endpoints (Critical)

| Test Class | Endpoint | Tests |
|-----------|----------|-------|
| `PublicLoginEndpointTests` | `POST /public/auth/login` | Valid login, invalid password, nonexistent user, inactive account, unverified account |
| `PublicSignUpEndpointTests` | `POST /public/auth/signup` | Valid signup, duplicate email, invalid payload |
| `AdminLoginEndpointTests` | `POST /admin/auth/login` | Valid login, non-admin role rejection |
| `PublicRefreshTokenEndpointTests` | `POST /public/auth/refresh` | Valid refresh, expired token, invalid token |
| `PublicSignOutEndpointTests` | `POST /public/auth/signout` | Valid signout, session invalidation |

### Content CRUD Endpoints

| Test Class | Endpoint | Tests |
|-----------|----------|-------|
| `PublicGetActiveCategoriesEndpointTests` | `GET /public/categories` | Returns active categories, pagination, anonymous access |
| `PublicGetExclusiveCategoryEndpointTests` | `GET /public/categories/exclusive` | Returns exclusive with videos, 404 when none, pagination |
| `AdminCreateCategoryEndpointTests` | `POST /admin/categories` | Valid create, duplicate slug, auth required, validation errors |
| `AdminUpdateCategoryEndpointTests` | `PUT /admin/categories/{id}` | Valid update, not found, slug conflict |
| `AdminSetExclusiveCategoryEndpointTests` | `PATCH /admin/categories/{id}/exclusive` | Set exclusive, video-only guard, inactive guard |
| `PublicGetPublishedVideosEndpointTests` | `GET /public/videos` | Returns published, pagination, category filter |
| `PublicGetPublishedArticlesEndpointTests` | `GET /public/articles` | Returns published, pagination |
| `AdminCreateVideoEndpointTests` | `POST /admin/videos` | Valid create, auth required |
| `AdminCreateArticleEndpointTests` | `POST /admin/articles` | Valid create, auth required |

### User Management Endpoints

| Test Class | Endpoint | Tests |
|-----------|----------|-------|
| `PublicGetOwnProfileEndpointTests` | `GET /public/me/profile` | Returns profile, auth required |
| `AdminGetUsersEndpointTests` | `GET /admin/users` | Returns users, admin auth, pagination |
| `AdminGetUserByIdEndpointTests` | `GET /admin/users/{id}` | Returns user, not found |

## Phase 4: API Endpoint Tests (Medium Priority)

### Content Interactions

| Test Class | Endpoint | Tests |
|-----------|----------|-------|
| `PublicLikeArticleEndpointTests` | `POST /public/articles/{id}/like` | Like, unlike, auth required |
| `PublicBookmarkArticleEndpointTests` | `POST /public/articles/{id}/bookmark` | Bookmark, remove, auth required |
| `PublicCommentArticleEndpointTests` | `POST /public/articles/{id}/comments` | Add comment, auth required |
| `PublicRateVideoEndpointTests` | `POST /public/videos/{id}/rate` | Rate, auth required |
| `PublicShareEndpointTests` | `POST /public/{type}/{id}/share` | Share, auth required |

### Commerce

| Test Class | Endpoint | Tests |
|-----------|----------|-------|
| `AdminCreateOrderEndpointTests` | `POST /admin/orders` | Valid create, auth required |
| `AdminSubmitOrderEndpointTests` | `POST /admin/orders/{id}/submit` | Submit, validation |
| `AdminVerifyPaymentEndpointTests` | `POST /admin/orders/{id}/verify-payment` | Verify, auth required |

### Session Management

| Test Class | Endpoint | Tests |
|-----------|----------|-------|
| `PublicGetSessionsEndpointTests` | `GET /public/me/sessions` | Returns sessions, auth required |
| `PublicRevokeSessionEndpointTests` | `POST /public/me/sessions/revoke/{id}` | Revoke, auth required |
| `AdminExportSessionsEndpointTests` | `GET /admin/sessions/export` | Export CSV/Excel, auth required |

## Phase 5: Interceptors & Decorators

Cover the 4 infrastructure components with 0% unit test coverage.

### Interceptors

| Test Class | What It Tests | Tests |
|-----------|---------------|-------|
| `AuditableEntityInterceptorTests` | `created_at`, `updated_at`, `created_by`, `updated_by` populated on insert/update | CreatedAt set on insert, UpdatedAt changes on update, CreatedBy set from ClaimsPrincipal, UpdatedAt not set on insert |
| `DispatchDomainEventsInterceptorTests` | Domain events dispatched after `SaveChangesAsync` | Events dispatched after save, Events cleared after dispatch, Multiple events dispatched in order, No dispatch when no events |

### Decorators

| Test Class | What It Tests | Tests |
|-----------|---------------|-------|
| `ValidationDecoratorTests` | Validator runs before command handler, throws `ValidationException` | Valid command passes through, Invalid command throws ValidationException with field errors, Multiple validation errors aggregated |
| `LoggingDecoratorTests` | Command/query execution logged with timing | Command logged with type name and duration, Failed command logged with exception details |

## Phase 6: Seeders

Cover the 2 seeders with 0% unit test coverage + verify ContentTypeSeeder.

| Test Class | What It Tests | Tests |
|-----------|---------------|-------|
| `SuperAdminSeederTests` | Creates SuperAdmin user with correct role, password, and claims | SuperAdmin created on first run, Idempotent on subsequent runs, Correct role assigned, Password matches DEFAULT_USER_PASSWORD |
| `VisitorRoleSeederTests` | Creates Visitor role with correct permissions | Visitor role created on first run, Idempotent on subsequent runs, Correct permissions assigned |
| `ContentTypeSeederTests` | Seeds all content types (Video, Article, ShortVideo, etc.) | All content types seeded, Idempotent on subsequent runs, Names match EnumCoreContentType values |

## Phase 7: Mappers (Round-Trip)

Cover the 11 mappers with 0% unit test coverage.

### Content Mappers

| Test Class | What It Tests | Tests |
|-----------|---------------|-------|
| `CategoryMapperTests` | `ToCategoryDto`, `ToCategoryDtoAsync` | Maps all fields, Handles null poster, Maps content type name |
| `VideoMapperTests` | `ToVideoDto`, `ToVideoSummaryDto`, `ToVideoSummaryDtosAsync` | Maps all fields, Maps category, Maps tags, Handles null thumbnail |
| `ArticleMapperTests` | `ToArticleDto`, `ToArticleSummaryDto` | Maps all fields, Maps tags, Handles null poster |
| `CustomerMapperTests` | `ToCustomerDto` | Maps all fields, Maps user email from Identity |
| `ContentTypeMapperTests` | `ToContentTypeDto` | Maps all fields |
| `TagMapperTests` | `ToTagDto` | Maps all fields |
| `PackageMapperTests` | `ToPackageDto`, `ToPackageSummaryDto` | Maps all fields, Maps items |
| `PlaylistMapperTests` | `ToPlaylistDto` | Maps all fields, Maps videos |
| `LyricsMapperTests` | `ToLyricsDto` | Maps all fields |
| `ShortVideoMapperTests` | `ToShortVideoDto` | Maps all fields |
| `OrderMapperTests` | `ToOrderDto` | Maps all fields, Maps order items, Maps payment status |

## Phase 8: Interaction Entities & EF Core Configurations

### Interaction Entities (13 with 0% coverage)

Test that these entities persist correctly, respect unique constraints, and cascade-delete when the parent is removed.

| Test Class | Entity | Tests |
|-----------|--------|-------|
| `ArticleCommentEntityTests` | `ArticleCommentEntity` | Create, update, cascade delete with article, FK to customer |
| `ArticleLikeEntityTests` | `ArticleLikeEntity` | Create, unique (article + customer), cascade delete |
| `ArticleBookmarkEntityTests` | `ArticleBookmarkEntity` | Create, unique (article + customer), cascade delete |
| `ArticleShareEntityTests` | `ArticleShareEntity` | Create, cascade delete |
| `VideoLikeEntityTests` | `VideoLikeEntity` | Create, unique (video + customer), cascade delete |
| `VideoBookmarkEntityTests` | `VideoBookmarkEntity` | Create, unique (video + customer), cascade delete |
| `VideoShareEntityTests` | `VideoShareEntity` | Create, cascade delete |
| `VideoRatingEntityTests` | `VideoRatingEntity` | Create, unique (video + customer), value range, cascade delete |
| `ShortVideoLikeEntityTests` | `ShortVideoLikeEntity` | Create, unique, cascade delete |
| `ShortVideoBookmarkEntityTests` | `ShortVideoBookmarkEntity` | Create, unique, cascade delete |
| `ShortVideoShareEntityTests` | `ShortVideoShareEntity` | Create, cascade delete |
| `ContentItemTierEntityTests` | `ContentItemTierEntity` | Create, FK to content item + pricing tier, cascade delete |
| `CustomerEntityTests` | `CustomerEntity` | Create, unique user ID, FK to auth user |

### EF Core Configurations (~30 configs)

Verify FK cascades, unique indexes, and composite keys defined in `IEntityTypeConfiguration<T>` implementations.

| Test Class | Schema | Tests |
|-----------|--------|-------|
| `IdentitySchemaConfigTests` | `identity` | Auth user email unique, Role name unique, Session FK to auth user, Permission resource+action unique |
| `CoreSchemaConfigTests` | `core` | File name unique |
| `ContentSchemaConfigTests` | `content` | Category slug unique, Video slug unique, Article slug unique, Tag name unique, Interaction composite keys, FK cascade behavior, Content item → category FK, Order → customer FK |

## Phase 9: Module Registration & Background Jobs

### Module Registration

Verify that each module's DI registration is correct — all services resolve, DbContexts are wired, interceptors are registered.

| Test Class | What It Tests | Tests |
|-----------|---------------|-------|
| `IdentityModuleRegistrationTests` | Identity DI container | All Identity services resolve, IdentityDbContext resolves with correct schema, JWT services configured |
| `CoreModuleRegistrationTests` | Core DI container | All Core services resolve, CoreDbContext resolves with correct schema |
| `ContentModuleRegistrationTests` | Content DI container | All Content services resolve, ContentDbContext resolves with correct schema, Interceptors registered in correct order |

### Background Jobs

| Test Class | What It Tests | Tests |
|-----------|---------------|-------|
| `AbandonedDraftCleanupJobTests` | Full job execution against real database | Deletes drafts older than threshold, Preserves recent drafts, Preserves published content, Handles empty database, Runs idempotently |

## Phase 10: Cross-Cutting Concerns

| Test Class | What It Tests | Priority |
|-----------|---------------|----------|
| `ExceptionMiddlewareTests` | ProblemDetails format for each exception type (400, 401, 403, 404, 409, 422, 429, 500) | High |
| `RateLimitingTests` | ContentBrowsing, Authentication rate limits, 429 response format | Medium |
| `ApiVersioningTests` | v1 routes work, unknown versions return 404 | Medium |
| `CorsTests` | Allowed origins accepted (Dashboard, Webapp), others rejected | Low |

## Phase 11: Cross-Module Workflow Tests

End-to-end flows that span multiple modules. These validate that the modules work together correctly.

| Test Class | Flow | Tests |
|-----------|------|-------|
| `AuthenticationFlowTests` | Identity → Session | Signup → verify email → login → get token → refresh token → sign out → session revoked |
| `ContentPublicationFlowTests` | Identity → Content | Login as admin → create category → create video → publish → verify public visibility → verify SEO fields |
| `InteractionFlowTests` | Identity → Content | Login as visitor → like article → bookmark article → comment → verify counts → unlike → verify counts decremented |
| `OrderLifecycleTests` | Identity → Content → Commerce | Login → create order → add items → submit → verify payment → verify customer access |

## Estimated Test Count

| Phase | Test Classes | Tests (est.) |
|-------|-------------|-------------|
| Phase 1 | 0 (infrastructure) | 0 |
| Phase 2: Repositories | ~15 | ~60 |
| Phase 3: Endpoints (High) | ~15 | ~75 |
| Phase 4: Endpoints (Medium) | ~10 | ~40 |
| Phase 5: Interceptors & Decorators | 4 | ~16 |
| Phase 6: Seeders | 3 | ~12 |
| Phase 7: Mappers | 11 | ~35 |
| Phase 8: Interactions & EF Config | ~16 | ~65 |
| Phase 9: Modules & Jobs | 4 | ~18 |
| Phase 10: Cross-Cutting | ~4 | ~20 |
| Phase 11: Workflows | 4 | ~20 |
| **Total** | **~86** | **~361** |

## Execution Order

Implement phases sequentially. Each phase depends on the previous:

1. **Phase 1** — must work before any tests can run
2. **Phase 2** — repository tests validate database behavior independently
3. **Phase 3** — API tests depend on Phase 1 infrastructure + auth seeding
4. **Phase 4** — secondary API tests, can be done in parallel with Phase 3
5. **Phase 5** — interceptor/decorator tests need real EF Core pipeline
6. **Phase 6** — seeder tests need real database
7. **Phase 7** — mapper tests need real entities with navigation properties loaded
8. **Phase 8** — interaction entity + EF config tests need real database
9. **Phase 9** — module registration tests need full DI container
10. **Phase 10** — cross-cutting tests need full HTTP pipeline
11. **Phase 11** — workflow tests depend on all previous phases

## CI Pipeline Integration

```yaml
jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test tests/Unit --no-build

  integration-tests:
    runs-on: ubuntu-latest
    needs: unit-tests
    services:
      docker:
        image: docker:dind
    steps:
      - run: dotnet test tests/Integration --no-build
```

Integration tests run after unit tests pass. They require Docker for Testcontainers.
