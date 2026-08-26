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
| D3 | The avatar workflow | keep avatar methods in Core behind the contract, or move them | **Move to Identity.** `UploadAndStoreAvatarAsync`/`UpdateAvatarFrom*`/`GetAvatarFileAsync` encode Identity's business flow (sizes, folder, replace policy); Core keeps only generic store/replace/delete. Same eviction for `SlugHelper` and `IImageColorService` — slug generation belongs to Content, dominant-colour to the consumer that renders it `[05 §5]`. |
| D4 | Delete/replace safety | fix in place, or redesign the flows | **Fix in place, three rules:** upload-then-swap-then-mark (never mark first); `resource_type` derived from the stored `FileType`; every replace path atomic with its DB write through `ICoreUnitOfWork` + Stage 13's transaction seam `[05 §3]` / `[05 §4]`. |
| D5 | Resilience | Polly on the raw SDK calls, or wrap the SDK | **Wrap.** `ICloudStorageClient` adapter owns the `Cloudinary` instance; Polly pipeline (10 s timeout, 3 retries with jitter on 5xx/timeouts, circuit breaker) lives in the adapter's DI registration, so `CloudinaryService` stays policy-free `[05 §7]`. |
| D6 | Enforcement | conventions doc, or NetArchTest | **NetArchTest with an explicit debt allowlist.** Rules fail the build on *new* violations; the current 115-file debt is enumerated and only allowed to shrink. |

---

## Checklist

- [ ] 14.1 — `Directory.Packages.props` + `Directory.Build.props`; all csproj versions centralized
- [ ] 14.2 — `Core.Contracts`: `IFileStore`, `FileRef`, `EnumStoredFileKind`; shared `FileRef` mapper (kills the duplicated `FileEntity → FileDto` `[02 §9]`)
- [ ] 14.2b — id→URL resolution cached behind `IFileStore` (deferred from Stage 10.5: `FileRef` is a flat record, so it may be cached; `FileEntity` may not). Evicted by `FileSoftDeletedEvent` via `RemoveByTagAsync`
- [ ] 14.3 — Avatar workflow + `SlugHelper` + colour service evicted to their owners
- [ ] 14.4 — Identity off Core internals; then Content; the 115 usings → 0
- [ ] 14.5 — Pipeline: swap-then-mark, per-kind `resource_type`, Polly-wrapped client, atomic upload+write, `FileEntity.Create` guards `[05 §10]`
- [ ] 14.6 — Host registration mismatch fixed (`Api.csproj`/`AddCoreModule`) `[02 §4]` / `[02 §12]`
- [ ] 14.7 — `tests/Architecture` with boundary + layer rules and the shrinking allowlist
- [ ] 14.8 — Verify (build 0/0, csharpier, unit, integration, architecture)

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
services.AddSingleton<ICloudStorageClient>(sp =>
{
    CloudinaryOptions options = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;
    ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
        .AddTimeout(TimeSpan.FromSeconds(10))
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions())
        .Build();

    return new CloudinaryStorageClient(new Cloudinary(options.ToAccount()), pipeline);
});
```

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

At introduction the boundary test runs against the debt: the allowlist is a
`KnownViolations.txt` the assertion subtracts, committed with exactly the current failing types,
and CI fails if the file grows. 14.4 drives it to empty within this stage for Core; the
remaining cross-module rules ratchet through Stage 18.

---

## Tests

- **Integration — replace safety:** stub the storage client to fail the upload → old file still
  resolves, no row marked replaced. Fail *after* upload → old row marked, new row live.
- **Integration — delete kinds:** store a `Video` kind through the stub, delete, assert the stub
  received `ResourceType.Video` (this is the `[05 §2]` regression).
- **Unit:** `FileEntity.Create` guards; the Polly pipeline retries a transient stub fault and
  opens the breaker on repeated ones.
- **Architecture:** the two rule sets above, plus layer rules per module.

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
4. `KnownViolations.txt` for the Core boundary → empty.

---

**PR:** `refactor(core): storage contracts, hardened file pipeline, architecture tests and CPM`
