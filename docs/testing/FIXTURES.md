# Test Fixtures — Builders and Factories

## Overview

Test fixtures are organized in two layers:
- **Builders** — Internal fluent builders for fine-grained control (located in `tests/Fixtures/Builders/`)
- **Factories** — Public static factories for quick entity creation in tests (located in `tests/Fixtures/Factories/`)

Prefer Factories in test code. Use Builders only when you need precise configuration.

---

## Content Builders

All Content builders live in `tests/Fixtures/Builders/Entities/Content/`.

### ContentTypeBuilder (internal)

File: `ContentTypeBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithName(string name)` | Set the name |
| `AsInactive()` | Mark as inactive (calls `Deactivate()`) |
| `Build()` | Returns `ContentTypeEntity` |

Default: random Guid, random word (truncated to 30 chars), IsActive = true

### PricingTierBuilder (internal)

File: `PricingTierBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithName(string name)` | Set the name |
| `WithDescription(string? description)` | Set the description |
| `AsInactive()` | Mark as inactive |
| `Build()` | Returns `PricingTierEntity` |

Default: random Guid, random word (truncated to 40 chars), no description, IsActive = true

### PromotionLevelBuilder (internal)

File: `PromotionLevelBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithName(string name)` | Set the name |
| `WithDurationDays(int days)` | Set duration days |
| `WithPriceUsd(decimal price)` | Set price in USD |
| `AsInactive()` | Mark as inactive |
| `Build()` | Returns `PromotionLevelEntity` |

Default: random Guid, random name, random duration (1–30), random price, IsActive = true

### TagBuilder (internal)

File: `TagBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithName(string name)` | Set the name |
| `WithSlug(string slug)` | Set the slug |
| `Build()` | Returns `TagEntity` |

Default: random Guid, random name and slug

### CategoryBuilder (internal)

File: `CategoryBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithName(string name)` | Set the name |
| `WithSlug(string slug)` | Set the slug |
| `WithDescription(string? description)` | Set the description |
| `AsInactive()` | Mark as inactive |
| `Build()` | Returns `CategoryEntity` |

### CategoryPricingBuilder (internal)

File: `CategoryPricingBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithCategoryId(Guid categoryId)` | Set the category ID |
| `WithPricingTierId(Guid pricingTierId)` | Set the pricing tier ID |
| `WithPriceUsd(decimal price)` | Set the price |
| `Build()` | Returns `CategoryPricingEntity` |

### CustomerBuilder (internal)

File: `CustomerBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithFullName(string name)` | Set the full name |
| `WithEmail(string email)` | Set the email |
| `WithPhone(string? phone)` | Set the phone |
| `WithCompany(string? company)` | Set the company |
| `WithNotes(string? notes)` | Set notes |
| `Build()` | Returns `CustomerEntity` |

### PackageBuilder (internal)

File: `PackageBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithCustomerId(Guid customerId)` | Set the customer ID |
| `WithName(string name)` | Set the name |
| `WithDescription(string? description)` | Set the description |
| `WithFlatPriceUsd(decimal price)` | Set the flat price |
| `Build()` | Returns `PackageEntity` |

### PackageSlotBuilder (internal)

File: `PackageSlotBuilder.cs`

| Method | Description |
|--------|-------------|
| `WithId(Guid id)` | Set a specific ID |
| `WithPackageId(Guid packageId)` | Set the package ID |
| `WithCategoryPricingId(Guid categoryPricingId)` | Set the category pricing ID |
| `WithQuantity(int quantity)` | Set the quantity |
| `Build()` | Returns `PackageSlotEntity` |

---

## Content Factories

All Content factories live in `tests/Fixtures/Factories/Content/`.

### ContentTypeFactory

File: `ContentTypeFactory.cs`

| Method | Description |
|--------|-------------|
| `Create()` | Random defaults |
| `Create(string name)` | With specific name |
| `CreateWithId(Guid id)` | With specific ID |
| `CreateInactive()` | Inactive entity |
| `CreateDefault()` | Uses `TestConstants.Content.ContentType.ValidName` |
| `CreateMany(int count)` | List of `count` entities |

### PricingTierFactory

File: `PricingTierFactory.cs`

| Method | Description |
|--------|-------------|
| `Create()` | Random defaults |
| `Create(string name)` | With specific name |
| `CreateWithId(Guid id)` | With specific ID |
| `CreateInactive()` | Inactive entity |
| `CreateWithDescription(string name, string description)` | With name and description |
| `CreateDefault()` | Uses `TestConstants.Content.PricingTier.ValidName` |
| `CreateMany(int count)` | List of `count` entities |

### PromotionLevelFactory

File: `PromotionLevelFactory.cs`

| Method | Description |
|--------|-------------|
| `Create()` | Random defaults |
| `Create(string name, int durationDays, decimal priceUsd)` | With specific values |
| `CreateWithId(Guid id)` | With specific ID |
| `CreateInactive()` | Inactive entity |
| `CreateDefault()` | Uses `ValidName`, `ValidDurationDays`, `ValidPriceUsd` from TestConstants |
| `CreateMany(int count)` | List of `count` entities |

### TagFactory

File: `TagFactory.cs`

| Method | Description |
|--------|-------------|
| `Create()` | Random defaults |
| `Create(string name, string slug)` | With specific name and slug |
| `CreateWithId(Guid id)` | With specific ID |
| `CreateDefault()` | Uses `TestConstants.Content.Tag.ValidName` and `ValidSlug` |
| `CreateMany(int count)` | List of `count` entities |

### CategoryFactory

File: `CategoryFactory.cs`

| Method | Description |
|--------|-------------|
| `Create()` | Random defaults |
| `Create(string name, string slug)` | With specific name and slug |
| `CreateWithId(Guid id)` | With specific ID |
| `CreateInactive()` | Inactive entity |
| `CreateDefault()` | Uses `TestConstants.Content.Category.ValidName` and `ValidSlug` |
| `CreateMany(int count)` | List of `count` entities |

### CategoryPricingFactory

File: `CategoryPricingFactory.cs`

| Method | Description |
|--------|-------------|
| `Create(Guid categoryId, Guid pricingTierId)` | With category and pricing tier IDs |
| `CreateDefault()` | Uses random IDs with default price |

### CustomerFactory

File: `CustomerFactory.cs`

| Method | Description |
|--------|-------------|
| `Create()` | Random defaults |
| `CreateDefault()` | Uses `TestConstants.Content.Customer` values |
| `CreateWithId(Guid id)` | With specific ID |
| `CreateMany(int count)` | List of `count` entities |

### PackageFactory

File: `PackageFactory.cs`

| Method | Description |
|--------|-------------|
| `Create(Guid customerId)` | With specific customer ID |
| `CreateDefault(Guid customerId)` | Uses `TestConstants.Content.Package` values |
| `CreateWithId(Guid id, Guid customerId)` | With specific ID and customer |
| `CreateMany(int count, Guid customerId)` | List of `count` entities |

### PackageSlotFactory

File: `PackageSlotFactory.cs`

| Method | Description |
|--------|-------------|
| `Create(Guid packageId, Guid categoryPricingId)` | With package and category pricing IDs |
| `CreateDefault(Guid packageId, Guid categoryPricingId)` | With default quantity |

---

## TestConstants

File: `tests/Fixtures/Constants/TestConstants.cs`

Key content module constants under `TestConstants.Content`:

| Class | Key Constants |
|-------|---------------|
| `ContentType` | `ValidName = "Article"`, `AnotherValidName = "Video"`, `NameMaxLength = 30` |
| `PricingTier` | `ValidName = "base_upload"`, `AnotherValidName = "social_boost"`, `NameMaxLength = 40`, `DescriptionMaxLength = 200` |
| `PromotionLevel` | `ValidName = "Featured — 7 days"`, `ValidDurationDays = 7`, `ValidPriceUsd = 50m`, `ZeroPriceUsd = 0m`, `NameMaxLength = 40` |
| `Tag` | `ValidName = "Fally Ipupa"`, `ValidSlug = "fally-ipupa"`, `NameMaxLength = 50`, `SlugMaxLength = 60` |
| `Category` | `ValidName = "Artist Profile"`, `ValidSlug = "artist-profile"`, `NameMaxLength = 60`, `SlugMaxLength = 80` |
| `Customer` | `ValidFullName = "John Doe"`, `ValidEmail = "customer@example.com"`, `EmailMaxLength = 200` |
| `Package` | `ValidName = "Artist Starter Pack"`, `ValidFlatPriceUsd = 300m`, `NameMaxLength = 100` |
