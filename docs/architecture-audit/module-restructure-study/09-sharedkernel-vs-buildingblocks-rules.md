# 09 — SharedKernel vs BuildingBlocks: the rule, and the adapted structure

**The rule (adopted, non-negotiable):**

- **`SharedKernel` = reusable DOMAIN concepts.** Things a domain expert would recognise, expressible
  with **zero framework/infrastructure dependencies**. The vocabulary a module's *Domain layer* speaks.
- **`BuildingBlocks` = reusable TECHNICAL / ARCHITECTURAL infrastructure.** CQRS, EF, HTTP, DI,
  serialization, caching, scheduling, exception→HTTP mapping, the module system. May depend on any
  framework.

This doc gives the *why*, the decision test, and — adapted to **this** codebase (Identity / Content /
Core / Mailer, not the generic e-commerce template) — the **final layered project structure**. It does
not overwrite [08](08-shared-foundation-structure.md); where it sharpens 08, that is called out
explicitly. Consistency with the wider audit is checked at the end.

---

## Why two, not one

They change for different reasons and have different dependency rules:

- SharedKernel changes when the **business model** gains a shared concept. It must stay
  **dependency-free** so any module's Domain can reference it without inheriting EF/ASP.NET/Carter.
- BuildingBlocks changes when the **technical platform** does (swap EF, add Redis, change the HTTP
  edge). It is *allowed* to drag frameworks, so it must be kept **out of** the domain.

Fuse them and the domain inherits the web stack — the exact rot [01 §1.9](../01-composition-root-and-shared-kernel.md)
found in today's `Shared`.

## The decision test

For any candidate, ask in order:

1. **Is it a business concept** a domain expert would name (an order total, a slug, "a payment cannot
   complete twice"), expressible with **no framework package**? → **SharedKernel**.
2. **Is it technical machinery** (a command bus, an EF interceptor, a ProblemDetails mapper, a rate
   limiter)? → **BuildingBlocks**, at the layer that machinery lives in.
3. **Is it meaningful to exactly one module?** → **that module** (not the shared foundation at all).

### Edge cases (where people misplace things)

| Candidate | Home | Why |
|---|---|---|
| `Entity` / `AggregateRoot` / `ValueObject` / `IDomainEvent` base | **SharedKernel** | tactical domain base types, zero deps |
| **`Result<T>` / `Error`** | **SharedKernel** | how the *domain* expresses success/failure — a concept, not tech |
| **`DomainException` / `BusinessRule`** | **SharedKernel** | domain-rule violations are domain vocabulary |
| **`Specification<T>` pattern** | **BuildingBlocks.Domain** | a *query-abstraction pattern* (technical), not a business concept — it merely operates *on* domain objects. **(Sharpens [08](08-shared-foundation-structure.md), which parked it in SharedKernel.)** |
| CQRS `ICommand`/`IQuery`/`IDispatcher` | **BuildingBlocks.Application** | application messaging machinery |
| `NotFoundException`/`ConflictException` (HTTP-shaped) | **BuildingBlocks** | they encode an HTTP status — technical, not a domain error |
| `IRepository` / `IUnitOfWork` | **BuildingBlocks.Application** (contract) + `.Infrastructure` (EF impl) | persistence is a technical seam |
| `EnumAuditActor` | **SharedKernel** | audit vocabulary carried on `IEntity` |
| a concrete VO like `Money`, `Slug`, `Email` | **its owning module** unless ≥2 modules use it | one-module concept ≠ shared. Only the `ValueObject` *base* is shared |
| JWT token service, password hasher | **Identity** | auth is Identity's bounded context, not generic infra |
| email sending, file storage | **Mailer / Core(Storage)** | those are modules' concerns, not building blocks |

**The one-line litmus:** *name the owner before you name the file.* Everyone + no framework →
SharedKernel. Everyone + framework → BuildingBlocks. One module → that module.

---

## Why `SharedKernel` is ONE project and `BuildingBlocks` is FOUR

This follows directly from the rule and **matches your reference tree** (SharedKernel = 1 `.csproj`,
BuildingBlocks = 4).

- A SharedKernel is **domain-only by definition** — it occupies exactly **one** clean-architecture
  layer (Domain). There is no shared *Application*, *Infrastructure*, or *Presentation* domain concept;
  those are precisely what BuildingBlocks provides. So splitting SharedKernel "by layer" yields **one**
  project. Creating empty `SharedKernel.Application/.Infrastructure` projects would be pure ceremony
  ([02](02-decision-b-layers-as-projects.md) / [07](07-migration-plan-and-verdict.md) warn against
  exactly that). *If* a genuinely shared, domain-flavoured application concern ever emerges, it becomes
  `SharedKernel.Application` then — not pre-emptively.
- **Technical concerns exist at every layer**, so BuildingBlocks splits into four layer-projects:
  `BuildingBlocks.Domain` (patterns over the domain — Specification), `.Application` (CQRS, behaviors,
  pagination), `.Infrastructure` (EF, outbox, caching, scheduling), `.Presentation` (HTTP edge).

Reference direction (inward only, each a project):

```text
SharedKernel                     (0 packages)
    ▲
BuildingBlocks.Domain            (refs SharedKernel; System.Linq.Expressions only)
    ▲
BuildingBlocks.Application       (refs SharedKernel + .Domain; FluentValidation, DI abstractions)
    ▲                     ▲
BuildingBlocks.Infrastructure   BuildingBlocks.Presentation
(refs .Application; EF,          (refs .Application; ASP.NET, Carter,
 Npgsql, Quartz, Redis)           Swashbuckle, Asp.Versioning)
```

A module's `*.Domain` references **only `SharedKernel`**; its `*.Application` references
`BuildingBlocks.Application`; `*.Infrastructure` and `*.Presentation` reference the matching
BuildingBlocks layer.

---

## Adapting the reference to THIS codebase

Your reference is a generic e-commerce template; much of it (Kafka, RabbitMQ, Inbox, Stripe, gRPC,
generic Email/Files/JWT) is **not in this project and not on its roadmap**. Adapted honestly, every
entry below is either **`exists`** (a real file that moves here) or **`add`** (a new file the audit
already recommends, with the reference). Anything in the template that maps to a module or to nothing is
in the **Omitted** table.

### Omitted from the shared foundation (and where it actually goes)

| Template item | Why not shared here |
|---|---|
| Kafka / RabbitMQ / `IMessageBus` / Inbox | in-process modular monolith — no cross-process bus exists or is planned ([02 §2](../02-module-boundaries.md)). Cross-module comms are in-process; the only durability gap is a **domain-event outbox**, which *is* included below |
| `IIntegrationEventPublisher` / `IntegrationEvent` | same — no integration-event transport exists |
| `Idempotency*`, `gRPC`, `StripePaymentProvider` | not in this codebase |
| generic `IEmailSender` / SMTP | **Mailer** owns email ([05](../05-core-and-mailer.md), [14](../14-notifications-email-and-subscriptions.md)) |
| generic `IFileStorage` / `LocalFileStorage` | **Core→Storage** owns files/Cloudinary ([13](../13-core-storage-and-settings-module.md)) |
| `JwtTokenService`, `PasswordHasher` | **Identity** owns auth ([07](../07-identity-and-security.md)) — BuildingBlocks provides only the *generic* permission-authorization machinery |
| `IDateTimeProvider` | this project uses BCL **`TimeProvider`** and the audit praises it ([01 “what is done well”](../01-composition-root-and-shared-kernel.md)) — keep it; no custom abstraction |
| concrete VOs `Money`/`Slug`/`Email`/`Address` | one-module concepts → their module (`Money`/`Slug`→Content [03 §8](../03-content-domain.md), `Email`→Identity). Only the `ValueObject` **base** is shared |

---

## Final folder structure

Every file marked `(exists)` = a real file relocating here; `(add)` = new, with the audit finding that
calls for it. `SharedKernel` = 1 project; `BuildingBlocks` = 4 layer-projects.

### `SharedKernel` — reusable domain concepts (ONE project, zero packages)

```text
SharedKernel/
├── SharedKernel.csproj                        # zero PackageReferences
├── Abstractions/
│   ├── IEntity.cs                             (exists)
│   ├── Entity.cs                              (exists)  # Id → protected set; audit → internal set (01 §1.9)
│   ├── IAggregateRoot.cs                      (exists, renamed from IAggregate)
│   ├── AggregateRoot.cs                       (exists, renamed from Aggregate)
│   ├── IHasDomainEvents.cs                    (add)     # split the event surface off the aggregate
│   ├── IValueObject.cs                        (add)
│   └── ValueObject.cs                         (add)     # base only; no VO base exists today
├── Events/
│   ├── IDomainEvent.cs                        (exists)  # stable EventId + OccurredOn (01 §1.8)
│   └── DomainEvent.cs                         (add)     # base record stamping identity/time (01 §1.8)
├── Audit/
│   └── EnumAuditActor.cs                      (exists)
├── Results/
│   ├── Result.cs                              (add)     # no Result type exists today
│   ├── ResultOfT.cs                           (add)
│   └── ResultExtensions.cs                    (add)
├── Errors/
│   ├── Error.cs                               (add)
│   └── ErrorType.cs                           (add)
├── Rules/
│   ├── IBusinessRule.cs                       (add)     # optional pattern; today rules are inline guards
│   ├── BusinessRule.cs                        (add)
│   └── BusinessRuleViolationException.cs      (add)
├── Exceptions/
│   └── DomainException.cs                     (add)     # code-only; replaces errors-injection (03 §6)
├── Enumerations/
│   └── Enumeration.cs                         (add)     # optional smart-enum; today raw enums
└── Primitives/
    └── StronglyTypedId.cs                     (add)     # strongly-typed IDs (03 §8); today raw Guid
```

Notes: the empty `IRepository` marker is **deleted** ([01 §1.9](../01-composition-root-and-shared-kernel.md)) —
a real repository contract lives in `BuildingBlocks.Application`. Several entries are `add` because the
codebase is currently thin on domain primitives (no `Result`, no VO base, no strongly-typed IDs); each
is an audit recommendation, not invention.

### `BuildingBlocks.Domain` — technical patterns over the domain

```text
BuildingBlocks.Domain/
├── BuildingBlocks.Domain.csproj               # refs SharedKernel; System.Linq.Expressions only
└── Specifications/
    ├── ISpecification.cs                       (exists, moved from Shared/Application/Specifications)
    ├── Specification.cs                        (exists)
    ├── AndSpecification.cs                     (exists)
    ├── OrSpecification.cs                      (exists)
    └── NotSpecification.cs                     (exists)
```

(The EF evaluator `ApplySpecification` is framework-bound → `BuildingBlocks.Infrastructure`.)

### `BuildingBlocks.Application` — reusable application machinery

```text
BuildingBlocks.Application/
├── BuildingBlocks.Application.csproj           # refs SharedKernel + .Domain; FluentValidation
├── Cqrs/
│   ├── ICommand.cs                             (exists)
│   ├── ICommandHandler.cs                      (exists)
│   ├── IQuery.cs                               (exists)
│   ├── IQueryHandler.cs                        (exists)
│   ├── IRequest.cs                             (exists)
│   ├── IRequestHandler.cs                      (exists)
│   └── IDispatcher.cs                          (exists)
├── Behaviors/
│   ├── ValidationBehavior.cs                   (exists, = ValidationDecorator)
│   ├── LoggingBehavior.cs                      (exists, = LoggingDecorator; stop logging secrets 01 §1.2)
│   └── TransactionBehavior.cs                  (add)     # no transaction boundary today (04 §7)
├── Abstractions/
│   ├── Persistence/
│   │   ├── IUnitOfWork.cs                      (exists)
│   │   └── IRepository.cs                      (add)     # real contract; the empty marker is deleted
│   ├── Security/
│   │   └── ICurrentActor.cs                    (exists)
│   ├── Messaging/
│   │   ├── IDomainEventPublisher.cs            (exists)
│   │   └── IDomainEventHandler.cs              (exists)
│   └── Caching/
│       └── ICacheService.cs                    (add)     # distributed cache (04 §8)
├── Exceptions/                                 # technical (HTTP-shaped) exceptions handlers throw
│   ├── NotFoundException.cs                    (exists)
│   ├── ConflictException.cs                    (exists)
│   ├── BadRequestException.cs                  (exists)
│   ├── AuthenticationException.cs              (exists)
│   ├── AuthorizationException.cs               (exists)
│   ├── BadGatewayException.cs                  (exists)
│   ├── InternalServerException.cs              (exists)
│   ├── MethodNotAllowedException.cs            (exists)
│   ├── RateLimitExceededException.cs           (exists)
│   ├── ResourceNotFoundException.cs            (exists)
│   └── InvalidFormatException.cs               (exists)
├── Pagination/
│   ├── PaginatedRequest.cs                     (exists)  # clamp pageSize in the ctor (06 §2)
│   └── PaginatedResult.cs                      (exists)
├── Validation/
│   └── ValidationExtensions.cs                 (exists, = IsValidGuid + hoisted ValidHttpUrl / ValidationUtils)
├── Dtos/
│   └── AuditableDto.cs                         (exists)
└── Constants/
    ├── RateLimitPolicies.cs                    (exists)  # policy NAME registry (endpoints + wiring both see it here)
    └── FileConstants.cs                        (exists)  # cross-module file/size/MIME rules
```

### `BuildingBlocks.Infrastructure` — frameworks & drivers

```text
BuildingBlocks.Infrastructure/
├── BuildingBlocks.Infrastructure.csproj        # refs .Application; EF Core, Npgsql, Quartz, Redis, Scrutor
├── Modules/
│   ├── BaseModule.cs                           (exists)
│   ├── ModuleOptions.cs                        (exists)
│   ├── IDataSeeder.cs                          (exists)
│   └── ApplicationBuilderExtensions.cs         (exists)  # UseMigration/UseSeed; move out of pipeline (04 §10)
├── Persistence/
│   ├── UnitOfWork.cs                           (add)     # generic UnitOfWork<TContext> (replaces 4 identical copies)
│   ├── Extensions/
│   │   ├── SpecificationExtensions.cs          (exists)  # ApplySpecification (the EF bridge)
│   │   ├── DbSetExtensions.cs                  (exists)
│   │   ├── EntityEntryExtensions.cs            (exists)
│   │   └── QueryableExtensions.cs              (add)     # ToPaginatedResultAsync (kills 32 dup slices)
│   └── Interceptors/
│       ├── AuditableEntityInterceptor.cs       (exists)
│       ├── DispatchDomainEventsInterceptor.cs  (exists)
│       ├── OutboxInterceptor.cs                (add)     # write domain events to outbox in same SaveChanges (01 §1.7)
│       └── SoftDeleteInterceptor.cs            (add)     # global soft-delete (04 §4)
├── Outbox/
│   ├── OutboxMessage.cs                        (add)     # domain-event outbox (01 §1.7)
│   ├── OutboxMessageConfiguration.cs           (add)
│   ├── IOutboxProcessor.cs                     (add)
│   └── OutboxProcessor.cs                      (add)
├── Messaging/
│   ├── DomainEventPublisher.cs                 (exists)
│   ├── IDomainEventHandlerRegistry.cs          (exists)
│   └── DomainEventHandlerRegistry.cs           (exists)
├── Security/
│   ├── HttpCurrentActor.cs                     (exists)  # ICurrentActor impl
│   └── Authorization/
│       ├── PermissionRequirement.cs            (add)     # generic permission machinery (07 S3 — concrete roles stay in Identity)
│       ├── PermissionAuthorizationHandler.cs   (add)
│       └── PermissionPolicyProvider.cs         (add)
├── Caching/
│   ├── Memory/MemoryCacheService.cs            (exists)  # AddMemoryCache today
│   └── Redis/RedisCacheService.cs              (add)     # distributed cache for multi-instance (04 §8)
├── RateLimiting/
│   ├── FixedWindowBuilder.cs                   (exists)  # partition per caller (01 §1.1)
│   ├── SlidingWindowBuilder.cs                 (exists)
│   ├── TokenBucketBuilder.cs                   (exists)
│   ├── RateLimitingExtension.cs                (exists)
│   └── RateLimitConstants/*.cs (10)            (exists)  # numeric tuning
├── Scheduling/
│   ├── IScheduledJob.cs                        (exists)
│   └── QuartzExtension.cs                      (exists)  # add clustering for multi-instance (04 §8)
├── Resilience/
│   └── ResilienceExtensions.cs                 (add)     # retry / circuit-breaker / timeout (05 §7)
├── Http/
│   └── CorrelationDelegatingHandler.cs         (add)     # propagate correlation id (08 §11)
├── Observability/
│   ├── OpenTelemetryExtensions.cs              (add)     # tracing + metrics (08 §12)
│   └── HealthCheckExtensions.cs                (add)     # /health/live + /ready (08 §12)
├── Localization/
│   └── LocalizationExtension.cs                (exists)  # fix default culture / provider chain (08 §16)
├── Configuration/
│   └── AppEnvironment.cs                        (exists)  # migrate to validated IOptions (08 §10)
└── DependencyInjection/
    └── InfrastructureServiceExtensions.cs      (exists, = CqrsExtension + registrations)
```

### `BuildingBlocks.Presentation` — the HTTP edge

```text
BuildingBlocks.Presentation/
├── BuildingBlocks.Presentation.csproj          # refs .Application; ASP.NET, Carter, Swashbuckle, Asp.Versioning
├── Endpoints/
│   └── CarterRegistration.cs                   (exists, = CarterExtension)
├── Errors/
│   ├── ExceptionHandler.cs                     (exists)  # global IExceptionHandler
│   ├── ExceptionStrategyRegistry.cs            (exists)
│   ├── ProblemDetailsFactory.cs                (exists, = BaseExceptionStrategy; make it real RFC7807 08 §11)
│   ├── Strategies/*.cs (13)                    (exists)
│   └── Messages/SharedExceptionMessage(.resx)  (exists)  # localized error text
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs          (exists)
│   ├── CorrelationIdMiddleware.cs              (add)     # correlate trace id to logs (08 §11)
│   ├── SecurityHeadersMiddleware.cs            (add)     # HSTS / CSP / nosniff (08 §7)
│   ├── ResourceNotFoundMiddleware.cs           (exists)
│   └── SwaggerDescriptionMiddleware.cs         (exists)
├── Routing/
│   ├── ApiVersionExtension.cs                  (exists)  # replace static field holder (01 §1.14)
│   ├── EndpointGroupExtensions.cs              (add)     # MapAdminGroup / MapPublicGroup (06 §10)
│   └── ApiVersionUrl.cs                        (exists)  # HttpContext versioned-URL helper
├── Authorization/
│   └── AuthorizationExtension.cs               (exists, = WithAuthorization)
├── OpenApi/
│   ├── SwaggerExtension.cs                      (exists)  # gate behind non-prod (08 §7)
│   ├── SwaggerMiddlewareExtension.cs           (exists)
│   └── EnumSchemaFilter.cs                     (exists)
└── Metadata/
    └── RouteMetadata.cs                        (exists)
```

### What leaves the shared foundation entirely (→ owning module)

`UserConstants`, `RoleConstants`, `PermissionConstants`, `SessionConstants`, `JwtClaimsConstants` →
**Identity**; `CloudinarySettings` + `CloudinaryExtensions` → **Core/Storage**; `SlugHelper` /
`ColorContrastHelper` → **Content** / **Core** respectively (one-module concepts). Dead package refs in
today's `Shared.csproj` (**Bogus, Mapster, StackExchangeRedis** — unused) are dropped.

---

## Consistency with the rest of the audit

- **Modules** stay Identity / Content / Core / Mailer, with the planned renames intact —
  Core→**Storage** ([13](../13-core-storage-and-settings-module.md)), Mailer→**Notifications**
  ([14](../14-notifications-email-and-subscriptions.md)), plus the new **Settings** module
  ([13](../13-core-storage-and-settings-module.md)). This doc changes only the *shared foundation*, not
  the module set.
- **No message bus / integration events** — matches the in-process reality
  ([02 §2](../02-module-boundaries.md)); the only durability addition is the **domain-event outbox**
  the audit already recommends ([01 §1.7](../01-composition-root-and-shared-kernel.md)).
- **Specification → `BuildingBlocks.Domain`** sharpens [08](08-shared-foundation-structure.md) (which
  had it in SharedKernel) under the clearer rule: a specification is a technical *pattern*, not a
  business concept. That one line in 08 should be reconciled to point here.
- Every `(add)` traces to an existing audit finding (cited inline); nothing is invented.

**One-line summary:** `SharedKernel` is the one Domain-layer project of shared *business* concepts (zero
deps); `BuildingBlocks` is four layer-projects of shared *technical* machinery — and if it isn't clearly
one of those two, it belongs to a module.
