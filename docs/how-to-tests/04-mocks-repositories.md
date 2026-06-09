# Mock Repositories Reference

All repository mocks are **static helper classes** in `tests/Unit/Common/Mocks/Repositories/`. Each follows the same pattern:

```csharp
// 1. Create the mock
var mock = MockArticleRepository.Create();

// 2. Set up specific scenarios (chain as needed)
mock.SetupGetByIdOrThrow(article);
mock.SetupGetByIdOrThrowNotFound(nonExistentId);

// 3. Pass to handler
var handler = new MyHandler(mock.Object, ...);

// 4. After acting, verify calls
mock.VerifyUpdateCalled();
```

---

## Pattern Rules

- `Create()` returns a `Mock<IRepository>` with sensible defaults already applied
- Setup methods configure `ReturnsAsync` or `ThrowsAsync` on specific calls
- Verify methods assert `Times.Once` or `Times.Never` on specific method calls
- All async methods use `It.IsAny<CancellationToken>()` for the cancellation token parameter
- Setup methods return `Mock<T>` to enable chaining

---

## Identity Module Repositories

### `MockRoleRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockRoleRepository.cs`
**Mocks:** `IRoleRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(RoleEntity role)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByIdWithPermissionsOrThrow(RoleEntity role)
mock.SetupGetByIdWithPermissionsOrThrowNotFound(Guid id)
mock.SetupExistsByName(string name, bool exists)
mock.SetupExistsByNameReturnsFalse()           // Any name → false
mock.SetupGetAllWithPagination(List<RoleEntity> roles, int totalCount)
mock.SetupGetAllWithPaginationEmpty()
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddCalled(Func<RoleEntity, bool> predicate)
mock.VerifyDeleteCalled(RoleEntity role)
mock.VerifyDeleteCalled()                      // Any role
```

**Defaults:** `AddAsync` → `Task.CompletedTask`

---

### `MockPermissionRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockPermissionRepository.cs`
**Mocks:** `IPermissionRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(PermissionEntity permission)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupExistsByResourceAndAction(string resource, string action, bool exists)
mock.SetupExistsByResourceAndActionReturnsFalse()
mock.SetupGetAllWithPagination(List<PermissionEntity> permissions, int totalCount)
mock.SetupGetAllWithPaginationEmpty()
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddCalled(Func<PermissionEntity, bool> predicate)
mock.VerifyDeleteCalled(PermissionEntity permission)
mock.VerifyDeleteCalled()
```

---

### `MockRolePermissionRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockRolePermissionRepository.cs`
**Mocks:** `IRolePermissionRepository`

**Setup Methods:**
```csharp
mock.SetupExistsByRoleAndPermission(Guid roleId, Guid permissionId, bool exists)
mock.SetupExistsByRoleAndPermissionReturnsFalse()
mock.SetupGetByRoleAndPermission(RolePermissionEntity entity)
mock.SetupGetByRoleAndPermissionReturnsNull(Guid roleId, Guid permissionId)
mock.SetupGetPermissionIdsByRoleId(Guid roleId, List<Guid> permissionIds)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddCalled(Func<RolePermissionEntity, bool> predicate)
mock.VerifyDeleteCalled(RolePermissionEntity entity)
mock.VerifyDeleteCalled()
```

---

### `MockUserRoleRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockUserRoleRepository.cs`
**Mocks:** `IUserRoleRepository`

**Setup Methods:**
```csharp
mock.SetupExistsByUserAndRole(Guid userId, Guid roleId, bool exists)
mock.SetupExistsByUserAndRoleReturnsFalse()
mock.SetupGetByUserAndRole(UserRoleEntity entity)
mock.SetupGetByUserAndRoleReturnsNull(Guid userId, Guid roleId)
mock.SetupGetUserRolesWithRole(Guid userId, List<UserRoleEntity> userRoles)
mock.SetupGetUserRolesWithRoleEmpty(Guid userId)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddCalled(Func<UserRoleEntity, bool> predicate)
mock.VerifyDeleteCalled(UserRoleEntity entity)
mock.VerifyDeleteCalled()
```

---

### `MockSessionRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockSessionRepository.cs`
**Mocks:** `ISessionRepository`

**Setup Methods:**
```csharp
mock.SetupGetById(SessionEntity session)
mock.SetupGetByIdReturnsNull(Guid sessionId)
mock.SetupGetByRefreshTokenHash(string hash, SessionEntity session)
mock.SetupGetByRefreshTokenHashReturnsNull(string hash)
mock.SetupGetActiveSessionByUserIdAndDeviceId(Guid userId, string deviceId, SessionEntity session)
mock.SetupGetActiveSessionByUserIdAndDeviceIdReturnsNull(Guid userId, string deviceId)
mock.SetupGetUserSessions(Guid userId, List<SessionEntity> sessions)
mock.SetupGetUserSessionsEmpty(Guid userId)
mock.SetupGetAllWithPagination(List<SessionEntity> sessions, int totalCount)
mock.SetupGetAllWithPaginationEmpty()
mock.SetupGetActiveSessionCountByBrowser(Dictionary<EnumBrowser, int> counts)
mock.SetupGetActiveSessionCountByDevice(Dictionary<EnumDevice, int> counts)
mock.SetupGetActiveSessionCountByPlatform(Dictionary<EnumPlatform, int> counts)
mock.SetupGetActiveSessionCountByClient(Dictionary<EnumClient, int> counts)
mock.SetupGetTotalActiveSessionsCount(int count)
mock.SetupGetTotalActiveUsersCount(int count)
mock.SetupGetSessionsForExport(List<SessionEntity> sessions)
mock.SetupDeleteExpiredSessions(int count)
```

**Verify Methods:**
```csharp
mock.VerifyCreateCalled()
mock.VerifyCreateCalled(Func<SessionEntity, bool> predicate)
mock.VerifyRevokeCalled(Guid sessionId)
mock.VerifyDeleteAllByUserIdCalled(Guid userId)
mock.VerifyUpdateRefreshTokenCalled(Guid sessionId)
```

**Defaults:** `CreateAsync`, `RevokeAsync`, `DeleteAllByUserIdAsync`, `UpdateRefreshTokenAsync` → `Task.CompletedTask`

---

### `MockAuthRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockAuthRepository.cs`
**Mocks:** `IAuthRepository`

**Setup Methods:**
```csharp
mock.SetupFindUserByIdOrThrow(UserEntity user)
mock.SetupFindUserByIdOrThrowNotFound(Guid userId)
mock.SetupGetUserWithRolesByEmailOrThrow(string email, UserEntity user)
mock.SetupGetUserWithRolesByEmailOrThrowNotFound(string email)
mock.SetupGetUserWithRolesAndPermissionsById(UserEntity user)
mock.SetupGetUserWithRolesAndPermissionsByIdNotFound(Guid userId)
mock.SetupGetUserWithRolesAndPermissionsByCredentials(credentials, UserEntity user)
mock.SetupGetUserWithRolesAndPermissionsByCredentialsNotFound(credentials)
mock.SetupGetUserWithSessionsById(UserEntity user)
mock.SetupExistsByEmail(string email, bool exists)
mock.SetupExistsByUserName(string userName, bool exists)
mock.SetupValidateUniqueCredentialsSuccess()
mock.SetupIsUserAccountActiveReturnsTrue()
mock.SetupIsUserAccountVerifiedReturnsTrue()
mock.SetupIsSessionValid(Guid sessionId)
mock.SetupIsSessionValidReturnsTrue()
mock.SetupGetSessionIdFromClaims(Guid sessionId)
mock.SetupGetUserIdFromClaims(Guid userId)
mock.SetupIsUserAdminReturnsTrue()
mock.SetupIsUserAdminReturnsFalse()
mock.SetupGetUserWithRolesByEmail(UserEntity user)      // Any email
mock.SetupGetUserWithRolesByEmailNotFound(string email)
mock.SetupGetUserWithRolesByCredentials(UserEntity user)
mock.SetupGetUserWithRolesByCredentialsNotFound(credentials)
mock.SetupIsUserAdminThrowsAuthorizationException()
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddCalled(Func<UserEntity, bool> predicate)
mock.VerifyAssignVisitorRoleCalled(Guid userId)
```

---

### `MockOtpRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockOtpRepository.cs`
**Mocks:** `IOtpRepository`

**Setup Methods:**
```csharp
mock.SetupValidateOtp(OtpEntity otp)
mock.SetupValidateOtpNotFound(Guid userId, string code, EnumOtpPurpose purpose)
mock.SetupValidateOtpInvalidCode(Guid userId, string code, EnumOtpPurpose purpose)
mock.SetupValidateOtpExpired(Guid userId, string code, EnumOtpPurpose purpose)
mock.SetupValidateUsedOtp(OtpEntity otp)
mock.SetupInvalidateExistingOtps(Guid userId, EnumOtpPurpose purpose)
mock.SetupCleanupExpiredOtps(int count)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddCalled(Func<OtpEntity, bool> predicate)
mock.VerifyInvalidateExistingOtpsCalled(Guid userId, EnumOtpPurpose purpose)
```

---

## Core Module Repositories

### `MockFileRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockFileRepository.cs`
**Mocks:** `IFileRepository`

**Setup Methods:**
```csharp
mock.SetupGetById(FileEntity file)
mock.SetupGetByIdReturnsNull(Guid fileId)
mock.SetupGetAvatarFile(Guid avatarFileId, FileEntity file)
mock.SetupGetAvatarFileReturnsNull(Guid avatarFileId)
mock.SetupUploadAndStoreAvatar(FileEntity file)
mock.SetupDownloadAndStoreAvatarFromUrl(FileEntity file)
mock.SetupUpdateAvatarFromUrl(FileEntity? file)
mock.SetupUpdateAvatarFromFile(FileEntity file)
mock.SetupUpdateAvatarUrlFromSource(FileEntity? file)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddCalled(Func<FileEntity, bool> predicate)
mock.VerifyUpdateCalled()
mock.VerifyUpdateCalled(Func<FileEntity, bool> predicate)
mock.VerifyRemoveCalled(FileEntity file)
mock.VerifyRemoveCalled()
mock.VerifySaveChangesCalled()
```

---

## Content Module Repositories

### `MockArticleRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`
**Mocks:** `IArticleRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(ArticleEntity entity)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByIdAsync(Guid id, ArticleEntity? entity)
mock.SetupGetByOrderItemIdAsync(Guid orderItemId, ArticleEntity? entity)
mock.SetupGetBySlug(string slug, ArticleEntity? entity)
mock.SetupGetAllAsync(List<ArticleEntity> articles, int totalCount)
mock.SetupGetPromotedAsync(List<ArticleEntity> articles)
mock.SetupGetAbandonedDraftsAsync(List<ArticleEntity> articles)
mock.SetupGetImagesByArticleId(Guid articleId, List<ArticleImageEntity> images)
mock.SetupGetTagsByArticleId(Guid articleId, List<TagEntity> tags)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyUpdateCalled()
mock.VerifyRemoveCalled(ArticleEntity article)
mock.VerifyAddImageCalled()
mock.VerifyRemoveImagesCalled()
mock.VerifyAddTagCalled()       // Times.AtLeastOnce
mock.VerifyRemoveTagCalled()
```

---

### `MockVideoRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockVideoRepository.cs`
**Mocks:** `IVideoRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(VideoEntity entity)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByIdAsync(Guid id, VideoEntity? entity)
mock.SetupGetByOrderItemIdAsync(Guid orderItemId, VideoEntity? entity)
mock.SetupGetBySlug(string slug, VideoEntity? entity)
mock.SetupGetAllAsync(List<VideoEntity> videos, int totalCount)
mock.SetupGetPromotedAsync(List<VideoEntity> videos)
mock.SetupGetTagsByVideoId(Guid videoId, List<TagEntity> tags)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyUpdateCalled()
mock.VerifyRemoveCalled(VideoEntity video)
mock.VerifyAddTagCalled()
mock.VerifyRemoveTagCalled()
```

---

### `MockCategoryRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockCategoryRepository.cs`
**Mocks:** `ICategoryRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(CategoryEntity entity)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByIdAsync(Guid id, CategoryEntity? entity)
mock.SetupGetBySlug(string slug, CategoryEntity? entity)
mock.SetupGetAllAsync(List<CategoryEntity> list, int totalCount)
mock.SetupGetActiveByContentType(Guid contentTypeId, List<CategoryEntity> list)
mock.SetupGetPricingByCategory(Guid categoryId, List<CategoryPricingEntity> list)
mock.SetupGetPricing(Guid categoryId, Guid tierId, CategoryPricingEntity? pricing)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddPricingCalled()
mock.VerifyRemovePricingCalled(CategoryPricingEntity pricing)
```

---

### `MockLookupRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockLookupRepository.cs`
**Mocks:** `ILookupRepository`

**Setup Methods:**
```csharp
// ContentType
mock.SetupContentTypeExistsByName(string name, bool exists)
mock.SetupGetContentTypeByIdOrThrow(ContentTypeEntity entity)
mock.SetupGetContentTypeByIdOrThrowNotFound(Guid id)
mock.SetupGetAllContentTypes(List<ContentTypeEntity> list)

// PricingTier
mock.SetupPricingTierExistsByName(string name, bool exists)
mock.SetupGetPricingTierByIdOrThrow(PricingTierEntity entity)
mock.SetupGetPricingTierByIdOrThrowNotFound(Guid id)
mock.SetupGetAllPricingTiers(List<PricingTierEntity> list)

// PromotionLevel
mock.SetupPromotionLevelExistsByName(string name, bool exists)
mock.SetupGetPromotionLevelByIdOrThrow(PromotionLevelEntity entity)
mock.SetupGetPromotionLevelByIdOrThrowNotFound(Guid id)
mock.SetupGetAllPromotionLevels(List<PromotionLevelEntity> list)
mock.SetupGetActivePromotionLevels(List<PromotionLevelEntity> list)

// Tag
mock.SetupGetTagByIdOrThrow(TagEntity entity)
mock.SetupGetTagByIdOrThrowNotFound(Guid id)
mock.SetupGetTagBySlug(string slug, TagEntity? entity)
mock.SetupGetAllTags(List<TagEntity> list)
```

**Verify Methods:**
```csharp
mock.VerifyAddContentTypeCalled()
mock.VerifyAddContentTypeNotCalled()
mock.VerifyAddPricingTierCalled()
mock.VerifyAddPricingTierNotCalled()
mock.VerifyAddPromotionLevelCalled()
mock.VerifyAddPromotionLevelNotCalled()
mock.VerifyAddTagCalled()
mock.VerifyAddTagNotCalled()
```

---

### `MockPackageRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockPackageRepository.cs`
**Mocks:** `IPackageRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdWithSlotsOrThrow(PackageEntity entity)
mock.SetupGetByIdWithSlotsOrThrowNotFound(Guid id)
mock.SetupGetSlotById(Guid slotId, PackageSlotEntity? slot)
mock.SetupGetAllAsync(List<PackageEntity> list, int totalCount)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyAddSlotCalled()
mock.VerifyRemoveSlotCalled(PackageSlotEntity slot)
```

---

### `MockCustomerRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockCustomerRepository.cs`
**Mocks:** `ICustomerRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(CustomerEntity entity)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByEmail(string email, CustomerEntity? customer)
mock.SetupGetAllAsync(List<CustomerEntity> list, int totalCount)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
```

---

### `MockShortVideoRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockShortVideoRepository.cs`
**Mocks:** `IShortVideoRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(ShortVideoEntity entity)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByIdAsync(Guid id, ShortVideoEntity? entity)
mock.SetupGetBySlug(string slug, ShortVideoEntity? entity)
mock.SetupGetAllAsync(List<ShortVideoEntity> list, int totalCount)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyUpdateCalled()
mock.VerifyRemoveCalled(ShortVideoEntity shortVideo)
```

---

### `MockLyricsRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockLyricsRepository.cs`
**Mocks:** `ILyricsRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(LyricsEntity entity)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByIdAsync(Guid id, LyricsEntity? entity)
mock.SetupGetBySongTitleAndArtist(string songTitle, string artistName, LyricsEntity? entity)
mock.SetupGetAllAsync(List<LyricsEntity> lyrics, int totalCount)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyUpdateCalled()
```

---

### `MockContentOrderRepository`
**File:** `tests/Unit/Common/Mocks/Repositories/MockContentOrderRepository.cs`
**Mocks:** `IContentOrderRepository`

**Setup Methods:**
```csharp
mock.SetupGetByIdOrThrow(ContentOrderEntity order)
mock.SetupGetByIdOrThrowNotFound(Guid id)
mock.SetupGetByIdWithItems(ContentOrderEntity? order)   // nullable — null simulates not found
mock.SetupGetAllAsync(IReadOnlyList<ContentOrderEntity> list, int totalCount)
mock.SetupGetPaymentByOrderId(Guid orderId, ContentPaymentEntity? payment)
mock.SetupGetItemById(Guid orderId, Guid itemId, ContentOrderItemEntity? item)
```

**Verify Methods:**
```csharp
mock.VerifyAddCalled()
mock.VerifyUpdateCalled()
mock.VerifyAddItemCalled()
mock.VerifyAddItemTierCalled()
mock.VerifyAddPaymentCalled()
mock.VerifyUpdatePaymentCalled()
```

**Defaults:** All `Add*` and `Update*` async methods → `Task.CompletedTask`; `Get*` → `null` or empty list
