# Coverage TODO Tracker

**Goal:** 100% integration test coverage on all specifications, errors/error messages, validators, query builders, handlers (via endpoint tests), and repositories.

**Baseline Report:** 2026-06-21 13:26:09 — 92.5% (19,347 / 20,915)
**Latest Report:** 2026-06-21 18:48:45 — 94.4% (19,761 / 20,919)

After fixing each file, run coverage and update the "After" column. Mark the checkbox when the class reaches 100%.

## Identity Module — Specifications

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `ActivePermissionSpecification` | 0% | 0% | GET /admin/permissions?isActive=true — seed active + inactive permissions, assert filtered |
| [ ] | `ActiveRoleSpecification` | 0% | 0% | GET /admin/roles?isActive=true — seed active + inactive roles, assert filtered |
| [x] | `PermissionIsDeletedSpecification` | 0% | 100% | GET /admin/permissions?isDeleted=true — seed soft-deleted permission, assert returned |
| [x] | `PermissionNotDeletedSpecification` | 0% | 100% | GET /admin/permissions (default) — assert only non-deleted returned |
| [x] | `RoleIsDeletedSpecification` | 0% | 100% | GET /admin/roles?isDeleted=true — seed soft-deleted role, assert returned |
| [x] | `RoleIsNotActiveSpecification` | 0% | 100% | GET /admin/roles?isActive=false — seed inactive role, assert returned |
| [x] | `RoleNotDeletedSpecification` | 0% | 100% | GET /admin/roles (default) — assert only non-deleted returned |
| [ ] | `RolePermissionByIdSpecification` | 0% | 0% | DELETE /admin/roles/{id}/permissions/{permId} — remove permission from role |
| [x] | `SessionByIpAddressSpecification` | 0% | 100% | GET /admin/sessions?ipAddress=127.0.0.1 — seed sessions with different IPs |
| [x] | `SessionCreatedAfterSpecification` | 0% | 100% | GET /admin/sessions?fromDate=2026-01-01 — seed sessions before/after date |
| [x] | `SessionCreatedBeforeSpecification` | 0% | 100% | GET /admin/sessions?toDate=2026-12-31 — seed sessions before/after date |
| [x] | `SessionIsRevokedSpecification` | 0% | 100% | GET /admin/sessions?status=Revoked — seed revoked session |
| [ ] | `UserByPhoneNumberSpecification` | 0% | 0% | POST /public/auth/sign-up with phone number, or PATCH /public/me/profile with phone |
| [x] | `UserHasAdminRoleSpecification` | 0% | 100% | Covered transitively by admin login tests (admin JWT auth) |
| [ ] | `UserHasRoleSpecification` | 0% | 0% | Covered transitively by role assignment tests |
| [ ] | `UserHasVisitorRoleSpecification` | 0% | 0% | Covered transitively by public sign-up/login tests |
| [ ] | `UserIsActiveAdminSpecification` | 0% | 0% | Covered transitively by admin endpoint tests with active admin user |
| [ ] | `UserIsActiveAndVerifiedSpecification` | 0% | 0% | Test with unverified user hitting protected endpoint → 403 |
| [ ] | `UserIsActiveSpecification` | 0% | 0% | Test with inactive user hitting protected endpoint → 423 |
| [ ] | `UserIsVerifiedSpecification` | 0% | 0% | Covered transitively by verified-user guard in auth handlers |

## Identity Module — Errors and Error Messages

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `UserErrors` | 25.4% | 61% | 35 methods. Each test below covers 1+ methods: |
| [ ] | — `PhoneNumberAlreadyExists` | | | PATCH /public/me/profile with duplicate phone → 409 |
| [ ] | — `RoleAlreadyExists` | | | POST /admin/roles with existing name → 409 |
| [ ] | — `RoleAlreadyAssignedToUser` | | | POST /admin/users/{id}/roles with already-assigned role → 409 |
| [ ] | — `PermissionAlreadyExists` | | | POST /admin/permissions with existing name → 409 |
| [ ] | — `PermissionAlreadyAssignedToRole` | | | POST /admin/roles/{id}/permissions with already-assigned perm → 409 |
| [ ] | — `RoleAlreadyActive` | | | PATCH /admin/roles/{id}/activate on active role → 409 |
| [ ] | — `RoleAlreadyInactive` | | | PATCH /admin/roles/{id}/deactivate on inactive role → 409 |
| [ ] | — `RoleAlreadyDeleted` | | | DELETE /admin/roles/{id} on deleted role → 409 |
| [ ] | — `RoleNotDeleted` | | | PATCH /admin/roles/{id}/restore on non-deleted role → 409 |
| [ ] | — `PermissionAlreadyActive` | | | PATCH /admin/permissions/{id}/activate on active perm → 409 |
| [ ] | — `PermissionAlreadyInactive` | | | PATCH /admin/permissions/{id}/deactivate on inactive perm → 409 |
| [ ] | — `PermissionAlreadyDeleted` | | | DELETE /admin/permissions/{id} on deleted perm → 409 |
| [ ] | — `PermissionNotDeleted` | | | PATCH /admin/permissions/{id}/restore on non-deleted perm → 409 |
| [ ] | — `CoreRoleCannotBeDeleted` | | | DELETE /admin/roles/{coreRoleId}/hard-delete → 400 |
| [ ] | — `RoleIsInactive` | | | POST /admin/users/{id}/roles with inactive role → 400 |
| [ ] | — `RoleIsDeleted` | | | POST /admin/users/{id}/roles with deleted role → 400 |
| [ ] | — `PermissionIsInactive` | | | POST /admin/roles/{id}/permissions with inactive perm → 400 |
| [ ] | — `PermissionIsDeleted` | | | POST /admin/roles/{id}/permissions with deleted perm → 400 |
| [ ] | — `PermissionNotAssignedToRole` | | | DELETE /admin/roles/{id}/permissions/{notAssignedId} → 400 |
| [ ] | — `RoleNotAssignedToUser` | | | DELETE /admin/users/{id}/roles/{notAssignedId} → 400 |
| [ ] | — `AccountInactive` | | | Any protected endpoint with inactive user → 423 |
| [ ] | — `AccountNotVerified` | | | Any protected endpoint with unverified user → 403 |
| [ ] | — `InvalidCredentials` | | | POST /admin/auth/login with wrong password → 401 |
| [ ] | — `AccountAlreadyVerified` | | | POST /public/auth/verify-otp for already-verified user → 409 |
| [ ] | — `InvalidOtpCode` | | | POST /public/auth/reset-password with wrong 6-digit code → 400 |
| [ ] | — `OtpExpired` | | | POST /public/auth/reset-password with expired OTP → 410 |
| [ ] | — `MaxOtpAttemptsReached` | | | POST /public/auth/reset-password after 5+ failed attempts → 429 |
| [ ] | — `NewPasswordSameAsOld` | | | PATCH /public/auth/change-password old == new → 409 |
| [ ] | — `IncorrectCurrentPassword` | | | PATCH /public/auth/change-password wrong old → 400 |
| [ ] | — `PasswordNotConfigured` | | | PATCH /public/auth/change-password for OAuth user → 400 |
| [ ] | — `PasswordOnlyForExternalAuth` | | | POST /public/auth/set-password for local user → 400 |
| [ ] | — `InvalidUserAuthentication` | | | Covered transitively by tampered JWT → 401 |
| [ ] | — `InsufficientPermissions` | | | Covered transitively by non-admin hitting admin-only check → 403 |
| [ ] | `SessionErrors` | 60% | 60% | 3 methods total: |
| [ ] | — `InvalidRefreshToken` | | | POST /admin/auth/sign-out with invalid token → 403 |
| [ ] | — `SessionNotFound` | | | POST /public/me/sessions/revoke/{random-guid} → 404 |
| [ ] | — `DeviceIdRequired` | | | POST /public/auth/login without deviceId header → 400 |
| [ ] | `AuthenticationErrorMessage` | 37.5% | 50% | Covered transitively by UserErrors/SessionErrors tests above |
| [ ] | `AuthorizationErrorMessage` | 80% | 80% | Covered transitively by AccessDeniedException tests |
| [ ] | `ConflictErrorMessage` (Identity) | 29.4% | 82.3% | Covered transitively by all 409 Conflict tests above |
| [ ] | `ValidationErrorMessage` (Identity) | 71.4% | 87.1% | Covered transitively by validator tests + export tests |

## Identity Module — Validators

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [x] | `AdminSignOutValidator` | 0% | 100% | POST /admin/auth/sign-out with empty RefreshToken → 400 |
| [x] | `AdminRefreshTokenValidator` | 0% | 100% | POST /admin/sessions/refresh-token with empty token → 400 |
| [x] | `PublicChangePasswordValidator` | 0% | 100% | PATCH /public/auth/change-password with empty old/new → 400 |
| [x] | `PublicResetPasswordValidator` | 0% | 100% | POST /public/auth/reset-password with empty email/code → 400 |
| [x] | `PublicSetPasswordValidator` | 0% | 100% | POST /public/auth/set-password with empty password → 400 |
| [x] | `PublicRevokeSessionValidator` | 0% | 100% | POST /public/me/sessions/revoke/not-a-guid → 400 |
| [x] | `PublicUpdateAvatarValidator` | 0% | 100% | PATCH /public/auth/update-avatar with no file → 400 |
| [ ] | `FileValidation` | 70.4% | 70.4% | PATCH /public/auth/update-avatar with oversized file → 400 (isRequired=true branch) |
| [ ] | `ValidationUtils` | 83.3% | 83.3% | Test with non-HTTP URL (ftp://x) → 400 (ValidUrl uncovered) |
| [ ] | `ValidationExtension` (Shared) | 85% | 85% | Covered transitively by GUID validation tests (isRequired=true 5-param overload) |

## Identity Module — Query Builders

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `SessionQueryBuilder` | 38% | 86.9% | 5 tests: GET /admin/sessions with ?ipAddress, ?fromDate+toDate, ?status=Active, ?status=Revoked, ?isActive=false |
| [x] | `PermissionQueryBuilder` | 77.2% | 100% | 2 tests: GET /admin/permissions with ?isDeleted=true, ?isDeleted=false |
| [x] | `RoleQueryBuilder` | 77.2% | 100% | 2 tests: GET /admin/roles with ?isDeleted=true, ?isDeleted=false |

## Identity Module — Handlers (at 0%, via endpoint tests)

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [x] | `AdminSignOutHandler` | 0% | 100% | POST /admin/auth/sign-out — seed session with known refresh token, send valid token → 200 |
| [x] | `AdminSignOutSessionFactory` | 0% | 100% | Covered transitively by AdminSignOut happy path |
| [x] | `AdminRefreshTokenHandler` | 0% | 100% | POST /admin/sessions/refresh-token with expired session → 403 |
| [x] | `PublicChangePasswordHandler` | 0% | 100% | PATCH /public/auth/change-password — seed user with known hash → 200 |
| [x] | `PublicResetPasswordHandler` | 0% | 100% | POST /public/auth/reset-password — seed user + valid OTP → 200 |
| [x] | `PublicResetPasswordAuthFactory` | 0% | 100% | Covered transitively by ResetPassword happy path |
| [x] | `PublicSetPasswordHandler` | 0% | 100% | POST /public/auth/set-password — seed OAuth user → 200 |
| [x] | `PublicRevokeSessionHandler` | 0% | 100% | POST /public/me/sessions/revoke/{id} — seed own session → 200 |
| [x] | `PublicUpdateAvatarHandler` | 0% | 100% | PATCH /public/auth/update-avatar — send valid image → 200 |
| [x] | `PublicUpdateAvatarAuthFactory` | 0% | 100% | Covered transitively by UpdateAvatar happy path |
| [ ] | `AccountStatusRequirementHandler` | 21.8% | 21.8% | 2 tests: inactive user → 423, unverified user → 403 |
| [ ] | `AdminExportSessionDataEndpointV1` | 81.8% | 81.8% | GET /admin/sessions/export?format=invalid → 400 |

## Identity Module — Exception Handlers

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [x] | `OtpAttemptsLimitExceptionHandler` | 0% | 100% | Covered by ResetPassword with MaxAttempts OTP → 429 |
| [x] | `OtpExpirationExceptionHandler` | 0% | 100% | Covered by ResetPassword with Expired OTP → 410 |
| [x] | `AccessDeniedExceptionHandler` | 0% | 100% | Covered by RevokeSession other user's session → 403 |
| [x] | `AccountInactiveExceptionHandler` | 0% | 100% | Any protected endpoint with inactive user → 423 |
| [x] | `AccountNotVerifiedExceptionHandler` | 0% | 100% | Any protected endpoint with unverified user → 403 |

## Identity Module — Repository

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `AuthRepository` | 52% | 88% | Covered transitively by all Identity handler tests — 15 methods each triggered by handler happy/error paths |

## Content Module — Specifications

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [x] | `ArticleTagByArticleIdSpecification` | 0% | 100% | PUT /admin/articles/{id}/tags — seed article WITH existing tags, then update tags (handler fetches old tags via this spec) |
| [x] | `GossipArticleSpecification` | 0% | 100% | GET /public/articles/promotion-feed — seed gossip category + published articles |
| [x] | `ShortVideoBookmarkByUserAndShortVideoSpecification` | 0% | 100% | POST /public/short-videos/{id}/bookmark — authenticate and bookmark a short video |
| [x] | `VideoByOrderItemIdSpecification` | 0% | 100% | PATCH /admin/orders/{id}/verify-payment — seed order with video item, verify payment |
| [x] | `TagByNameSpecification` | 0% | DEAD | Zero callers in codebase — recommend removal |

## Content Module — Errors

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `ArticleErrors` | 28.5% | 78.5% | 12 methods — each needs one test: |
| [ ] | — `NotFound` | | | GET /public/articles/non-existent-slug → 404 |
| [ ] | — `SlugAlreadyExists` | | | POST /admin/articles with existing slug → 409 |
| [ ] | — `AlreadySubmitted` | | | PATCH /admin/articles/{id}/submit on submitted article → 409 |
| [ ] | — `AlreadyPendingReview` | | | PATCH /admin/articles/{id}/submit on pending article → 409 |
| [ ] | — `AlreadyApproved` | | | PATCH /admin/articles/{id}/approve on approved article → 409 |
| [ ] | — `AlreadyPublished` | | | PATCH /admin/articles/{id}/publish on published article → 409 |
| [ ] | — `AlreadyRejected` | | | PATCH /admin/articles/{id}/reject on rejected article → 409 |
| [ ] | — `AlreadyArchived` | | | PATCH /admin/articles/{id}/archive on archived article → 409 |
| [ ] | — `InvalidStatusTransition` | | | PATCH /admin/articles/{id}/publish on Draft (skip submit+approve) → 400 |
| [ ] | — `CannotDeletePublishedArticle` | | | DELETE /admin/articles/{id} on published article → 400 |
| [ ] | — `TitleRequired` | | | POST /admin/articles with null title bypassing validator → 400 |
| [ ] | — `SlugRequired` | | | POST /admin/articles with null slug bypassing validator → 400 |
| [x] | `ArticleInteractionErrors` | 62.5% | 100% | 6 methods: |
| [ ] | — `AlreadyLiked` | | | POST /public/articles/{id}/like twice → 409 |
| [ ] | — `LikeNotFound` | | | DELETE /public/articles/{id}/like without prior like → 400 |
| [ ] | — `AlreadyBookmarked` | | | POST /public/articles/{id}/bookmark twice → 409 |
| [ ] | — `BookmarkNotFound` | | | DELETE /public/articles/{id}/bookmark without prior bookmark → 400 |
| [ ] | — `CommentNotFound` | | | PATCH /public/articles/{id}/comments/{randomId} → 404 |
| [ ] | — `NotCommentOwner` | | | PATCH /public/articles/{id}/comments/{otherUserId} → 400 |
| [ ] | `CategoryErrors` | 35.7% | 64.2% | 12 methods: |
| [ ] | — `AlreadyExists` | | | POST /admin/categories with existing slug → 409 |
| [ ] | — `NotFound` | | | Reference non-existent categoryId in order item → 404 |
| [ ] | — `AlreadyActive` | | | PATCH /admin/categories/{id}/activate on active → 409 |
| [ ] | — `AlreadyInactive` | | | PATCH /admin/categories/{id}/deactivate on inactive → 409 |
| [ ] | — `PricingAlreadyExists` | | | POST /admin/categories/{id}/pricing with existing tier → 409 |
| [ ] | — `PricingNotFound` | | | DELETE /admin/categories/{id}/pricing/{randomId} → 404 |
| [ ] | — `PriceMustBeNonNegative` | | | POST /admin/categories/{id}/pricing with priceUsd: -1 → 400 |
| [ ] | — `CannotMakeInactiveExclusive` | | | PATCH /admin/categories/{inactiveId}/set-exclusive → 400 |
| [ ] | — `OnlyVideoCategoryCanBeExclusive` | | | PATCH /admin/categories/{articleCatId}/set-exclusive → 400 |
| [ ] | — `NoExclusiveCategoryFound` | | | GET /public/categories/exclusive when none set → 404 |
| [ ] | — `NameRequired` | | | Covered by entity validation when handler creates with null name |
| [ ] | — `SlugRequired` | | | Same as above |
| [ ] | `ContentOrderErrors` | 46.6% | 73.3% | 13 methods: |
| [ ] | — `NotFound` | | | GET /admin/orders/{randomId} → 404 |
| [ ] | — `ItemNotFound` | | | POST /admin/orders/{id}/items/{randomId}/tiers → 404 |
| [ ] | — `TierAlreadyAttached` | | | POST /admin/orders/{id}/items/{itemId}/tiers twice with same tier → 409 |
| [ ] | — `AlreadySubmitted` | | | PATCH /admin/orders/{id}/submit on submitted order → 409 |
| [ ] | — `AlreadyPaid` | | | PATCH /admin/orders/{id}/verify-payment on paid order → 409 |
| [ ] | — `AlreadyCancelled` | | | PATCH /admin/orders/{id}/cancel on cancelled order → 409 |
| [ ] | — `CannotCancelPaidOrder` | | | PATCH /admin/orders/{paidId}/cancel → 400 |
| [ ] | — `CannotAddItemToNonDraftOrder` | | | POST /admin/orders/{submittedId}/items → 400 |
| [ ] | — `MustHaveAtLeastOneItemWithTier` | | | PATCH /admin/orders/{id}/submit with no items → 400 |
| [ ] | — `PaymentAlreadyVerified` | | | PATCH /admin/orders/{id}/verify-payment on verified → 409 |
| [ ] | — `PaymentAlreadyRejected` | | | PATCH /admin/orders/{id}/reject-payment on rejected → 409 |
| [ ] | — `ItemTierNotFound` | | | Reference non-existent tier ID → 404 |
| [ ] | — `PaymentNotFound` | | | PATCH /admin/orders/{id}/verify-payment on order without payment → 404 |
| [ ] | `ContentTypeErrors` | 57.1% | 71.4% | 5 methods: |
| [ ] | — `AlreadyExists` | | | POST /admin/content-types with existing name → 409 |
| [ ] | — `AlreadyActive` | | | PATCH /admin/content-types/{id}/activate on active → 409 |
| [ ] | — `AlreadyInactive` | | | PATCH /admin/content-types/{id}/deactivate on inactive → 409 |
| [ ] | — `NameRequired` | | | Covered by entity validation |
| [x] | — `NotFound` | | DEAD | No handler throws it — recommend removal |
| [ ] | `CustomerErrors` | 66.6% | 66.6% | 4 methods: |
| [ ] | — `AlreadyExists` | | | POST /admin/customers with existing email → 409 |
| [ ] | — `NotFound` | | | POST /admin/orders with non-existent customerId → 404 |
| [ ] | — `FullNameRequired` | | | Covered by entity validation |
| [ ] | — `EmailRequired` | | | Covered by entity validation |
| [ ] | `LyricsErrors` | 42.8% | 57.1% | 5 methods: |
| [ ] | — `NotFound` | | | GET /public/lyrics/video/{videoWithNoLyrics} → 404 |
| [ ] | — `AlreadyExists` | | | POST /admin/lyrics with same song+artist → 409 |
| [ ] | — `SongTitleRequired` | | | Covered by entity validation |
| [ ] | — `ArtistNameRequired` | | | Covered by entity validation |
| [ ] | — `LyricsTextRequired` | | | Covered by entity validation |
| [ ] | `PackageErrors` | 44.4% | 44.4% | 7 methods: |
| [ ] | — `NotFound` | | | POST /admin/orders with non-existent packageId → 404 |
| [ ] | — `AlreadyActive` | | | PATCH /admin/packages/{id}/activate on active → 409 |
| [ ] | — `AlreadyInactive` | | | PATCH /admin/packages/{id}/deactivate on inactive → 409 |
| [ ] | — `NameRequired` | | | Covered by entity validation |
| [ ] | — `PriceMustBeNonNegative` | | | POST /admin/packages with negative price → 400 |
| [ ] | — `SlotQuantityMustBePositive` | | | POST /admin/packages/{id}/slots with quantity=0 → 400 |
| [ ] | — `SlotNotFound` | | | DELETE /admin/packages/{id}/slots/{randomId} → 404 |
| [x] | `PlaylistErrors` | 60% | 100% | 3 methods: |
| [ ] | — `NotFound` | | | GET /public/playlists/{randomId} → 404 |
| [ ] | — `NotOwner` | | | DELETE /public/playlists/{otherUserId} → 400 |
| [ ] | — `VideoAlreadyInPlaylist` | | | POST /public/playlists/{id}/videos with same video twice → 409 |
| [ ] | `PricingTierErrors` | 25% | 62.5% | 6 methods: |
| [ ] | — `AlreadyExists` | | | POST /admin/pricing-tiers with existing name → 409 |
| [ ] | — `NotFound` | | | POST /admin/orders/{id}/items/{itemId}/tiers with non-existent tier → 404 |
| [ ] | — `AlreadyActive` | | | PATCH /admin/pricing-tiers/{id}/activate on active → 409 |
| [ ] | — `AlreadyInactive` | | | PATCH /admin/pricing-tiers/{id}/deactivate on inactive → 409 |
| [ ] | — `IsInactive` | | | POST /admin/categories/{id}/pricing with inactive tier → 400 |
| [ ] | — `NameRequired` | | | Covered by entity validation |
| [ ] | `PromotionLevelErrors` | 20% | 50% | 8 methods: |
| [ ] | — `AlreadyExists` | | | POST /admin/promotion-levels with existing name → 409 |
| [ ] | — `NotFound` | | | POST /admin/orders/{id}/items with non-existent promotionLevelId → 404 |
| [ ] | — `AlreadyActive` | | | PATCH /admin/promotion-levels/{id}/activate on active → 409 |
| [ ] | — `AlreadyInactive` | | | PATCH /admin/promotion-levels/{id}/deactivate on inactive → 409 |
| [ ] | — `NameRequired` | | | Covered by entity validation |
| [ ] | — `DurationMustBePositive` | | | POST /admin/promotion-levels with durationDays=0 → 400 |
| [ ] | — `PriceMustBeNonNegative` | | | POST /admin/promotion-levels with priceUsd=-1 → 400 |
| [ ] | — `InvalidSpotPriority` | | | POST /admin/promotion-levels with spotPriority=0 or 4 → 400 |
| [ ] | `ShortVideoErrors` | 71.4% | 71.4% | 5 methods: |
| [ ] | — `NotFound` | | | GET /public/shorts/non-existent-slug → 404 |
| [ ] | — `AlreadyActive` | | | PATCH /admin/shorts/{id}/activate on active → 409 |
| [ ] | — `AlreadyInactive` | | | PATCH /admin/shorts/{id}/deactivate on inactive → 409 |
| [ ] | — `TitleRequired` | | | Covered by CreateShortVideoValidator tests (invalid payload) |
| [ ] | — `SlugAlreadyExists` | | | Blocked — handler at 0% (Cloudinary) |
| [ ] | `ShortVideoInteractionErrors` | 16.6% | 83.3% | 4 methods: |
| [ ] | — `AlreadyLiked` | | | POST /public/shorts/{id}/like twice → 409 |
| [ ] | — `LikeNotFound` | | | DELETE /public/shorts/{id}/like without prior like → 400 |
| [ ] | — `AlreadyBookmarked` | | | POST /public/shorts/{id}/bookmark twice → 409 |
| [ ] | — `BookmarkNotFound` | | | DELETE /public/shorts/{id}/bookmark without prior bookmark → 400 |
| [ ] | `TagErrors` | 50% | 50% | 4 methods: |
| [ ] | — `SlugAlreadyExists` | | | POST /admin/tags with existing slug → 409 |
| [ ] | — `NameRequired` | | | Covered by entity validation |
| [ ] | — `SlugRequired` | | | Covered by entity validation |
| [x] | — `NotFound` | | DEAD | No handler throws it — recommend removal |
| [ ] | `VideoErrors` | 66.6% | 66.6% | 14 methods: |
| [ ] | — `NotFound` | | | GET /public/videos/non-existent-slug → 404 |
| [ ] | — `SlugAlreadyExists` | | | POST /admin/videos with existing slug → 409 |
| [ ] | — `AlreadySubmitted` | | | PATCH /admin/videos/{id}/submit on submitted → 409 |
| [ ] | — `AlreadyPendingReview` | | | PATCH /admin/videos/{id}/submit on pending → 409 |
| [ ] | — `AlreadyApproved` | | | PATCH /admin/videos/{id}/approve on approved → 409 |
| [ ] | — `AlreadyPublished` | | | PATCH /admin/videos/{id}/publish on published → 409 |
| [ ] | — `AlreadyRejected` | | | PATCH /admin/videos/{id}/reject on rejected → 409 |
| [ ] | — `AlreadyArchived` | | | PATCH /admin/videos/{id}/archive on archived → 409 |
| [ ] | — `InvalidStatusTransition` | | | PATCH /admin/videos/{id}/publish on Draft → 400 |
| [ ] | — `CannotPublishWithoutYoutubeUrl` | | | PATCH /admin/videos/{id}/publish without YouTube URL → 400 |
| [ ] | — `CannotDeletePublishedVideo` | | | DELETE /admin/videos/{id} on published → 400 |
| [ ] | — `CannotAttachYoutubeUrlBeforeShoot` | | | PATCH /admin/videos/{id}/youtube-url before shoot date → 400 |
| [ ] | — `TitleRequired` | | | Covered by entity validation |
| [ ] | — `SlugRequired` | | | Covered by entity validation |

## Content Module — Error Messages

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `ArticleErrorMessage` | 67.8% | 92.8% | Covered transitively by ArticleErrors tests |
| [ ] | `ArticleInteractionErrorMessage` | 54.5% | 81.8% | Covered transitively by ArticleInteractionErrors tests |
| [ ] | `CategoryErrorMessage` | 72.2% | 94.4% | Covered transitively by CategoryErrors tests |
| [ ] | `ContentOrderErrorMessage` | 66.6% | 88.8% | Covered transitively by ContentOrderErrors tests |
| [x] | `ContentTypeErrorMessage` | 85.7% | 100% | Covered transitively by ContentTypeErrors tests |
| [ ] | `LyricsErrorMessage` | 81.8% | 90.9% | Covered transitively by LyricsErrors tests |
| [ ] | `PackageErrorMessage` | 90% | 90% | Covered transitively by PackageErrors tests |
| [ ] | `PlaylistErrorMessage` | 42.8% | 71.4% | Covered transitively by PlaylistErrors tests |
| [ ] | `PricingTierErrorMessage` | 60% | 90% | Covered transitively by PricingTierErrors tests |
| [x] | `PromotionLevelErrorMessage` | 70% | 100% | Covered transitively by PromotionLevelErrors tests |
| [ ] | `ShortVideoErrorMessage` | 28.5% | 28.5% | Covered transitively by ShortVideoErrors tests |
| [ ] | `ShortVideoInteractionErrorMessage` | 16.6% | 83.3% | Covered transitively by ShortVideoInteractionErrors tests |
| [ ] | `VideoErrorMessage` | 77.7% | 77.7% | Covered transitively by VideoErrors tests |
| [x] | `ContentI18n` | 96.8% | 100% | Covered transitively by all Content error tests |

## Content Module — Validators

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `CreateShortVideoValidator` | 0% | 0% | 8 tests: empty title, long title, empty slug, invalid slug, null file, empty file, oversized file, wrong extension → each 400 |
| [ ] | `UpdateShortVideoValidator` | 0% | 0% | 3 tests: invalid GUID, oversized file, wrong extension → 400 |
| [ ] | `UploadArticleImageValidator` | 0% | 0% | 2 tests: invalid GUID, no file → 400 |
| [ ] | `CategoryValidation` | 65.7% | 65.7% | Tests for isRequired=false branches: PUT /admin/categories with name too long, slug too long, description too long |
| [ ] | `ContentTypeValidation` | 58.3% | 58.3% | Test for isRequired=false: PUT /admin/content-types with name too long |
| [ ] | `EditorialValidation` | 62.7% | 62.7% | Tests for: empty categoryId GUID, title too long (update), slug invalid format (update), headline too short/long, empty body, empty description, invalid YouTube URL, orderItemId without customerId, unpromote reason too long, meta title/description too short/long, shoot date in past |
| [ ] | `PricingTierValidation` | 75% | 75% | Tests for: empty tier GUID, name too long (update), description too long |
| [ ] | `PromotionLevelValidation` | 66.6% | 66.6% | Tests for: name too long (update), durationDays=0, priceUsd=-1, spotPriority=0 |
| [ ] | `TagValidation` | 63.6% | 63.6% | Tests for: name too long (update), slug invalid format (update), PUT /admin/articles/{id}/tags with tagNames: [""] |

## Content Module — Handlers (below 100%)

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `AdminAttachYoutubeVideoUrlHandler` | 93.7% | 93.7% | PATCH /admin/videos/{id}/youtube-url before scheduled shoot → 400 (covers the 1 uncovered error branch) |
| [x] | `AdminCreateShortVideoHandler` | 0% | BLOCKED | Cloudinary stub. Handler code unreachable. Validator tests cover validator only. |
| [x] | `AdminUpdateShortVideoHandler` | 0% | BLOCKED | Same |
| [x] | `AdminUploadArticleImageHandler` | 0% | BLOCKED | Same |

## Content Module — Repositories

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `ArticleRepository` | 63.6% | 81.8% | GET /admin/articles?search=keyword, GET /admin/articles?categoryId={id} |
| [ ] | `VideoRepository` | 55.5% | 55.5% | GET /admin/videos?search=keyword |
| [x] | `ShortVideoRepository` | 60% | 100% | GET /admin/shorts?search=keyword |
| [ ] | `LyricsRepository` | 60% | 60% | GET /admin/lyrics?search=keyword |
| [x] | `PlaylistRepository` | 60% | 100% | GET /public/me/playlists, GET /public/playlists/{id} |

## Core Module — Errors (partially coverable)

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `FileErrors` | 17.2% | 17.2% | 3 coverable methods via Identity/Content file uploads. ~12 methods BLOCKED behind Cloudinary stub. |
| [ ] | `CoreI18n` | 50% | 50% | Covered transitively by any test triggering Core error paths |
| [ ] | `Core.ConflictErrorMessage` | 33.3% | 33.3% | Covered transitively |
| [x] | `Core.InternalServerErrorMessage` | 16.6% | BLOCKED | Requires infrastructure failure |
| [ ] | `Core.ValidationErrorMessage` | 5.2% | 5.2% | Covered transitively. `StorageUrlCannotBeEmpty` = DEAD CODE |

## Core Module — Specifications (all BLOCKED)

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [x] | `FileByFileNameSpecification` | 0% | BLOCKED | Only used by stubbed FileService |
| [x] | `FileByMimeTypeSpecification` | 0% | BLOCKED | Only used by stubbed FileService |
| [x] | `FileByOriginalFileNameSpecification` | 0% | BLOCKED | Only used by stubbed FileService |
| [x] | `FileBySizeRangeSpecification` | 0% | BLOCKED | Only used by stubbed FileService |
| [x] | `FileIsDeletedSpecification` | 0% | BLOCKED | Only used by stubbed FileService |
| [x] | `FileIsImageSpecification` | 0% | BLOCKED | Only used by stubbed FileService |
| [x] | `FileIsValidAvatarSpecification` | 0% | BLOCKED | Only used by stubbed FileService |

## Shared Module — Exception Handlers (all BLOCKED)

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [x] | `AuthenticationExceptionHandler` (Shared) | 0% | BLOCKED | Framework-level auth — not triggered by endpoint tests |
| [x] | `BadGatewayExceptionHandler` | 0% | BLOCKED | Requires 502 from upstream |
| [x] | `DefaultExceptionHandler` | 0% | BLOCKED | Catch-all for unhandled exceptions |
| [x] | `FormatExceptionStrategy` | 0% | BLOCKED | Framework intercepts malformed JSON first |
| [x] | `InternalServerExceptionHandler` | 0% | BLOCKED | Requires 500 infrastructure error |
| [x] | `RateLimitExceededExceptionHandler` | 0% | BLOCKED | Test rate limits are high |
| [ ] | `ExceptionHandler` (main) | 92.8% | | DetermineLogLevel edge cases — partially coverable |
| [ ] | `ExceptionStrategyRegistry` | 65% | | GET /api/v1/nonexistent-route triggers inheritance resolution |

## Shared Module — Specifications

| Done | Class | Before | After | Test to Write |
| --- | --- | --- | --- | --- |
| [ ] | `NotSpecification<T>` | 0% | | Covered transitively by ShortVideoQueryBuilder `.Not()` — GET /admin/shorts with filters |
| [x] | `OrSpecification<T>` | 0% | DEAD | `.Or()` never called anywhere |
| [ ] | `Specification<T>` | 9% | | `And()` covered. `Not()` coverable via query builder. `Or/IsSatisfiedBy/AndAll/OrAll` = DEAD CODE |

## Dead Code Summary (excluded from 100% target)

| Class/Method | Module | Reason |
| --- | --- | --- |
| `TagByNameSpecification` | Content | Zero callers |
| `ContentTypeErrors.NotFound` | Content | No handler throws it |
| `TagErrors.NotFound` | Content | No handler throws it |
| `EditorialValidation.ValidArticleId` | Content | Zero callers |
| `EditorialValidation.ValidVideoId` | Content | Zero callers |
| `EditorialValidation.ValidLyricsId` | Content | Zero callers |
| `UserErrors.CoreRoleCannotBeModified` | Identity | No handler calls it |
| `OrSpecification<T>` | Shared | `.Or()` never called |
| `Specification.Or/IsSatisfiedBy/AndAll/OrAll` | Shared | Never called |
| `Core.ValidationErrorMessage.StorageUrlCannotBeEmpty` | Core | Zero callers |

## Progress Summary

| Category | Total Classes | At 100% | Below 100% (coverable) | Blocked | Dead |
| --- | --- | --- | --- | --- | --- |
| Specifications | 77 | 53 | 15 | 7 (Core File*) | 2 |
| Errors + Error Messages | 39 | 7 | 30 | 1 (Core.InternalServerErrorMessage) | 1 method |
| Validators | 22 | 17 | 5 | 0 | 3 methods |
| Query Builders | 7 | 6 | 1 | 0 | 0 |
| Handlers (endpoint) | ~200 | ~198 | 2 | 3 (Cloudinary) | 0 |
| Repositories | 10+ | 7+ | 3 | 0 | 0 |
| **Total** | | **~35 done** | **~56 coverable** | **11 blocked** | **~6** |
