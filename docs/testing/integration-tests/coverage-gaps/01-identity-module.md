# Identity Module - Integration Test Coverage Specifications

**Current Coverage:** 92.5% (7,167 / 7,741 lines) | Branch: 44.9%
**Uncovered Lines:** 574
**Target Coverage:** 100% (excluding dead code and structurally blocked value objects)

## 1. Handlers at 0% - Precise Test Specifications

### 1.1 AdminSignOut (0%)

**Source:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Admin/Commands/SignOut/`
**Endpoint:** `POST /api/v1/admin/auth/sign-out`
**Auth:** `RequireAdminOrSuperAdmin`
**Validator:** `AdminSignOutValidator` - validates `RefreshToken` via `ValidRefreshToken()`
**Existing tests:** 2 tests in `AdminSignOutEndpointV1Tests.cs` (NoAuth->401, SignOutAll NoAuth->401)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `SignOut_AsSuperAdmin_WithValidRefreshToken_ReturnsOk` | Seed `SessionEntity` for SuperAdmin via `SessionFactory.Create()`, set known refresh token hash. Seed user via `UserFactory.CreateVerifiedActive()` with SuperAdmin role. | `POST /admin/auth/sign-out` body: `{ RefreshToken: "<known-token>" }` | 200 OK | `AdminSignOutHandler` full happy path, `AdminSignOutValidator`, `SessionFactory.SignOutAsync()`, `TokenDeliveryService.ClearTokenCookies` (web sign-out path) |
| 2 | `SignOut_AsSuperAdmin_WithEmptyRefreshToken_ReturnsValidationError` | None | `POST /admin/auth/sign-out` body: `{ RefreshToken: "" }` | 400/422 | `AdminSignOutValidator.ValidRefreshToken()` rule |
| 3 | `SignOut_AsSuperAdmin_WithInvalidRefreshToken_ReturnsError` | None | `POST /admin/auth/sign-out` body: `{ RefreshToken: "invalid-token-value" }` | 403 | `SessionErrors.InvalidRefreshToken()` -> `AuthenticationException`, `AuthenticationErrorMessage.InvalidCredentials()` |

### 1.2 AdminRefreshToken (0%)

**Source:** `src/Modules/Identity/Identity/Application/Session/UseCases/Admin/Commands/RefreshToken/`
**Endpoint:** `POST /api/v1/admin/sessions/refresh-token`
**Auth:** `AllowAnonymous`
**Validator:** `AdminRefreshTokenValidator` - validates `RefreshToken` via `ValidRefreshToken()`
**Existing tests:** 2 tests (NoToken->403, InvalidTokenInBody->403)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `RefreshToken_WithEmptyToken_ReturnsValidationError` | None | `POST /admin/sessions/refresh-token` body: `{ RefreshToken: "" }` | 400/422 | `AdminRefreshTokenValidator` empty rule |
| 2 | `RefreshToken_WithExpiredSession_ReturnsForbidden` | Seed `SessionFactory.CreateExpired()` for admin user | `POST /admin/sessions/refresh-token` with expired token in cookie | 403 | `AdminRefreshTokenHandler` expired session path, `RefreshTokenFactory` null-session throw, `TokenDeliveryService.ReadRefreshToken` web branch, admin cookie path |

### 1.3 PublicChangePassword (0%)

**Source:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/ChangePassword/`
**Endpoint:** `PATCH /api/v1/public/auth/change-password`
**Auth:** `RequireVisitorOnly`
**Validator:** `PublicChangePasswordValidator` - validates `OldPassword` via `ValidOldPassword()`, `NewPassword` via `ValidPassword()`
**Handler checks:** user exists -> active -> verified -> session valid -> password configured (not OAuth) -> old password correct -> new != old
**Existing tests:** 2 tests (NoAuth->401, AsAdmin->403)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `ChangePassword_AsVisitor_WithValidPasswords_ReturnsOk` | Seed `UserFactory.CreateVerifiedActive()` with known bcrypt hash for `Test123!abc`, assign Visitor role, seed active session | `PATCH /public/auth/change-password` body: `{ OldPassword: "Test123!abc", NewPassword: "NewPass456!xyz" }` | 200 OK | Full handler happy path, `UserEntity.ChangePassword()`, `AuthRepository` password update |
| 2 | `ChangePassword_AsVisitor_WithEmptyOldPassword_ReturnsValidationError` | None | body: `{ OldPassword: "", NewPassword: "NewPass456!xyz" }` | 400/422 | `PublicChangePasswordValidator.ValidOldPassword()` |
| 3 | `ChangePassword_AsVisitor_WithEmptyNewPassword_ReturnsValidationError` | None | body: `{ OldPassword: "Test123!abc", NewPassword: "" }` | 400/422 | `PublicChangePasswordValidator.ValidPassword()` |
| 4 | `ChangePassword_AsVisitor_WithSamePassword_ReturnsConflict` | Same seed as #1 | body: `{ OldPassword: "Test123!abc", NewPassword: "Test123!abc" }` | 409 | `UserErrors.NewPasswordSameAsOld()`, `ConflictErrorMessage.NewPasswordSameAsOld()` |
| 5 | `ChangePassword_AsVisitor_WithWrongOldPassword_ReturnsBadRequest` | Same seed as #1 | body: `{ OldPassword: "WrongPass!1", NewPassword: "NewPass456!xyz" }` | 400 | `UserErrors.IncorrectCurrentPassword()`, `ValidationErrorMessage` password methods |
| 6 | `ChangePassword_AsVisitor_OAuthUserWithoutPassword_ReturnsBadRequest` | Seed `UserFactory.CreateExternalAuth()` (no password hash), assign Visitor role | body: `{ OldPassword: "Test123!abc", NewPassword: "NewPass456!xyz" }` | 400 | `UserErrors.PasswordNotConfigured()`, `ValidationErrorMessage` password-related methods |

### 1.4 PublicResetPassword (0%)

**Source:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/ResetPassword/`
**Endpoint:** `POST /api/v1/public/auth/reset-password`
**Auth:** `AllowAnonymous`
**Validator:** `PublicResetPasswordValidator` - validates `Email`, `Code` (6-digit via `ValidOtpCode()`), `NewPassword` via `ValidPassword()`
**Handler:** calls `authFactory.GetUserForResetAsync(email)` -> `otpRepository.ValidateUsedOtpAsync(code, userId, PasswordReset)` -> `authFactory.ResetPasswordAsync(user, newPassword)`

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `ResetPassword_WithValidOtp_ReturnsOk` | Seed `UserFactory.CreateVerifiedActive()`, `OtpFactory.CreateValid(userId)` (code=`123456`, purpose=PasswordReset) | `POST /public/auth/reset-password` body: `{ Email: "user@test.com", Code: "123456", NewPassword: "NewPass456!xyz" }` | 200 OK | Full handler happy path, OTP validation, `UserEntity.ResetPassword()`, `AuthRepository.ResetPasswordAsync()` |
| 2 | `ResetPassword_WithEmptyEmail_ReturnsValidationError` | None | body: `{ Email: "", Code: "123456", NewPassword: "NewPass456!xyz" }` | 400/422 | Validator `Email` rule |
| 3 | `ResetPassword_WithInvalidOtpCode_ReturnsValidationError` | None | body: `{ Email: "user@test.com", Code: "12", NewPassword: "NewPass456!xyz" }` | 400/422 | Validator `ValidOtpCode()` (must be 6 digits) |
| 4 | `ResetPassword_WithNonExistentUser_ReturnsNotFound` | None | body: `{ Email: "nobody@test.com", Code: "123456", NewPassword: "NewPass456!xyz" }` | 404 | `UserErrors.UserNotFoundByEmail()` |
| 5 | `ResetPassword_WithExpiredOtp_Returns410` | Seed user + `OtpFactory.CreateExpired()` | body with correct email/code | 410 | `UserErrors.OtpExpired()`, `OtpExpirationException`, `OtpExpirationExceptionHandler`, `OtpEntity` expiration branch |
| 6 | `ResetPassword_WithMaxAttemptsOtp_Returns429` | Seed user + `OtpFactory.CreateMaxAttempts()` (attempts >= 5) | body with correct email/wrong code | 429 | `UserErrors.MaxOtpAttemptsReached()`, `OtpAttemptsLimitException`, `OtpAttemptsLimitExceptionHandler`, `OtpEntity` attempts branch |
| 7 | `ResetPassword_WithWrongOtpCode_ReturnsBadRequest` | Seed user + `OtpFactory.CreateValid(userId)` (code=`123456`) | body: `{ Email: "user@test.com", Code: "654321", NewPassword: "NewPass456!xyz" }` | 400 | `UserErrors.InvalidOtpCode()` |
| 8 | `ResetPassword_WithNoValidOtp_ReturnsBadRequest` | Seed user with no OTP records | body: `{ Email: "user@test.com", Code: "123456", NewPassword: "NewPass456!xyz" }` | 400 | `UserErrors.NoValidOtpFound()` |

### 1.5 PublicSetPassword (0%)

**Source:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/SetPassword/`
**Endpoint:** `POST /api/v1/public/auth/set-password`
**Auth:** `RequireVisitorOnly`
**Validator:** `PublicSetPasswordValidator` - validates `Password` via `ValidPassword()`
**Handler:** For OAuth users only - calls `authRepository.SetPasswordForExternalUser(user, hashedPassword)`
**Existing tests:** 1 test (NoAuth->401)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `SetPassword_AsVisitor_OAuthUser_ReturnsOk` | Seed `UserFactory.CreateExternalAuth()` (no password hash), assign Visitor role, seed session | `POST /public/auth/set-password` body: `{ Password: "NewPass456!xyz" }` | 200 OK | Full handler happy path, `UserEntity.SetPasswordForExternalUser()`, `AuthRepository.SetPasswordForExternalUser()` |
| 2 | `SetPassword_AsVisitor_WithEmptyPassword_ReturnsValidationError` | None | body: `{ Password: "" }` | 400/422 | `PublicSetPasswordValidator.ValidPassword()` |
| 3 | `SetPassword_AsVisitor_AlreadyHasPassword_ReturnsBadRequest` | Seed `UserFactory.CreateVerifiedActive()` (has password) | body: `{ Password: "NewPass456!xyz" }` | 400 | `UserErrors.PasswordOnlyForExternalAuth()` |
| 4 | `SetPassword_AsVisitor_WithMissingEmail_ReturnsBadRequest` | Seed `UserFactory.CreateExternalAuth()` with null email | body: `{ Password: "NewPass456!xyz" }` | 400 | `UserErrors.EmailRequiredToSetPassword()` |

### 1.6 PublicRevokeSession (0% handler, 0% validator)

**Source:** `src/Modules/Identity/Identity/Application/Session/UseCases/Public/Commands/RevokeSession/`
**Endpoint:** `POST /api/v1/public/me/sessions/revoke/{id}`
**Auth:** `RequireVisitorOnly`
**Validator:** `PublicRevokeSessionValidator` - validates `SessionId` via `IsValidGuid()`
**Handler:** Parses GUID, fetches session, verifies `session.UserId == command.UserId`, calls `sessionRepository.RevokeAsync()`
**Existing tests:** 1 test (NoAuth->401)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `RevokeSession_AsVisitor_OwnSession_ReturnsOk` | Seed `SessionFactory.Create()` for Visitor user (ID: `TestConstants.User.VisitorId`) | `POST /public/me/sessions/revoke/{sessionId}` | 200 OK | Full handler + validator happy path |
| 2 | `RevokeSession_AsVisitor_InvalidGuid_ReturnsValidationError` | None | `POST /public/me/sessions/revoke/not-a-guid` | 400/422 | `PublicRevokeSessionValidator.IsValidGuid()` |
| 3 | `RevokeSession_AsVisitor_NonExistentSession_ReturnsNotFound` | None | `POST /public/me/sessions/revoke/{random-guid}` | 404 | `SessionErrors.SessionNotFound(id)` |
| 4 | `RevokeSession_AsVisitor_OtherUsersSession_ReturnsForbidden` | Seed session for a different user | `POST /public/me/sessions/revoke/{otherSessionId}` | 403 | Handler ownership check (`session.UserId != command.UserId`), `UserErrors.InsufficientPermissions()`, `AccessDeniedException`, `AccessDeniedExceptionHandler`, `AuthorizationErrorMessage.AccessDenied()` |

### 1.7 PublicUpdateAvatar (0%)

**Source:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/UpdateAvatar/`
**Endpoint:** `PATCH /api/v1/public/auth/update-avatar`
**Auth:** `RequireVisitorOnly`
**Validator:** `PublicUpdateAvatarValidator` - validates avatar file
**Auth factory:** `PublicUpdateAvatarAuthFactory`

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `UpdateAvatar_AsVisitor_WithValidFile_ReturnsOk` | Seed `UserFactory.CreateVerifiedActive()` with Visitor role, seed session | `PATCH /public/auth/update-avatar` multipart with valid image file (JPEG, under size limit) | 200 OK | `PublicUpdateAvatarHandler` happy path, `PublicUpdateAvatarAuthFactory`, `PublicUpdateAvatarValidator`, `UserEntity.UpdateAvatar()`, `ValidationErrorMessage.AvatarUrlInvalid()` (valid path) |
| 2 | `UpdateAvatar_AsVisitor_WithNoFile_ReturnsValidationError` | None | `PATCH /public/auth/update-avatar` empty multipart | 400/422 | `PublicUpdateAvatarValidator` required file rule, `FileValidation.ValidAvatar` isRequired=true null file branch |
| 3 | `UpdateAvatar_AsVisitor_WithOversizedFile_ReturnsValidationError` | None | `PATCH /public/auth/update-avatar` multipart with file exceeding size limit | 400/422 | `FileValidation.ValidAvatar` oversized file branch |
| 4 | `UpdateAvatar_AsVisitor_WithInvalidMimeType_ReturnsValidationError` | None | `PATCH /public/auth/update-avatar` multipart with `.txt` file | 400/422 | `FileValidation.ValidAvatar` wrong MIME type branch |
| 5 | `UpdateAvatar_AsVisitor_WithInvalidExtension_ReturnsValidationError` | None | `PATCH /public/auth/update-avatar` multipart with `.bmp` file (valid MIME but disallowed extension) | 400/422 | `FileValidation.ValidAvatar` wrong extension branch |

## 2. Error Classes and Messages - Target 100%

Every error factory method must be exercised by at least one integration test. The table below maps each method to the test that triggers it.

### 2.1 UserErrors.cs (25.4% -> 100%)

33 factory methods. Each returns a typed exception with a specific error message.

| # | Method | Exception Type | Triggered By Test | Test Scenario |
|---|--------|---------------|-------------------|---------------|
| 1 | `PhoneNumberAlreadyExists` | Conflict | Section 9 #3 (AdminUpdateProfile) | Update profile with phone number that belongs to another user |
| 2 | `RoleAlreadyExists` | Conflict | Section 9 #4 (AdminCreateRole) | Create role with name that already exists |
| 3 | `RoleAlreadyAssignedToUser` | Conflict | Section 9 #5 (AdminAssignRoleToUser) | Assign same role to user twice |
| 4 | `RoleNotFoundByName` | NotFound | Structurally hard - requires missing Visitor role in seeded data; see Section 10 |
| 5 | `PermissionAlreadyExists` | Conflict | Section 9 #6 (AdminCreatePermission) | Create permission with name that already exists |
| 6 | `PermissionAlreadyAssignedToRole` | Conflict | Section 9 #7 (AdminAssignPermissionToRole) | Assign same permission to role twice |
| 7 | `RoleAlreadyActive` | Conflict | Section 9 #8 (AdminActivateRole) | Activate role that is already active |
| 8 | `RoleAlreadyInactive` | Conflict | Section 9 #9 (AdminDeactivateRole) | Deactivate role that is already inactive |
| 9 | `RoleAlreadyDeleted` | Conflict | Section 9 #10 (AdminSoftDeleteRole) | Soft-delete role that is already deleted |
| 10 | `RoleNotDeleted` | Conflict | Section 9 #11 (AdminRestoreRole) | Restore role that is not deleted |
| 11 | `PermissionAlreadyActive` | Conflict | Section 9 #12 (AdminActivatePermission) | Activate permission that is already active |
| 12 | `PermissionAlreadyInactive` | Conflict | Section 9 #13 (AdminDeactivatePermission) | Deactivate permission that is already inactive |
| 13 | `PermissionAlreadyDeleted` | Conflict | Section 9 #14 (AdminSoftDeletePermission) | Soft-delete permission that is already deleted |
| 14 | `PermissionNotDeleted` | Conflict | Section 9 #15 (AdminRestorePermission) | Restore permission that is not deleted |
| 15 | `CoreRoleCannotBeModified` | BadRequest | Dead code - see Section 10 |
| 16 | `CoreRoleCannotBeDeleted` | BadRequest | Section 9 #16 (AdminHardDeleteRole) | Hard-delete a core/system role (SuperAdmin, Admin, or Visitor) |
| 17 | `RoleIsInactive` | BadRequest | Section 9 #17 (AdminAssignRoleToUser) | Assign an inactive role to a user |
| 18 | `RoleIsDeleted` | BadRequest | Section 9 #18 (AdminAssignRoleToUser) | Assign a deleted role to a user |
| 19 | `PermissionIsInactive` | BadRequest | Section 9 #19 (AdminAssignPermissionToRole) | Assign an inactive permission to a role |
| 20 | `PermissionIsDeleted` | BadRequest | Section 9 #20 (AdminAssignPermissionToRole) | Assign a deleted permission to a role |
| 21 | `PermissionNotAssignedToRole` | BadRequest | Section 9 #21 (AdminRemovePermissionFromRole) | Remove permission that is not assigned to the role |
| 22 | `RoleNotAssignedToUser` | BadRequest | Section 9 #22 (AdminRemoveRoleFromUser) | Remove role that is not assigned to the user |
| 23 | `AccountInactive` | AccountInactiveException | Section 7 #1 (AccountStatusRequirementHandler) | Access protected endpoint with inactive account |
| 24 | `AccountNotVerified` | AccountNotVerifiedException | Section 7 #2 (AccountStatusRequirementHandler) | Access protected endpoint with unverified account |
| 25 | `InvalidCredentials` | AuthenticationException | Section 9 #23 (PublicLogin) | Login with wrong password |
| 26 | `AccountAlreadyVerified` | Conflict | Section 9 #24 (PublicVerifyOtp) | Verify OTP for already-verified account |
| 27 | `NoValidOtpFound` | BadRequest | Section 1.4 #8 (PublicResetPassword) | Reset password with no OTP records |
| 28 | `InvalidOtpCode` | BadRequest | Section 1.4 #7 (PublicResetPassword) | Reset password with wrong OTP code |
| 29 | `OtpExpired` | OtpExpirationException | Section 1.4 #5 (PublicResetPassword) | Reset password with expired OTP |
| 30 | `MaxOtpAttemptsReached` | OtpAttemptsLimitException | Section 1.4 #6 (PublicResetPassword) | Reset password after 5+ failed OTP attempts |
| 31 | `InvalidUserAuthentication` | AuthenticationException | Section 9 #25 (TamperedJWT) | Access endpoint with tampered/invalid JWT claims |
| 32 | `InsufficientPermissions` | AccessDeniedException | Section 1.6 #4 (PublicRevokeSession) | Revoke another user's session |
| 33 | `NewPasswordSameAsOld` | Conflict | Section 1.3 #4 (PublicChangePassword) | Change password to same value |
| 34 | `PasswordNotConfigured` | BadRequest | Section 1.3 #6 (PublicChangePassword) | OAuth user without password tries to change password |
| 35 | `IncorrectCurrentPassword` | BadRequest | Section 1.3 #5 (PublicChangePassword) | Change password with wrong current password |
| 36 | `EmailRequiredToSetPassword` | BadRequest | Section 1.5 #4 (PublicSetPassword) | Set password without email |
| 37 | `PasswordOnlyForExternalAuth` | BadRequest | Section 1.5 #3 (PublicSetPassword) | Non-OAuth user tries to set password |

### 2.2 SessionErrors.cs (60% -> 100%)

| # | Method | Exception Type | Triggered By Test | Test Scenario |
|---|--------|---------------|-------------------|---------------|
| 1 | `InvalidRefreshToken` | AuthenticationException | Section 1.1 #3 (AdminSignOut) | Sign out with invalid refresh token |
| 2 | `SessionNotFound` | NotFoundException | Section 1.6 #3 (PublicRevokeSession) | Revoke non-existent session |
| 3 | `DeviceIdRequired` | BadRequest | Section 9 #26 (PublicLogin) | Login without deviceId in request headers/body |

### 2.3 AuthenticationErrorMessage.cs (37.5% -> 100%)

6 methods. Covered transitively by the tests that trigger `AuthenticationException` instances.

| # | Method | Triggered By |
|---|--------|-------------|
| 1 | `InvalidCredentials` | Section 9 #23 (wrong password login) |
| 2 | `InvalidToken` | Section 1.1 #3 (invalid refresh token) |
| 3 | `TokenExpired` | Section 1.2 #2 (expired session refresh) |
| 4 | `InvalidUserAuthentication` | Section 9 #25 (tampered JWT) |
| 5 | `SessionExpired` | Section 1.2 #2 (expired session) |
| 6 | `AccountNotVerified` | Section 7 #2 (unverified account) |

### 2.4 AuthorizationErrorMessage.cs (80% -> 100%)

| # | Method | Triggered By |
|---|--------|-------------|
| 1 | `AccessDenied` | Section 1.6 #4 (revoke other user's session) |
| 2 | `InsufficientPermissions` | Already covered |
| 3 | `Unauthorized` | Already covered |

### 2.5 ConflictErrorMessage.cs (29.4% -> 100%)

17 methods. Each maps 1:1 to a `UserErrors` conflict method. All covered transitively.

| # | Method | Triggered By |
|---|--------|-------------|
| 1 | `PhoneNumberAlreadyExists` | Section 9 #3 |
| 2 | `RoleAlreadyExists` | Section 9 #4 |
| 3 | `RoleAlreadyAssignedToUser` | Section 9 #5 |
| 4 | `PermissionAlreadyExists` | Section 9 #6 |
| 5 | `PermissionAlreadyAssignedToRole` | Section 9 #7 |
| 6 | `RoleAlreadyActive` | Section 9 #8 |
| 7 | `RoleAlreadyInactive` | Section 9 #9 |
| 8 | `RoleAlreadyDeleted` | Section 9 #10 |
| 9 | `RoleNotDeleted` | Section 9 #11 |
| 10 | `PermissionAlreadyActive` | Section 9 #12 |
| 11 | `PermissionAlreadyInactive` | Section 9 #13 |
| 12 | `PermissionAlreadyDeleted` | Section 9 #14 |
| 13 | `PermissionNotDeleted` | Section 9 #15 |
| 14 | `NewPasswordSameAsOld` | Section 1.3 #4 |
| 15 | `AccountAlreadyVerified` | Section 9 #24 |
| 16 | `CoreRoleCannotBeDeleted` | Section 9 #16 |
| 17 | `CoreRoleCannotBeModified` | Dead code - Section 10 |

### 2.6 ValidationErrorMessage.cs (71.4% -> 100%)

49 methods. Uncovered methods and the tests that cover them:

| # | Method | Triggered By |
|---|--------|-------------|
| 1 | `ExportColumnsInvalid` | Section 5.4 #2 (session export invalid columns) |
| 2 | `ExportFormatInvalid` | Section 5.4 #1 (session export invalid format) |
| 3 | `ExportStatusInvalid` | Section 5.4 #3 (session export invalid status) |
| 4 | `ExportDateRangeInvalid` | Section 5.4 #4 (session export invalid date range) |
| 5 | `AvatarUrlInvalid` | Section 1.7 #4 (invalid MIME type avatar) |
| 6 | `CountryCodeInvalid` | Section 9 #27 (update profile with invalid country code) |
| 7 | `CountryCodeRequired` | Section 9 #28 (update profile with phone but no country code) |
| 8 | `PhoneNumberInvalid` | Section 9 #29 (update profile with invalid phone format) |
| 9 | `CoreRoleCannotBeModified` | Dead code - Section 10 |
| 10 | `CoreRoleCannotBeDeleted` | Section 9 #16 (hard-delete core role) |
| 11 | `PasswordNotConfigured` | Section 1.3 #6 (OAuth user change password) |
| 12 | `IncorrectCurrentPassword` | Section 1.3 #5 (wrong current password) |
| 13 | `EmailRequiredToSetPassword` | Section 1.5 #4 (set password without email) |
| 14 | `PasswordOnlyForExternalAuth` | Section 1.5 #3 (non-OAuth set password) |

## 3. Specifications - Target 100%

All 20 specifications at 0%. Each is a 2-3 line predicate class covered transitively when query builders and handlers exercise them.

| # | Specification | Used By | Integration Test That Covers It |
|---|---------------|---------|-------------------------------|
| 1 | `UserByPhoneNumberSpecification` | `AuthRepository.GetUserByPhoneNumberAsync` | Section 9 #3 (update profile with duplicate phone) |
| 2 | `ActivePermissionSpecification` | `PermissionQueryBuilder` active filter | Section 4.2 #3 (`GET /admin/permissions?isActive=true`) |
| 3 | `ActiveRoleSpecification` | `RoleQueryBuilder` active filter | Section 4.3 #3 (`GET /admin/roles?isActive=true`) |
| 4 | `PermissionIsDeletedSpecification` | `PermissionQueryBuilder.WithDeletedStatus(true)` | Section 4.2 #1 (`GET /admin/permissions?isDeleted=true`) |
| 5 | `PermissionNotDeletedSpecification` | `PermissionQueryBuilder.WithDeletedStatus(false)` | Section 4.2 #2 (`GET /admin/permissions?isDeleted=false`) |
| 6 | `RoleIsDeletedSpecification` | `RoleQueryBuilder.WithDeletedStatus(true)` | Section 4.3 #1 (`GET /admin/roles?isDeleted=true`) |
| 7 | `RoleIsNotActiveSpecification` | `RoleQueryBuilder.WithActiveStatus(false)` | Section 4.3 #4 (`GET /admin/roles?isActive=false`) |
| 8 | `RoleNotDeletedSpecification` | `RoleQueryBuilder.WithDeletedStatus(false)` | Section 4.3 #2 (`GET /admin/roles?isDeleted=false`) |
| 9 | `RolePermissionByIdSpecification` | Direct lookup in handlers | Section 9 #21 (remove permission from role) |
| 10 | `UserHasAdminRoleSpecification` | `AuthRepository.IsUserAdmin`, `UserIsActiveAdminSpecification` | Section 9 #23 (admin login test) |
| 11 | `UserHasRoleSpecification` | Composable spec | Section 9 #5 (role assignment tests) |
| 12 | `UserHasVisitorRoleSpecification` | Composable spec | Public login/sign-up tests (existing) |
| 13 | `UserIsActiveAdminSpecification` | Composite spec | Admin endpoint with active admin (existing admin tests) |
| 14 | `UserIsActiveAndVerifiedSpecification` | Composite for public auth | Section 7 #2 (unverified user test) |
| 15 | `UserIsActiveSpecification` | Component of composites | Section 7 #1 (inactive user test) |
| 16 | `UserIsVerifiedSpecification` | Component of composites | Section 7 #2 (unverified user test) |
| 17 | `SessionByIpAddressSpecification` | `SessionQueryBuilder.WithIpAddress` | Section 4.1 #1 (`GET /admin/sessions?ipAddress=127.0.0.1`) |
| 18 | `SessionCreatedAfterSpecification` | `SessionQueryBuilder.WithFromDate` | Section 4.1 #2 (`GET /admin/sessions?fromDate=2026-01-01`) |
| 19 | `SessionCreatedBeforeSpecification` | `SessionQueryBuilder.WithToDate` | Section 4.1 #2 (`GET /admin/sessions?toDate=2026-12-31`) |
| 20 | `SessionIsRevokedSpecification` | `SessionQueryBuilder` revoked status | Section 4.1 #4 (`GET /admin/sessions?status=Revoked`) |

## 4. Query Builders - Target 100%

### 4.1 SessionQueryBuilder (38% -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Session/Builders/SessionQueryBuilder.cs`
**Endpoint:** `GET /api/v1/admin/sessions`
**Methods to cover:** `WithStatus`, `WithIpAddress`, `WithFromDate`, `WithToDate`, `WithActiveStatus`
**Existing tests:** 3 tests (SuperAdmin->200, NoAuth->401, Visitor->403) - no filter tests

**Seed data for all filter tests:** 3+ sessions with different attributes:
- Session A: IP `127.0.0.1`, created `2026-03-15`, status Active
- Session B: IP `192.168.1.1`, created `2026-06-15`, status Revoked
- Session C: IP `10.0.0.1`, created `2025-11-01`, status Active (inactive)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetAllSessions_FilterByIpAddress_ReturnsFiltered` | Sessions A, B, C as above | `GET /admin/sessions?ipAddress=127.0.0.1` | 200 OK, response contains only Session A | `SessionQueryBuilder.WithIpAddress()`, `SessionByIpAddressSpecification` |
| 2 | `GetAllSessions_FilterByDateRange_ReturnsFiltered` | Sessions A, B, C as above | `GET /admin/sessions?fromDate=2026-01-01&toDate=2026-12-31` | 200 OK, response contains Sessions A and B only (C is before range) | `SessionQueryBuilder.WithFromDate()`, `SessionQueryBuilder.WithToDate()`, `SessionCreatedAfterSpecification`, `SessionCreatedBeforeSpecification` |
| 3 | `GetAllSessions_FilterByStatusActive_ReturnsFiltered` | Sessions A, B, C as above | `GET /admin/sessions?status=Active` | 200 OK, response contains only active sessions | `SessionQueryBuilder.WithStatus("Active")`, `WithActiveStatus(true)` |
| 4 | `GetAllSessions_FilterByStatusRevoked_ReturnsFiltered` | Sessions A, B, C as above | `GET /admin/sessions?status=Revoked` | 200 OK, response contains only Session B | `SessionQueryBuilder.WithStatus("Revoked")`, `SessionIsRevokedSpecification` |
| 5 | `GetAllSessions_FilterByInactive_ReturnsFiltered` | Sessions A, B, C as above | `GET /admin/sessions?isActive=false` | 200 OK, response contains only inactive sessions | `SessionQueryBuilder.WithActiveStatus(false)` |

### 4.2 PermissionQueryBuilder (77.2% -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Roles/Builders/PermissionQueryBuilder.cs`
**Endpoint:** `GET /api/v1/admin/permissions`
**Methods to cover:** `WithDeletedStatus`

**Seed data:** 3+ permissions:
- Permission X: active, not deleted
- Permission Y: inactive, not deleted
- Permission Z: active, soft-deleted (IsDeleted=true)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetAllPermissions_FilterByDeletedTrue_ReturnsDeleted` | Permissions X, Y, Z as above | `GET /admin/permissions?isDeleted=true` | 200 OK, response contains only Permission Z | `PermissionQueryBuilder.WithDeletedStatus(true)`, `PermissionIsDeletedSpecification` |
| 2 | `GetAllPermissions_FilterByDeletedFalse_ReturnsNonDeleted` | Permissions X, Y, Z as above | `GET /admin/permissions?isDeleted=false` | 200 OK, response contains Permissions X and Y | `PermissionQueryBuilder.WithDeletedStatus(false)`, `PermissionNotDeletedSpecification` |
| 3 | `GetAllPermissions_FilterByActiveTrue_ReturnsActive` | Permissions X, Y, Z as above | `GET /admin/permissions?isActive=true` | 200 OK, response contains only active permissions | `ActivePermissionSpecification` |

### 4.3 RoleQueryBuilder (77.2% -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Roles/Builders/RoleQueryBuilder.cs`
**Endpoint:** `GET /api/v1/admin/roles`
**Methods to cover:** `WithDeletedStatus`

**Seed data:** 3+ roles (beyond core seeded roles):
- Role X: active, not deleted
- Role Y: inactive, not deleted
- Role Z: active, soft-deleted (IsDeleted=true)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetAllRoles_FilterByDeletedTrue_ReturnsDeleted` | Roles X, Y, Z as above | `GET /admin/roles?isDeleted=true` | 200 OK, response contains only Role Z | `RoleQueryBuilder.WithDeletedStatus(true)`, `RoleIsDeletedSpecification` |
| 2 | `GetAllRoles_FilterByDeletedFalse_ReturnsNonDeleted` | Roles X, Y, Z as above | `GET /admin/roles?isDeleted=false` | 200 OK, response contains Roles X and Y (plus core roles) | `RoleQueryBuilder.WithDeletedStatus(false)`, `RoleNotDeletedSpecification` |
| 3 | `GetAllRoles_FilterByActiveTrue_ReturnsActive` | Roles X, Y, Z as above | `GET /admin/roles?isActive=true` | 200 OK, response contains only active roles | `ActiveRoleSpecification` |
| 4 | `GetAllRoles_FilterByActiveFalse_ReturnsInactive` | Roles X, Y, Z as above | `GET /admin/roles?isActive=false` | 200 OK, response contains only Role Y | `RoleIsNotActiveSpecification` |

## 5. Validators - Target 100%

### 5.1 FileValidation.cs (70.4% -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Shared/Validators/FileValidation.cs`
**Method:** `ValidAvatar` with `isRequired=true` branch

Covered by Section 1.7 tests:
| # | Validation Rule | Triggered By |
|---|----------------|-------------|
| 1 | Null file when required | Section 1.7 #2 (no file uploaded) |
| 2 | File exceeds size limit | Section 1.7 #3 (oversized file) |
| 3 | Invalid MIME type | Section 1.7 #4 (text file) |
| 4 | Invalid file extension | Section 1.7 #5 (disallowed extension) |

### 5.2 ValidationUtils.cs (83.3% -> 100%)

**Source:** `src/Shared/Shared/Validators/ValidationUtils.cs`
**Uncovered methods:** `ValidUrl` non-HTTP scheme, `GetPropertyValue` property not found

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `UpdateProfile_WithFtpUrl_ReturnsValidationError` | Seed verified active user with Visitor role | `PATCH /public/auth/update-profile` body with `website: "ftp://example.com"` | 400/422 | `ValidationUtils.ValidUrl` non-HTTP scheme branch |
| 2 | `GetPropertyValue_PropertyNotFound_ReturnsNull` | Covered structurally by any validator using a dynamic property lookup that fails | N/A | N/A | `ValidationUtils.GetPropertyValue` property-not-found branch |

### 5.3 SessionValidation (partially covered -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Session/Validators/SessionValidation.cs`
**Uncovered methods:** `ValidExportFormat`, `ValidExportColumns`, `ValidExportStatus`

### 5.4 Session Export Validator Tests

**Endpoint:** `GET /api/v1/admin/sessions/export`

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `ExportSessions_WithInvalidFormat_ReturnsValidationError` | Seed SuperAdmin user with session | `GET /admin/sessions/export?format=invalid` | 400/422 | `SessionValidation.ValidExportFormat()`, `ValidationErrorMessage.ExportFormatInvalid()` |
| 2 | `ExportSessions_WithInvalidColumns_ReturnsValidationError` | Same seed | `GET /admin/sessions/export?format=csv&columns=invalid` | 400/422 | `SessionValidation.ValidExportColumns()`, `ValidationErrorMessage.ExportColumnsInvalid()` |
| 3 | `ExportSessions_WithInvalidStatus_ReturnsValidationError` | Same seed | `GET /admin/sessions/export?format=csv&status=invalid` | 400/422 | `SessionValidation.ValidExportStatus()`, `ValidationErrorMessage.ExportStatusInvalid()` |
| 4 | `ExportSessions_WithInvalidDateRange_ReturnsValidationError` | Same seed | `GET /admin/sessions/export?format=csv&fromDate=2027-01-01&toDate=2026-01-01` | 400/422 | `ValidationErrorMessage.ExportDateRangeInvalid()` |

### 5.5 Handler Validators at 0%

All handler-specific validators (e.g., `AdminSignOutValidator`, `PublicChangePasswordValidator`) are covered by the handler tests in Section 1, since validators run before handlers via the `ValidationDecorator`. Every test hitting an endpoint exercises its validator.

## 6. Exception Handlers at 0%

These exception handler classes convert domain exceptions into HTTP responses. Each must be triggered by at least one integration test.

| # | Exception Handler | HTTP Status | Triggered By Test | Scenario |
|---|------------------|-------------|-------------------|----------|
| 1 | `OtpAttemptsLimitExceptionHandler` | 429 Too Many Requests | Section 1.4 #6 | ResetPassword with OTP that has reached max attempts |
| 2 | `OtpExpirationExceptionHandler` | 410 Gone | Section 1.4 #5 | ResetPassword with expired OTP |
| 3 | `AccessDeniedExceptionHandler` | 403 Forbidden | Section 1.6 #4 | RevokeSession targeting another user's session |
| 4 | `AccountInactiveExceptionHandler` | 423 Locked | Section 7 #1 | Any protected endpoint with inactive user |
| 5 | `AccountNotVerifiedExceptionHandler` | 403 Forbidden | Section 7 #2 | Any protected endpoint with unverified user |

## 7. AccountStatusRequirementHandler (21.8% -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Shared/Authorizations/Handlers/AccountStatusRequirementHandler.cs`
**Triggered by:** Any `RequireVisitorOnly` / `RequireAdminOrSuperAdmin` endpoint with inactive/unverified user

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `AnyEndpoint_WithInactiveUser_Returns423` | Seed `UserFactory.CreateInactive()` with Visitor role, generate JWT for this user | Any protected endpoint (e.g., `GET /public/me/profile`) | 423 Locked | `AccountStatusRequirementHandler` inactive branch, `AccountInactiveExceptionHandler`, `AccountInactiveException`, `UserErrors.AccountInactive()`, `UserIsActiveSpecification` |
| 2 | `AnyEndpoint_WithUnverifiedUser_Returns403` | Seed `UserFactory.CreateUnverified()` with Visitor role, generate JWT | Any protected endpoint (e.g., `GET /public/me/profile`) | 403 Forbidden | `AccountStatusRequirementHandler` unverified branch, `AccountNotVerifiedExceptionHandler`, `AccountNotVerifiedException`, `UserErrors.AccountNotVerified()`, `UserIsVerifiedSpecification`, `UserIsActiveAndVerifiedSpecification` |

## 8. Partially Covered Services

### 8.1 WangkanaiClientOriginDetectionAdapter (47.7% -> 100%)

**Source:** `src/Modules/Identity/Identity/Infrastructure/Adapters/WangkanaiClientOriginDetectionAdapter.cs`
**Method:** `GetInfo()` - switch arms for device/browser/platform detection

Tests require sending requests with specific `User-Agent` headers to any session-creating endpoint (e.g., login).

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `Login_WithMobileUserAgent_DetectsMobileDevice` | Seed verified active user | `POST /public/auth/login` with `User-Agent: Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1` | 200 OK, session metadata shows mobile device/browser/platform | `GetInfo()` mobile device branch, mobile browser branch, iOS platform branch |
| 2 | `Login_WithDesktopUserAgent_DetectsDesktopDevice` | Same seed | `POST /public/auth/login` with `User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36` | 200 OK, session metadata shows desktop device/browser/platform | `GetInfo()` desktop device branch, Chrome browser branch, Windows platform branch |
| 3 | `Login_WithTabletUserAgent_DetectsTabletDevice` | Same seed | `POST /public/auth/login` with `User-Agent: Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1` | 200 OK, session metadata shows tablet device/browser/platform | `GetInfo()` tablet device branch, iPad/iPadOS branch |
| 4 | `Login_WithEmptyUserAgent_DetectsUnknown` | Same seed | `POST /public/auth/login` with `User-Agent: ` (empty string) | 200 OK, session metadata shows Unknown for all fields | `GetInfo()` unknown/fallback branches for device, browser, and platform |

### 8.2 TokenDeliveryService (63.6% -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Shared/Services/TokenDeliveryService.cs`
**Uncovered:** `ClearTokenCookies` (web sign-out), `ReadRefreshToken` web branch, admin cookie path

| # | Uncovered Path | Triggered By Test |
|---|----------------|-------------------|
| 1 | `ClearTokenCookies` (web client) | Section 1.1 #1 (AdminSignOut happy path - web client clears cookies) |
| 2 | `ReadRefreshToken` web branch | Section 1.2 #2 (AdminRefreshToken reads token from cookie) |
| 3 | Admin cookie path | Section 1.2 #2 (AdminRefreshToken with admin-scoped cookie) |

### 8.3 RefreshTokenFactory (54.5% -> 100%)

**Source:** `src/Modules/Identity/Identity/Application/Session/Services/RefreshTokenFactory.cs`
**Uncovered:** null-session throw path

| # | Uncovered Path | Triggered By Test |
|---|----------------|-------------------|
| 1 | Null session -> throw | Section 1.2 #2 (AdminRefreshToken with expired/missing session) |
| 2 | Token generation for admin | Section 1.2 #2 (AdminRefreshToken with valid session, if also testing happy path) |

### 8.4 AuthRepository (52% -> 100%)

**Source:** `src/Modules/Identity/Identity/Infrastructure/Repositories/AuthRepository.cs`
**15 methods, many uncovered.** Covered transitively by handler tests:

| # | Method | Triggered By Test |
|---|--------|-------------------|
| 1 | `SetPasswordForExternalUser()` | Section 1.5 #1 (PublicSetPassword happy path) |
| 2 | `ResetPasswordAsync()` | Section 1.4 #1 (PublicResetPassword happy path) |
| 3 | `GetUserForResetAsync()` | Section 1.4 #1 (PublicResetPassword happy path) |
| 4 | `GetUserByPhoneNumberAsync()` | Section 9 #3 (update profile with duplicate phone) |
| 5 | `IsUserAccountActive()` | Section 7 #1 (inactive user test) |
| 6 | `IsUserAccountVerified()` | Section 7 #2 (unverified user test) |
| 7 | `GetUserIdFromClaims()` | Section 9 #25 (tampered JWT) |
| 8 | `IsUserAdmin()` | Section 9 #25 (non-admin accessing admin check) |
| 9 | `AssignVisitorRoleAsync()` | Public sign-up tests (existing) |
| 10 | `ChangePasswordAsync()` | Section 1.3 #1 (PublicChangePassword happy path) |
| 11 | `ValidateCredentialsAsync()` | Section 9 #23 (login with wrong password) |
| 12 | `SignOutAsync()` | Section 1.1 #1 (AdminSignOut happy path) |
| 13 | `UpdateAvatarAsync()` | Section 1.7 #1 (PublicUpdateAvatar happy path) |

### 8.5 SuperAdminConfiguration (80%)

**Source:** `src/Modules/Identity/Identity/Infrastructure/Configuration/SuperAdminConfiguration.cs`
**Missing:** throw when environment variable is missing

This is structurally blocked from endpoint tests because the configuration is loaded at application startup. The missing branch throws when `SUPER_ADMIN_EMAIL` or `SUPER_ADMIN_PASSWORD` environment variables are not set. This cannot be triggered via HTTP requests.

**Recommendation:** Mark as structurally blocked. Could be covered by a unit test that instantiates the configuration class without the required env vars.

## 9. Error Path Tests for Handlers at ~90% and Additional Error Triggers

These tests exercise the remaining uncovered error paths in handlers that are already at ~90% coverage, plus additional scenarios needed to cover error classes and specifications.

| # | Test Name | Seed Data | Request | Expected Status | Covers |
|---|-----------|-----------|---------|-----------------|--------|
| 1 | `ActivatePermission_AlreadyActive_ReturnsConflict` | Seed active permission | `PATCH /admin/permissions/{id}/activate` | 409 Conflict | `ActivatePermissionHandler` already-active branch, `UserErrors.PermissionAlreadyActive()`, `ConflictErrorMessage.PermissionAlreadyActive()`, `PermissionEntity.Activate()` guard |
| 2 | `DeactivatePermission_AlreadyInactive_ReturnsConflict` | Seed inactive permission | `PATCH /admin/permissions/{id}/deactivate` | 409 Conflict | `DeactivatePermissionHandler` already-inactive branch, `UserErrors.PermissionAlreadyInactive()`, `ConflictErrorMessage.PermissionAlreadyInactive()` |
| 3 | `UpdateProfile_WithDuplicatePhone_ReturnsConflict` | Seed 2 users, both with Visitor role. User B has phone `+1234567890` | `PATCH /public/auth/update-profile` as User A with body `{ PhoneNumber: "+1234567890", CountryCode: "US" }` | 409 Conflict | `UserErrors.PhoneNumberAlreadyExists()`, `ConflictErrorMessage.PhoneNumberAlreadyExists()`, `UserByPhoneNumberSpecification`, `AuthRepository.GetUserByPhoneNumberAsync()` |
| 4 | `CreateRole_WithDuplicateName_ReturnsConflict` | Seed role named "TestRole" | `POST /admin/roles` body: `{ Name: "TestRole", Description: "duplicate" }` | 409 Conflict | `UserErrors.RoleAlreadyExists()`, `ConflictErrorMessage.RoleAlreadyExists()` |
| 5 | `AssignRoleToUser_AlreadyAssigned_ReturnsConflict` | Seed user with "TestRole" already assigned | `POST /admin/users/{userId}/roles` body: `{ RoleId: "{testRoleId}" }` | 409 Conflict | `UserErrors.RoleAlreadyAssignedToUser()`, `ConflictErrorMessage.RoleAlreadyAssignedToUser()`, `UserHasRoleSpecification` |
| 6 | `CreatePermission_WithDuplicateName_ReturnsConflict` | Seed permission with resource="test", action="read" | `POST /admin/permissions` body: `{ Resource: "test", Action: "read", Description: "duplicate" }` | 409 Conflict | `UserErrors.PermissionAlreadyExists()`, `ConflictErrorMessage.PermissionAlreadyExists()` |
| 7 | `AssignPermissionToRole_AlreadyAssigned_ReturnsConflict` | Seed role with permission already assigned | `POST /admin/roles/{roleId}/permissions` body: `{ PermissionId: "{permId}" }` | 409 Conflict | `UserErrors.PermissionAlreadyAssignedToRole()`, `ConflictErrorMessage.PermissionAlreadyAssignedToRole()` |
| 8 | `ActivateRole_AlreadyActive_ReturnsConflict` | Seed active role | `PATCH /admin/roles/{id}/activate` | 409 Conflict | `UserErrors.RoleAlreadyActive()`, `ConflictErrorMessage.RoleAlreadyActive()`, `RoleEntity.Activate()` guard, `ActiveRoleSpecification` |
| 9 | `DeactivateRole_AlreadyInactive_ReturnsConflict` | Seed inactive role | `PATCH /admin/roles/{id}/deactivate` | 409 Conflict | `UserErrors.RoleAlreadyInactive()`, `ConflictErrorMessage.RoleAlreadyInactive()`, `RoleEntity.Deactivate()` guard |
| 10 | `SoftDeleteRole_AlreadyDeleted_ReturnsConflict` | Seed soft-deleted role | `DELETE /admin/roles/{id}` | 409 Conflict | `UserErrors.RoleAlreadyDeleted()`, `ConflictErrorMessage.RoleAlreadyDeleted()`, `RoleEntity.SoftDelete()` guard |
| 11 | `RestoreRole_NotDeleted_ReturnsConflict` | Seed non-deleted role | `PATCH /admin/roles/{id}/restore` | 409 Conflict | `UserErrors.RoleNotDeleted()`, `ConflictErrorMessage.RoleNotDeleted()`, `RoleEntity.Restore()` guard |
| 12 | `ActivatePermission_AlreadyActive_ReturnsConflict` | (duplicate of #1 for clarity in error mapping) Already covered by #1 | Already covered by #1 | 409 | Already covered by #1 |
| 13 | `DeactivatePermission_AlreadyInactive_ReturnsConflict` | (duplicate of #2 for clarity) Already covered by #2 | Already covered by #2 | 409 | Already covered by #2 |
| 14 | `SoftDeletePermission_AlreadyDeleted_ReturnsConflict` | Seed soft-deleted permission | `DELETE /admin/permissions/{id}` | 409 Conflict | `UserErrors.PermissionAlreadyDeleted()`, `ConflictErrorMessage.PermissionAlreadyDeleted()`, `PermissionEntity.SoftDelete()` guard |
| 15 | `RestorePermission_NotDeleted_ReturnsConflict` | Seed non-deleted permission | `PATCH /admin/permissions/{id}/restore` | 409 Conflict | `UserErrors.PermissionNotDeleted()`, `ConflictErrorMessage.PermissionNotDeleted()` |
| 16 | `HardDeleteRole_CoreRole_ReturnsBadRequest` | Use seeded SuperAdmin/Admin/Visitor role ID | `DELETE /admin/roles/{coreRoleId}/hard` | 400 Bad Request | `UserErrors.CoreRoleCannotBeDeleted()`, `ValidationErrorMessage.CoreRoleCannotBeDeleted()`, `ConflictErrorMessage.CoreRoleCannotBeDeleted()` |
| 17 | `AssignRoleToUser_InactiveRole_ReturnsBadRequest` | Seed inactive role, seed user | `POST /admin/users/{userId}/roles` body: `{ RoleId: "{inactiveRoleId}" }` | 400 Bad Request | `UserErrors.RoleIsInactive()` |
| 18 | `AssignRoleToUser_DeletedRole_ReturnsBadRequest` | Seed soft-deleted role, seed user | `POST /admin/users/{userId}/roles` body: `{ RoleId: "{deletedRoleId}" }` | 400 Bad Request | `UserErrors.RoleIsDeleted()` |
| 19 | `AssignPermissionToRole_InactivePermission_ReturnsBadRequest` | Seed inactive permission, seed role | `POST /admin/roles/{roleId}/permissions` body: `{ PermissionId: "{inactivePermId}" }` | 400 Bad Request | `UserErrors.PermissionIsInactive()` |
| 20 | `AssignPermissionToRole_DeletedPermission_ReturnsBadRequest` | Seed soft-deleted permission, seed role | `POST /admin/roles/{roleId}/permissions` body: `{ PermissionId: "{deletedPermId}" }` | 400 Bad Request | `UserErrors.PermissionIsDeleted()` |
| 21 | `RemovePermissionFromRole_NotAssigned_ReturnsBadRequest` | Seed role, seed permission NOT assigned to that role | `DELETE /admin/roles/{roleId}/permissions/{permId}` | 400 Bad Request | `UserErrors.PermissionNotAssignedToRole()`, `RolePermissionByIdSpecification` |
| 22 | `RemoveRoleFromUser_NotAssigned_ReturnsBadRequest` | Seed user, seed role NOT assigned to that user | `DELETE /admin/users/{userId}/roles/{roleId}` | 400 Bad Request | `UserErrors.RoleNotAssignedToUser()` |
| 23 | `PublicLogin_WithWrongPassword_ReturnsUnauthorized` | Seed `UserFactory.CreateVerifiedActive()` with known password | `POST /public/auth/login` body: `{ Email: "user@test.com", Password: "WrongPassword!1" }` | 401 Unauthorized | `UserErrors.InvalidCredentials()`, `AuthenticationException`, `AuthenticationErrorMessage.InvalidCredentials()`, `UserHasAdminRoleSpecification` (negative path), `AuthRepository.ValidateCredentialsAsync()` |
| 24 | `PublicVerifyOtp_AlreadyVerified_ReturnsConflict` | Seed verified user, seed valid OTP | `POST /public/auth/verify-otp` body: `{ Email: "user@test.com", Code: "123456" }` | 409 Conflict | `UserErrors.AccountAlreadyVerified()`, `ConflictErrorMessage.AccountAlreadyVerified()` |
| 25 | `AnyEndpoint_WithTamperedJWT_ReturnsUnauthorized` | Generate JWT with non-existent user ID in claims | Any protected endpoint with tampered token | 401 Unauthorized | `UserErrors.InvalidUserAuthentication()`, `AuthenticationException`, `AuthRepository.GetUserIdFromClaims()` |
| 26 | `PublicLogin_WithoutDeviceId_ReturnsBadRequest` | Seed verified active user | `POST /public/auth/login` body: `{ Email: "user@test.com", Password: "Test123!abc" }` without `X-Device-Id` header or deviceId field | 400 Bad Request | `SessionErrors.DeviceIdRequired()` |
| 27 | `UpdateProfile_WithInvalidCountryCode_ReturnsValidationError` | Seed verified active user with Visitor role | `PATCH /public/auth/update-profile` body: `{ CountryCode: "INVALID", PhoneNumber: "+1234567890" }` | 400/422 | `ValidationErrorMessage.CountryCodeInvalid()` |
| 28 | `UpdateProfile_WithPhoneButNoCountryCode_ReturnsValidationError` | Seed verified active user with Visitor role | `PATCH /public/auth/update-profile` body: `{ PhoneNumber: "+1234567890" }` (no CountryCode) | 400/422 | `ValidationErrorMessage.CountryCodeRequired()` |
| 29 | `UpdateProfile_WithInvalidPhoneFormat_ReturnsValidationError` | Seed verified active user with Visitor role | `PATCH /public/auth/update-profile` body: `{ CountryCode: "US", PhoneNumber: "not-a-phone" }` | 400/422 | `ValidationErrorMessage.PhoneNumberInvalid()` |
| 30 | `BulkUpdateRolePermissions_PermissionNotFound_ReturnsNotFound` | Seed role | `PUT /admin/roles/{roleId}/permissions` body: `{ PermissionIds: ["{non-existent-guid}"] }` | 404 Not Found | `BulkUpdateRolePermissionsHandler` permission-not-found branch |
| 31 | `HardDeleteRole_StillActive_ReturnsBadRequest` | Seed active non-core role | `DELETE /admin/roles/{id}/hard` | 400 Bad Request | `HardDeleteRoleHandler` role-still-active branch |

## 10. Dead Code - Excluded from Coverage Target

| Method | Location | Reason |
|--------|----------|--------|
| `UserErrors.CoreRoleCannotBeModified()` | `UserErrors.cs` | Not called by any handler. No endpoint triggers this path. |
| `ConflictErrorMessage.CoreRoleCannotBeModified()` | `ConflictErrorMessage.cs` | Only used by `UserErrors.CoreRoleCannotBeModified()` which is dead. |
| `ValidationErrorMessage.CoreRoleCannotBeModified()` | `ValidationErrorMessage.cs` | Only used by `UserErrors.CoreRoleCannotBeModified()` which is dead. |
| `UserErrors.RoleNotFoundByName()` | `UserErrors.cs` | Only called by `AuthRepository.AssignVisitorRoleAsync()` when the Visitor role is missing from the database. This requires corrupting seeded data, which is not a valid integration test scenario. |

**Recommendation:** These 4 methods should be excluded from coverage targets. Consider adding `[ExcludeFromCodeCoverage]` attributes or removing the dead code entirely.

## 11. Value Objects - Structurally Blocked

These are enum-like value objects or abstract base classes. Their `ToExpression()`, implicit operators, and abstract methods are not directly testable via integration tests.

| Class | Type | Coverage | Notes |
|-------|------|----------|-------|
| `AuthProvider` | Value object (enum) | 0% | `ToExpression`/implicit operator exercised only if specification pattern uses it directly. Covered transitively by SocialLogin tests. |
| `Client` | Value object (enum) | 0% | Represents client type (Web/Mobile). Covered transitively by login/session tests. |
| `ExportFormat` | Value object (enum) | 0% | Covered by session export tests (Section 5.4). |
| `SessionStatus` | Value object (enum) | 0% | Covered by session query builder filter tests (Section 4.1). |
| `SessionExportBase` | Abstract base | 0% | Covered through `CsvExportStrategy` and `XlsxExportStrategy` concrete implementations. |
| `OtpPurpose` | Value object | 28.5% | EmailVerification covered; PasswordReset covered by Section 1.4 tests. |
| `Email` | Value object | 71.4% | Validation edge cases (empty, invalid format). |

**Recommendation:** Mark as not coverable to 100% for integration tests. The remaining uncovered lines are implicit operators and expression builders that are exercised at the EF Core provider level but not directly measurable by line coverage.

## 12. Entity Domain Methods

### 12.1 UserEntity (40.7% -> 100%)

| # | Method | Triggered By Test |
|---|--------|-------------------|
| 1 | `ChangePassword()` | Section 1.3 #1 (PublicChangePassword happy path) |
| 2 | `ResetPassword()` | Section 1.4 #1 (PublicResetPassword happy path) |
| 3 | `SetPasswordForExternalUser()` | Section 1.5 #1 (PublicSetPassword happy path) |
| 4 | `UpdateAvatar()` | Section 1.7 #1 (PublicUpdateAvatar happy path) |
| 5 | `AssignRole()` | Section 9 #5 (role assignment tests) |
| 6 | `RemoveRole()` | Section 9 #22 (remove role from user) |
| 7 | `MarkAsVerified()` | Already covered by existing VerifyOtp tests |

### 12.2 RoleEntity (77.7% -> 100%)

| # | Method/Guard | Triggered By Test |
|---|-------------|-------------------|
| 1 | `Activate()` already-active guard | Section 9 #8 |
| 2 | `Deactivate()` already-inactive guard | Section 9 #9 |
| 3 | `SoftDelete()` already-deleted guard | Section 9 #10 |
| 4 | `Restore()` not-deleted guard | Section 9 #11 |

### 12.3 PermissionEntity (89.5% -> 100%)

| # | Method/Guard | Triggered By Test |
|---|-------------|-------------------|
| 1 | `Activate()` already-active guard | Section 9 #1 |
| 2 | `Deactivate()` already-inactive guard | Section 9 #2 |
| 3 | `SoftDelete()` already-deleted guard | Section 9 #14 |
| 4 | `Restore()` not-deleted guard | Section 9 #15 |

### 12.4 OtpEntity (87.5% -> 100%)

| # | Branch | Triggered By Test |
|---|--------|-------------------|
| 1 | Expiration check | Section 1.4 #5 (expired OTP) |
| 2 | Max attempts check | Section 1.4 #6 (max attempts OTP) |

## 13. Summary

### Test Count by Section

| Section | Description | New Tests | Lines Covered |
|---------|-------------|-----------|---------------|
| 1.1 | AdminSignOut | 3 | ~30 |
| 1.2 | AdminRefreshToken | 2 | ~25 |
| 1.3 | PublicChangePassword | 6 | ~45 |
| 1.4 | PublicResetPassword | 8 | ~55 |
| 1.5 | PublicSetPassword | 4 | ~30 |
| 1.6 | PublicRevokeSession | 4 | ~30 |
| 1.7 | PublicUpdateAvatar | 5 | ~35 |
| 4.1 | SessionQueryBuilder filters | 5 | ~40 |
| 4.2 | PermissionQueryBuilder filters | 3 | ~20 |
| 4.3 | RoleQueryBuilder filters | 4 | ~25 |
| 5.2 | ValidationUtils | 2 | ~10 |
| 5.4 | Session export validators | 4 | ~20 |
| 7 | AccountStatusRequirementHandler | 2 | ~25 |
| 8.1 | WangkanaiClientOriginDetectionAdapter | 4 | ~30 |
| 9 | Error path tests | 31 | ~120 |
| **Total** | | **87** | **~540** |

### Coverage Projection

| Category | Current | After All Tests | Notes |
|----------|---------|----------------|-------|
| **Overall Lines** | 92.5% (7,167/7,741) | ~99.3% (7,687/7,741) | 540 of 574 uncovered lines now covered |
| **Handlers at 0%** | 0% (7 handlers) | 100% | All happy paths + error paths |
| **UserErrors** | 25.4% | 100% (excl. 2 dead methods) | All 35 live methods triggered |
| **SessionErrors** | 60% | 100% | All 3 methods triggered |
| **Specifications** | 0% (20 specs) | 100% | All 20 specs exercised transitively |
| **SessionQueryBuilder** | 38% | 100% | All 7 methods covered |
| **PermissionQueryBuilder** | 77.2% | 100% | All 4 methods covered |
| **RoleQueryBuilder** | 77.2% | 100% | All 4 methods covered |
| **Exception Handlers** | 0% (5 handlers) | 100% | All 5 triggered by tests |
| **AccountStatusHandler** | 21.8% | 100% | Both branches covered |
| **AuthRepository** | 52% | 100% | All 15 methods covered transitively |
| **Entity methods** | 40-89% | 100% | All guard clauses and methods exercised |

### Remaining Uncoverable Lines (~54 lines)

| Category | Approx Lines | Reason |
|----------|-------------|--------|
| Dead code (4 methods) | ~12 | `CoreRoleCannotBeModified` (3 files) + `RoleNotFoundByName` |
| Value object implicit operators | ~20 | `AuthProvider`, `Client`, `ExportFormat`, `SessionStatus` ToExpression/implicit |
| Abstract base class | ~8 | `SessionExportBase` abstract methods |
| SuperAdminConfiguration env check | ~6 | Startup-time configuration, not reachable via HTTP |
| ValidationUtils.GetPropertyValue | ~4 | Reflection fallback path |
| Mobile response DTOs | ~4 | `PublicSocialLoginMobileResponse`, `PublicRefreshTokenMobileResponse` - only returned when mobile client detected; covered if login tests send mobile User-Agent (Section 8.1 #1) |

**Final projected coverage: ~99.3% lines** (excluding dead code and structurally blocked value objects).
