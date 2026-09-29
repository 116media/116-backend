# Dead factory members

Unused **and** with no hand-rolled twin anywhere in the repository, so deleting them loses nothing.
Each was checked with both call syntaxes — `Class.Method(` and, for extension methods, `.Method(`.

## Content — 8, verified across `src` and `tests`

| Member | Why it is dead |
| --- | --- |
| `MockAddItemTierFactory.VerifyAttachTierCalled` | No `.Verify` on `AttachTierAsync` anywhere |
| `MockAddOrderItemFactory.SetupCreateItemAsyncThrows` | No throwing setup on `CreateItemAsync` anywhere |
| `MockAddOrderItemFactory.VerifyCreateItemCalled` | No `.Verify` on `CreateItemAsync` anywhere |
| `MockOrderPaymentFactory.VerifyGetByOrderIdCalled` | The only hand-rolled verifies on `GetByOrderIdOrThrowAsync` are `Times.Never`, never `Times.Once` |
| `MockSubmitOrderFactory.SetupSubmitAsyncThrows` | No throwing setup on `SubmitAsync` anywhere |
| `MockVerifyPaymentFactory.SetupVerifyAsyncThrows` | No throwing setup on `VerifyAsync` anywhere |
| `MockVerifyPaymentFactory.VerifyVerifyCalled` | Three hand-rolled `.Verify` on `VerifyAsync` exist and **none matches**: `AdminVerifyPaymentHandlerTests.cs:76-86` asserts specific arguments (strictly stronger than the member's all-`It.IsAny`), and `:110-120`, `:142-152` are `Times.Never` |
| `ContentOrderItemFactory.CreateWithCategory` (`:50-51`) | Zero callers, and no test uses `new ContentOrderItemBuilder()…WithCategory(…)`. Deleting it orphans `ContentOrderItemBuilder.WithCategory`, its only caller, which per [12-defects-found.md](12-defects-found.md) D2 does nothing a plain `WithCategoryId` does not — delete both |

Near-miss, do **not** delete: a `VerifyVerifyNotCalled` (`Times.Never`) would serve 2 sites. Below the
bar; leave those direct.

## Storage — 5

Rests on one fact: **`new FileBuilder()` appears at zero call sites outside `FileFactory`**, so every
Storage test goes through a factory and an unused member cannot have a hand-rolled twin.

| Member | Body |
| --- | --- |
| `FileFactory.CreateWithSize` (`:64`) | `new FileBuilder().WithSizeInBytes(sizeInBytes).Build()` — `WithSizeInBytes` appears only in `FileBuilder.cs:107`, this line, and `FileFactory.cs:76` |
| `FileFactory.CreateWithTestValues` (`:70-77`) | A 5-call `TestConstants.File.Valid*` chain. The only other place that combination appears is `FileEntityTests.cs:36-214`, which passes the constants to `FileEntity.Create` **on purpose** as guard tests |
| `FileReferenceDtoFactory.CreatePng` (`:37`) | `FileFactory.CreatePng().ToFileReferenceDto()` — no site composes those two calls |
| `FileReferenceDtoFactory.CreateWithStorageKey` (`:64-65`) | `FileFactory.CreateWithStorageKey(storageKey).ToFileReferenceDto()` — same |
| `StoredFileFactory.Create()` (`:40-43`) | `From(FileReferenceDtoFactory.Create())`. Keep both `From` overloads — 14 files use them |

## Identity — 5

`OtpFactory.CreateWithAttemptCount` (`:151`), `RolePermissionFactory.CreateWithId` (`:43`),
`UserRoleFactory.CreateWithRole` (`:35`), `UserRoleFactory.CreateWithUserId` (`:50`),
`UserRoleFactory.CreateWithRoleId` (`:57`).

Keep despite having fewer than three callers: `UserRoleFactory.CreateWithId(Guid,Guid,Guid)` (1 caller,
load-bearing for a deterministic key) and `RolePermissionFactory.CreateWithPermission` (2 callers,
load-bearing for a populated navigation).

## Dead for a reason — do not delete, and do not substitute

`MockSessionRepository.SetupGetActiveSessionByUserIdAndDeviceId` and `…ReturnsNull` target
`GetActiveSessionByUserIdAndDeviceIdAsync`. The only caller-shaped code, `SessionFactoryTests`, stubs the
**different** member `GetSessionByUserIdAndDeviceIdAsync` (`ISessionRepository.cs:89` vs `:113`).
Swapping in the existing helper would compile and silently stop matching. Add the missing pair instead —
[10-missing-factories.md](10-missing-factories.md).

## Members with one or two callers that are *not* listed here

Roughly 50 of the 95 name a shape no test asks for, but the rule says inline-or-delete, and inlining is
sometimes worse. Two documented examples:

- Six more `FileFactory` members have exactly one calling file. Inlining them would introduce the
  first-ever direct `new FileBuilder()` in a Storage test, against the grain of that module.
- `PlaylistSpecificationsTests.cs:33, 44, 60, 71` could move to `PlaylistFactory.CreateWithId`, but the
  explicit constructor arguments make the specification inputs legible. Borderline either way.
