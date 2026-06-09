# Unused Code in Unit Tests

This file documents code in `tests/Unit/Common/` that has no callers in the current unit test suite.
These items are intentionally **not deleted** — they may be used by integration tests or added to unit tests in the future.

---

## 1. Factory Methods — No Unit Test Callers

### `CommandFactory`

| Method | Location |
|--------|----------|
| `Role.CreateCommand()` (no-arg) | `CommandFactory.cs:23` |
| `Permission.CreateCommand()` (no-arg) | `CommandFactory.cs:85` |

Both no-arg `CreateCommand()` overloads use random builder data. Tests prefer the more explicit overloads (`CreateValidCommand()`, `CreateCommand(name, desc)`, etc.) to ensure deterministic values.

---

### `FileFactory`

| Method | Location |
|--------|----------|
| `CreateMany(int count)` | `FileFactory.cs:69` |
| `CreateWithTestValues()` | `FileFactory.cs:75` |

---

### `OtpFactory`

`OtpFactory.CreateWithAttemptCount()` is now used in `OtpRepositoryTests.cs`. Six additional overloads were added and are all used: `CreateExpired(userId, purpose)`, `CreateUsed(userId, purpose)`, `CreateExpired(userId, code, purpose)`, `CreateMaxAttemptsReached(userId, code, purpose)`, `CreateUsed(userId, code, purpose)`, `CreateUsedAndExpired(userId, code, purpose)`. All `new OtpBuilder()` calls in test files have been replaced. The following remain unused:

| Method | Location |
|--------|----------|
| `CreateWithId(Guid id)` | `OtpFactory.cs:40` |
| `CreateValid(Guid userId)` | `OtpFactory.cs:103` |
| `CreateMany(int count)` | `OtpFactory.cs:111` |

---

### `PermissionFactory`

| Method | Location |
|--------|----------|
| `CreateUpdate(string resource)` | `PermissionFactory.cs:92` |
| `CreateCrud(string resource)` | `PermissionFactory.cs:106` |
| `CreateMany(int count)` | `PermissionFactory.cs:70` |

---

### `RoleFactory`

| Method | Location |
|--------|----------|
| `CreateMany(int count)` | `RoleFactory.cs:80` |

---

### `RolePermissionFactory`

| Method | Location |
|--------|----------|
| `CreateMany(int count)` | `RolePermissionFactory.cs:48` |

---

### `UserFactory`

| Method | Location |
|--------|----------|
| `CreateMany(int count)` | `UserFactory.cs:90` |

---

### `UserRoleFactory`

| Method | Location |
|--------|----------|
| `CreateMany(int count)` | `UserRoleFactory.cs:48` |

---

## 2. `TestConstants` — Unused Constants

These constants have no callers in the unit test suite.

### `TestConstants.Role`

| Constant | Value |
|----------|-------|
| `NameMinLength` | `2` |
| `DescriptionMinLength` | `10` |

Tests use `TestConstants.Role.NameMaxLength` and `DescriptionMaxLength` for boundary validation; the min-length variants are not tested.

---

### `TestConstants.User`

| Constant | Value |
|----------|-------|
| `EmailMaxLength` | `256` |
| `UserNameMinLength` | `3` |
| `PasswordMinLength` | `8` |
| `PasswordMaxLength` | `128` |
| `CountryMaxLength` | `100` |
| `PhoneMaxLength` | `20` |

Tests use `UserConstants.*` from the source domain for user length validation, not these mirrors.

---

### `TestConstants.Session`

| Constant | Value |
|----------|-------|
| `DeviceIdMaxLength` | `256` |
| `IpAddressMaxLength` | `45` |
| `UserAgentMaxLength` | `512` |
| `DefaultAccessTokenExpirationMinutes` | `60` |

---

### `TestConstants.Otp`

| Constant | Value |
|----------|-------|
| `CodeLength` | `6` |
| `InvalidCode` | `"000000"` |

`TestConstants.Otp.ValidCode` and `MaxAttempts` are used; `CodeLength` and `InvalidCode` are not.

---

### `TestConstants.Jwt`

| Constant | Value |
|----------|-------|
| `ValidSecret` | `"ThisIsAVerySecureSecretKeyForTesting123!@#"` |
| `ValidIssuer` | `"116_test"` |
| `ValidAudience` | `"116_test_client"` |
| `RefreshTokenExpirationDays` | `30` |

`AccessTokenExpirationMinutes` and `ValidAccessToken` are used; the rest are not.

---

### `TestConstants.ValidationMessages` (top-level)

| Constant | Value |
|----------|-------|
| `RequiredField` | `"is required"` |
| `InvalidFormat` | `"is not valid"` |
| `TooShort` | `"too short"` |
| `TooLong` | `"too long"` |
| `AlreadyExists` | `"already exists"` |
| `NotFound` | `"not found"` |

These are generic fragments. No test uses them directly; tests reference the specific nested messages instead.

---

### `TestConstants.ValidationMessages.Guid`

| Constant | Value |
|----------|-------|
| `RoleIdRequired` | `"Role ID is required."` |
| `RoleIdInvalid` | `"Role ID is invalid."` |

`PermissionIdRequired` and `PermissionIdInvalid` are used; the role ID variants are not.

---

## 3. `AuthDataBuilder` — Unused Chain Methods

`AuthDataBuilder` was created to support fluent construction of `PublicLoginAuthData`, `AdminLoginAuthData`, and `PublicSocialLoginAuthData`. `WithRoles(List<RoleDto>)` is now wrapped via `AuthTestHelpers.CreateAdminLoginAuthData(user, roles)` and used in `AdminLoginHandlerTests`. The following chain methods have no callers anywhere in the unit test suite:

| Method | Notes |
|--------|-------|
| `WithUser(UserEntity user)` | Redundant with `AuthDataBuilder(UserEntity user)` constructor |
| `WithUserPermissions(List<RolePermissionEntity>)` | No test needs a custom user-permissions list |
| `WithUserPermission(RolePermissionEntity)` | Single-item variant of above |
| `WithRole(RoleDto role)` | Single-item variant of `WithRoles`; no test calls it directly |
| `WithPermissions(List<PermissionDto>)` | No test needs a custom permissions list |
| `WithPermission(PermissionDto)` | Single-item variant of above |

---

## Summary

| Category | Unused Items |
|----------|-------------|
| Factory methods | 14 methods across 8 factories (all `new OtpBuilder/SessionBuilder/RoleBuilder()` calls in test files replaced) |
| `AuthDataBuilder` chain methods | 6 methods (all direct `new AuthDataBuilder()` calls replaced via `AuthTestHelpers`) |
| `TestConstants` entries | ~18 constants across 5 nested classes (`ValidationMessages.Role` fully used by new validator tests) |

All items above are preserved. No code was deleted.
