# TODO — Backend Technical Debt & Improvements

A tracked list of cross-cutting improvements to address across the entire solution.

---

## 1. Standardize Endpoint Route Parameter Parsing

**Priority:** Medium
**Scope:** All modules — Identity, Core, Content (Catalog, Editorial, Lookup, Interactions, Commerce), and any future modules

All **mutating** endpoints (`POST` with path params, `PUT`, `PATCH`, `DELETE`) that accept a resource identifier must use `string id` in the route and parse it explicitly with `Guid.Parse(id)` inside the handler lambda.

**Why this matters for mutating verbs:**
When `Guid id` is used directly, ASP.NET treats it as a route constraint — if the value is not a valid GUID the route simply does not match (silent fallback). For mutating operations this is dangerous and produces no useful error. Using `string id` + `Guid.Parse(id)` ensures the route always matches and any invalid input flows through the global exception middleware → ProblemDetails 400, which the frontend can display correctly.

**GET endpoints:** `Guid id` directly is acceptable. The client always has a valid UUID from a prior API response. A silent route mismatch (fallback to the list endpoint) is harmless on reads.

**Rule by HTTP verb:**

| Verb | Approach | Reason |
|------|----------|--------|
| `GET /{id}` | `Guid id` — route constraint is fine | Client always has a valid UUID; silent fallback is acceptable |
| `DELETE /{id}` | `string id` + `Guid.Parse(id)` | Irreversible — must produce explicit ProblemDetails 400, not a silent route mismatch |
| `POST /{parentId}/...` | `string id` + `Guid.Parse(id)` | Sub-resource creation needs a clear error on invalid parent ID |
| `PUT /{id}` | `string id` + `Guid.Parse(id)` | Mutating — explicit ProblemDetails required |
| `PATCH /{id}` | `string id` + `Guid.Parse(id)` | Same as PUT |

**Pattern to enforce:**
```csharp
// Correct — mutating verb
.MapDelete("/{id}/...", async (string id, IDispatcher dispatcher) =>
{
    Guid resourceId = Guid.Parse(id);
    ...
})

// Correct — read verb
.MapGet("/{id}/...", async (Guid id, IDispatcher dispatcher) => { ... })

// Wrong — mutating verb with Guid id directly (silent route mismatch on invalid input)
.MapDelete("/{id}/...", async (Guid id, IDispatcher dispatcher) => { ... })
```

**Prerequisite:** Verify the global exception middleware catches `FormatException` and converts it to a ProblemDetails 400. If not, `Guid.Parse` on invalid input will produce a 500 instead.

---

## 2. Audit and Apply Specification Pattern Consistently

**Priority:** High
**Scope:** All modules — Identity, Core, Content, and any future modules

The `Specification<T>` base class exists in `Shared/Shared/Specifications/` and is used in some repositories but not consistently everywhere. Any place where a repository method applies a `.Where(x => ...)` predicate inline should be extracted into a named specification class.

**Benefits:**
- Reusability across handlers
- Testability — specifications can be compiled and asserted in unit tests
- Readability — business intent is named, not buried in lambdas

**Action:** Search all `*Repository.cs` files across all modules for inline LINQ `.Where(...)` predicates not backed by a specification. Create missing specification classes under `Application/<Submodule>/Specifications/`.

---

## 3. Ensure DTOs Expose Auditable Fields On Demand

**Priority:** Low–Medium
**Scope:** All modules — any DTO returned from a list or detail query

Entities inheriting from `Aggregate<T>` carry `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`. Admin-facing detail DTOs should expose these fields. Public-facing DTOs generally should not.

**Rules:**
- Admin detail queries → always include all audit fields
- Admin list/summary queries → include `CreatedAt` at minimum
- Public queries → omit audit fields entirely

**Action:** Review all `*Dto` records returned by query handlers across all modules. Add missing audit fields to admin DTOs. Ensure mappers (Mapster profiles or extension methods) map them correctly.

---

## 4. Audit Unit Tests for Pattern Consistency and Bogus Usage

**Priority:** High
**Scope:** `tests/Unit/` — all modules

Unit tests must follow consistent patterns across the codebase. Bogus (`Faker<T>`) should be used wherever realistic random test data is beneficial (names, emails, descriptions, URLs). Static hardcoded strings like `"test"`, `"foo"`, or `"name"` are acceptable only for identity checks, not for value validation tests.

**Checklist:**
- All entity builders (`*Builder.cs`) should use `Faker<T>` or `Bogus.Faker` for string/numeric fields
- All factory helpers (`*Factory.cs`) should delegate to builders
- Test method naming must follow `MethodName_WhenCondition_ShouldExpectedBehavior`
- No test should use `Thread.Sleep` — use `FakeTimeProvider` or `ISystemClock` for time
- Mocks should use `It.Is<T>(...)` for meaningful argument verification, not just `It.IsAny<T>()`
- Every handler test must cover: happy path + all known error branches

**Action:** Run a deep pass over all `tests/Unit/` files across all modules. File issues per module.

---

## 5. Implement Analytics Endpoints for All Modules (Admin)

**Priority:** Medium–High
**Scope:** All modules — Identity, Core, Content (Editorial, Commerce, Interactions), and any future modules

Each module should expose an admin analytics endpoint that returns aggregated metrics relevant to that domain. These endpoints must be protected behind `RequireAdminOrSuperAdmin` and rate-limited with `AdminMetrics` policy.

**Expected endpoints (per module):**
- `GET /api/v1/admin/analytics/content` — article/video counts by status, top categories, engagement totals
- `GET /api/v1/admin/analytics/commerce` — order counts by status, revenue summaries, pending payments
- `GET /api/v1/admin/analytics/interactions` — views, likes, bookmarks, shares totals
- `GET /api/v1/admin/analytics/identity` — user counts by role, active sessions, recent signups

**Design notes:**
- Use dedicated `*AnalyticsDto` records, not reuse of entity DTOs
- Queries should hit read-optimized projections (select aggregates, not full entities)
- Consider a single `GET /api/v1/admin/analytics` endpoint that returns a composite dashboard DTO

---

## 6. Implement i18n (Internationalization) Across the Solution

**Priority:** Low (future milestone)
**Scope:** All modules — all user-facing string outputs including error messages, validation messages, and response labels

The solution currently returns all strings in English. Adding i18n support requires a strategy that covers error message strings, validation messages (FluentValidation), and potentially API response labels across every module.

**Proposed approach:**
- Use `IStringLocalizer<T>` from `Microsoft.Extensions.Localization`
- Store translations in `Resources/*.resx` files per module
- Accept `Accept-Language` header to select locale (default: `en`)
- FluentValidation messages: override via localized message factories
- Error messages in `*Errors.cs` and `*ErrorMessage.cs` classes: inject `IStringLocalizer` or use a static resource lookup

**Testing strategy:**
- Unit test: assert that error factories return the correct localized string given a mocked locale
- Integration test: send requests with `Accept-Language: fr` / `Accept-Language: en` and assert response messages differ
- Snapshot testing (Verify library) can be used to assert full response bodies don't regress

**Action:** Research and prototype i18n integration in one module (Identity auth errors) before rolling out solution-wide.

---

## 7. Audit and Optimize EF Core Queries + Benchmarking

**Priority:** High
**Scope:** All modules — every repository and query handler that touches the database

All EF Core queries across the solution must be reviewed for correctness and performance. After optimization, benchmarks must be established so query performance can be tracked across development and staging environments.

**Common issues to look for:**
- **N+1 queries** — missing `.Include()` / `.ThenInclude()` on navigations that are accessed after the query
- **Over-fetching** — selecting full entities when only a few columns are needed; prefer `.Select(x => new { ... })` or projection DTOs
- **Missing `.AsNoTracking()`** — all read-only queries (GET handlers, analytics) must use `.AsNoTracking()` to skip the EF change tracker
- **Missing indexes** — queries filtering or ordering on non-indexed columns; cross-reference repository `.Where(...)` clauses with migration configurations
- **Unbounded queries** — any query that can return an unlimited number of rows without pagination
- **Client-side evaluation** — LINQ expressions that cannot be translated to SQL and silently execute in memory
- **Redundant round-trips** — multiple sequential queries that could be combined into one or batched

**Best practices to enforce:**
```csharp
// Read-only queries — always AsNoTracking
await _context.Articles
    .AsNoTracking()
    .Where(a => a.Status == ArticleStatus.Published)
    .Select(a => new ArticleSummaryDto(a.Id, a.Title))
    .ToListAsync(cancellationToken);

// Paginated queries — never unbounded
await _context.Orders
    .AsNoTracking()
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);

// Avoid loading navigation then filtering in memory
// Wrong: .Include(o => o.Items).Where(o => o.Items.Any(i => i.Status == x))
// Correct: .Where(o => o.Items.Any(i => i.Status == x)) — let SQL do the work
```

**Benchmarking setup:**
- Use **BenchmarkDotNet** for micro-benchmarks on critical query paths (repository methods that are called frequently or on hot endpoints)
- Use **EF Core logging** (`EnableSensitiveDataLogging` + `LogTo`) in Development to log generated SQL and detect slow queries
- Use **MiniProfiler** or **Seq query timings** in Staging to track real-world query durations per endpoint
- Set a query duration threshold (e.g. > 100ms = warning, > 500ms = alert) and hook it into Serilog structured logs

**Action:**
1. Enable EF Core SQL logging in Development immediately (zero-cost change)
2. Audit all `*Repository.cs` files across all modules for the issues listed above
3. Add BenchmarkDotNet project under `tests/Benchmarks/` for the top 10 most-called queries
4. Review generated SQL for every query in the audit using `.ToQueryString()` before and after optimization

---

## 8. Implement Fine-Grained RBAC via Resource/Action Permissions

**Priority:** High (architectural — affects all modules)
**Scope:** All modules — every command handler, query handler, and endpoint across the solution

The current authorization model relies entirely on `WithAuthorization(policy)` at the endpoint level (e.g. `RequireVisitorOnly`, `RequireAdminOnly`). This is coarse-grained — it controls *who can reach an endpoint*, but not *what actions a given role is allowed to perform on a given resource*. As roles and permissions become admin-configurable, this model breaks down.

**Goal:** Move to a permission-based RBAC system where every handler declares the `resource` + `action` it requires, and the authorization layer checks whether the authenticated user holds a permission for that pair. Permissions and roles remain admin-managed (already exists in the Identity module), but the enforcement layer becomes handler-driven rather than policy-driven.

---

### Proposed Design

#### Permission Model (already in place)
`PermissionEntity` stores `Resource` (e.g. `articles`) and `Action` (e.g. `read`, `create`, `update`, `delete`). Users hold permissions transitively through their roles.

#### Handler-Level Permission Declaration
Each command/query handler declares the permission it requires via a marker interface or attribute:

```csharp
// Option A — marker interface (recommended for testability)
public interface IRequirePermission
{
    string Resource { get; }
    string Action { get; }
}

public class AdminPublishArticleHandler
    : ICommandHandler<AdminPublishArticleCommand, AdminPublishArticleResult>,
      IRequirePermission
{
    public string Resource => "articles";
    public string Action  => "publish";
}

// Option B — attribute (simpler, but less discoverable at runtime)
[RequirePermission("articles", "publish")]
public class AdminPublishArticleHandler : ICommandHandler<...> { }
```

#### Authorization Decorator (CQRS Pipeline)
A new `PermissionAuthorizationDecorator<TRequest, TResponse>` wraps every handler in the dispatcher pipeline. It:
1. Checks if the handler implements `IRequirePermission` (or has the attribute)
2. Resolves the current user's permissions from the JWT claims or Identity module
3. Throws `ForbiddenException` (403) if the required `resource:action` is not found

```csharp
public class PermissionAuthorizationDecorator<TRequest, TResponse>
    : ICommandHandler<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken ct)
    {
        if (_innerHandler is IRequirePermission p)
        {
            bool allowed = await _permissionService.HasPermissionAsync(
                userId: _currentUser.Id,
                resource: p.Resource,
                action: p.Action,
                ct);

            if (!allowed) throw ForbiddenException.Default();
        }

        return await _innerHandler.Handle(request, ct);
    }
}
```

#### Endpoint Layer — Thin Authorization
Endpoints keep `WithAuthorization` only as a coarse gate (authenticated vs anonymous). The fine-grained check moves entirely to the handler layer:

```csharp
// Before — policy does both authentication and permission check
.WithAuthorization(UserRolePolicies.RequireAdminOnly)

// After — endpoint only asserts "must be authenticated"
.RequireAuthorization()
// Handler decorator enforces the specific permission
```

---

### Permission Naming Convention

Use `resource.action` dot notation, consistent with the existing `VisitorPermissions` value object:

| Resource | Actions |
|----------|---------|
| `articles` | `read`, `create`, `update`, `delete`, `publish`, `approve`, `reject`, `archive` |
| `videos` | `read`, `create`, `update`, `delete`, `publish`, `approve`, `reject` |
| `orders` | `read`, `create`, `submit`, `cancel` |
| `payments` | `read`, `verify`, `reject`, `attach_proof` |
| `users` | `read`, `update`, `deactivate` |
| `roles` | `read`, `create`, `update`, `delete` |
| `permissions` | `read`, `create`, `update`, `delete` |

---

### Supporting Future Permissions Easily

The system must not require code changes when an admin creates a new permission or assigns it to a role. The enforcement chain must be purely data-driven at runtime:

1. Admin creates `Permission(resource: "reports", action: "export")` via API
2. Admin assigns it to a role
3. Any handler that declares `Resource = "reports"` / `Action = "export"` automatically becomes accessible to users holding that role — **no deployment needed**

To support this, the permission check must query the Identity module (or a shared read model) at request time, not be hardcoded in policy classes.

---

### Caching Strategy

Permission lookups per user will be hot on every authenticated request. Cache resolved permission sets:
- In-memory cache keyed by `userId`, TTL = 5 minutes
- Invalidate on role/permission change events (domain events from Identity module)
- In distributed deployments: Redis with short TTL + event-driven invalidation

---

### Migration Path

1. Add `IRequirePermission` interface to `Shared.Contracts`
2. Implement `PermissionAuthorizationDecorator` and register it in the dispatcher pipeline via Scrutor
3. Add `ICurrentUserPermissionService` to resolve permissions for the authenticated user
4. Annotate handlers one module at a time, starting with Content (highest surface area)
5. Keep existing `WithAuthorization(policy)` at endpoints as a safety net during migration; remove once all handlers in a module are annotated
6. Write unit tests: decorator grants access when permission present, denies when absent

---

*Last updated: 2026-03-18*