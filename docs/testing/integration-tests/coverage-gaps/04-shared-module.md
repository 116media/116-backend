# Shared Module - Integration Test Coverage Specifications

**Current Coverage:** 60.2% (441 / 732 lines) | Branch: 50%
**Uncovered Lines:** 291
**Target:** ~62% overall (structurally limited); coverable error messages to 100%

## 1. Structurally Uncoverable Code (~276 lines)

### 1.1 Exception Handler Strategies (~40 lines, all 0%)

These strategies only trigger on infrastructure-level errors that cannot be reliably produced via endpoint tests.

| Strategy | Lines | Trigger Condition |
| --- | --- | --- |
| `AuthenticationExceptionHandler` | ~8 | Framework-level auth (not the Identity module one) |
| `BadGatewayExceptionHandler` | ~8 | 502 from upstream proxy |
| `DefaultExceptionHandler` | ~8 | Unhandled exception catch-all |
| `FormatExceptionStrategy` | ~8 | Malformed JSON body (framework intercepts first) |
| `InternalServerExceptionHandler` | ~8 | 500 internal server error |
| `RateLimitExceededExceptionHandler` | ~8 | Rate limit exceeded (test limits are high) |

**Already covered strategies** (exercised by normal endpoint tests):
- `AuthorizationExceptionHandler` — via 403 responses
- `BadRequestExceptionHandler` — via 400 responses
- `ConflictExceptionHandler` — via 409 responses
- `NotFoundExceptionHandler` — via 404 responses
- `ValidationExceptionHandler` — via validation error responses

### 1.2 Rate Limit Builders (~60 lines, all 0%)

Run once at startup to configure rate limiting middleware. Not per-request code.

| Builder | Lines |
| --- | --- |
| `FixedWindowBuilder` | ~20 |
| `SlidingWindowBuilder` | ~20 |
| `TokenBucketBuilder` | ~20 |

### 1.3 App Configuration Extensions (~150 lines)

All extension methods run at `Program.cs` startup. Not reachable via endpoint tests.

| Extension | Lines |
| --- | --- |
| `ApiVersionExtension` | ~10 |
| `AuthorizationExtension` | ~15 |
| `CarterExtension` | ~10 |
| `CloudinaryExtensions` | ~10 |
| `CloudinarySettings` | ~5 |
| `CqrsExtension` | ~15 |
| `EnumSchemaFilter` | ~15 |
| `EnumReferenceDocumentFilter` | ~15 |
| `ExceptionHandlerExtension` | ~10 |
| `LocalizationExtension` | ~10 |
| `QuartzExtension` | ~10 |
| `RateLimitingExtension` (14.2%) | ~15 |
| `SwaggerExtension` | ~20 |
| `ValidationExtension` (85%) | ~10 |

### 1.4 Interceptor Sync Methods (~20 lines)

ASP.NET uses async paths exclusively. The sync `SavingChanges()` overrides are never called.

| Interceptor | Coverage | Blocked Lines |
| --- | --- | --- |
| `AuditableEntityInterceptor` (85%) | Sync `SavingChanges` + `ResolveActor` System branch | ~10 |
| `DispatchDomainEventsInterceptor` (60%) | Sync `SavingChanges` + null-context guard | ~10 |

### 1.5 Other Infrastructure

| Class | Coverage | Blocked Lines | Reason |
| --- | --- | --- | --- |
| `HttpCurrentActor` | 75% | ~5 | Null-context path (background job only) |
| `BaseModule` | 83.3% | ~5 | Module registration internals |
| `Dispatcher` | 57.8% | ~15 | Send(void) never called; error branches defensive |
| `LoggingDecorator` | 73.6% | ~5 | >3s performance warning branch |

## 2. Exception Base Classes

| Class | Coverage | Analysis |
| --- | --- | --- |
| `AuthenticationException` | 25% | 1-arg constructor covered. 2-arg `(message, details)` + `Details` property: **dead code** — never used anywhere |
| `AuthorizationException` | 25% | Same pattern — 2-arg + Details = dead code |
| `BadRequestException` | 25% | Same — 2-arg + Details = dead code |
| `ConflictException` | 25% | Same — 2-arg + Details = dead code |
| `NotFoundException` | 78.5% | `CleanEntityName` covered. 2-arg overload = dead code |
| `ResourceNotFoundException` | 50% | `(path, method)` used by middleware. `(message)` overload = **dead code** |
| `MethodNotAllowedException` | 50% | `(path, method)` used by middleware. `(message)` overload = **dead code** |
| `InternalServerException` | 0% | Only thrown by stubbed services. Structurally blocked |
| `InvalidFormatException` | 0% | Only thrown by FormatExceptionStrategy. Structurally blocked |
| `RateLimitExceededException` | 0% | Only thrown by RateLimitExceededExceptionHandler. Structurally blocked |
| `BadGatewayException` | 0% | Only thrown by BadGatewayExceptionHandler. Structurally blocked |

The 1-arg constructors are all covered by existing endpoint tests. The 2-arg constructors with `Details` are never used in the entire codebase — they are dead code.

## 3. Coverable Shared Code

### 3.1 SharedExceptionMessage (50%)

| Method | Covered? | Called By |
| --- | --- | --- |
| `Localizer` | Partially | DI property |
| `RateLimitExceeded(seconds)` | No | `RateLimitExceededExceptionHandler` — blocked |
| `EntityNotFoundById(entity, id)` | Yes | `NotFoundExceptionHandler` — covered by 404 tests |
| `EntityNotFoundByKey(entity, key, value)` | Partially | `NotFoundExceptionHandler` — covered by some 404 tests |
| `InvalidIdentifier()` | No | `FormatExceptionStrategy` — blocked |

**Test to improve:** Any 404 test using a string key lookup (e.g., `GET /public/articles/{slug}` with non-existent slug) covers `EntityNotFoundByKey`.

### 3.2 ExceptionStrategyRegistry (65%)

| Uncovered Path | Coverable? |
| --- | --- |
| Strategy resolution via inheritance traversal | Yes — `ResourceNotFoundException` extends `NotFoundException`, triggered by undefined route |
| Default strategy fallback | Partially — requires unregistered exception type |

**Test:** `GET /api/v1/nonexistent-route` → triggers `ResourceNotFoundMiddleware` → `ResourceNotFoundException` → registry resolves via `NotFoundException` strategy inheritance.

### 3.3 ExceptionHandler (92.8%)

| Uncovered Path | Coverable? |
| --- | --- |
| `DetermineLogLevel` FormatException case | No — framework intercepts malformed JSON |
| `DetermineLogLevel` default Error case | Partially — requires unregistered exception |

### 3.4 Specification Base Classes

| Class | Coverage | Analysis |
| --- | --- | --- |
| `Specification<T>` | 9% | `And()` heavily used — covered. `Not()` used in 2 places — coverable. `Or()`, `IsSatisfiedBy()`, `AndAll()`, `OrAll()` = **dead code** (zero callers) |
| `NotSpecification<T>` | 0% | Used by `.Not()` in `ShortVideoQueryBuilder` and `CategoryRepository`. **Coverable** — integration tests for short video listing or category queries exercise it |
| `OrSpecification<T>` | 0% | **Dead code** — `.Or()` never called anywhere in application code |

**Test to cover NotSpecification:** `GET /admin/short-videos` with filter params that trigger `ShortVideoQueryBuilder` `.Not()` path. Or any category query that uses the negated spec.

### 3.5 ValidationExtension (85%)

Two `IsValidGuid` overloads. The 5-param overload with `requiredKey`/`invalidKey` has an uncovered `isRequired: true` branch.

**Test:** Any validator using `IsValidGuid` with `isRequired: true` and sending an empty GUID value triggers this path. Covered transitively by Identity/Content handler tests that validate GUID parameters.

### 3.6 Aggregate<T> (28.5%)

| Method | Covered? | Analysis |
| --- | --- | --- |
| `AddDomainEvent` | No | **Dead code** — no entity in the codebase calls `AddDomainEvent()`. Domain events are not yet implemented |
| `ClearDomainEvents` | Partially | Called by `DispatchDomainEventsInterceptor`, but only when events exist |
| `DomainEvents` property | Partially | Same |

### 3.7 ApiVersionUrl (BuildingBlocks, 87.5%)

| Uncovered Path | Coverable? |
| --- | --- |
| `GetVersionFromContext` fallback: no route data | Yes — hit a non-versioned endpoint |
| `GetVersionFromContext` fallback: unparseable version | Partially — requires malformed URL |

## 4. Dead Code (exclude from coverage target)

| Code | Reason |
| --- | --- |
| Exception 2-arg constructors + `Details` property (Auth, Authz, BadRequest, Conflict, NotFound) | Never used anywhere |
| `ResourceNotFoundException(message)` 1-arg constructor | Never used |
| `MethodNotAllowedException(message)` 1-arg constructor | Never used |
| `OrSpecification<T>` | `.Or()` never called |
| `Specification<T>.Or()`, `.IsSatisfiedBy()`, `.AndAll()`, `.OrAll()` | Never called |
| `Dispatcher.Send` (void overload) | Never called |
| `Aggregate<T>.AddDomainEvent` | No domain events raised |

## 5. Realistic Coverage Target

| Category | Current Lines | Achievable | Blocked |
| --- | --- | --- | --- |
| Exception strategies | ~48 covered | +0 | ~40 |
| Rate limit builders | 0 | +0 | ~60 |
| Startup extensions | 0 | +0 | ~150 |
| Interceptor sync methods | 0 | +0 | ~20 |
| Exception base classes | ~25% avg | +0 (dead code) | ~30 |
| Decorators/Dispatcher | ~40 covered | +5 (NotSpecification) | ~15 |
| Specification base | ~10 covered | +5 (NotSpecification) | ~10 |
| SharedExceptionMessage | 50% | ~60% | ~2 methods blocked |
| ExceptionStrategyRegistry | 65% | ~75% | default fallback |
| Infrastructure | ~80% avg | +0 | ~15 |
| **Module total** | **441/732 (60.2%)** | **~455/732 (62.1%)** | **~276 lines** |

**Conclusion:** Shared module will remain at ~60-62% coverage. Nearly all uncovered code is infrastructure that runs at application startup, dead code with unused constructors, or exception strategies requiring fault injection. The ~14 additional lines of improvement come from:
1. `NotSpecification` coverage via short video/category query tests (+5 lines)
2. `ExceptionStrategyRegistry` inheritance resolution via undefined route test (+5 lines)
3. `SharedExceptionMessage.EntityNotFoundByKey` via slug-based 404 tests (+4 lines)

No dedicated Shared module tests are worth writing — all gains come naturally from Identity/Content tests.
