# Builders Reference

Builders construct individual entities with full control over every field via fluent chaining. They are the lowest level of test data creation. You almost never use builders directly in tests — use factories instead.

---

## What a Builder Is

- A class that holds private mutable fields initialized with sensible defaults (often via Bogus Faker)
- Every field has a corresponding `With*()` fluent method that returns `this`
- A `Build()` method calls the entity's factory method and applies state changes

```csharp
// Pattern
var role = new RoleBuilder()
    .WithName("Editor")
    .WithDescription("Can edit articles")
    .AsInactive()
    .Build();
```

---

## Bogus Faker Usage

All string/numeric default values that are not structurally required use Bogus:

```csharp
private readonly Faker _faker = new();

// In constructor or field initializers:
private string _name = _faker.Lorem.Word();
private string _description = _faker.Lorem.Sentence(wordCount: 5);
private string _email = _faker.Internet.Email();
private string _deviceId = _faker.Random.AlphaNumeric(16);
private string _action = _faker.PickRandom("read", "create", "update", "delete", "approve");
private int _number = _faker.Random.Number(100000, 999999);
```

**Rule:** Use Bogus for values that do not affect test logic. Use `TestConstants` or explicit values for values that the test asserts against.

---

## Navigation Properties via Reflection

When an entity needs a navigation property set (EF Core does not expose a setter), builders use reflection:

```csharp
private static void SetNavigationProperty<T>(object entity, string propertyName, T value)
{
    var property = entity.GetType()
        .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    property?.SetValue(entity, value);
}

// Usage in Build():
SetNavigationProperty(userRole, "Role", _role);
```

---

## Identity Builders

### `UserBuilder`
**File:** `tests/Fixtures/Builders/Entities/UserBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithEmail(string email)` | Sets email |
| `WithUserName(string userName)` | Sets username |
| `WithPasswordHash(string hash)` | Sets password hash |
| `WithAuthProvider(EnumAuthProvider provider)` | Switches between local/external creation |
| `AsVerified()` | Marks email verified |
| `AsUnverified()` | Leaves email unverified (default) |
| `AsActive()` | Sets account active |
| `AsInactive()` | Sets account inactive |
| `WithRole(RoleEntity role)` | Assigns role via reflection |
| `WithPhoneNumber(string full, string partial)` | Sets phone number fields |
| `Build()` | Calls `UserEntity.Create()` or `UserEntity.CreateExternal()` based on auth provider |

### `RoleBuilder`
**File:** `tests/Fixtures/Builders/Entities/RoleBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithName(string name)` | Sets name (default: `_faker.Lorem.Word()`) |
| `WithDescription(string description)` | Sets description |
| `AsInactive()` | Calls `Deactivate()` on built entity |
| `AsActive()` | Default — active |
| `AsDeleted()` | Calls `SoftDelete()` on built entity |
| `WithPermission(PermissionEntity p)` | Adds single permission |
| `WithPermissions(IEnumerable<PermissionEntity> ps)` | Adds multiple permissions |
| `Build()` | Calls `RoleEntity.Create()`, applies state, adds permissions via `RolePermissionFactory` |

### `PermissionBuilder`
**File:** `tests/Fixtures/Builders/Entities/PermissionBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithResource(string resource)` | Sets resource (default: random word) |
| `WithAction(string action)` | Sets action (default: random from read/create/update/delete/approve) |
| `WithDescription(string description)` | Sets description |
| `WithResourceAction(string resource, string action)` | Sets both in one call |
| `AsInactive()` | Calls `Deactivate()` |
| `AsDeleted()` | Calls `SoftDelete()` |
| `Build()` | Calls `PermissionEntity.Create()`, applies state |

### `SessionBuilder`
**File:** `tests/Fixtures/Builders/Entities/SessionBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithUserId(Guid userId)` | Sets user ID |
| `WithDeviceId(string deviceId)` | Sets device ID |
| `WithRefreshTokenHash(string hash)` | Sets refresh token hash |
| `WithExpiresAt(DateTime expiresAt)` | Sets expiration |
| `AsExpired()` | Sets expiresAt to 1 day in the past |
| `WithBrowser(EnumBrowser browser)` | Sets browser enum |
| `WithDevice(EnumDevice device)` | Sets device enum |
| `WithPlatform(EnumPlatform platform)` | Sets platform enum |
| `WithClient(EnumClient client)` | Sets client enum |
| `WithIpAddress(string? ip)` | Sets IP address |
| `WithUserAgent(string? ua)` | Sets user agent |
| `AsRevoked()` | Calls `Revoke()` on built entity |
| `AsMobileSession()` | Preset: device=Mobile, client=MobileApp, platform=Ios, browser=Safari |
| `AsDesktopSession()` | Preset: device=Desktop, client=WebApp, platform=Windows, browser=Chrome |
| `Build()` | Calls `SessionEntity.Create()`, applies revoke if needed |

### `OtpBuilder`
**File:** `tests/Fixtures/Builders/Entities/OtpBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithUserId(Guid userId)` | Sets user ID |
| `WithCode(string code)` | Sets OTP code |
| `WithPurpose(EnumOtpPurpose purpose)` | Sets purpose |
| `ForEmailVerification()` | Sets purpose = EmailVerification |
| `ForPasswordReset()` | Sets purpose = PasswordReset |
| `WithExpiresAt(DateTime expiresAt)` | Sets expiration |
| `AsExpired()` | Sets expiresAt to past |
| `WithAttemptCount(int count)` | Increments attempts count times |
| `AsMaxAttemptsReached()` | Sets attempts to MaxAttempts |
| `AsUsed()` | Marks OTP as used |
| `Build()` | Calls `OtpEntity.Create()`, applies state |

### `FileBuilder`
**File:** `tests/Fixtures/Builders/Entities/FileBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithFileName(string fileName)` | Sets storage file name |
| `WithOriginalFileName(string name)` | Sets original file name |
| `WithMimeType(string mimeType)` | Sets MIME type |
| `WithStorageUrl(string url)` | Sets Cloudinary URL |
| `WithSizeInBytes(long size)` | Sets file size |
| `AsDeleted()` | Marks as deleted |
| `AsJpegImage()` | Sets mime type `image/jpeg`, extension `.jpg` |
| `AsPngImage()` | Sets mime type `image/png`, extension `.png` |
| `AsPdfDocument()` | Sets mime type `application/pdf`, extension `.pdf` |
| `Build()` | Calls `FileEntity.Create()`, deletes if marked |

### `RolePermissionBuilder`
**File:** `tests/Fixtures/Builders/Entities/RolePermissionBuilder.cs`

| Method | Effect |
|--------|--------|
| `ForRoleAndPermission(Guid roleId, Guid permissionId)` | Sets both IDs in one call |
| `WithPermission(PermissionEntity p)` | Sets permissionId + navigation property via reflection |
| `Build()` | Calls `RolePermissionEntity.Create()`, sets navigation via reflection |

### `UserRoleBuilder`
**File:** `tests/Fixtures/Builders/Entities/UserRoleBuilder.cs`

| Method | Effect |
|--------|--------|
| `ForUserAndRole(Guid userId, Guid roleId)` | Sets both IDs |
| `WithRole(RoleEntity role)` | Sets roleId + navigation property via reflection |
| `Build()` | Calls `UserRoleEntity.Create()`, sets navigation via reflection |

---

## Content Builders

### `ArticleBuilder`
**File:** `tests/Fixtures/Builders/Entities/Content/ArticleBuilder.cs`

**Constructor:** Requires `Guid categoryId`.

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithCategoryId(Guid id)` | Sets category |
| `WithTitle(string title)` | Sets title |
| `WithSlug(string slug)` | Sets slug |
| `WithAuthorId(Guid authorId)` | Sets author |
| `WithCustomer(Guid customerId, Guid orderItemId)` | Switches to paid article mode |
| `AsPendingPayment()` | Target status: PendingPayment |
| `AsPendingReview()` | Target status: PendingReview |
| `AsApproved()` | Target status: Approved |
| `AsPublished()` | Target status: Published |
| `AsRejected(string? reason)` | Target status: Rejected |
| `AsArchived()` | Target status: Archived |
| `WithSocialBoost()` | Calls `StampSocialBoost()` |
| `AsPromoted(DateTimeOffset until)` | Calls `StampPromotion(until)` |
| `Build()` | Calls `ArticleEntity.CreatePaid()` or `CreateFree()`, applies status chain |

### `ContentOrderBuilder`
**File:** `tests/Fixtures/Builders/Entities/Content/ContentOrderBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithCustomerId(Guid id)` | Sets customer ID |
| `WithPackageId(Guid? id)` | Sets optional package ID |
| `AsSubmitted()` | Calls `Submit()` in `Build()` |
| `AsPaid()` | Calls `Submit()` then `MarkPaid()` |
| `AsCancelled()` | Calls `Cancel()` |
| `Build()` | Creates order, applies state chain |

### `ContentOrderItemBuilder`
**File:** `tests/Fixtures/Builders/Entities/Content/ContentOrderItemBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithOrderId(Guid orderId)` | Sets parent order |
| `WithContentKind(EnumCoreContentType kind)` | Sets content type |
| `WithCategoryId(Guid id)` | Sets category |
| `WithPromotionLevelId(Guid id, decimal priceSnapshot)` | Sets promotion level and price snapshot |
| `AsSocialBoost()` | Sets social boost flag |
| `AsBonus()` | Sets bonus flag |
| `Build()` | Calls `ContentOrderItemEntity.Create()` |

### `ContentPaymentBuilder`
**File:** `tests/Fixtures/Builders/Entities/Content/ContentPaymentBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithOrderId(Guid orderId)` | Sets parent order |
| `WithAmountUsd(decimal amount)` | Sets payment amount |
| `WithProofFileId(Guid fileId, EnumPaymentMethod method)` | Calls `AttachProof()` in `Build()` |
| `AsVerified(Guid adminUserId, string receiptUrl)` | Calls `Verify()` in `Build()` |
| `AsRejected(string? notes)` | Calls `Reject()` in `Build()` |
| `Build()` | Creates payment, applies state |

### `ContentItemTierBuilder`
**File:** `tests/Fixtures/Builders/Entities/Content/ContentItemTierBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithId(Guid id)` | Sets entity ID |
| `WithOrderItemId(Guid id)` | Sets parent order item |
| `WithPricingTierId(Guid id)` | Sets pricing tier |
| `WithPriceSnapshotUsd(decimal price)` | Sets price snapshot |
| `Build()` | Calls `ContentItemTierEntity.Create()` |

---

## `AuthDataBuilder`
**File:** `tests/Fixtures/Builders/AuthDataBuilder.cs`

Builds auth data structures returned by login/auth factories.

```csharp
// Constructor options
new AuthDataBuilder()                        // Random verified active user
new AuthDataBuilder(UserEntity user)         // Specific user
```

| Method | Effect |
|--------|--------|
| `WithUser(UserEntity user)` | Sets the user |
| `WithUserPermissions(List<RolePermissionEntity> permissions)` | Replaces all permissions |
| `WithUserPermission(RolePermissionEntity permission)` | Adds single permission |
| `BuildPublicLoginAuthData()` | Returns `PublicLoginAuthData` |
| `BuildPublicSocialLoginAuthData()` | Returns `PublicSocialLoginAuthData` |
| `BuildAdminLoginAuthData()` | Returns `AdminLoginAuthData` |

---

## Command Builders

### `CreateRoleCommandBuilder`
**File:** `tests/Fixtures/Builders/Commands/Roles/CreateRoleCommandBuilder.cs`

| Method | Effect |
|--------|--------|
| `WithName(string name)` | Sets role name |
| `WithDescription(string description)` | Sets description |
| `WithValidData()` | Uses `TestConstants.Role` values |
| `WithEmptyName()` | Sets name to `""` |
| `WithEmptyDescription()` | Sets description to `""` |
| `WithNameExceedingMaxLength()` | String longer than `TestConstants.Role.NameMaxLength` |
| `WithDescriptionExceedingMaxLength()` | String longer than max |
| `Build()` | Returns `AdminCreateRoleCommand(_name, _description)` |

### `UpdateRoleCommandBuilder`
**File:** `tests/Fixtures/Builders/Commands/Roles/UpdateRoleCommandBuilder.cs`
Same pattern as Create. Also has `WithRoleId(Guid)`, `WithoutName()` (null), `WithoutDescription()` (null).

### `CreatePermissionCommandBuilder`
**File:** `tests/Fixtures/Builders/Commands/Roles/CreatePermissionCommandBuilder.cs`
Fields: `_resource`, `_action`, `_description`. Methods: `WithResource`, `WithAction`, `WithDescription`, `WithEmptyResource`, `WithResourceExceedingMaxLength`, etc.

### `UpdatePermissionCommandBuilder`
**File:** `tests/Fixtures/Builders/Commands/Roles/UpdatePermissionCommandBuilder.cs`
Same as Create with nullable fields and `WithoutResource()`, `WithoutAction()`, `WithoutDescription()`.
