# Factories Reference

Factories are static classes that wrap builders and expose named, intent-revealing creation methods. **Always use factories in tests, not builders directly.**

---

## Factory vs Builder

| | Builder | Factory |
|-|---------|---------|
| Type | Instance class, fluent | Static class, static methods |
| Used by | Factories | Tests |
| Purpose | Fine-grained field control | Named scenarios (`CreatePublished()`, `CreateVerifiedActive()`) |
| Location | `tests/Fixtures/Builders/` | `tests/Fixtures/Factories/` |

---

## General Factories

### `RolePermissionFactory`
**File:** `tests/Fixtures/Factories/RolePermissionFactory.cs`

```csharp
RolePermissionFactory.Create()                                          // Random values
RolePermissionFactory.Create(Guid roleId, Guid permissionId)            // Specific IDs
RolePermissionFactory.CreateWithPermission(Guid roleId, PermissionEntity permission)  // With navigation property
RolePermissionFactory.CreateWithId(Guid id)                            // Specific association ID
RolePermissionFactory.CreateMany(int count)                             // List
```

### `CommandFactory`
**File:** `tests/Fixtures/Factories/CommandFactory.cs`

Nested static classes `CommandFactory.Role` and `CommandFactory.Permission`.

```csharp
// Role commands
CommandFactory.Role.CreateCommand()
CommandFactory.Role.CreateValidCommand()
CommandFactory.Role.CreateCommand(string name)
CommandFactory.Role.CreateCommand(string name, string description)
CommandFactory.Role.UpdateCommand(Guid roleId)
CommandFactory.Role.UpdateValidCommand(Guid roleId)
CommandFactory.Role.UpdateCommand(Guid roleId, string? name, string? description)

// Permission commands
CommandFactory.Permission.CreateCommand()
CommandFactory.Permission.CreateValidCommand()
CommandFactory.Permission.CreateCommand(string resource, string action)
CommandFactory.Permission.CreateCommand(string resource, string action, string description)
CommandFactory.Permission.UpdateCommand(Guid permissionId)
CommandFactory.Permission.UpdateValidCommand(Guid permissionId)
CommandFactory.Permission.UpdateCommand(Guid permissionId, string? resource, string? action, string? description)
```

---

## Identity Factories

### `UserFactory`
**File:** `tests/Fixtures/Factories/UserFactory.cs`

```csharp
UserFactory.Create()                                        // Random verified active user
UserFactory.Create(string email)                            // Specific email
UserFactory.Create(string email, string userName)           // Email + username
UserFactory.CreateWithId(Guid id)                          // Specific ID
UserFactory.CreateWithId(Guid id, string email)            // ID + email
UserFactory.CreateVerifiedActive()                          // Verified + active
UserFactory.CreateUnverified()                              // Unverified account
UserFactory.CreateInactive()                                // Inactive account
UserFactory.CreateWithRole(RoleEntity role)                 // Has role assigned
UserFactory.CreateExternal(EnumAuthProvider provider)       // Google/social login
UserFactory.CreateWithPhoneNumber(string full, string partial)
UserFactory.CreateMany(int count)                           // List<UserEntity>
UserFactory.CreateSuperAdmin()                              // With SuperAdmin role
UserFactory.CreateAdmin()                                   // With Admin role
UserFactory.CreateVisitor()                                 // With Visitor role
```

### `RoleFactory`
**File:** `tests/Fixtures/Factories/RoleFactory.cs`

```csharp
RoleFactory.Create()
RoleFactory.Create(string name)
RoleFactory.Create(string name, string description)
RoleFactory.CreateWithId(Guid id)
RoleFactory.CreateWithId(Guid id, string name)
RoleFactory.CreateInactive()
RoleFactory.CreateInactive(string name)
RoleFactory.CreateDeleted()
RoleFactory.CreateDeleted(string name)
RoleFactory.CreateMany(int count)
RoleFactory.Create(string name, IEnumerable<PermissionEntity> permissions)
RoleFactory.CreateSuperAdmin()   // Uses TestConstants.Role.SuperAdminName
RoleFactory.CreateAdmin()        // Uses TestConstants.Role.AdminName
RoleFactory.CreateVisitor()      // Uses TestConstants.Role.VisitorName
```

### `PermissionFactory`
**File:** `tests/Fixtures/Factories/PermissionFactory.cs`

```csharp
PermissionFactory.Create()
PermissionFactory.Create(string resource, string action)
PermissionFactory.Create(string resource, string action, string description)
PermissionFactory.CreateWithId(Guid id)
PermissionFactory.CreateWithId(Guid id, string resource, string action)
PermissionFactory.CreateInactive()
PermissionFactory.CreateDeleted()
PermissionFactory.CreateMany(int count)
PermissionFactory.CreateRead(string resource)    // action = "read"
PermissionFactory.CreateCreate(string resource)  // action = "create"
PermissionFactory.CreateUpdate(string resource)  // action = "update"
PermissionFactory.CreateDelete(string resource)  // action = "delete"
PermissionFactory.CreateCrud(string resource)    // List of 4 CRUD permissions
```

### `SessionFactory`
**File:** `tests/Fixtures/Factories/SessionFactory.cs`

```csharp
SessionFactory.Create()
SessionFactory.Create(Guid userId)
SessionFactory.Create(Guid userId, string deviceId)
SessionFactory.CreateWithId(Guid id)
SessionFactory.CreateWithId(Guid id, Guid userId)
SessionFactory.CreateExpired()
SessionFactory.CreateExpired(Guid userId)
SessionFactory.CreateRevoked()
SessionFactory.CreateRevoked(Guid userId)
SessionFactory.CreateMobile()
SessionFactory.CreateMobile(Guid userId)
SessionFactory.CreateDesktop()
SessionFactory.CreateDesktop(Guid userId)
SessionFactory.CreateMany(int count)
SessionFactory.CreateMany(Guid userId, int count)
SessionFactory.CreateWithRefreshTokenHash(string hash)
SessionFactory.CreateWithRefreshTokenHash(Guid userId, string hash)
SessionFactory.CreateWithIpAddress(string ipAddress)
SessionFactory.CreateWithBrowser(EnumBrowser browser)
SessionFactory.CreateWithPlatform(EnumPlatform platform)
SessionFactory.CreateWithClient(EnumClient client)
SessionFactory.CreateWithExpiresAt(DateTime expiresAt)
SessionFactory.CreateExpiredWithRefreshTokenHash(string hash)
```

### `OtpFactory`
**File:** `tests/Fixtures/Factories/OtpFactory.cs`

```csharp
OtpFactory.Create()
OtpFactory.Create(Guid userId)
OtpFactory.Create(Guid userId, string code)
OtpFactory.Create(Guid userId, EnumOtpPurpose purpose)
OtpFactory.Create(Guid userId, string code, EnumOtpPurpose purpose)
OtpFactory.CreateWithId(Guid id)
OtpFactory.CreateForEmailVerification(Guid userId)
OtpFactory.CreateForPasswordReset(Guid userId)
OtpFactory.CreateExpired()
OtpFactory.CreateExpired(Guid userId)
OtpFactory.CreateExpired(Guid userId, EnumOtpPurpose purpose)
OtpFactory.CreateExpired(Guid userId, string code, EnumOtpPurpose purpose)
OtpFactory.CreateUsed()
OtpFactory.CreateUsed(Guid userId)
OtpFactory.CreateUsed(Guid userId, EnumOtpPurpose purpose)
OtpFactory.CreateUsed(Guid userId, string code, EnumOtpPurpose purpose)
OtpFactory.CreateUsedAndExpired(...)
OtpFactory.CreateMaxAttemptsReached()
OtpFactory.CreateMaxAttemptsReached(Guid userId)
OtpFactory.CreateMaxAttemptsReached(Guid userId, string code, EnumOtpPurpose purpose)
OtpFactory.CreateValid(Guid userId)            // Uses TestConstants.Otp.ValidCode
OtpFactory.CreateMany(int count)
OtpFactory.CreateWithCode(string code)
OtpFactory.CreateWithExpiresAt(DateTime expiresAt)
OtpFactory.CreateWithAttemptCount(Guid userId, string code, EnumOtpPurpose purpose, int attemptCount)
```

### `FileFactory`
**File:** `tests/Fixtures/Factories/FileFactory.cs`

```csharp
FileFactory.Create()
FileFactory.Create(string originalFileName)
FileFactory.CreateWithId(Guid id)
FileFactory.CreateJpeg()
FileFactory.CreatePng()
FileFactory.CreatePdf()
FileFactory.CreateDeleted()
FileFactory.CreateDeletedWithId(Guid id)
FileFactory.CreateWithSize(long sizeInBytes)
FileFactory.CreateMany(int count)
FileFactory.CreateWithTestValues()              // Uses TestConstants.File values
FileFactory.CreateWithFileName(string fileName)
FileFactory.CreateWithMimeType(string mimeType)
FileFactory.CreateWithStorageUrl(string url)
```

---

## Content Factories

### `ArticleFactory`
**File:** `tests/Fixtures/Factories/Content/ArticleFactory.cs`

All methods require `Guid categoryId` as the first argument.

```csharp
ArticleFactory.Create(Guid categoryId)
ArticleFactory.CreateWithId(Guid id, Guid categoryId)
ArticleFactory.CreatePendingPayment(Guid categoryId)
ArticleFactory.CreatePendingReview(Guid categoryId)
ArticleFactory.CreateApproved(Guid categoryId)
ArticleFactory.CreatePublished(Guid categoryId)
ArticleFactory.CreateRejected(Guid categoryId, string? reason = null)
ArticleFactory.CreateArchived(Guid categoryId)
ArticleFactory.CreatePromoted(Guid categoryId)               // Published + StampPromotion
ArticleFactory.CreateWithSocialBoost(Guid categoryId)        // Published + StampSocialBoost
ArticleFactory.CreatePaid(Guid categoryId, Guid customerId, Guid orderItemId)
ArticleFactory.CreateMany(int count, Guid categoryId)
```

### `VideoFactory`
**File:** `tests/Fixtures/Factories/Content/VideoFactory.cs`

```csharp
VideoFactory.Create(Guid categoryId)
VideoFactory.CreateWithId(Guid id, Guid categoryId)
VideoFactory.CreatePendingReview(Guid categoryId)
VideoFactory.CreateApproved(Guid categoryId)
VideoFactory.CreatePublished(Guid categoryId)
VideoFactory.CreateRejected(Guid categoryId, string? reason = null)
VideoFactory.CreateArchived(Guid categoryId)
VideoFactory.CreatePromoted(Guid categoryId)
VideoFactory.CreateMany(int count, Guid categoryId)
```

### `ShortVideoFactory`
**File:** `tests/Fixtures/Factories/Content/ShortVideoFactory.cs`

```csharp
ShortVideoFactory.Create()                          // Standalone short video
ShortVideoFactory.CreateStandalone()
ShortVideoFactory.CreateTeaser(Guid videoId)        // Teaser linked to a video
ShortVideoFactory.CreateInactive()
ShortVideoFactory.CreateWithId(Guid id)
ShortVideoFactory.CreateMany(int count)
```

### `ContentOrderFactory`
**File:** `tests/Fixtures/Factories/Content/ContentOrderFactory.cs`

```csharp
ContentOrderFactory.Create()                        // Draft order
ContentOrderFactory.Create(Guid customerId)
ContentOrderFactory.CreateWithId(Guid id)
ContentOrderFactory.CreateSubmitted()               // Submitted (PendingPayment)
ContentOrderFactory.CreateSubmitted(Guid customerId)
ContentOrderFactory.CreatePaid()                    // Paid
ContentOrderFactory.CreatePaid(Guid customerId)
ContentOrderFactory.CreateCancelled()
ContentOrderFactory.CreateForCustomer(Guid customerId)
ContentOrderFactory.CreateMany(int count)
```

### `ContentOrderItemFactory`
**File:** `tests/Fixtures/Factories/Content/ContentOrderItemFactory.cs`

```csharp
ContentOrderItemFactory.Create(Guid orderId, Guid categoryId)
ContentOrderItemFactory.CreateWithId(Guid id, Guid orderId, Guid categoryId)
ContentOrderItemFactory.CreateWithPromotion(Guid orderId, Guid categoryId, Guid promotionLevelId, decimal priceSnapshot)
ContentOrderItemFactory.CreateSocialBoost(Guid orderId, Guid categoryId)
ContentOrderItemFactory.CreateBonus(Guid orderId, Guid categoryId)
ContentOrderItemFactory.CreateMany(int count, Guid orderId, Guid categoryId)
```

### `ContentPaymentFactory`
**File:** `tests/Fixtures/Factories/Content/ContentPaymentFactory.cs`

```csharp
ContentPaymentFactory.Create(Guid orderId)          // Pending payment
ContentPaymentFactory.CreateDefault(Guid orderId)
ContentPaymentFactory.CreateWithProof(Guid orderId, Guid proofFileId)
ContentPaymentFactory.CreateVerified(Guid orderId, Guid adminUserId)
ContentPaymentFactory.CreateRejected(Guid orderId, string? notes = null)
ContentPaymentFactory.CreateWithId(Guid id, Guid orderId)
```

### `ContentItemTierFactory`
**File:** `tests/Fixtures/Factories/Content/ContentItemTierFactory.cs`

```csharp
ContentItemTierFactory.Create(Guid orderItemId, Guid pricingTierId)
ContentItemTierFactory.CreateWithId(Guid id, Guid orderItemId, Guid pricingTierId)
ContentItemTierFactory.CreateWithPrice(Guid orderItemId, Guid pricingTierId, decimal priceSnapshotUsd)
```

### `CategoryFactory`
**File:** `tests/Fixtures/Factories/Content/CategoryFactory.cs`

```csharp
CategoryFactory.Create(Guid contentTypeId)
CategoryFactory.CreateWithId(Guid id, Guid contentTypeId)
CategoryFactory.CreateInactive(Guid contentTypeId)
CategoryFactory.CreateFree(Guid contentTypeId)      // IsFree = true
CategoryFactory.CreatePaid(Guid contentTypeId)      // IsFree = false
CategoryFactory.CreateMany(int count, Guid contentTypeId)
```

### `CategoryPricingFactory`
**File:** `tests/Fixtures/Factories/Content/CategoryPricingFactory.cs`

```csharp
CategoryPricingFactory.Create(Guid categoryId, Guid pricingTierId)
CategoryPricingFactory.CreateWithId(Guid id, Guid categoryId, Guid pricingTierId)
CategoryPricingFactory.CreateWithPrice(Guid categoryId, Guid pricingTierId, decimal priceUsd)
```

### `PricingTierFactory`
**File:** `tests/Fixtures/Factories/Content/PricingTierFactory.cs`

```csharp
PricingTierFactory.Create()
PricingTierFactory.CreateWithId(Guid id)
PricingTierFactory.CreateInactive()
PricingTierFactory.CreateMany(int count)
```

### `PromotionLevelFactory`
**File:** `tests/Fixtures/Factories/Content/PromotionLevelFactory.cs`

```csharp
PromotionLevelFactory.Create()
PromotionLevelFactory.CreateWithId(Guid id)
PromotionLevelFactory.CreateInactive()
PromotionLevelFactory.CreateMany(int count)
```

### `CustomerFactory`
**File:** `tests/Fixtures/Factories/Content/CustomerFactory.cs`

```csharp
CustomerFactory.Create()
CustomerFactory.CreateWithId(Guid id)
CustomerFactory.CreateWithEmail(string email)
CustomerFactory.CreateMany(int count)
```

### `ContentTypeFactory`
**File:** `tests/Fixtures/Factories/Content/ContentTypeFactory.cs`

```csharp
ContentTypeFactory.Create()
ContentTypeFactory.CreateWithId(Guid id)
ContentTypeFactory.CreateInactive()
ContentTypeFactory.CreateMany(int count)
```

### `TagFactory`
**File:** `tests/Fixtures/Factories/Content/TagFactory.cs`

```csharp
TagFactory.Create()
TagFactory.CreateWithId(Guid id)
TagFactory.CreateMany(int count)
```

### `PackageFactory`
**File:** `tests/Fixtures/Factories/Content/PackageFactory.cs`

```csharp
PackageFactory.Create()
PackageFactory.CreateWithId(Guid id)
PackageFactory.CreateInactive()
PackageFactory.CreateMany(int count)
```

### `ArticleImageFactory`
**File:** `tests/Fixtures/Factories/Content/ArticleImageFactory.cs`

```csharp
ArticleImageFactory.Create(Guid articleId)                                          // Body image, defaults
ArticleImageFactory.CreateCover(Guid articleId)                                     // Cover image
ArticleImageFactory.CreateBody(Guid articleId)                                      // Body image
ArticleImageFactory.CreateCover(Guid articleId, string storageKey, string url)      // Custom cover
ArticleImageFactory.CreateBody(Guid articleId, string storageKey, string url)       // Custom body
ArticleImageFactory.CreateMany(Guid articleId, int count)                           // Numbered keys/URLs
ArticleImageFactory.CreateFromStorageKeys(Guid articleId, IEnumerable<string> keys) // From keys
```

### `PackageSlotFactory`
**File:** `tests/Fixtures/Factories/Content/PackageSlotFactory.cs`

```csharp
PackageSlotFactory.Create(Guid packageId)                   // Default random slot
PackageSlotFactory.Create(Guid packageId, Guid categoryId)  // With specific category
PackageSlotFactory.CreateOpen(Guid packageId)               // Open slot (no category)
PackageSlotFactory.CreateRequired(Guid packageId)           // Required slot
PackageSlotFactory.CreateOptional(Guid packageId)           // Non-required slot
```

### `LyricsFactory`
**File:** `tests/Fixtures/Factories/Content/LyricsFactory.cs`

```csharp
LyricsFactory.Create()
LyricsFactory.CreateWithId(Guid id)
LyricsFactory.CreateMany(int count)
```
