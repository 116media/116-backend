# Identity roles & permissions reference

## Domain entities

| Entity | Location | Description |
| --- | --- | --- |
| `RoleEntity` | `Identity/Domain/Entities/RoleEntity.cs` | Role with Name, Description, IsActive, IsDeleted |
| `PermissionEntity` | `Identity/Domain/Entities/PermissionEntity.cs` | Permission with Resource, Action, IsActive, IsDeleted |
| `UserRoleEntity` | `Identity/Domain/Entities/UserRoleEntity.cs` | M:N junction: User ↔ Role |
| `RolePermissionEntity` | `Identity/Domain/Entities/RolePermissionEntity.cs` | M:N junction: Role ↔ Permission |

## Entity methods (RoleEntity / PermissionEntity)

All transitions are guarded: an illegal precondition throws a coded `IdentityRuleException`,
an idempotent repeat returns `false` and raises nothing.

| Method | Returns | Description |
| --- | --- | --- |
| `Create(...)` | Entity | Factory with validation |
| `Update(...)` | `bool` | Update fields; `false` and no event when values are identical |
| `Activate()` / `Deactivate()` | `bool` | Flip `IsActive`; `false` when already in that state |
| `SoftDelete(DateTime now)` | `bool` | Sets `IsDeleted`, clears `IsActive`, stamps `DeletedAt` |
| `Restore()` | `bool` | Clears `IsDeleted` and `DeletedAt` |
| `MarkHardDeleted()` | `void` | Raises the changed event for a permanent removal (D14) |

## Core roles

`Identity/Domain/Enums/EnumCoreUserRole.cs`: `SuperAdmin`, `Admin`, `Visitor`.

## Specifications

`Identity/Application/Roles/Specifications/`:

- **RoleSpecifications**: `RoleByNameSpecification`, `RoleByIdSpecification`
- **RolePermissionSpecifications**: by role id, by permission id, by role+permission
- **UserRoleSpecifications**: `UserHasAdminRoleSpecification`, `UserHasRoleSpecification`,
  `UserHasVisitorRoleSpecification`, `UserIsActiveAdminSpecification`

## Visitor permissions (28)

Seeded by `VisitorRoleSeeder` from `Identity/Domain/ValueObjects/VisitorPermissions.cs`:

| Category | Permissions |
| --- | --- |
| Content | articles.read, videos.read, contents.read |
| Profile | own_profile.read, own_profile.update |
| Likes | likes.create, own_likes.delete, likes.read |
| Comments | comments.read, comments.create, own_comments.update, own_comments.delete |
| Bookmarks | bookmarks.create, own_bookmarks.delete, own_bookmarks.read, bookmarks.read |
| Navigation | tags.read, categories.read |
| Playlists | playlists.create, own_playlists.update, own_playlists.delete, own_playlists.read |
| Ads | ads_banners.read, ads_stories.read |
| Rates | rates.create, rates.read |
| Shares | shares.create, shares.read, own_shares.read |

`SuperAdminSeeder` seeds the SuperAdmin account.

## Admin role & permission endpoints

Role CRUD (`/api/v1/admin/roles`), permission CRUD (`/api/v1/admin/permissions`), status
management (`activate`/`deactivate`/`restore`, soft and hard `DELETE`), role-permission
assignment (`/api/v1/admin/roles/{id}/permissions`), and user-role assignment
(`/api/v1/admin/users/{id}/roles`). Reads require Admin; mutations require SuperAdmin. The
implemented endpoints under `Identity/Application/Roles/UseCases/` and
`Identity/Application/User/UseCases/` are the source of truth.

### Business rules

1. **Core roles are protected**: SuperAdmin, Admin, Visitor cannot be hard deleted.
2. **Deactivated roles** cannot be assigned to new users; existing assignments remain.
3. **Soft-deleted roles** are hidden from lists by default and restorable.
4. **Hard delete** is permanent and cascades to UserRole/RolePermission.
5. Role/permission mutations bump the affected users' token version.
6. Role/permission names must be unique among non-deleted items.
