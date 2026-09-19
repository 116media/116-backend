# Stage 14 — Storage contracts, file pipeline hardening, architecture tests & packaging

Closes **[02 §1]** / **[05 §6]** (High), **[02 §3]**, **[02 §4]**, **[02 §6]**, **[02 §9]**,
**[02 §12]**, **[05 §2]**, **[05 §3]**, **[05 §4]**, **[05 §7]**, **[05 §10]**, **[05 §5]**,
**[study 02]**, **[study 04]**, **[study 13]**.

Verified in the current tree:

- **115 files** in Identity + Content `using _116.Core.Domain/Application/Infrastructure` — no
  contracts boundary exists.
- `IFileRepository` is 19 methods mixing persistence (`GetByIdAsync`, `AddAsync`,
  `SaveChangesAsync`), cloud upload (`UploadAndStoreImageFileAsync`, `ReplaceVideoFileAsync`),
  and Identity's avatar workflow (`UpdateAvatarFromUrlAsync`, `DownloadAndStoreAvatarFromUrlAsync`)
  in one interface `[02 §6]`.
- `CloudinaryService.DeleteBatchAsync` hardcodes `ResourceType = ResourceType.Image` — deleting
  a video or PDF is a silent no-op and the asset leaks forever `[05 §2]`.
- Replace flows call `MarkReplacedByIdAsync(currentFileId)` **before**
  `UploadAndStoreImageFileAsync` — a failed upload leaves the entity pointing at a file already
  marked replaced `[05 §3]`.
- `CloudinaryDotNet`'s client is constructed directly — no `IHttpClientFactory`, no timeout, no
  retry, no circuit breaker `[05 §7]`.
- Zero `Directory.Packages.props`; every csproj pins its own versions (`ClosedXML 0.105.0`
  et al. inline) `[study 04]`. No `tests/Architecture` exists `[02 §3]`.

> Draft — finalized against the tree Stage 13 lands on.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Order of work | contracts first, or packaging first | **CPM first.** Introducing `Core.Contracts` adds a project; adding projects before central versions multiplies inline-pin churn. CPM is a mechanical, riskless PR-within-the-PR. |
| D2 | Contract shape | expose `FileEntity` through contracts, or an opaque reference | **Opaque.** `IFileStore` + `FileRef` — consumers get an id, a URL, and metadata; the entity, the DbContext and Cloudinary stay behind the boundary. The 19-method interface splits three ways: `IFileStore` (contract), `IFileRepository` (Core-internal persistence), and the avatar workflow **moves to Identity** where it belongs. |
| D3 | The avatar workflow | keep avatar methods in Core behind the contract, or move them | **Move to Identity.** `UploadAndStoreAvatarAsync`/`UpdateAvatarFrom*`/`GetAvatarFileAsync` encode Identity's business flow (sizes, folder, replace policy); Core keeps only generic store/replace/delete. `SlugHelper` moves to Content for the same reason. **Amended on delivery — `IImageColorService` stays in Core.** The original rationale ("dominant-colour to the consumer that renders it") assumed colours were computed at render time; they are not. `FileUploadService.UploadImageAsync` extracts them once at upload and persists them on `core.files`, and every consumer reads them off `FileReferenceDto`. `FileEntity`'s colour properties have private setters written only by Core's own factory, so moving the service would mean widening `IFileStorageService.UploadAsync` to accept colours — leaking a presentation concern *into* the cross-module contract to satisfy a rule about keeping it out. The service lives where the column is written `[05 §5]`. |
| D4 | Delete/replace safety | fix in place, or redesign the flows | **Fix in place, three rules:** upload-then-swap-then-mark (never mark first); `resource_type` derived from the stored `FileType`; every replace path atomic with its DB write through `ICoreUnitOfWork` + Stage 13's transaction seam `[05 §3]` / `[05 §4]`. |
| D5 | Resilience | Polly on the raw SDK calls, or wrap the SDK | **Wrap.** `ICloudStorageClient` owns the `Cloudinary` instance and the Polly pipeline, so `CloudinaryService` stays policy-free `[05 §7]`. **Three corrections on delivery.** The interface is vendor-neutral (`CloudStorageUpload` / `CloudStorageAsset`) and lives in `Application/Shared/Services`, so `CloudinaryStorageClient` is the only type in the solution that names CloudinaryDotNet. Config is the existing `CloudinarySettings` singleton, not `IOptions<CloudinaryOptions>`, which never existed. And every Polly value is set explicitly: the default `CircuitBreakerStrategyOptions` needs 100 calls in a 30-second window to open, which an upload workload never reaches, so a defaulted breaker would have been inert. |
| D6 | Enforcement | conventions doc, or NetArchTest | **NetArchTest with an explicit debt allowlist.** Rules fail the build on *new* violations; the allowlist may only shrink. **Shipped empty.** The module boundary was already clean — the project graph lets a module reference only another module's Contracts — so the rules that matter are the intra-module layer rules, and the nine violations they found were fixed rather than allowlisted. |

---

## Checklist

- [x] 14.1 — `Directory.Packages.props` + `Directory.Build.props`; all csproj versions centralized
- [x] 14.2 — `Core.Contracts`, shipped as `IFileStorageService` + `FileReferenceDto` (not `IFileStore`/`FileRef`) with `EnumStoredFileKind` and a shared `FileMapper`. Upload is two-phase — `UploadAsync` returns a handle, `RecordAsync` persists it — which makes D4's "never mark first" rule structural rather than a convention `[02 §9]`
- [x] 14.2b — file projections cached behind the contract under `CoreCacheTags.Files`, evicted by `FileCacheHandler` on `FileSoftDeletedEvent` and `FileReplacedEvent`. `ResolveAsync` reads through; the batch paths keep their single query and write back, because fanning a batch across per-id lookups would reintroduce the N+1 the contract exists to avoid. `ResolveUrlsAsync` now derives from the reference projection so both share one entry per file, which retired `IFileRepository.GetStorageUrlsByIdsAsync`
- [x] 14.3 — Avatar workflow moved to Identity (`AvatarService`), `SlugHelper` moved to Content. Colour service stays in Core — see the amendment on D3
- [x] 14.4 — Identity off Core internals; then Content; the 115 usings → 0
- [x] 14.5 — Pipeline: swap-then-mark (structural, via the two-phase upload), per-kind `resource_type`, Polly-wrapped client, atomic upload+write, `FileEntity.Create` guards `[05 §10]`
- [x] 14.6 — Host registration mismatch fixed (`Api.csproj`/`AddCoreModule`) `[02 §4]` / `[02 §12]`
- [x] 14.7 — `tests/Architecture` with boundary rules, layer rules, a provider-SDK rule, and the allowlist (empty). Runs as its own CI job
- [x] 14.8 — Verify (build 0/0, csharpier, unit, integration, architecture)

---

## Part A — Packaging

`Directory.Packages.props` at the repo root:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="ClosedXML" Version="0.105.0" />
    <PackageVersion Include="CsvHelper" Version="33.1.0" />
    <PackageVersion Include="Google.Apis.Auth" Version="1.76.0" />
    <!-- one line per package, versions lifted verbatim from the csprojs -->
  </ItemGroup>
</Project>
```

Every `<PackageReference Include="X" Version="…" />` loses its `Version` attribute. Lift versions
**verbatim** — upgrading anything in the same commit hides behaviour changes inside a mechanical
diff (and violates the latest-check rule for *new* pins only).

## Part B — The contract

> **As delivered.** The names below are the draft's. What shipped is `IFileStorageService` and
> `FileReferenceDto` in `Core.Contracts`, and upload is two-phase (`UploadAsync` → `RecordAsync`)
> rather than `StoreAsync`/`ReplaceAsync`. The sketch is kept for the reasoning; the signatures
> are in the code.


```csharp
namespace _116.Core.Contracts;

/// <summary>
/// An opaque reference to a stored file: everything a consuming module may know about it.
/// </summary>
public record FileRef(Guid Id, string Url, string MimeType, long SizeInBytes);

/// <summary>
/// The storage capability Core exposes to other modules. Upload, replace, resolve and delete —
/// no entity, no persistence detail, no provider type leaks through this seam.
/// </summary>
public interface IFileStore
{
    /// <summary>
    /// Stores a new file and returns its reference.
    /// </summary>
    Task<FileRef> StoreAsync(
        Stream content,
        string fileName,
        string mimeType,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stores a replacement and marks the previous file replaced — in that order, so a failed
    /// upload leaves the old file live.
    /// </summary>
    Task<FileRef> ReplaceAsync(
        Guid currentFileId,
        Stream content,
        string fileName,
        string mimeType,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resolves references in one round trip; absent ids are simply missing from the result.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, FileRef>> ResolveAsync(
        IReadOnlySet<Guid> fileIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Soft-deletes the row and removes the stored asset using the file's own kind.
    /// </summary>
    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}
```

`EnumStoredFileKind` (`Image`, `Video`, `Raw`) is what kills the hardcode: the adapter maps it to
Cloudinary's `ResourceType` per call instead of assuming `Image` in `DeleteBatchAsync`
(`CloudinaryService.cs:164`).

Content's mappers (Stage 9's `BuildDtosAsync` batches) swap `IFileRepository.GetByIdsAsync` for
`IFileStore.ResolveAsync` — same shape, contract type. Identity gains an
`AvatarService` owning the moved avatar workflow, calling `IFileStore` for the storage part.

## Part C — Pipeline hardening

Replace order inverts (today `MarkReplacedByIdAsync` runs first at `FileRepository.cs:362-365`):

```csharp
/// <inheritdoc />
public async Task<FileRef> ReplaceAsync(
    Guid currentFileId,
    Stream content,
    string fileName,
    string mimeType,
    EnumStoredFileKind kind,
    CancellationToken cancellationToken = default
)
{
    // Upload first: a storage failure must leave the current file untouched and referenced.
    FileRef replacement = await StoreAsync(content, fileName, mimeType, kind, cancellationToken);

    await unitOfWork.ExecuteInTransactionAsync(
        async ct => await fileRepository.MarkReplacedByIdAsync(currentFileId, ct),
        cancellationToken
    );

    return replacement;
}
```

The wrapped client:

```csharp
// Singleton so one pipeline — and one circuit-breaker state — is shared process-wide;
// a scoped pipeline would reset per request and could never open.
services.AddSingleton<ICloudStorageClient>(sp => new CloudinaryStorageClient(
    sp.GetRequiredService<CloudinarySettings>(),
    CloudStorageResilience.CreatePipeline(),
    sp.GetRequiredService<ILogger<CloudinaryStorageClient>>()
));
```

`CloudStorageResilience` owns the policy and sets every value explicitly: 3 jittered exponential
retries from a 500 ms base, a breaker at 50% failures over 10 calls in a 30-second window, and a
10-second per-attempt timeout. The defaults are not usable here — Polly's breaker wants 100 calls
per window before it will open, which an upload workload never reaches.

`FileEntity.Create` gains the missing guards (`[05 §10]`): non-empty storage key, positive size,
known mime type — Stage 7's coded-exception shape (`CoreRuleException` + rule codes), and the
presentation strings hardcoded on the entity move out with the colour service.

## Part D — Architecture tests

```csharp
namespace _116.Architecture.Tests;

/// <summary>
/// Module-boundary rules: a module may reference another module's Contracts project only.
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly Assembly Content = typeof(_116.Content.ContentModule).Assembly;
    private static readonly Assembly Identity = typeof(_116.Identity.IdentityModule).Assembly;

    [Fact]
    public void ContentMustNotTouchCoreInternals()
    {
        TestResult result = Types
            .InAssembly(Content)
            .ShouldNot()
            .HaveDependencyOnAny("_116.Core.Domain", "_116.Core.Application", "_116.Core.Infrastructure")
            .GetResult();

        result.FailingTypes.Should().BeEmpty();
    }

    [Fact]
    public void DomainMustNotDependOutward()
    {
        TestResult result = Types
            .InAssembly(Content)
            .That().ResideInNamespace("_116.Content.Domain")
            .ShouldNot()
            .HaveDependencyOnAny("_116.Content.Application", "_116.Content.Infrastructure", "Microsoft.AspNetCore")
            .GetResult();

        result.FailingTypes.Should().BeEmpty();
    }
}
```

`KnownViolations.txt` is subtracted from every rule's failing types, and may only shrink.

**It shipped empty.** The module-boundary rules passed on introduction because the project graph
already enforces them: no module csproj references another module's implementation project, only
its Contracts. The rules that earn their place are therefore the intra-module ones, which nothing
else can enforce since a module is a single assembly — and those found nine real violations
(eight Content query builders reaching into Infrastructure, one Mailer entity on
`Microsoft.AspNetCore.WebUtilities`). All nine were fixed rather than allowlisted, so the ratchet
starts at zero. A `RuleCoverageTests` guard asserts each namespace filter still matches types, so
a renamed layer breaks the build instead of silently disabling every rule.

---

## Tests

- **Unit — delete kinds:** `CloudinaryService` addresses each asset under its own kind, asserted
  per kind for both the single and batch delete. This is the `[05 §2]` regression.
- **Unit — resilience:** the retry policy retries before surfacing, the breaker opens under
  sustained failure and then fails fast, and a guard asserts the breaker does not fall back to
  Polly's default throughput.
- **Unit:** `FileEntity.Create` guards; `FileCacheHandler` evicts on both file lifecycle events.
- **Integration — file cache:** a warmed projection survives a raw row update, proving the cache
  serves; both lifecycle handlers resolve from the container, proving eviction is wired.
- **Integration — provider seam:** the stub replaces `ICloudStorageClient` rather than
  `ICloudinaryService`, so the real validation and result projection run under integration.
- **Architecture:** module boundary, layer rules, provider-SDK rule, and the vacuity guard.

**Not delivered as specified.** The delete-kinds check was written as an integration test and
could not observe the provider call: on the video-delete path the file soft-delete commits, but
the resulting `FileSoftDeletedEvent` is raised on Core's context and does not dispatch within the
request, so the remote delete is left to the Core outbox replay job — which is disabled in tests.
The assertion moved to the unit suite. The same gap means the existing
`DeleteVideo_WhenCloudinaryDeleteFails_*` and `DeleteShortVideo_*` tests inject a failure into a
call that never happens; worth a look, but it is eventing behaviour outside this stage. Replace
safety is likewise still uncovered for the same reason.

---

## Rollout

CPM and contracts are deploy-neutral. The `resource_type` fix changes *delete* behaviour for
videos/PDFs — previously-leaked assets are not retro-deleted; a one-off cleanup script is a
separate follow-up.

---

## Verification

1. Build/format/unit/integration/architecture green.
2. `grep -rln "using _116.Core.Domain\|using _116.Core.Application\|using _116.Core.Infrastructure" src/Modules/Identity src/Modules/Content` → empty (was 115).
3. `grep -rn "Version=" src/**/*.csproj` → empty.
4. `KnownViolations.txt` → empty (no module-boundary or layer violations remain).
5. `dotnet test tests/Architecture` green, and its CI job runs on every push and PR.

---

**PR:** `refactor(core): storage contracts, hardened file pipeline, architecture tests and CPM`
