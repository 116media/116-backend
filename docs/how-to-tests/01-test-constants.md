# Test Constants Reference

**File:** `tests/Fixtures/Constants/TestConstants.cs`

`TestConstants` is a static class with nested static classes for each domain. Always use these instead of inline hardcoded strings to keep tests consistent and easy to update.

---

## Rule

```csharp
// Wrong — hardcoded inline
var command = new AdminCreateRoleCommand("Admin", "Some description");

// Correct — use TestConstants
var command = new AdminCreateRoleCommand(
    TestConstants.Role.ValidName,
    TestConstants.Role.ValidDescription
);
```

---

## `TestConstants.Role`

| Constant | Value / Type | Purpose |
|----------|-------------|---------|
| `NameMaxLength` | `int` (20) | Max length for role name |
| `DescriptionMaxLength` | `int` (300) | Max length for role description |
| `ValidName` | `string` | Valid role name for happy-path tests |
| `ValidDescription` | `string` | Valid role description for happy-path tests |
| `SuperAdminName` | `string` | "SuperAdmin" role name |
| `SuperAdminDescription` | `string` | SuperAdmin description |
| `AdminName` | `string` | "Admin" role name |
| `AdminDescription` | `string` | Admin description |
| `VisitorName` | `string` | "Visitor" role name |
| `VisitorDescription` | `string` | Visitor description |

---

## `TestConstants.Permission`

| Constant | Value / Type | Purpose |
|----------|-------------|---------|
| `ResourceMaxLength` | `int` (15) | Max length for resource field |
| `ActionMaxLength` | `int` (15) | Max length for action field |
| `DescriptionMaxLength` | `int` (300) | Max length for description |

---

## `TestConstants.User`

| Constant | Value / Type | Purpose |
|----------|-------------|---------|
| `EmailMaxLength` | `int` (256) | Max length for email |
| `PasswordMinLength` | `int` (8) | Min length for password |
| `DefaultPasswordHash` | `string` | Bcrypt hash for use in tests |

---

## `TestConstants.Session`

| Constant | Value / Type | Purpose |
|----------|-------------|---------|
| `DeviceIdMaxLength` | `int` (256) | Max device ID length |
| `IpAddressMaxLength` | `int` (45) | Max IP address length |
| `DefaultIpAddress` | `string` | Default IP for session tests |
| `DefaultUserAgent` | `string` | Default user agent string |
| `DefaultBrowser` | `EnumBrowser` | Default browser enum value |
| `DefaultDevice` | `EnumDevice` | Default device enum value |
| `DefaultPlatform` | `EnumPlatform` | Default platform enum value |
| `DefaultClient` | `EnumClient` | Default client enum value |

---

## `TestConstants.Otp`

| Constant | Value / Type | Purpose |
|----------|-------------|---------|
| `CodeLength` | `int` (6) | OTP code length |
| `MaxAttempts` | `int` (5) | Max validation attempts |
| `ExpirationMinutes` | `int` (10) | OTP TTL |
| `ValidCode` | `string` | Valid OTP code for happy-path |
| `InvalidCode` | `string` | Invalid/wrong OTP code |
| `DefaultCode` | `string` | Default code used in mock setups |

---

## `TestConstants.File`

| Constant | Value / Type | Purpose |
|----------|-------------|---------|
| `FileNameMaxLength` | `int` (255) | Max file name length |
| `MimeTypeMaxLength` | `int` (100) | Max MIME type length |
| `ValidFileName` | `string` | Valid file name |
| `ValidStorageUrl` | `string` | Valid Cloudinary storage URL |

---

## `TestConstants.Jwt`

| Constant | Value / Type | Purpose |
|----------|-------------|---------|
| `ValidSecret` | `string` | 32+ char JWT signing secret |
| `ValidIssuer` | `string` | JWT issuer |
| `ValidAudience` | `string` | JWT audience |
| `AccessTokenExpirationMinutes` | `int` (60) | Access token TTL |
| `RefreshTokenExpirationDays` | `int` (30) | Refresh token TTL |

---

## `TestConstants.Content`

Nested classes for all Content module domains.

### `TestConstants.Content.ContentType`
Constants for content type names, descriptions, slugs.

### `TestConstants.Content.PricingTier`
Constants for pricing tier names, descriptions.

### `TestConstants.Content.PromotionLevel`
Constants for promotion level names, duration days, price values.

### `TestConstants.Content.Tag`
Constants for tag names and slugs.

### `TestConstants.Content.Category`
Constants for category names, slugs, descriptions.

### `TestConstants.Content.Customer`
Constants for customer names, emails.

### `TestConstants.Content.Package`
Constants for package names, descriptions, price values.

### `TestConstants.Content.PackageSlot`
Constants for package slot configurations.

### `TestConstants.Content.CategoryPricing`
Constants for category pricing values.

### `TestConstants.Content.Commerce`

| Constant | Purpose |
|----------|---------|
| `ValidReceiptUrl` | Receipt URL for payment verification tests |

### `TestConstants.Content.Editorial.Article`

| Constant | Purpose |
|----------|---------|
| `ValidTitle` | Valid article title for happy-path tests |
| `ValidSlug` | Valid article slug |
| `ValidRejectionReason` | Valid reason string for rejection tests |

### `TestConstants.Content.Editorial.Video`
Same as Article but for videos.

### `TestConstants.Content.Editorial.ShortVideo`
Constants for short video titles, slugs.

### `TestConstants.Content.Editorial.Lyrics`
Constants for lyrics song title, artist name, content.

### `TestConstants.Content.Editorial.ArticleImage`
Constants for article image URLs and storage keys.

### `TestConstants.Content.Editorial.Cloudinary`
Constants for Cloudinary upload results used in mock setups.

---

## `TestConstants.ApiRoutes`

| Constant | Value |
|----------|-------|
| Admin base path | `/api/v1/admin/...` |
| Public base path | `/api/v1/public/...` |

Used for endpoint route tests.
