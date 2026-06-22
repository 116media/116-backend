# Integration Test Improvement — Overview

This corpus documents everything wrong with the current integration test suite
(`apps/backend/tests/Integration`) and the concrete work required to fix it. It
is the source of truth for the overhaul tracked in
[`06-todo-tracker.md`](06-todo-tracker.md).

## Why this exists

The suite is **broad but shallow**. It exercises almost every endpoint, but the
assertions rarely prove the endpoint did the right thing — most only check the
HTTP status code. Combined with duplicated route strings and structural debt,
the suite gives a false sense of safety: it stays green even when response
bodies, persistence, and error shapes regress.

## Baseline metrics (at time of writing)

| Metric | Value |
| --- | --- |
| Endpoint test files (`*EndpointV1Tests.cs`) | ~211 |
| Endpoint tests asserting **only** the status code | **185 (87%)** |
| Tests using strongly-typed body reads (`ReadFromJsonAsync`) | **0** |
| Tests inspecting the body at all (stringly-typed `JsonDocument`) | 26 |
| `StatusCode.Should()` assertions | 874 |
| `ApiRoutes.*` references | 863 |
| Hardcoded `/api/v1/...` string literals | ~13 |
| Hardcoded sub-resource/action route segments | ~68 |
| `Randomizer.Seed` (Bogus determinism) calls | 0 |
| External service stubs | 2 (`ICloudinaryService`, `IYoutubeThumbnailService`) |

## The four problem areas

1. **Assertion quality** — the headline issue. See
   [`01-assertion-quality.md`](01-assertion-quality.md).
2. **Route duplication / hardcoding** — tests re-hardcode strings that already
   exist as `src/Modules/**/Constants/*RouteConstants.cs`. See
   [`02-route-constants.md`](02-route-constants.md).
3. **Non-deterministic / ad-hoc test data** — no Bogus seed, request payloads
   built as anonymous objects with hardcoded values. See
   [`03-test-data-bogus.md`](03-test-data-bogus.md).
4. **Structure & isolation** — stale `.gitkeep`, dead dir, shared-DB count
   assertions, fragile fixtures. See
   [`04-structure-and-isolation.md`](04-structure-and-isolation.md).

Conventions every new/updated test must follow live in
[`05-conventions.md`](05-conventions.md).

## Phase map

| Phase | Theme | Doc / Specs |
| --- | --- | --- |
| 0 | Documentation (this corpus) | all of `improvement/` |
| 1 | Structural cleanup | `04`, `specs/infrastructure/` |
| 2 | Route single source of truth | `02`, `specs/routes/` |
| 3 | Deterministic test data | `03`, `specs/test-data/` |
| 4 | Deep assertions (211 files) | `01`, `specs/assertions/` |
| 5 | Infrastructure improvements | `04`, `specs/infrastructure/` |

## How to use these docs

- Start here, then read `01`–`05` for the rationale of each area.
- Execute from the **specs** (`specs/`): each spec file is
  **Problem → Before → After → TODO checklist → Acceptance** and lists the exact
  files to touch.
- Track progress in [`06-todo-tracker.md`](06-todo-tracker.md).
