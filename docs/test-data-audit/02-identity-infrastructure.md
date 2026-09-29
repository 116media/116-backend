# Identity — Infrastructure and Domain suites

Area: `Identity.Unit.Tests/Infrastructure/`, `Identity.Unit.Tests/Domain/`,
`Identity.Integration.Tests/Infrastructure/`. 69 files, ~19,800 lines; 52 read in full, 17 read in part
after grepping every data-construction expression in them.

## Entity constructed inline where a factory already names the shape — 46 sites, 8 files

The decisive argument for fixing these: the module's **integration** `AuthRepositoryTests`, along with
`UserTokenStateRepositoryTests:173-174` and `UserLookupServiceTests:57,109-110`, already call
`UserRoleFactory.Create(userId, roleId)`. The unit side was simply never migrated.

A second argument: `UserRoleEntity.Create` and `RolePermissionEntity.Create` are **`internal`** and
documented "Created and removed only through `UserEntity`'s grant/revoke methods". These tests reach
past the aggregate; the builders are what encapsulate it.

### `SuperAdminRepositoryManagerTests.cs` — 17 sites

| Line | Today | Use |
| --- | --- | --- |
| 67 | `UserEntity.Create(Guid.NewGuid(), SuperAdminConfiguration.Email, "testuser", "hashedPassword")` | `UserFactory.Create(SuperAdminConfiguration.Email)` |
| 92 | same shape, other address | `UserFactory.Create("other@example.com", "otheruser")` |
| 397, 414 | same shape | `UserFactory.Create("test@example.com", "testuser")` |
| 116 | `PermissionEntity.Create(Guid.NewGuid(), "system", "all", "Test permission")` | `PermissionFactory.Create("system", "all", "Test permission")` |
| 153 | same, `"read"` | `PermissionFactory.Create(...)` |
| 301, 318 | same, `"test"/"create"` | `PermissionFactory.Create("test", "create", "Test permission")` |
| 177 | `RoleEntity.Create(Guid.NewGuid(), "SuperAdmin", "Test role")` | `RoleFactory.Create("SuperAdmin", "Test role")` |
| 349, 366, 541 | same shape | `RoleFactory.Create("TestRole", "Test role description")` |
| 220 | `RolePermissionEntity.Create(roleId, permissionId)` | `RolePermissionFactory.Create(roleId, permissionId)` |
| 445, 462 | `RolePermissionEntity.Create(Guid.NewGuid(), Guid.NewGuid())` | `RolePermissionFactory.Create()` — its defaults are already two random Guids |
| 262 | `UserRoleEntity.Create(userId, roleId)` | `UserRoleFactory.Create(userId, roleId)` |
| 493, 510 | same with random ids | `UserRoleFactory.Create()` |

### `AuthRepositoryTests.cs` (unit) — 12 sites

- `:98, 138, 491, 535, 558, 735, 761` — `UserRoleEntity.Create(user.Id, role.Id)` → `UserRoleFactory.Create(user.Id, role.Id)`
- `:139, 492` — `RolePermissionEntity.Create(role.Id, permission.Id)` → `RolePermissionFactory.Create(role.Id, permission.Id)`
- `:851, 904, 935, 967, 1000` — `UserEntity.CreateExternal(Guid.NewGuid(), "existinguser", EnumAuthProvider.Google, "google-subject-N", "external@example.com")`. Five identical shapes; needs the new factory in [10-missing-factories.md](10-missing-factories.md) M3, since `UserFactory.CreateExternal(provider)` cannot pin the subject id, username or email — the three things these tests assert on.

### The rest

| File | Sites | Use |
| --- | --- | --- |
| `SuperAdminSeederTests.cs` | `:202, 230` | `UserFactory.Create(SuperAdminConfiguration.Email, "existingadmin")`. **Not** `CreateSuperAdmin()` — it grants a role, so adding the user cascade-inserts a `UserRole` pointing at an unseeded `Role` and breaks `userCount.Should().Be(1)` |
| `SuperAdminSeedingStrategyTests.cs` | `:124, 169, 214, 215, 216` | `PermissionFactory.Create`, `RoleFactory.Create`, `RolePermissionFactory.Create(role.Id, permission.Id)` |
| `VisitorRoleSeederTests.cs` | `:135, 157, 179` | `RoleFactory.Create(nameof(EnumCoreUserRole.Visitor), "Existing role")` — **not** `CreateVisitor()`, which substitutes `TestConstants.Role.VisitorDescription` |
| `SessionRepositoryTests.cs` | `:77, 78` | `UserRoleFactory.Create(user.Id, role.Id)`, `RolePermissionFactory.Create(role.Id, permission.Id)` |
| `RoleRepositoryTests.cs` | `:53, 54` | `RolePermissionFactory.Create(...)`. Here the swap is a fix, not a style change: both rows are created with `Id == Guid.Empty` and lean on EF's client-side generator while the test asserts `RolePermissions.Should().HaveCount(2)` |

### ⚠ This swap is not inert for two entity types

`UserRoleEntity.Create` and `RolePermissionEntity.Create` deliberately leave `Id` unset for the store
generator — documented at `UserRoleEntity.cs:32-38` / `RolePermissionEntity.cs:31-38` and asserted by a
test named `Create_ShouldLeaveTheKeyUnsetForTheStoreGenerator` (`UserRoleEntityTests.cs:41`,
`RolePermissionEntityTests.cs:42`). `UserRoleBuilder.Build()` and `RolePermissionBuilder.Build()`
reflectively set `Id = Guid.NewGuid()`. It is safe at all 20 sites in this area, but it belongs in the
PR description rather than being assumed.

Two smaller deltas worth stating: `UserBuilder` substitutes `TestConstants.User.DefaultPasswordHash`
for the literal `"hashedPassword"` (never asserted) and calls `Activate()`, which is a no-op because
`UserConstants.DefaultIsActive` is already true.

## Factory exists for the state, but the test mutates after construction — 5 sites

| Site | Today | Use |
| --- | --- | --- |
| `RoleRepositoryTests.cs:234-235` | `RoleFactory.Create("InactiveRole"); inactiveRole.Deactivate();` | `RoleFactory.CreateInactive("InactiveRole")` |
| `RoleRepositoryTests.cs:254-255` | `RoleFactory.Create("DeletedRole"); deletedRole.SoftDelete(DateTime.UtcNow);` | `RoleFactory.CreateDeleted("DeletedRole")`. Final state identical — `SoftDelete` clears `IsActive` itself — though `AsDeleted` calls `Deactivate()` first, so 2 `RoleChangedEvent`s instead of 1, unasserted here |
| `PermissionRepositoryTests.cs:211-212, 293-294` | `PermissionFactory.Create("inactive","read"); …Deactivate();` | `new PermissionBuilder().WithResourceAction("inactive","read").AsInactive().Build()` — a builder chain, **not** a new factory: only 2 sites share it |
| `PermissionRepositoryTests.cs:295-296` | `PermissionFactory.Create("activeDeleted","read"); …SoftDelete(now)` | `…AsDeleted().Build()`. `PermissionEntity.SoftDelete` sets `IsActive = false`, so the variable name is already a misnomer |

Low priority, same class: integration `AuthRepositoryTests.cs:128` `UserFactory.CreateAdmin(); user.Deactivate();`
— no builder path expresses "admin + inactive" without duplicating `CreateAdmin`'s chain. Leave until a
second site appears.

## Inside `*EntityTests.cs`, where `Create` is *not* the subject — 7 sites

The pattern-matching pass excused these files wholesale by filename. Reading them shows seven arrange
blocks where the domain factory is incidental:

- `RoleEntityTests.cs:320, 334, 348` — `RoleEntity.Create(...); role.ClearDomainEvents();` inside `SoftDelete_/MarkHardDeleted_/Restore_ShouldRaise…` → `RoleFactory.Create()`. `ClearDomainEvents()` discards whatever `Create` raised and the name is never asserted.
- `PermissionEntityTests.cs:400, 414` — same pattern → `PermissionFactory.Create("articles","read","Read articles")`. **The same file already does exactly this at `:432`**, which settles the house style.

## `SessionBuilder` has no `CreatedAt` default, forcing a ~20-site workaround

`Entity<T>.CreatedAt` is `DateTime?` with no default (`src/shared/src/Shared.Domain/Entity.cs:13`), so
`SessionRepositoryTests` bolts `.WithCreatedAt(DateTime.UtcNow)` onto every session — which is why that
file can use no `SessionFactory` member at all and instead carries a private helper (`:45-46`) plus 13
inline chains.

Sites where the stamp is the only obstacle: `:310, 311, 353, 570` (→ `SessionFactory.CreateExpired()`),
`:668, 672, 698, 702` (→ `CreateWithBrowser`), `:611, 615` (→ `CreateWithIpAddress`), `:726` (→ `CreateDesktop()`),
`:727` (→ `CreateMobile()`), `:426-430` (→ `Create(userId, deviceId)`), plus unit `AuthRepositoryTests.cs:593, 630, 661-665`.

**Root fix:** default `_createdAt = DateTime.UtcNow` in the `SessionBuilder` constructor
(`SessionBuilder.cs:43-53`), keeping `WithCreatedAt` for tests that need a specific instant
(`SessionRepositoryTests:508, 510, 640, 642, 887, 889`). Confidence medium — it changes what
`SessionBuilder` produces for every consumer, and the Application-folder session tests need a check
first.

## Dead members to delete while in the area

`OtpFactory.CreateWithAttemptCount` (`:151`), `RolePermissionFactory.CreateWithId` (`:43`),
`UserRoleFactory.CreateWithRole` (`:35`), `CreateWithUserId` (`:50`), `CreateWithRoleId` (`:57`).

Keep despite the 3-caller rule: `UserRoleFactory.CreateWithId(Guid,Guid,Guid)` (1 caller) and
`RolePermissionFactory.CreateWithPermission` (2) — load-bearing for a deterministic key and a populated
navigation respectively.
