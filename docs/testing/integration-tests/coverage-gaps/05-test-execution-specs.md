# Test Execution Specification

## Coverage Targets

| Metric | Current | Target | Gap |
| ------ | ------- | ------ | --- |
| Overall line coverage | 92.5% (19,347 / 20,915) | 99% (20,706 / 20,915) | +1,359 lines |
| Specifications | Partial | 100% | All 20 Identity specs at 0% |
| Validators | Partial | 100% | Cloudinary-blocked validators coverable via invalid payloads |
| Query builders | Partial | 100% | All 3 Identity query builders |
| Error messages | Partial | 100% | All uncovered error methods |

Maximum achievable via integration tests alone: **95.8%** (20,039 / 20,915). To reach 99%, unit tests for domain entities, value objects, startup extensions, and infrastructure fault injection are additionally required.

## Seed Data Pattern

Every integration test follows this pattern:

```csharp
[Collection("Database")]
public class MyEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Action_Condition_ReturnsExpected()
    {
        // 1. Seed
        await using var context = CreateDbContext<ModuleDbContext>();
        var entity = EntityFactory.CreateInState();
        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        // 2. Auth
        Client.AuthenticateAsSuperAdmin();

        // 3. Act
        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Resource}/{entity.Id}/action", null);

        // 4. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

**Key infrastructure:**
- `BaseApiTest` provides `Client`, `CreateDbContext<T>()`, auth helpers
- Test users: SuperAdmin (ID `...001`), Admin (`...002`), Visitor (`...003`)
- JWT auth helpers generate tokens directly (no DB sessions required for auth)
- `[Collection("Database")]` ensures serial execution with Testcontainers PostgreSQL
- Respawn resets DB between tests

## Execution Waves

Tests are grouped into 6 waves, ordered by coverage impact. Each wave targets a specific coverage domain and builds on the infrastructure established by earlier waves.

### Wave 1: Identity Handlers at 0% (~25 tests, ~200 lines)

**Priority:** Highest. These handlers have zero coverage and each test covers the full handler + validator + specification chain.

#### 1A. AdminSignOut (3 tests)

**File:** `tests/Integration/Modules/Identity/.../SignOut/V1/AdminSignOutEndpointV1Tests.cs`
**Existing tests:** NoAuth→401, SignOutAll NoAuth→401

```csharp
// Test 1: Happy path
// Seed: SessionFactory.Create() for SuperAdmin user
// Auth: Client.AuthenticateAsSuperAdmin()
// Act: POST /api/v1/admin/auth/sign-out { RefreshToken: "<valid>" }
// Assert: 200 OK
// Covers: AdminSignOutHandler, AdminSignOutValidator, TokenDeliveryService

// Test 2: Empty token → 400/422
// Auth: Client.AuthenticateAsSuperAdmin()
// Act: POST /api/v1/admin/auth/sign-out { RefreshToken: "" }
// Assert: 400/422
// Covers: AdminSignOutValidator.ValidRefreshToken()

// Test 3: Invalid token → 400/403
// Auth: Client.AuthenticateAsSuperAdmin()
// Act: POST /api/v1/admin/auth/sign-out { RefreshToken: "invalid-token-value" }
// Assert: 400/403
// Covers: RefreshTokenFactory validation path
```

#### 1B. AdminRefreshToken (2 tests)

**File:** `tests/Integration/Modules/Identity/.../RefreshToken/V1/AdminRefreshTokenEndpointV1Tests.cs`
**Existing tests:** NoToken→403, InvalidTokenInBody→403

```csharp
// Test 1: Empty token → 400/422
// Act: POST /api/v1/admin/sessions/refresh-token { RefreshToken: "" }
// Assert: 400/422
// Covers: AdminRefreshTokenValidator empty rule

// Test 2: Expired session → 401/403
// Seed: SessionFactory.CreateExpired() for SuperAdmin
// Act: POST /api/v1/admin/sessions/refresh-token { RefreshToken: "<expired>" }
// Assert: 401/403
// Covers: Session expiration check in handler
```

#### 1C. PublicChangePassword (5 tests)

**File:** `tests/Integration/Modules/Identity/.../ChangePassword/V1/PublicChangePasswordEndpointV1Tests.cs`
**Existing tests:** NoAuth→401, AsAdmin→403

```csharp
// Test 1: Happy path
// Seed: UserFactory.CreateVerifiedActive() with known bcrypt hash, Visitor role, active session
// Auth: Generate JWT for seeded user
// Act: PATCH /api/v1/public/auth/change-password { OldPassword: "Test123!abc", NewPassword: "NewPass456!xyz" }
// Assert: 200 OK
// Covers: PublicChangePasswordHandler full path, PublicChangePasswordValidator

// Test 2: Empty old password → 400/422 (validator)
// Test 3: Empty new password → 400/422 (validator)
// Test 4: Same password → 400 (UserErrors.NewPasswordMustBeDifferent)
// Test 5: Wrong old password → 400 (UserErrors.InvalidOldPassword)
```

#### 1D. PublicResetPassword (6 tests)

**File:** `tests/Integration/Modules/Identity/.../ResetPassword/V1/PublicResetPasswordEndpointV1Tests.cs`

```csharp
// Test 1: Happy path — seed user + OtpFactory.CreateValid(userId) (code="123456", purpose=PasswordReset)
// Act: POST /api/v1/public/auth/reset-password { Email, Code: "123456", NewPassword: "NewPass456!xyz" }
// Assert: 200 OK
// Covers: PublicResetPasswordHandler, PublicResetPasswordValidator, PublicResetPasswordAuthFactory

// Test 2: Empty email → 400/422 (validator)
// Test 3: Invalid OTP code format ("12") → 400/422 (ValidOtpCode must be 6 digits)
// Test 4: Non-existent user → 404 (UserErrors.UserNotFoundByEmail)
// Test 5: Expired OTP — seed OtpFactory.CreateExpired() → 410 (OtpExpirationExceptionHandler)
// Test 6: Max attempts OTP — seed OtpFactory.CreateMaxAttempts() → 429 (OtpAttemptsLimitExceptionHandler)
```

**Note:** Tests 5 and 6 cover the OTP exception handlers at 0%.

#### 1E. PublicSetPassword (3 tests)

**File:** `tests/Integration/Modules/Identity/.../SetPassword/V1/PublicSetPasswordEndpointV1Tests.cs`
**Existing tests:** NoAuth→401

```csharp
// Test 1: Happy path — seed UserFactory.CreateExternalAuth() (no password hash), Visitor role
// Act: POST /api/v1/public/auth/set-password { Password: "NewPass456!xyz" }
// Assert: 200 OK
// Covers: PublicSetPasswordHandler, PublicSetPasswordValidator

// Test 2: Empty password → 400/422 (validator)
// Test 3: User already has password → 400 (UserErrors.PasswordAlreadyConfigured)
```

#### 1F. PublicRevokeSession (4 tests)

**File:** `tests/Integration/Modules/Identity/.../RevokeSession/V1/PublicRevokeSessionEndpointV1Tests.cs`

```csharp
// Test 1: Happy path
// Seed: SessionFactory.Create() for Visitor user ID
// Auth: Client.AuthenticateAsVisitor()
// Act: POST /api/v1/public/me/sessions/revoke/{sessionId}
// Assert: 200 OK
// Covers: PublicRevokeSessionHandler, PublicRevokeSessionValidator, SessionByIdSpecification

// Test 2: Invalid GUID
// Auth: Client.AuthenticateAsVisitor()
// Act: POST /api/v1/public/me/sessions/revoke/not-a-guid
// Assert: 400/422
// Covers: PublicRevokeSessionValidator.IsValidGuid()

// Test 3: Non-existent session
// Auth: Client.AuthenticateAsVisitor()
// Act: POST /api/v1/public/me/sessions/revoke/{Guid.NewGuid()}
// Assert: 404
// Covers: SessionErrors.SessionNotFound()

// Test 4: Other user's session
// Seed: SessionFactory.Create() for Admin user
// Auth: Client.AuthenticateAsVisitor()
// Act: POST /api/v1/public/me/sessions/revoke/{adminSessionId}
// Assert: 400/403
// Covers: Handler ownership check, AccessDeniedExceptionHandler
```

#### 1G. PublicUpdateAvatar (3 tests)

**File:** `tests/Integration/Modules/Identity/.../UpdateAvatar/V1/PublicUpdateAvatarEndpointV1Tests.cs`

```csharp
// Test 1: Happy path
// Seed: UserFactory.CreateVerifiedActive() + Visitor role
// Auth: Generate JWT for seeded user
// Act: PATCH /api/v1/public/me/avatar (multipart form with image file)
// Assert: 200 OK
// Covers: PublicUpdateAvatarHandler, PublicUpdateAvatarValidator, PublicUpdateAvatarAuthFactory

// Test 2: No file attached → 400/422
// Auth: Client.AuthenticateAsVisitor()
// Act: PATCH /api/v1/public/me/avatar (empty multipart)
// Assert: 400/422
// Covers: Validator file-required rule

// Test 3: Invalid file type → 400/422
// Auth: Client.AuthenticateAsVisitor()
// Act: PATCH /api/v1/public/me/avatar (multipart with .txt file)
// Assert: 400/422
// Covers: Validator file-type rule
```

#### Wave 1 Transitive Coverage

Beyond the direct handler/validator coverage, Wave 1 tests transitively cover:

- OtpAttemptsLimitExceptionHandler
- OtpExpirationExceptionHandler
- AccessDeniedExceptionHandler
- AccountStatusRequirementHandler (with 2 additional tests for inactive/unverified in Wave 3)
- UserErrors methods (NewPasswordMustBeDifferent, InvalidOldPassword, UserNotFoundByEmail, PasswordAlreadyConfigured)
- SessionErrors methods (SessionNotFound)
- AuthRepository methods
- RefreshTokenFactory
- TokenDeliveryService

### Wave 2: Identity Query Builder Filters + Specs (~15 tests, ~80 lines)

**Priority:** High. Covers all 20 Identity specs at 0% transitively, all 3 query builders to 100%, and SessionValidation methods.

#### 2A. SessionQueryBuilder (5 filter tests)

Extend `AdminGetAllSessionsEndpointV1Tests.cs`:

```csharp
// Test 1: Filter by IP address
// Seed: SessionFactory.Create() with known IP
// Act: GET /api/v1/admin/sessions?ipAddress=192.168.1.1
// Assert: 200 OK, filtered results
// Covers: SessionByIpAddressSpecification, SessionQueryBuilder.WithIpAddress()

// Test 2: Filter by date range (fromDate + toDate)
// Seed: Sessions at different dates
// Act: GET /api/v1/admin/sessions?fromDate=2024-01-01&toDate=2024-12-31
// Assert: 200 OK, filtered results
// Covers: SessionCreatedAfterSpecification, SessionCreatedBeforeSpecification

// Test 3: Filter by status Active
// Seed: Active + revoked sessions
// Act: GET /api/v1/admin/sessions?status=Active
// Assert: 200 OK, only active
// Covers: SessionQueryBuilder.WithActiveStatus(), SessionIsActiveSpecification

// Test 4: Filter by status Revoked
// Seed: Active + revoked sessions
// Act: GET /api/v1/admin/sessions?status=Revoked
// Assert: 200 OK, only revoked
// Covers: SessionIsRevokedSpecification

// Test 5: Filter by isActive=false
// Seed: Active + inactive sessions
// Act: GET /api/v1/admin/sessions?isActive=false
// Assert: 200 OK, only inactive
// Covers: SessionQueryBuilder.WithInactiveStatus()
```

#### 2B. PermissionQueryBuilder (2 filter tests)

Extend `AdminGetAllPermissionsEndpointV1Tests.cs`:

```csharp
// Test 1: GET /api/v1/admin/permissions?isDeleted=true
// Seed: PermissionFactory.CreateDeleted()
// Covers: PermissionIsDeletedSpecification, PermissionQueryBuilder.WithDeletedFilter()

// Test 2: GET /api/v1/admin/permissions?isDeleted=false
// Seed: PermissionFactory.Create() (non-deleted)
// Covers: PermissionNotDeletedSpecification
```

#### 2C. RoleQueryBuilder (2 filter tests)

Extend `AdminGetAllRolesEndpointV1Tests.cs`:

```csharp
// Test 1: GET /api/v1/admin/roles?isDeleted=true
// Seed: RoleFactory.CreateDeleted()
// Covers: RoleIsDeletedSpecification, RoleQueryBuilder.WithDeletedFilter()

// Test 2: GET /api/v1/admin/roles?isDeleted=false
// Seed: RoleFactory.Create() (non-deleted)
// Covers: RoleNotDeletedSpecification
```

#### 2D. SessionValidation (3 export tests)

Extend session export tests:

```csharp
// Test 1: Export with invalid format → 400/422
// Act: GET /api/v1/admin/sessions/export?format=invalid
// Covers: SessionValidation.ValidExportFormat()

// Test 2: Export with invalid columns → 400/422
// Act: GET /api/v1/admin/sessions/export?columns=nonexistent
// Covers: SessionValidation.ValidExportColumns()

// Test 3: Export with invalid status filter → 400/422
// Act: GET /api/v1/admin/sessions/export?status=BadValue
// Covers: SessionValidation.ValidSessionStatus()
```

#### 2E. WangkanaiClientOriginDetectionAdapter (3 User-Agent tests)

Extend session creation or listing tests:

```csharp
// Test 1: Mobile User-Agent header
// Act: Login with User-Agent: "Mozilla/5.0 (iPhone; ...)"
// Assert: Session client = Mobile
// Covers: WangkanaiClientOriginDetectionAdapter mobile detection

// Test 2: Desktop User-Agent header
// Act: Login with User-Agent: "Mozilla/5.0 (Windows NT 10.0; ...)"
// Assert: Session client = Desktop
// Covers: WangkanaiClientOriginDetectionAdapter desktop detection

// Test 3: Unknown/empty User-Agent
// Act: Login without User-Agent header
// Assert: Session client = Unknown
// Covers: WangkanaiClientOriginDetectionAdapter fallback path
```

### Wave 3: Identity Error Paths (~20 tests, ~120 lines)

**Priority:** High. Covers state conflict errors, assignment errors, and account status checks for Identity module entities.

#### 3A. Role State Conflicts (4 tests)

| Test | Endpoint | Seed | Expected | Covers |
|------|----------|------|----------|--------|
| Activate already active role | `PATCH /admin/roles/{id}/activate` | Active role | 409 | ActiveRoleSpecification, RoleErrors.AlreadyActive() |
| Deactivate already inactive role | `PATCH /admin/roles/{id}/deactivate` | Inactive role | 409 | RoleIsNotActiveSpecification, RoleErrors.AlreadyInactive() |
| Soft-delete already deleted role | `DELETE /admin/roles/{id}` | Deleted role | 409 | RoleIsDeletedSpecification, RoleErrors.AlreadyDeleted() |
| Restore non-deleted role | `PATCH /admin/roles/{id}/restore` | Non-deleted role | 409 | RoleNotDeletedSpecification, RoleErrors.NotDeleted() |

#### 3B. Permission State Conflicts (4 tests)

| Test | Endpoint | Seed | Expected | Covers |
|------|----------|------|----------|--------|
| Activate already active permission | `PATCH /admin/permissions/{id}/activate` | Active perm | 409 | ActivePermissionSpecification, PermissionErrors.AlreadyActive() |
| Deactivate already inactive permission | `PATCH /admin/permissions/{id}/deactivate` | Inactive perm | 409 | PermissionIsNotActiveSpecification, PermissionErrors.AlreadyInactive() |
| Soft-delete already deleted permission | `DELETE /admin/permissions/{id}` | Deleted perm | 409 | PermissionIsDeletedSpecification, PermissionErrors.AlreadyDeleted() |
| Restore non-deleted permission | `PATCH /admin/permissions/{id}/restore` | Non-deleted perm | 409 | PermissionNotDeletedSpecification, PermissionErrors.NotDeleted() |

#### 3C. Role/Permission Assignment Errors (4 tests)

| Test | Endpoint | Seed | Expected | Covers |
|------|----------|------|----------|--------|
| Duplicate role-permission assignment | `POST /admin/roles/{id}/permissions` | Already assigned | 409 | RolePermissionErrors.AlreadyAssigned() |
| Assign inactive permission to role | `POST /admin/roles/{id}/permissions` | Inactive perm | 400 | PermissionErrors.IsInactive() |
| Assign deleted permission to role | `POST /admin/roles/{id}/permissions` | Deleted perm | 400 | PermissionErrors.IsDeleted() |
| Remove non-assigned permission | `DELETE /admin/roles/{id}/permissions/{permId}` | Not assigned | 404 | RolePermissionErrors.NotFound() |

#### 3D. Hard Delete + Unique Constraints (4 tests)

| Test | Endpoint | Seed | Expected | Covers |
|------|----------|------|----------|--------|
| Hard-delete core role | `DELETE /admin/roles/{id}/hard` | Core role (SuperAdmin/Admin/Visitor) | 400 | RoleErrors.CoreRoleCannotBeDeleted() |
| Duplicate phone number | `PATCH /public/me/profile` | Two users, same phone | 409 | UserErrors.PhoneNumberAlreadyExists() |
| Duplicate role name | `POST /admin/roles` | Existing role name | 409 | RoleErrors.DuplicateName() |
| Duplicate permission name | `POST /admin/permissions` | Existing resource+action | 409 | PermissionErrors.DuplicateName() |

#### 3E. Account Status Checks (2 tests)

```csharp
// Use any protected endpoint (e.g., GET /api/v1/public/me/profile)

// Test 1: Inactive user → 423 Locked
// Seed: UserFactory.CreateInactive() + Visitor role, generate JWT
// Covers: AccountInactiveExceptionHandler, AccountInactiveException, UserIsActiveSpecification

// Test 2: Unverified user → 403 Forbidden
// Seed: UserFactory.CreateUnverified() + Visitor role, generate JWT
// Covers: AccountNotVerifiedExceptionHandler, AccountNotVerifiedException, UserIsVerifiedSpecification
```

### Wave 4: Content Error Paths — Activate/Deactivate/State (~30 tests, ~150 lines)

**Priority:** Medium-high. Covers all activate/deactivate conflict errors and entity-specific validation errors across Content module entities.

#### 4A. Category Errors (9 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Activate already active | `PATCH /admin/categories/{id}/activate` | Active category | 409 | CategoryErrors.AlreadyActive() |
| Deactivate already inactive | `PATCH /admin/categories/{id}/deactivate` | Inactive category | 409 | CategoryErrors.AlreadyInactive() |
| Duplicate slug | `POST /admin/categories` | Existing slug | 409 | CategoryErrors.AlreadyExists() |
| Duplicate pricing | `POST /admin/categories/{id}/pricing` | Existing pricing | 409 | CategoryErrors.PricingAlreadyExists() |
| Make inactive category exclusive | `PATCH /admin/categories/{id}/exclusive` | Inactive category | 400 | CategoryErrors.CannotMakeInactiveExclusive() |
| Make non-video exclusive | `PATCH /admin/categories/{id}/exclusive` | Non-video category | 400 | CategoryErrors.OnlyVideoCategoryCanBeExclusive() |
| No exclusive category found | Endpoint that requires exclusive category | No exclusive set | 404 | CategoryErrors.NoExclusiveCategoryFound() |
| Pricing not found | `DELETE /admin/categories/{id}/pricing/{pId}` | Non-existent pricing | 404 | CategoryErrors.PricingNotFound() |
| Negative price | `POST /admin/categories/{id}/pricing` | Price: -1 | 400 | CategoryErrors.PriceMustBeNonNegative() |

#### 4B. ContentType Errors (3 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Activate already active | `PATCH /admin/content-types/{id}/activate` | Active type | 409 | ContentTypeErrors.AlreadyActive() |
| Deactivate already inactive | `PATCH /admin/content-types/{id}/deactivate` | Inactive type | 409 | ContentTypeErrors.AlreadyInactive() |
| Duplicate name | `POST /admin/content-types` | Existing name | 409 | ContentTypeErrors.AlreadyExists() |

#### 4C. PricingTier Errors (4 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Activate already active | `PATCH /admin/pricing-tiers/{id}/activate` | Active tier | 409 | PricingTierErrors.AlreadyActive() |
| Deactivate already inactive | `PATCH /admin/pricing-tiers/{id}/deactivate` | Inactive tier | 409 | PricingTierErrors.AlreadyInactive() |
| Duplicate name | `POST /admin/pricing-tiers` | Existing name | 409 | PricingTierErrors.AlreadyExists() |
| Use inactive tier | Endpoint referencing tier | Inactive tier | 400 | PricingTierErrors.IsInactive() |

#### 4D. PromotionLevel Errors (6 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Activate already active | `PATCH /admin/promotion-levels/{id}/activate` | Active level | 409 | PromotionLevelErrors.AlreadyActive() |
| Deactivate already inactive | `PATCH /admin/promotion-levels/{id}/deactivate` | Inactive level | 409 | PromotionLevelErrors.AlreadyInactive() |
| Duplicate name | `POST /admin/promotion-levels` | Existing name | 409 | PromotionLevelErrors.AlreadyExists() |
| Non-positive duration | `POST /admin/promotion-levels` | Duration: 0 | 400 | PromotionLevelErrors.DurationMustBePositive() |
| Negative price | `POST /admin/promotion-levels` | Price: -1 | 400 | PromotionLevelErrors.PriceMustBeNonNegative() |
| Invalid spot priority | `POST /admin/promotion-levels` | Invalid priority | 400 | PromotionLevelErrors.InvalidSpotPriority() |

#### 4E. Package Errors (5 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Activate already active | `PATCH /admin/packages/{id}/activate` | Active package | 409 | PackageErrors.AlreadyActive() |
| Deactivate already inactive | `PATCH /admin/packages/{id}/deactivate` | Inactive package | 409 | PackageErrors.AlreadyInactive() |
| Empty name | `POST /admin/packages` | Name: "" | 400 | PackageErrors.NameRequired() |
| Non-positive slot quantity | `POST /admin/packages` | SlotQuantity: 0 | 400 | PackageErrors.SlotQuantityMustBePositive() |
| Slot not found | `DELETE /admin/packages/{id}/slots/{sId}` | Non-existent slot | 404 | PackageErrors.SlotNotFound() |

#### 4F. ShortVideo + Tag Errors (3 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Activate already active short | `PATCH /admin/shorts/{id}/activate` | Active short | 409 | ShortVideoErrors.AlreadyActive() |
| Deactivate already inactive short | `PATCH /admin/shorts/{id}/deactivate` | Inactive short | 409 | ShortVideoErrors.AlreadyInactive() |
| Duplicate tag slug | `POST /admin/tags` | Existing slug | 409 | TagErrors.SlugAlreadyExists() |

### Wave 5: Content Error Paths — Editorial + Commerce + Interactions (~35 tests, ~170 lines)

**Priority:** Medium. Covers state machine transitions for articles and videos, commerce order lifecycle errors, and interaction conflict errors.

#### 5A. Article State Machine Errors (10 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Submit already submitted | `PATCH /admin/articles/{id}/submit` | Submitted article | 409 | ArticleErrors.AlreadySubmitted() |
| Review already pending | `PATCH /admin/articles/{id}/review` | Pending review article | 409 | ArticleErrors.AlreadyPendingReview() |
| Approve already approved | `PATCH /admin/articles/{id}/approve` | Approved article | 409 | ArticleErrors.AlreadyApproved() |
| Publish already published | `PATCH /admin/articles/{id}/publish` | Published article | 409 | ArticleErrors.AlreadyPublished() |
| Reject already rejected | `PATCH /admin/articles/{id}/reject` | Rejected article | 409 | ArticleErrors.AlreadyRejected() |
| Archive already archived | `PATCH /admin/articles/{id}/archive` | Archived article | 409 | ArticleErrors.AlreadyArchived() |
| Invalid status transition | `PATCH /admin/articles/{id}/publish` | Draft article (skip steps) | 400 | ArticleErrors.InvalidStatusTransition() |
| Delete published article | `DELETE /admin/articles/{id}` | Published article | 400 | ArticleErrors.CannotDeletePublishedArticle() |
| Duplicate slug | `POST /admin/articles` | Existing slug | 409 | ArticleErrors.SlugAlreadyExists() |
| Not found | `GET /admin/articles/{id}` | Non-existent ID | 404 | ArticleErrors.NotFound() |

#### 5B. Video State Machine Errors (12 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Submit already submitted | `PATCH /admin/videos/{id}/submit` | Submitted video | 409 | VideoErrors.AlreadySubmitted() |
| Review already pending | `PATCH /admin/videos/{id}/review` | Pending review video | 409 | VideoErrors.AlreadyPendingReview() |
| Approve already approved | `PATCH /admin/videos/{id}/approve` | Approved video | 409 | VideoErrors.AlreadyApproved() |
| Publish already published | `PATCH /admin/videos/{id}/publish` | Published video | 409 | VideoErrors.AlreadyPublished() |
| Reject already rejected | `PATCH /admin/videos/{id}/reject` | Rejected video | 409 | VideoErrors.AlreadyRejected() |
| Archive already archived | `PATCH /admin/videos/{id}/archive` | Archived video | 409 | VideoErrors.AlreadyArchived() |
| Invalid status transition | `PATCH /admin/videos/{id}/publish` | Draft video (skip steps) | 400 | VideoErrors.InvalidStatusTransition() |
| Delete published video | `DELETE /admin/videos/{id}` | Published video | 400 | VideoErrors.CannotDeletePublishedVideo() |
| Publish without YouTube URL | `PATCH /admin/videos/{id}/publish` | Approved video, no URL | 400 | VideoErrors.CannotPublishWithoutYoutubeUrl() |
| Attach URL before shoot | `PUT /admin/videos/{id}/youtube-url` | Draft video | 400 | VideoErrors.CannotAttachYoutubeUrlBeforeShoot() |
| Duplicate slug | `POST /admin/videos` | Existing slug | 409 | VideoErrors.SlugAlreadyExists() |
| Not found | `GET /admin/videos/{id}` | Non-existent ID | 404 | VideoErrors.NotFound() |

#### 5C. ContentOrder Errors (10 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Submit already submitted | `PATCH /admin/orders/{id}/submit` | Submitted order | 409 | ContentOrderErrors.AlreadySubmitted() |
| Pay already paid | `PATCH /admin/orders/{id}/pay` | Paid order | 409 | ContentOrderErrors.AlreadyPaid() |
| Cancel already cancelled | `PATCH /admin/orders/{id}/cancel` | Cancelled order | 409 | ContentOrderErrors.AlreadyCancelled() |
| Cancel paid order | `PATCH /admin/orders/{id}/cancel` | Paid order | 400 | ContentOrderErrors.CannotCancelPaidOrder() |
| Add item to non-draft | `POST /admin/orders/{id}/items` | Submitted order | 400 | ContentOrderErrors.CannotAddItemToNonDraftOrder() |
| Missing tier on item | `POST /admin/orders/{id}/submit` | Order with item, no tier | 400 | ContentOrderErrors.MustHaveAtLeastOneItemWithTier() |
| Payment already verified | `PATCH /admin/payments/{id}/verify` | Verified payment | 409 | ContentOrderErrors.PaymentAlreadyVerified() |
| Payment already rejected | `PATCH /admin/payments/{id}/reject` | Rejected payment | 409 | ContentOrderErrors.PaymentAlreadyRejected() |
| Item not found | `DELETE /admin/orders/{id}/items/{iId}` | Non-existent item | 404 | ContentOrderErrors.ItemNotFound() |
| Tier already attached | `POST /admin/orders/{id}/items/{iId}/tiers` | Already attached | 409 | ContentOrderErrors.TierAlreadyAttached() |

#### 5D. Interaction Errors (6 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Like already liked article | `POST /public/articles/{id}/likes` | Already liked | 409 | ArticleInteractionErrors.AlreadyLiked() |
| Unlike not-liked article | `DELETE /public/articles/{id}/likes` | Not liked | 400 | ArticleInteractionErrors.LikeNotFound() |
| Bookmark already bookmarked | `POST /public/articles/{id}/bookmarks` | Already bookmarked | 409 | ArticleInteractionErrors.AlreadyBookmarked() |
| Unbookmark not-bookmarked | `DELETE /public/articles/{id}/bookmarks` | Not bookmarked | 400 | ArticleInteractionErrors.BookmarkNotFound() |
| Edit comment not owner | `PATCH /public/articles/{id}/comments/{cId}` | Other user's comment | 400 | ArticleInteractionErrors.NotCommentOwner() |
| Comment not found | `DELETE /public/articles/{id}/comments/{cId}` | Non-existent comment | 404 | ArticleInteractionErrors.CommentNotFound() |

#### 5E. ShortVideo Interaction Errors (4 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Like already liked short | `POST /public/shorts/{id}/likes` | Already liked | 409 | ShortVideoInteractionErrors.AlreadyLiked() |
| Unlike not-liked short | `DELETE /public/shorts/{id}/likes` | Not liked | 400 | ShortVideoInteractionErrors.LikeNotFound() |
| Bookmark already bookmarked short | `POST /public/shorts/{id}/bookmarks` | Already bookmarked | 409 | ShortVideoInteractionErrors.AlreadyBookmarked() |
| Unbookmark not-bookmarked short | `DELETE /public/shorts/{id}/bookmarks` | Not bookmarked | 400 | ShortVideoInteractionErrors.BookmarkNotFound() |

#### 5F. Playlist + Lyrics + Customer Errors (5 tests)

| Test | Endpoint | Seed | Expected | Error Method |
|------|----------|------|----------|-------------|
| Playlist not found | `GET /public/playlists/{id}` | Non-existent ID | 404 | PlaylistErrors.NotFound() |
| Playlist not owner | `PATCH /public/playlists/{id}` | Other user's playlist | 400 | PlaylistErrors.NotOwner() |
| Video already in playlist | `POST /public/playlists/{id}/videos` | Already added | 409 | PlaylistErrors.VideoAlreadyInPlaylist() |
| Lyrics already exist | `POST /admin/lyrics` | Existing for video | 409 | LyricsErrors.AlreadyExists() |
| Lyrics not found | `GET /admin/lyrics/{id}` | Non-existent ID | 404 | LyricsErrors.NotFound() |

### Wave 6: Content Validators + Specs + Repos (~25 tests, ~100 lines)

**Priority:** Medium. Covers remaining validator branches (especially Cloudinary-blocked validators coverable via invalid payloads), Content specifications at 0%, and repository search paths.

#### 6A. Cloudinary-Blocked Validators (9 tests)

These validators can be partially covered by sending invalid payloads that trigger validation errors before reaching the Cloudinary upload.

**CreateShortVideoValidator (5 tests):**

```csharp
// Test 1: Missing title → 400/422
// Test 2: Missing category ID → 400/422
// Test 3: Invalid category ID (non-existent) → 400/422
// Test 4: Title too long → 400/422
// Test 5: Missing video file → 400/422
```

**UpdateShortVideoValidator (2 tests):**

```csharp
// Test 1: Empty title → 400/422
// Test 2: Invalid category ID → 400/422
```

**UploadArticleImageValidator (2 tests):**

```csharp
// Test 1: No file attached → 400/422
// Test 2: Invalid file type → 400/422
```

#### 6B. Shared Validator Branches (16 tests)

**CategoryValidation isRequired=false (2 tests):**

```csharp
// Test 1: Optional category ID null → passes validation
// Test 2: Optional category ID invalid → 400/422
```

**ContentTypeValidation isRequired=false (1 test):**

```csharp
// Test 1: Optional content type ID null → passes validation
```

**EditorialValidation branches (8 tests):**

```csharp
// Test 1: Title empty → 400/422
// Test 2: Title too long → 400/422
// Test 3: Description too long → 400/422
// Test 4: Slug empty → 400/422
// Test 5: Slug invalid format → 400/422
// Test 6: SpotPriority out of range → 400/422
// Test 7: PromotionLevel invalid → 400/422
// Test 8: Category inactive → 400/422
```

**PricingTierValidation (2 tests):**

```csharp
// Test 1: Name empty → 400/422
// Test 2: Price negative → 400/422
```

**PromotionLevelValidation (2 tests):**

```csharp
// Test 1: Name empty → 400/422
// Test 2: Duration zero → 400/422
```

**TagValidation isRequired=false + ValidTagNameItem (3 tests):**

```csharp
// Test 1: Optional tag null → passes validation
// Test 2: Tag name empty → 400/422
// Test 3: Tag name with invalid characters → 400/422
```

#### 6C. Content Specifications at 0% (4 tests)

| Test | Endpoint | Seed | Covers |
| ---- | -------- | ---- | ------ |
| ArticleTag by article ID | `GET /admin/articles/{id}` (with tags) | Article with tags | ArticleTagByArticleIdSpecification |
| Gossip article query | `GET /public/articles?type=gossip` | Gossip category + published article | GossipArticleSpecification |
| ShortVideo bookmark lookup | `POST /public/shorts/{id}/bookmarks` then unbookmark | Active short video | ShortVideoBookmarkByUserAndShortVideoSpecification |
| Video by order item ID | Commerce endpoint resolving video by order item | Order with video item | VideoByOrderItemIdSpecification |

#### 6D. Repository Search Paths (5 tests)

| Test | Endpoint | Query Params | Covers |
|------|----------|-------------|--------|
| Articles with search | `GET /admin/articles?search=keyword` | search | ArticleRepository search path |
| Videos with search | `GET /admin/videos?search=keyword` | search | VideoRepository search path |
| Shorts with search | `GET /admin/shorts?search=keyword` | search | ShortVideoRepository search path |
| Lyrics with search | `GET /admin/lyrics?search=keyword` | search | LyricsRepository search path |
| Playlists with search | `GET /public/me/playlists` | none | PlaylistRepository user filter |

## Factory Methods Required

Comprehensive table of all factory methods needed across all 6 waves:

| Factory | Method | Module |
|---------|--------|--------|
| SessionFactory | Create() | Identity |
| SessionFactory | CreateExpired() | Identity |
| OtpFactory | CreateValid(userId) | Identity |
| OtpFactory | CreateExpired() | Identity |
| OtpFactory | CreateMaxAttempts() | Identity |
| UserFactory | CreateVerifiedActive() | Identity |
| UserFactory | CreateUnverified() | Identity |
| UserFactory | CreateInactive() | Identity |
| UserFactory | CreateExternalAuth() | Identity |
| RoleFactory | Create() | Identity |
| RoleFactory | CreateInactive() | Identity |
| RoleFactory | CreateDeleted() | Identity |
| PermissionFactory | Create() | Identity |
| PermissionFactory | CreateInactive() | Identity |
| PermissionFactory | CreateDeleted() | Identity |
| ContentTypeFactory | Create() | Content |
| CategoryFactory | Create(contentTypeId) | Content |
| CategoryFactory | CreateFree(contentTypeId) | Content |
| CategoryFactory | CreateInactive(contentTypeId) | Content |
| TagFactory | Create(name, slug) | Content |
| ArticleFactory | Create(categoryId) | Content |
| ArticleFactory | CreateSubmitted() | Content |
| ArticleFactory | CreateApproved() | Content |
| ArticleFactory | CreatePublished() | Content |
| ArticleFactory | CreateRejected() | Content |
| ArticleFactory | CreateArchived() | Content |
| VideoFactory | Create(categoryId) | Content |
| VideoFactory | CreateSubmitted() | Content |
| VideoFactory | CreateApproved() | Content |
| VideoFactory | CreatePublished() | Content |
| VideoFactory | CreateRejected() | Content |
| VideoFactory | CreateArchived() | Content |
| ShortVideoFactory | CreateActive(categoryId) | Content |
| ShortVideoFactory | CreateInactive(categoryId) | Content |
| LyricsFactory | CreateForVideo(videoId) | Content |
| PricingTierFactory | Create() | Content |
| PricingTierFactory | CreateInactive() | Content |
| PromotionLevelFactory | Create() | Content |
| PromotionLevelFactory | CreateInactive() | Content |
| PackageFactory | Create() | Content |
| PackageFactory | CreateInactive() | Content |
| CustomerFactory | Create(email) | Content |
| ContentOrderFactory | Create(customerId) | Content |
| ContentOrderFactory | CreateSubmitted() | Content |
| ContentOrderFactory | CreatePaid() | Content |
| ContentOrderFactory | CreateCancelled() | Content |
| ContentPaymentFactory | Create(orderId) | Content |
| ContentPaymentFactory | CreateVerified() | Content |
| ContentPaymentFactory | CreateRejected() | Content |
| PlaylistFactory | Create(userId) | Content |

## Expected Coverage Impact

| Wave | Focus | Tests | Lines | Cumulative |
| ---- | ----- | ----- | ----- | ---------- |
| Current | — | — | 19,347 | 92.5% |
| Wave 1 | Identity 0% handlers | 25 | +200 | 93.5% |
| Wave 2 | Identity queries+specs | 15 | +80 | 93.9% |
| Wave 3 | Identity error paths | 20 | +120 | 94.4% |
| Wave 4 | Content state errors | 30 | +150 | 95.1% |
| Wave 5 | Content editorial+commerce | 35 | +170 | 95.9% |
| Wave 6 | Content validators+specs | 25 | +100 | 96.4% |
| **Integration total** | | **~150** | **+820** | **~96.4%** |

To reach 99% (20,706 lines), an additional ~539 lines must be covered beyond integration tests. This requires supplementary test types:

| Supplementary Test Type | Target | Est. Lines |
| ----------------------- | ------ | ---------- |
| Unit tests: entity domain methods | Protected setters, state guards, factory methods | +150 |
| Unit tests: value objects | AuthProvider, Client, ExportFormat, SessionStatus | +40 |
| Startup integration tests | Service extensions, module registration, DI wiring | +150 |
| Infrastructure fault injection | BadGateway, DefaultException, FormatException, InternalServer, RateLimit strategies | +40 |
| Unit tests: abstract/base classes | SessionExportBase, Aggregate internals | +30 |
| Unit tests: decorators/interceptors | LoggingDecorator >3s branch, sync interceptor paths | +25 |
| Unit tests: specifications | OrSpecification, Specification.Or/IsSatisfiedBy/AndAll/OrAll | +30 |
| Unit tests: dispatcher | Dispatcher.Send void overload | +10 |
| **Supplementary total** | | **+475** |
| **Combined total** | | **+1,295** |
| **Projected coverage** | | **~98.7%** |

The remaining ~64 lines (0.3%) are structurally impossible to cover (see Structurally Blocked section).

## Dead Code (excluded from coverage target)

These code paths have zero callers in the production codebase and cannot be reached by any test:

| Code | Reason |
| ---- | ------ |
| TagByNameSpecification | Zero callers |
| ContentTypeErrors.NotFound | No handler throws it |
| TagErrors.NotFound | No handler throws it |
| EditorialValidation.ValidArticleId | Zero callers |
| EditorialValidation.ValidVideoId | Zero callers |
| EditorialValidation.ValidLyricsId | Zero callers |
| UserErrors.CoreRoleCannotBeModified | No handler throws it |
| Exception 2-arg constructors (Auth/Authz/BadRequest/Conflict) | Never used |
| Exception Details property (Auth/Authz/BadRequest/Conflict) | Never used |
| OrSpecification | Never called |
| Specification.Or | Never called |
| Specification.IsSatisfiedBy | Never called |
| Specification.AndAll | Never called |
| Specification.OrAll | Never called |
| Dispatcher.Send (void overload) | Never called |
| Aggregate.AddDomainEvent | No domain events raised |
| ValidationErrorMessage.StorageUrlCannotBeEmpty (Core) | Zero callers |

Total dead code: ~80 lines (0.4% of 20,915)

## Structurally Blocked (cannot cover via HTTP integration tests)

| Code | Lines | Reason |
| ---- | ----- | ------ |
| CloudinaryService + FileService | ~120 | Stubbed external HTTP |
| Cloudinary-blocked handlers (NOT validators) | ~21 | Need real upload |
| YoutubeThumbnailService | ~30 | Stubbed |
| AbandonedDraftCleanupJob | ~20 | Background cron job |
| Startup extensions | ~150 | App boot config |
| Rate limit builders | ~60 | Startup config |
| Exception handler strategies (BadGateway, Default, Format, InternalServer, RateLimit) | ~40 | Infrastructure errors |
| Sync interceptor methods | ~20 | ASP.NET uses async |
| HttpCurrentActor null context | ~5 | Background job path |
| SuperAdminConfiguration throw | ~5 | Missing env var |
| LoggingDecorator >3s branch | ~5 | Need slow handler |
| Value objects (AuthProvider, Client, ExportFormat, SessionStatus) | ~40 | Enum-like |
| SessionExportBase | ~15 | Abstract class |
| Aggregate internals | ~15 | No domain events |
| Domain entity protected methods | ~150 | Internal state |
| Remaining infrastructure | ~100 | DI, decorators |

Total structurally blocked: ~796 lines (3.8% of 20,915)

Combined uncoverable (dead + blocked): ~876 lines (4.2% of 20,915)

Maximum achievable via integration tests alone: 20,915 - 876 = 20,039 / 20,915 = **95.8%**

To reach 99%, supplementary unit tests and startup integration tests (as detailed in the Expected Coverage Impact table) must cover ~475 of the 796 structurally blocked lines through direct invocation rather than HTTP endpoints.

## Running Coverage

```bash
cd apps/backend
dotnet test tests/Integration/_116.Integration.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage/ \
  /p:ExcludeByFile="**/Migrations/**"

reportgenerator \
  -reports:coverage/**/coverage.opencover.xml \
  -targetdir:coverage/report \
  -reporttypes:"TextSummary;Html;MarkdownSummary" \
  -assemblyfilters:-*Tests*\;-*Migrations*

cat coverage/report/Summary.txt
```
