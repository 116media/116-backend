# 08 — Shared Foundation: `SharedKernel` + `BuildingBlocks`

**Decision (adopted):** the one kitchen-sink `Shared` project is split into **two** foundation projects:

- **`SharedKernel`** — shared **domain-model primitives** every module's *Domain layer* is expressed
  in. **Hard rule: zero framework/infrastructure dependencies** (no EF, ASP.NET, Carter, Quartz,
  Mapster, FluentValidation, DI). If it needs a package, it is not SharedKernel.
- **`BuildingBlocks`** — shared **technical / cross-cutting plumbing** that is not domain and not any
  single module's business: CQRS + dispatcher, decorators, the exception framework, pagination, the
  module system, EF interceptors, cross-cutting extensions, the genuinely-global constants, and
  rate-limiting. May depend on frameworks.

This is grounded in a **file-by-file read of all 90 shared files** plus a **sweep of the four modules**
for generic plumbing that should be hoisted in. Every placement below cites what the file actually
imports.

> Project naming note (once): in strict DDD, `SharedKernel`'s contents are the *tactical building
> blocks / SeedWork*, and a true "Shared Kernel" is co-owned domain model between contexts (none exists
> here). We adopt the project **name** `SharedKernel` as decided; the hard zero-dependency rule is what
> actually keeps it honest, not the label.

---

## `SharedKernel` — exact contents (zero framework deps)

Everything here compiles with **no `PackageReference`**. Heaviest import is `System.Linq.Expressions`
(BCL).

| File | What it is | Deps |
|---|---|---|
| `Entities/IEntity.cs` | entity contract (`Id` + 4 audit fields) | BCL |
| `Entities/Entity.cs` | `Entity<T>` base | BCL |
| `Entities/IAggregate.cs` | aggregate contract (domain events) | BCL |
| `Entities/Aggregate.cs` | `Aggregate<TId>` base (event list + Add/Clear) | BCL |
| `Events/IDomainEvent.cs` | domain-event primitive | BCL |
| `Audit/EnumAuditActor.cs` | audit-actor enum (`System`/`Anonymous`) | BCL |
| `Abstractions/IRepository.cs` | marker `IRepository<T> where T : IAggregate` (empty today) | BCL |
| `Specifications/ISpecification.cs` | `ToExpression()` + `IsSatisfiedBy()` | `System.Linq.Expressions` |
| `Specifications/Specification.cs` | base w/ And/Or/Not/AndAll/OrAll | `System.Linq.Expressions` |
| `Specifications/AndSpecification.cs` · `OrSpecification.cs` · `NotSpecification.cs` | expression-tree compositors | `System.Linq.Expressions` |

**Moved *into* `SharedKernel`:**

- **`Specification<T>` family** — currently misfiled under `Shared/Application/Specifications`. Verified
  imports are `System.Linq.Expressions` only. *(Reconciled in [09](09-sharedkernel-vs-buildingblocks-rules.md):
  under the sharper rule a specification is a technical query **pattern**, not a business concept, so it
  moves to `BuildingBlocks.Domain` rather than SharedKernel. 09 is the current placement.)* (The **EF bridge**
  `SpecificationExtensions.ApplySpecification` is the framework-bound part — that goes to
  BuildingBlocks, which is what lets the family itself stay pure.)
- **`SlugHelper`** — today buried in `Core/Application/Shared/Helpers/`, but it is a BCL-only slug
  utility with **zero Core call sites** (its only consumers are 2 Content files). A slug is a
  cross-cutting domain concept (articles, videos, categories, artists all have one). Hoist it here as a
  domain utility (ideally later promoted to a `Slug` value object). This also removes one of the
  non-file reasons Content reaches into Core internals.

**Optional, forward-looking:** a `ValueObject` base class. None exists today (Identity's 8 VOs and
Content's `ShareChannel` are ad-hoc records with ctor guards); add one to SharedKernel only if you want
a common base for future VOs — do not retrofit the existing ones just to use it.

**Litmus for SharedKernel:** *is it a domain-model type a `*.Domain` project needs, with zero framework
packages?* If yes → here. If it needs any package → BuildingBlocks or a module.

---

## `BuildingBlocks` — exact contents (technical plumbing)

Organized by concern. All of these are cross-cutting and framework-bound (or exist only to serve the
plumbing pipeline).

**CQRS & dispatch** — the 7 `Shared.Contracts` interfaces (`ICommand`, `IQuery`, `IRequest`,
`I*Handler`, `IDispatcher`) + `Dispatcher` + `IDomainEventHandler`/`IDomainEventPublisher`/
`IDomainEventHandlerRegistry` + their implementations. (Dep-free interfaces, but they're *application*
messaging vocabulary, not domain — and the registry leaks `ServiceDescriptor`, so the set is
framework-bound as a whole.)

**Decorators** — `ValidationDecorator` (FluentValidation), `LoggingDecorator`.

**Exception framework** (one unit) — the 11 HTTP-shaped exception types (`NotFound`, `Conflict`,
`BadRequest`, `Authentication`, `Authorization`, `BadGateway`, `InternalServer`, `MethodNotAllowed`,
`RateLimitExceeded`, `ResourceNotFound`, `InvalidFormat`), `SharedExceptionMessage` + 3 `.resx`, the
`IExceptionStrategy`/`BaseExceptionStrategy` contracts, `ExceptionStrategyRegistry`, `ExceptionHandler`,
and all **13 strategy** classes. *(These exceptions all encode an HTTP status — none is a domain error,
so none goes to SharedKernel. If a domain `Result`/`Error` type is ever introduced, that goes to
SharedKernel; these don't.)*

**Cross-cutting extensions** — every `Application/Extensions/*` **except** the two Cloudinary files:
`ApiVersionExtension`, `AuthorizationExtension`, `CarterExtension`, `CqrsExtension`, `EnumSchemaFilter`,
`ExceptionHandlerExtension`, `LocalizationExtension`, `QuartzExtension`, `RateLimitingExtension`,
`ResourceNotFoundExtension`, `SwaggerExtension`, `SwaggerMiddlewareExtension`, `ValidationExtension`
(`IsValidGuid`).

**Module system & infrastructure** — the entire `Shared/Infrastructure/`: `BaseModule`,
`ModuleOptions`, `IDataSeeder`, `HttpCurrentActor`, `ApplicationBuilderExtension` (`UseMigration`/
`UseSeed`), `DbSetExtension`, `EntityEntryExtension`, `SpecificationExtensions` (the EF spec bridge),
`AuditableEntityInterceptor`, `DispatchDomainEventsInterceptor`.

**Application plumbing** — `PaginatedRequest` + `PaginatedResult`, `IUnitOfWork`, `AuditableDto`,
`ICurrentActor`, `IScheduledJob`, `RouteMetadata`, both middleware (`ResourceNotFound`,
`SwaggerDescription`), the 3 rate-limit builders, `AppEnvironment` (flagged — see below).

**Global constants** (from the current `BuildingBlocks`) — `RateLimitPolicies` + the 10
`RateLimit*Constants`, `UserRolePolicies`, `AccountStatusPolicies`, `FileConstants`, `ApiVersionUrl`.
All grep-verified as consumed by ≥2 modules.

---

## Hoisted *into* BuildingBlocks (generic plumbing found inside modules)

The module sweep found real duplication that should collapse into BuildingBlocks. Priority order by
leverage:

| # | Candidate | Evidence | New BuildingBlocks type |
|---|---|---|---|
| 1 | **Paginated slicing** re-hand-rolled | `.Skip((page-1)*size).Take(size)` + `CountAsync` in **32** repository sites (10 in `ArticleRepository` alone) | `IQueryable<T>.ToPaginatedResultAsync(page, size, ct)` (EF ext) |
| 2 | **`ValidationUtils`** duplicated verbatim | byte-identical reflection `GetPropertyValue<T>` in Identity + Content (11 + 16 call sites) | one `ValidationUtils` |
| 3 | **`ValidHttpUrl`** re-inlined | same `Uri.TryCreate` + http/https check in 6 places (Identity `ValidUrl`, 4 Content validators, Core `FileService`) | `ValidHttpUrl` FluentValidation ext + predicate |
| 4 | **Unit of Work** — 4 identical classes | `Core/Identity/Content/MailerUnitOfWork` each `ctx => ctx.SaveChangesAsync` | generic `UnitOfWork<TContext>` base (per-module marker ifaces stay for DI) |
| 5 | **File-upload validation** | `FileValidation.ValidAvatar` (Identity) + Content `EditorialValidation` both do size/MIME/extension off `FileConstants` | `BeValidFileSize`/`BeValidImageType`/`BeValidFileExtension` predicates (localized messages stay module-side) |
| 6 | **Error-catalog DI registration** | `ContentModule` hand-registers 44 `*Errors`/`*ErrorMessage` line-by-line (107 `AddScoped` total) | convention registrar `services.AddErrorCatalog(assembly)` (the facade classes stay in modules) |

`ColorContrastHelper` (Core) is generic but has **one** consumer (Core's `ImageColorService`) — **leave
it in Core**; move only if a second module needs it. `MappingRegistration` duplication (Identity +
Content) is a marginal win — optional `MapsterConfigBuilder` helper, low priority.

---

## Leaves the shared foundation entirely (→ owning module)

| File(s) | Today | New home | Why |
|---|---|---|---|
| `UserConstants`, `RoleConstants`, `PermissionConstants`, `SessionConstants`, `JwtClaimsConstants` | BuildingBlocks/Constants | **Identity** (`Identity.Domain/Constants`, keep JWT claims beside `IClaimsProvider`) | grep: consumed **only** by Identity |
| `CloudinarySettings`, `CloudinaryExtensions` | Shared/Application | **Core** (the file-management module) | Cloudinary is Core's storage provider; nothing else consumes it |

`AppEnvironment` (env-var accessor) stays in BuildingBlocks for now but is a grab-bag whose `Jwt()` /
`Cloudinary()` / `EmailProvider()` / `FrontendBaseUrl()` members each serve one module — a candidate to
dissolve per-module in a later pass (bind as validated `IOptions`, [08 §10](../08-cross-cutting.md)).

---

## The `shared/` tree

```text
shared/
├── src/
│   ├── SharedKernel/                  # domain-model primitives — ZERO packages
│   │   ├── Entities/                  #   IEntity, Entity, IAggregate, Aggregate
│   │   ├── Events/                    #   IDomainEvent
│   │   ├── Audit/                     #   EnumAuditActor
│   │   ├── Abstractions/              #   IRepository
│   │   ├── Specifications/            #   ISpecification, Specification, And/Or/Not  (moved from Application)
│   │   ├── ValueObjects/              #   (optional) Slug, ValueObject base           (SlugHelper hoisted from Core)
│   │   └── SharedKernel.csproj
│   └── BuildingBlocks/                # cross-cutting technical plumbing — framework-bound
│       ├── Cqrs/                      #   ICommand/IQuery/IRequest/I*Handler/IDispatcher + Dispatcher + domain-event pub/reg
│       ├── Behaviors/                 #   ValidationDecorator, LoggingDecorator
│       ├── Exceptions/                #   11 exception types + SharedExceptionMessage(.resx) + strategies + registry + handler
│       ├── Pagination/               #   PaginatedRequest, PaginatedResult, ToPaginatedResultAsync (hoisted)
│       ├── Persistence/               #   IUnitOfWork, UnitOfWork<TContext> (hoisted), DbSet/Specification/EntityEntry EF exts
│       ├── Modules/                   #   BaseModule, ModuleOptions, IDataSeeder, ApplicationBuilder migrate/seed exts
│       ├── Interceptors/              #   AuditableEntityInterceptor, DispatchDomainEventsInterceptor
│       ├── Actor/                     #   ICurrentActor, HttpCurrentActor
│       ├── Validation/                #   IsValidGuid, ValidationUtils (hoisted), ValidHttpUrl + file-upload predicates (hoisted)
│       ├── Web/                       #   Carter/ApiVersion/Authorization/Swagger/ResourceNotFound exts, middleware, RouteMetadata, ApiVersionUrl
│       ├── RateLimiting/              #   3 builders + RateLimitingExtension + RateLimitPolicies + 10 numeric constants
│       ├── Scheduling/                #   IScheduledJob, QuartzExtension
│       ├── Errors/                    #   AddErrorCatalog registrar (hoisted)
│       ├── Configuration/             #   AppEnvironment, AuditableDto
│       ├── Constants/                 #   FileConstants, UserRolePolicies, AccountStatusPolicies
│       └── BuildingBlocks.csproj
└── tests/
    ├── SharedKernel.Unit.Tests/
    ├── BuildingBlocks.Unit.Tests/
    └── Shared.TestKit/               # builders/factories/constants + the whole-app integration harness
```

Reference direction: `SharedKernel` (leaf, zero packages) ← `BuildingBlocks` (references SharedKernel +
frameworks) ← every module's layers. A module's **`*.Domain`** references **only `SharedKernel`**; its
`*.Application`/`*.Infrastructure` reference `BuildingBlocks`.

**csproj packages after the split:** `SharedKernel` = **none**. `BuildingBlocks` = EF Core + Npgsql +
EFCore.NamingConventions + Carter + Asp.Versioning + Quartz + FluentValidation + Swashbuckle + Scrutor +
Microsoft.Extensions.* (the plumbing's real deps). **Drop the dead refs** the sweep found in today's
`Shared.csproj`: **Bogus, Mapster, Microsoft.Extensions.Caching.StackExchangeRedis** — no file imports
them.

---

## Litmus (which of the two, or a module?)

1. Domain-model type a `*.Domain` needs, **zero packages**? → **SharedKernel**.
2. Cross-cutting technical plumbing (CQRS, exceptions, pagination, module system, interceptors,
   web/rate-limit/scheduling extensions, genuinely-global constants)? → **BuildingBlocks**.
3. Meaningful to exactly one module (its constants, its external-service settings)? → **that module**.

The rule that would have prevented the original drift: **name the owner before you name the file** — and
if the answer is "everyone, and it touches no framework" it's SharedKernel; "everyone, and it's
plumbing" is BuildingBlocks; anything else has a module owner.

See also: [12](../12-shared-kernel-and-buildingblocks.md) (reasoning), [03](03-full-target-structure.md)
(full module trees), [11](../11-project-structure-and-packages.md) (layer-split + CPM).
