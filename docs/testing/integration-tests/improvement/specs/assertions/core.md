# Assertions — Core

## Current
- `Modules/Core/Infrastructure/Repositories/FileRepositoryTests.cs` — already does
  real persistence assertions (good); keep, convert any stringly-typed checks.
- `Modules/Core/Infrastructure/Services/` — **empty** (see
  [`../infrastructure/03-fixtures-cleanup.md`](../infrastructure/03-fixtures-cleanup.md)).

## TODO checklist
- [ ] FileRepositoryTests.cs — confirm assertions are typed/identity-based (not count-based).
- [ ] Add FileServiceTests.cs (upload via `StubCloudinaryService`, lookup specs,
      soft-delete, avatar validation) — see infra spec 03.
- [ ] Remove `Services/.gitkeep` once `FileServiceTests.cs` lands.

## Acceptance
- `FileService` + `File*Specification` covered by integration tests; the empty
  `Services` placeholder is gone.
