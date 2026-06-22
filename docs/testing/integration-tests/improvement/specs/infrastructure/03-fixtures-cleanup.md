# Infra Spec 03 — Fixture cleanup & FileService gap

## A. Fragile rate-limit disabling

### Problem
`ApiFixture.DisableRateLimiting()` removes config descriptors by reflection and
string-matching `IConfigureOptions` type names — breaks silently on DI/runtime
upgrades.

### After
Register no-op policies via strongly-typed configuration instead of reflection:
```csharp
services.Configure<RateLimiterOptions>(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    foreach (var policy in RateLimitPolicies.All)   // expose the names from src
        options.AddPolicy(policy, _ => RateLimitPartition.GetNoLimiter("test"));
});
```
If a stray production configurator still applies real limits, prefer overriding
the policy registration rather than reflecting over `IConfigureOptions`.

### TODO
- [ ] Expose the policy names as a list in src (`RateLimitPolicies.All`) or
      enumerate them in one place in the fixture.
- [ ] Replace the reflection block in `ApiFixture.cs`.
- [ ] Confirm no test receives 429.

## B. Core FileService test gap

### Problem
`Modules/Core/Infrastructure/Services/` is empty (only `.gitkeep`). `FileService`
upload/lookup paths and `File*Specification`s lack direct tests; only delete
paths are hit transitively.

### TODO
- [ ] Add `FileServiceTests.cs` covering upload (with `StubCloudinaryService`),
      lookup-by-filename/mime/size specs, soft-delete, and avatar validation.
- [ ] Remove `Modules/Core/Infrastructure/Services/.gitkeep` once real files land.

### Acceptance
- `FileService` and `File*Specification` appear in integration coverage.
- The `.gitkeep` is gone (dir no longer empty).
