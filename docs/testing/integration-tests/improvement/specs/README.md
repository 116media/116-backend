# Specs

Executable, file-by-file specs for the integration test overhaul. Each spec
follows the same shape:

1. **Problem** — what's wrong.
2. **Before** — current code.
3. **After** — target code.
4. **TODO checklist** — exact files to change (`- [ ]` per file).
5. **Acceptance** — how to know it's done.

Recommended execution order:

1. `infrastructure/04-typed-http-helpers.md` — shared helpers everything else uses.
2. `routes/01-apiroutes-rewrite.md`, `routes/02-segment-replacements.md`.
3. `test-data/01-bogus-determinism.md`, `test-data/02-builders-in-requests.md`.
4. `infrastructure/01-seeding-helpers.md`.
5. `assertions/*` — module by module (the bulk of the work).
6. `infrastructure/02-isolation.md`, `infrastructure/03-fixtures-cleanup.md`.

Track overall status in [`../06-todo-tracker.md`](../06-todo-tracker.md).

## Folders
- `routes/` — route single-source-of-truth.
- `test-data/` — Bogus determinism + request builders.
- `assertions/` — deep-assertion upgrade, one spec per module/area.
- `infrastructure/` — helpers, seeding, isolation, fixtures.
