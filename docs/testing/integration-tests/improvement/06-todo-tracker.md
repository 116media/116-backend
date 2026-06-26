# 06 — Master TODO Tracker

Status legend: `[ ]` not started · `[~]` in progress · `[x]` done.

## Phase 1 — Structural cleanup
- [x] Remove `Workflows/.gitkeep`
- [x] Remove `Modules/Identity/Infrastructure/Mappers/.gitkeep`
- [x] Delete `Common/Constants/` (redundant)
- [x] Keep `Modules/Core/Infrastructure/Services/.gitkeep` (FileService gap)

## Phase 0 — Documentation
- [x] `00-overview.md` … `05-conventions.md`
- [~] `specs/` corpus (routes, assertions, test-data, infrastructure)

## Phase 2 — Routes ([`specs/routes/`](specs/routes/))
- [ ] `01-apiroutes-rewrite.md` — `TestConstants.ApiRoutes` composed from src constants
- [ ] `02-segment-replacements.md` — add `Routes` helper + replace ~68 segments / ~13 literals

## Phase 3 — Test data ([`specs/test-data/`](specs/test-data/))
- [ ] `01-bogus-determinism.md` — global `Randomizer.Seed`
- [ ] `02-builders-in-requests.md` — typed request builders

## Phase 4 — Assertions ([`specs/assertions/`](specs/assertions/))
- [ ] `specs/infrastructure/04-typed-http-helpers.md` — `ReadAsAsync<T>`, `ShouldBeProblem`, pagination helper (prerequisite)
- [ ] `identity-auth.md`
- [ ] `identity-roles.md`
- [ ] `identity-sessions.md`
- [ ] `identity-user.md`
- [ ] `content-catalog.md`
- [ ] `content-commerce.md`
- [ ] `content-editorial.md`
- [ ] `content-interactions.md`
- [ ] `content-lookup.md`
- [ ] `core.md`
- [ ] `workflows.md`
- [ ] `shared.md`

## Phase 5 — Infrastructure ([`specs/infrastructure/`](specs/infrastructure/))
- [ ] `01-seeding-helpers.md` — `SeedAsync<TDbContext>` base helper
- [ ] `02-isolation.md` — count-assertion → unique-key; transaction decision
- [ ] `03-fixtures-cleanup.md` — rate-limit config; FileService gap

## Global acceptance gates
- [ ] `dotnet build tests/Integration` + `tests/Fixtures` → 0 errors
- [ ] `dotnet csharpier .` clean
- [ ] `./scripts/run-tests-with-coverage.sh integration` green
- [ ] `grep -rn '"/api/v' tests/Integration` → 0
- [ ] `grep -rln 'JsonDocument' tests/Integration` → 0 (or only justified)
