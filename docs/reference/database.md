# Database reference

## Schemas

One schema per module, one `DbContext` per schema. All contexts share
`public.__EFMigrationsHistory`.

| Schema | Context | Content |
| --- | --- | --- |
| `identity` | `IdentityDbContext` | Users, roles, permissions, sessions, OTPs, per-user state tables |
| `content` | `ContentDbContext` | Editorial, commerce, interactions (49 entities) |
| `core` | `CoreDbContext` | File management |
| `mailer` | `MailerDbContext` | Email delivery |

Full schema DDL, provisioning scripts, and dbdiagram sources:
[`docs/database/schema/`](../database/schema/README.md).

## Key Identity entities

| Entity | Table | Description |
| --- | --- | --- |
| `UserEntity` | `users` | User accounts with email (value object), password hash |
| `RoleEntity` | `roles` | Role definitions (SuperAdmin, Admin, Visitor) |
| `PermissionEntity` | `permissions` | Permission definitions |
| `UserRoleEntity` | `user_roles` | User ↔ Role (M:N) |
| `RolePermissionEntity` | `role_permissions` | Role ↔ Permission (M:N) |
| `SessionEntity` | `sessions` | Login sessions with refresh-token hashes |
| `OtpEntity` | `otps` | One-time passwords |
| `UserLoginStateEntity` | `user_login_state` | Login brute-force counters (sibling aggregate, id = user id) |
| `UserOtpStateEntity` | `user_otp_state` | OTP failure counters (sibling aggregate) |
| `UserTokenStateEntity` | `user_token_state` | Security stamp + token version (sibling aggregate) |
| `FileEntity` | `files` (core) | Uploaded files with metadata |

## Auditable fields (automatic)

Entities inheriting `Aggregate<T>` get `created_at`, `updated_at`, `created_by`, `updated_by`,
written by `AuditableEntityInterceptor` through `TimeProvider`.

## Adding migrations

```bash
# Identity module (same pattern for Content, Core, Mailer)
dotnet ef migrations add MigrationName \
  --project src/Modules/Identity/Identity \
  --startup-project src/Api \
  --context IdentityDbContext
```

The API applies migrations itself at startup (`src/Api/DatabaseMigrator.cs`);
`dotnet run --project src/Api -- migrate` migrates and exits (used by deploys).
