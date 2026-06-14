# Integration Test Coverage Overview

**Generated:** 2026-06-21
**Report Source:** ReportGenerator 5.4.5.0 from Coverlet OpenCover XML
**Target:** 100% on specifications, validators, query builders, and error messages; ~96% overall

## Current Coverage by Module

| Module | Covered | Uncovered | Coverable | Line % | Branch % |
| --- | --- | --- | --- | --- | --- |
| Api (Program) | 78 | 0 | 78 | 100% | - |
| BuildingBlocks | 36 | 1 | 37 | 97.2% | 50% |
| Content | 11,515 | 524 | 12,039 | 95.6% | 56% |
| Core | 110 | 178 | 288 | 38.1% | 6.7% |
| Identity | 7,167 | 574 | 7,741 | 92.5% | 44.9% |
| Shared | 441 | 291 | 732 | 60.2% | 50% |
| **Total** | **19,347** | **1,568** | **20,915** | **92.5%** | **45.2%** |

## 100% Coverage Targets

These 4 categories must reach 100% integration test coverage across all modules:

### Specifications

| Module | At 100% | At 0% | Partially | Dead Code |
| --- | --- | --- | --- | --- |
| Content | 42 specs | 4 specs (ArticleTagByArticleId, GossipArticle, ShortVideoBookmarkByUserAndShortVideo, VideoByOrderItemId) | 0 | 1 (TagByNameSpecification — zero callers) |
| Identity | 0 specs | 20 specs (all reachable via query builder filter tests and handler tests) | 0 | 0 |
| Core | 0 specs | 7 specs (all structurally blocked — FileService stub) | 0 | 0 |
| Shared | 1 (AndSpecification) | 2 (NotSpecification — coverable; OrSpecification — dead code) | 1 (Specification base 9%) | 0 |
| **Total** | **43** | **33** | **1** | **2 dead** |

Coverable to 100%: 24 specs (4 Content + 20 Identity). 7 Core specs blocked. 2 dead code. NotSpecification coverable via query builder `.Not()` calls.

### Validators

| Module | At 100% | Below 100% | At 0% (coverable) | At 0% (blocked) |
| --- | --- | --- | --- | --- |
| Content | All handler validators | 6 shared validators (58-75%) | 3 Cloudinary validators (coverable via invalid payloads) | 0 |
| Identity | All handler validators at 100% | FileValidation (70.4%), ValidationUtils (83.3%) | 7 handler validators (covered by handler tests) | 0 |
| Shared | — | ValidationExtension (85%) | — | — |
| **Total** | — | **9** | **10** | **0** |

All validators are coverable. Cloudinary-blocked validators are coverable because FluentValidation runs BEFORE the handler — sending invalid payloads covers validator lines without hitting Cloudinary.

### Query Builders

| Module | At 100% | Below 100% |
| --- | --- | --- |
| Content | 4/4 (Article, Lyrics, ShortVideo, Video + Commerce builders) | 0 |
| Identity | 0/3 | SessionQueryBuilder (38%), PermissionQueryBuilder (77.2%), RoleQueryBuilder (77.2%) |
| **Total** | **4** | **3** |

All 3 Identity query builders are coverable via filter query parameter tests on GET endpoints.

### Error Messages

| Module | Classes | Below 100% | Approach |
| --- | --- | --- | --- |
| Content | 15 error + 13 message classes | All below 100% (16-90%) | Each error method maps to a handler error path. Negative test cases trigger uncovered methods. |
| Identity | 2 error + 4 message classes | All below 100% (25-80%) | Covered transitively by handler tests and error-path tests. |
| Core | 1 error + 3 message classes | All below 100% (5-50%) | Partially blocked by Cloudinary stub. Coverable methods covered by file upload tests. |
| Shared | 1 message class | SharedExceptionMessage (50%) | 2 methods blocked (RateLimitExceeded, InvalidIdentifier). Rest covered by 404 tests. |
| **Total** | **39 classes** | **39** | ~150 negative test cases cover all reachable error methods |

## Test Plan Summary

| Wave | Focus | Tests | Est. Lines |
| --- | --- | --- | --- |
| Wave 1 | Identity 0% handlers (AdminSignOut, AdminRefreshToken, PublicChangePassword, PublicResetPassword, PublicSetPassword, PublicRevokeSession, PublicUpdateAvatar) + AccountStatus | 27 | +200 |
| Wave 2 | Identity query builder filters + specs (Session, Permission, Role filters; SessionValidation; WangkanaiClientOriginDetection) | 15 | +80 |
| Wave 3 | Identity error paths (Role/Permission state conflicts, assignment errors, duplicate names, HardDeleteRole core) | 20 | +120 |
| Wave 4 | Content error paths — catalog state (Category, ContentType, PricingTier, PromotionLevel, Package, ShortVideo, Tag activate/deactivate/exists) | 30 | +150 |
| Wave 5 | Content error paths — editorial + commerce + interactions (Article/Video state machines, ContentOrder lifecycle, Article/ShortVideo interactions, Playlist, Lyrics, Customer) | 35 | +170 |
| Wave 6 | Content validators + specs + repos (Cloudinary validator invalid payloads, shared validator branches, 4 Content specs, repository search paths) | 25 | +100 |
| **Total** | | **~152** | **+820 lines** |

## Projected Coverage After All Waves

| Metric | Current | After All Waves | Maximum Achievable |
| --- | --- | --- | --- |
| Covered lines | 19,347 | ~20,167 | ~20,039 |
| Line coverage | 92.5% | ~96.4% | ~95.8% |
| Uncovered lines | 1,568 | ~748 | ~876 |

## Why Not 99%?

99% line coverage (20,706 / 20,915) requires covering all but 209 lines. After integration tests, ~876 lines remain uncoverable:

| Category | Lines | Reason |
| --- | --- | --- |
| Stubbed services (Cloudinary, YouTube, FileService) | ~130 | External HTTP calls |
| Cloudinary-blocked handlers (NOT validators) | ~21 | Need real file upload to reach handler code |
| Startup extensions + DI config | ~150 | Run once at boot |
| Rate limit builders | ~60 | Startup config |
| Exception handler strategies (infra errors) | ~40 | Need 502/500/429/format errors |
| Background jobs | ~20 | Cron-triggered |
| Domain entity protected methods | ~150 | Internal state, no endpoint reaches them |
| Value objects + abstract classes | ~55 | Enum-like, abstract |
| Dead code (unused constructors, methods) | ~80 | Never called |
| Interceptor sync methods | ~20 | ASP.NET uses async |
| Infrastructure internals | ~70 | DI, decorators, middleware |
| Aggregate internals | ~15 | No domain events yet |
| File specifications (Core) | ~21 | Behind stubbed FileService |
| Remaining | ~44 | Various edge cases |

**To reach 99%, you need integration tests PLUS:**

| Additional Test Type | Lines Recovered |
| --- | --- |
| Unit tests for entity domain methods | ~150 |
| Unit tests for value objects | ~40 |
| Startup integration tests (test `Program.cs` config) | ~150 |
| Infrastructure exception fault injection tests | ~40 |
| Background job tests (Quartz test harness) | ~20 |
| Unit tests for dead code or remove dead code | ~80 |
| **Subtotal** | **~480** |

Combined with integration test gains (+820), total would be ~20,647 / 20,915 = **98.7%**. True 99% requires additionally covering some of the stubbed service paths (~130 lines) via contract tests or a real Cloudinary test double.

## Dead Code Identified

These have zero callers and should be removed or excluded from coverage:

| Code | Module | Lines |
| --- | --- | --- |
| `TagByNameSpecification` | Content | ~2 |
| `ContentTypeErrors.NotFound` | Content | ~2 |
| `TagErrors.NotFound` | Content | ~2 |
| `EditorialValidation.ValidArticleId/ValidVideoId/ValidLyricsId` | Content | ~6 |
| `UserErrors.CoreRoleCannotBeModified` | Identity | ~2 |
| `ValidationErrorMessage.StorageUrlCannotBeEmpty` | Core | ~2 |
| Exception 2-arg constructors + `Details` (5 classes) | Shared | ~20 |
| `ResourceNotFoundException(message)` | Shared | ~2 |
| `MethodNotAllowedException(message)` | Shared | ~2 |
| `OrSpecification`, `Specification.Or/IsSatisfiedBy/AndAll/OrAll` | Shared | ~15 |
| `Dispatcher.Send` (void) | Shared | ~5 |
| `Aggregate.AddDomainEvent` | Shared | ~5 |
| **Total dead code** | | **~65 lines** |

## Documentation Index

| Document | Covers |
| --- | --- |
| [01-identity-module.md](01-identity-module.md) | ~62 tests: 7 handler groups at 0%, 33 UserErrors methods, 20 specifications, 3 query builders, all validators, exception handlers, entity methods, services |
| [02-content-module.md](02-content-module.md) | ~90 tests: 15 error classes (100+ methods), 4 coverable specifications, 6 shared validators, 3 Cloudinary validators (invalid payload path), 5 repositories, entity domain methods |
| [03-core-module.md](03-core-module.md) | Structurally limited — 3 coverable FileErrors methods via Identity/Content tests; ~146 lines behind Cloudinary stub |
| [04-shared-module.md](04-shared-module.md) | Structurally limited — ~14 lines coverable (NotSpecification, ExceptionStrategyRegistry, SharedExceptionMessage); ~276 lines blocked; ~65 lines dead code |
| [05-test-execution-specs.md](05-test-execution-specs.md) | 6-wave execution plan, ~152 tests, seed data patterns, factory methods, coverage verification, dead code registry |
| [06-coverage-todo-tracker.md](06-coverage-todo-tracker.md) | **TODO checklist** — every file below 100% in target categories (specs, errors, validators, query builders, handlers, repos) with before/after coverage tracking |
