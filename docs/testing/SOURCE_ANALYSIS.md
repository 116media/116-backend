# Content Module — Source File Analysis

Complete analysis of every source file relevant to unit tests.

---

## Domain Constants

**`ContentConstants.cs`**
- SchemaName = "content", ModuleName = "Content", Admin = "admin", Public = "public"
- MaxContentTypeNameLength = 30
- MaxPricingTierNameLength = 40, MaxPricingTierDescriptionLength = 200
- MaxPromotionLevelNameLength = 40
- MaxTagNameLength = 50, MaxTagSlugLength = 60
- MaxCategoryNameLength = 60, MaxCategorySlugLength = 80, MaxCategoryDescriptionLength = 300
- MaxCustomerFullNameLength = 100, MaxCustomerEmailLength = 200, MaxCustomerPhoneLength = 30
- MaxCustomerCompanyLength = 100, MaxCustomerNotesLength = 500
- MaxPackageNameLength = 100, MaxPackageDescriptionLength = 500

---

## Domain Entities

### `ContentTypeEntity`
- Properties: Id (Guid), Name (string), IsActive (bool, default=true)
- `Create(Guid id, string name)` → throws `BadRequestException` if name is null/empty/whitespace
- `Update(string name)` → throws `BadRequestException` if name is null/empty/whitespace
- `Activate()` → returns false if already active, sets IsActive=true, returns true
- `Deactivate()` → returns false if already inactive, sets IsActive=false, returns true

### `PricingTierEntity`
- Properties: Id (Guid), Name (string), Description (string?), IsActive (bool, default=true)
- `Create(Guid id, string name, string? description)` → throws `BadRequestException` if name null/empty
- `Update(string name, string? description)` → throws `BadRequestException` if name null/empty
- `Activate()` / `Deactivate()` → same as ContentType

### `PromotionLevelEntity`
- Properties: Id (Guid), Name (string), DurationDays (int), PriceUsd (decimal), IsActive (bool)
- `Create(Guid id, string name, int durationDays, decimal priceUsd)`
  - throws `BadRequestException` (NameRequired) if name null/empty
  - throws `BadRequestException` (DurationMustBePositive) if durationDays <= 0
  - throws `BadRequestException` (PriceMustBeNonNegative) if priceUsd < 0
- `Update(string name, int durationDays, decimal priceUsd)` — same validations
- `Activate()` / `Deactivate()` — same pattern

### `TagEntity`
- Properties: Id (Guid), Name (string), Slug (string)
- `Create(Guid id, string name, string slug)`
  - throws `BadRequestException` (NameRequired) if name null/empty
  - throws `BadRequestException` (SlugRequired) if slug null/empty
- No Activate/Deactivate — tags are never deactivated, just deleted

### `CategoryEntity`
- Properties: Id, ContentTypeId, Name, Slug, Description?, IsFree, IsActive (default=true)
- Navigation: ContentType, Pricing (collection), PackageSlots (collection)
- `Create(Guid id, Guid contentTypeId, string name, string slug, string? description, bool isFree)`
  - throws `BadRequestException` (NameRequired) if name null/empty
  - throws `BadRequestException` (SlugRequired) if slug null/empty
- `Update(string name, string slug, string? description)` — same validations
- `Activate()` / `Deactivate()` — same pattern

### `CategoryPricingEntity`
- Properties: Id, CategoryId, PricingTierId, PriceUsd
- Navigation: Category, PricingTier
- `Create(Guid id, Guid categoryId, Guid pricingTierId, decimal priceUsd)`
  - throws `BadRequestException` (PriceMustBeNonNegative) if priceUsd < 0
- `UpdatePrice(decimal priceUsd)` — same validation

### `CustomerEntity`
- Properties: Id, FullName, Email, Phone?, Company?, Notes?
- `Create(Guid id, string fullName, string email, string? phone, string? company, string? notes)`
  - throws `BadRequestException` (FullNameRequired) if fullName null/empty
  - throws `BadRequestException` (EmailRequired) if email null/empty
- `Update(string fullName, string? phone, string? company, string? notes)`
  - throws `BadRequestException` if fullName null/empty
  - Email is NOT updatable

### `PackageEntity`
- Properties: Id, Name, Description?, FlatPriceUsd, IsActive (default=true)
- Navigation: Slots (collection)
- `Create(Guid id, string name, string? description, decimal flatPriceUsd)`
  - throws `BadRequestException` (NameRequired) if name null/empty
  - throws `BadRequestException` (PriceMustBeNonNegative) if flatPriceUsd < 0
- `Activate()` / `Deactivate()` — same pattern

### `PackageSlotEntity`
- Properties: Id, PackageId, CategoryId?, IsRequired, Quantity
- Navigation: Package, Category?
- `Create(Guid id, Guid packageId, Guid? categoryId, bool isRequired, int quantity)`
  - throws `BadRequestException` (SlotQuantityMustBePositive) if quantity <= 0

---

## Error Factories

### `ContentTypeErrors`
- `AlreadyExists(string name)` → ConflictException
- `NotFound(Guid id)` → NotFoundException("ContentType", "id", id)
- `AlreadyActive()` → ConflictException
- `AlreadyInactive()` → ConflictException
- `NameRequired()` → BadRequestException

### `PricingTierErrors`
- `AlreadyExists(string name)` → ConflictException
- `NotFound(Guid id)` → NotFoundException("PricingTier", "id", id)
- `AlreadyActive()` → ConflictException
- `AlreadyInactive()` → ConflictException
- `IsInactive()` → BadRequestException (used in AddCategoryPricing)
- `NameRequired()` → BadRequestException

### `PromotionLevelErrors`
- `AlreadyExists(string name)` → ConflictException
- `NotFound(Guid id)` → NotFoundException("PromotionLevel", "id", id)
- `AlreadyActive()` / `AlreadyInactive()` → ConflictException
- `NameRequired()` / `DurationMustBePositive()` / `PriceMustBeNonNegative()` → BadRequestException

### `TagErrors`
- `SlugAlreadyExists(string slug)` → ConflictException
- `NotFound(Guid id)` → NotFoundException("Tag", "id", id)
- `NameRequired()` / `SlugRequired()` → BadRequestException

### `CategoryErrors`
- `AlreadyExists(string slug)` → ConflictException
- `NotFound(Guid id)` → NotFoundException("Category", "id", id)
- `AlreadyActive()` / `AlreadyInactive()` → ConflictException
- `NameRequired()` / `SlugRequired()` / `PriceMustBeNonNegative()` → BadRequestException
- `PricingAlreadyExists()` → ConflictException
- `PricingNotFound(Guid categoryId, Guid tierId)` → NotFoundException

### `CustomerErrors`
- `AlreadyExists(string email)` → ConflictException
- `NotFound(Guid id)` → NotFoundException("Customer", "id", id)
- `FullNameRequired()` / `EmailRequired()` → BadRequestException

### `PackageErrors`
- `NotFound(Guid id)` → NotFoundException("Package", "id", id)
- `AlreadyActive()` / `AlreadyInactive()` → ConflictException
- `NameRequired()` / `PriceMustBeNonNegative()` / `SlotQuantityMustBePositive()` → BadRequestException
- `SlotNotFound(Guid slotId)` → NotFoundException

---

## Specifications

### Lookup Specs
- `ContentTypeByIdSpecification(Guid id)` — matches by id
- `ContentTypeByNameSpecification(string name)` — ILike case-insensitive
- `PricingTierByIdSpecification(Guid id)`
- `PricingTierByNameSpecification(string name)` — ILike
- `PromotionLevelByIdSpecification(Guid id)`
- `PromotionLevelByNameSpecification(string name)` — ILike
- `ActivePromotionLevelSpecification()` — IsActive == true
- `TagBySlugSpecification(string slug)` — exact match
- `TagByNameSpecification(string name)` — exact match
- `TagSearchSpecification(string search)` — ILike on Name OR Slug with %search%

### Catalog Specs
- `CategorySpecifications`: BySlug, ByContentTypeId, ActiveOnly, ActiveByContentType, etc.
- `CategoryPricingSpecifications`: ByCategoryAndTier, ByCategoryId
- `CustomerSpecifications`: ByEmail
- `PackageSpecifications`: ByIdWithSlots, ActiveOnly

---

## Handler Analysis — Lookup

### `CreateContentTypeHandler`
Dependencies: ILookupRepository, IContentUnitOfWork, IMapper
Steps:
1. ContentTypeExistsByNameAsync(name) → if true → throw ContentTypeErrors.AlreadyExists(name)
2. ContentTypeEntity.Create(Guid.NewGuid(), name)
3. AddContentTypeAsync(entity)
4. CommitAsync()
5. Map to ContentTypeDto
Returns: CreateContentTypeResult(ContentType: dto)

### `UpdateContentTypeHandler`
Dependencies: ILookupRepository, IContentUnitOfWork, IMapper
Steps:
1. GetContentTypeByIdOrThrowAsync(id) → NotFoundException if not found
2. ContentTypeExistsByNameAsync(name) → if true AND name != current.Name (OrdinalIgnoreCase) → throw AlreadyExists
3. contentType.Update(name)
4. CommitAsync()
5. Map to dto
KEY: Same name update is allowed (name conflict check ignores same entity)

### `ActivateContentTypeHandler`
Dependencies: ILookupRepository, IContentUnitOfWork, IMapper
Steps:
1. GetContentTypeByIdOrThrowAsync(id) → NotFoundException if not found
2. contentType.Activate() → if false → throw ContentTypeErrors.AlreadyActive()
3. CommitAsync()
4. Map to dto

### `DeactivateContentTypeHandler`
Same as Activate but calls Deactivate() and throws AlreadyInactive()

### `CreatePricingTierHandler`
Same as CreateContentType but for PricingTier. Also passes description.

### `UpdatePricingTierHandler`
Same pattern as UpdateContentType. Passes name + description.

### `ActivatePricingTierHandler` / `DeactivatePricingTierHandler`
Same as ContentType variants.

### `CreatePromotionLevelHandler`
Same as Create pattern but with DurationDays + PriceUsd.

### `UpdatePromotionLevelHandler`
Same as Update pattern. Name conflict check with OrdinalIgnoreCase.

### `ActivatePromotionLevelHandler` / `DeactivatePromotionLevelHandler`
Same pattern.

### `CreateTagHandler`
Steps:
1. GetTagBySlugAsync(slug) → if NOT null → throw TagErrors.SlugAlreadyExists(slug)
2. TagEntity.Create(Guid.NewGuid(), name, slug)
3. AddTagAsync(tag)
4. CommitAsync()
5. Map to dto

### `GetAllContentTypesHandler`
Returns all content types mapped to dto list.

### `GetAllPricingTiersHandler`
Returns all pricing tiers mapped to dto list.

### `GetAllPromotionLevelsHandler`
Returns all promotion levels mapped to dto list.

### `GetActivePromotionLevelsHandler`
Returns only active promotion levels (uses GetActivePromotionLevelsAsync).

### `GetAllTagsHandler`
Returns all tags, optionally filtered by Search query parameter.

---

## Handler Analysis — Catalog

### `CreateCategoryHandler`
Dependencies: ILookupRepository, ICategoryRepository, IContentUnitOfWork, IMapper
Steps:
1. GetContentTypeByIdOrThrowAsync(ContentTypeId) → NotFoundException if not found
2. GetBySlugAsync(slug) → if NOT null → throw CategoryErrors.AlreadyExists(slug)
3. CategoryEntity.Create(...)
4. categoryRepository.AddAsync(category)
5. CommitAsync()
6. categoryRepository.GetByIdOrThrowAsync(category.Id) — reload with nav props
7. Map to dto

### `UpdateCategoryHandler`
Dependencies: ICategoryRepository, IContentUnitOfWork, IMapper
Steps:
1. GetByIdOrThrowAsync(id) → NotFoundException if not found
2. GetBySlugAsync(slug) → if NOT null AND slugConflict.Id != command.Id → throw AlreadyExists
3. category.Update(name, slug, description)
4. CommitAsync()
5. GetByIdOrThrowAsync(id) — reload
6. Map to dto

### `ActivateCategoryHandler` / `DeactivateCategoryHandler`
1. GetByIdOrThrowAsync(id) → NotFoundException
2. Activate/Deactivate() → if false → throw AlreadyActive/AlreadyInactive
3. CommitAsync()
4. GetByIdOrThrowAsync(id) — reload
5. Map

### `CreateCustomerHandler`
1. GetByEmailAsync(email) → if NOT null → throw CustomerErrors.AlreadyExists(email)
2. CustomerEntity.Create(...)
3. AddAsync(customer)
4. CommitAsync()
5. Map (does NOT reload)

### `UpdateCustomerHandler`
1. GetByIdOrThrowAsync(id) → NotFoundException
2. customer.Update(fullName, phone, company, notes)
3. CommitAsync()
4. Map (does NOT reload)

### `CreatePackageHandler`
1. PackageEntity.Create(...)
2. AddAsync(package)
3. CommitAsync()
4. GetByIdWithSlotsOrThrowAsync(package.Id) — reload
5. Map

### `ActivatePackageHandler` / `DeactivatePackageHandler`
1. GetByIdWithSlotsOrThrowAsync(id)
2. Activate/Deactivate() → if false → throw AlreadyActive/AlreadyInactive
3. CommitAsync()
4. GetByIdWithSlotsOrThrowAsync(id) — reload
5. Map

### `AddCategoryPricingHandler`
Dependencies: ICategoryRepository, ILookupRepository, IContentUnitOfWork, IMapper
Steps:
1. categoryRepository.GetByIdOrThrowAsync(CategoryId) → NotFoundException
2. lookupRepository.GetPricingTierByIdOrThrowAsync(PricingTierId) → NotFoundException
3. if !pricingTier.IsActive → throw PricingTierErrors.IsInactive()
4. GetPricingAsync(categoryId, pricingTierId) → if NOT null → throw CategoryErrors.PricingAlreadyExists()
5. CategoryPricingEntity.Create(...)
6. AddPricingAsync(pricing)
7. CommitAsync()
8. GetPricingAsync again to reload → map

### `UpdateCategoryPricingHandler`
1. GetPricingAsync(categoryId, pricingTierId) → if null → throw PricingNotFound
2. pricing.UpdatePrice(priceUsd)
3. CommitAsync()
4. Map

### `RemoveCategoryPricingHandler`
1. GetPricingAsync(categoryId, pricingTierId) → if null → throw PricingNotFound
2. RemovePricing(pricing)
3. CommitAsync()
4. GetPricingByCategoryAsync(categoryId) — get remaining
5. Map remaining → return result with IsSuccess=true

### `AddPackageSlotHandler`
Dependencies: IPackageRepository, ICategoryRepository, IContentUnitOfWork, IMapper
Steps:
1. GetByIdWithSlotsOrThrowAsync(packageId) → NotFoundException
2. if CategoryId has value → GetByIdAsync(categoryId) → if null → throw CategoryErrors.NotFound
3. PackageSlotEntity.Create(...)
4. AddSlotAsync(slot)
5. CommitAsync()
6. GetByIdWithSlotsOrThrowAsync(packageId) — reload
7. Map

### `RemovePackageSlotHandler`
1. GetByIdWithSlotsOrThrowAsync(packageId) → NotFoundException
2. GetSlotByIdAsync(slotId) → if null → throw PackageErrors.SlotNotFound
3. RemoveSlot(slot)
4. CommitAsync()
5. GetByIdWithSlotsOrThrowAsync(packageId) — reload
6. Map → return with IsSuccess=true

### `GetAllCategoriesHandler`
Returns paginated categories. Uses PaginatedResult<CategoryDto>.

### `GetCategoryByIdHandler`
Returns single category by id.

### `GetAllCustomersHandler`
Returns paginated customers.

### `GetCustomerByIdHandler`
Returns single customer by id.

### `GetAllPackagesHandler`
Returns paginated packages with optional IsActive filter.

### `GetPackageByIdHandler`
Uses GetByIdWithSlotsOrThrowAsync (includes slots).

### `GetPublicCategoriesHandler`
Returns IReadOnlyList of active categories, optionally filtered by ContentTypeId.

---

## Validators Analysis

### ContentType
- `CreateContentTypeValidator`: Name required, max 30 chars
- `UpdateContentTypeValidator`: Id not empty, Name required, max 30 chars
- `ActivateContentTypeValidator`: Id not empty
- `DeactivateContentTypeValidator`: Id not empty

### PricingTier
- `CreatePricingTierValidator`: Name required max 40, Description optional max 200
- `UpdatePricingTierValidator`: Id not empty, Name required max 40, Description optional max 200
- `ActivatePricingTierValidator`: Id not empty
- `DeactivatePricingTierValidator`: Id not empty

### PromotionLevel
- `CreatePromotionLevelValidator`: Name required max 40, DurationDays > 0, PriceUsd >= 0
- `UpdatePromotionLevelValidator`: Id not empty, Name required max 40, DurationDays > 0, PriceUsd >= 0
- `ActivatePromotionLevelValidator`: Id not empty
- `DeactivatePromotionLevelValidator`: Id not empty

### Tag
- `CreateTagValidator`: Name required max 50, Slug required max 60 + regex `^[a-z0-9]+(?:-[a-z0-9]+)*$`

### Category
- `CreateCategoryValidator`: ContentTypeId not empty, Name required max 60, Slug required max 80 + regex, Description optional max 300
- `UpdateCategoryValidator`: Id not empty, Name required max 60, Slug required max 80 + regex, Description optional max 300
- `ActivateCategoryValidator`: Id not empty
- `DeactivateCategoryValidator`: Id not empty

### Customer
- `CreateCustomerValidator`: FullName required max 100, Email required + valid email format + max 200, Phone optional max 30, Company optional max 100, Notes optional max 500
- `UpdateCustomerValidator`: Id not empty, FullName required max 100, Phone/Company/Notes optional

### Package
- `CreatePackageValidator`: Name required max 100, Description optional max 500, FlatPriceUsd >= 0
- `AddPackageSlotValidator`: PackageId not empty, Quantity > 0

### CategoryPricing
- `AddCategoryPricingValidator`: CategoryId not empty, PricingTierId not empty, PriceUsd >= 0
- `UpdateCategoryPricingValidator`: CategoryId not empty, PricingTierId not empty, PriceUsd >= 0

---

## Validation Error Messages (Exact Strings)

These are the EXACT error messages from validators — needed for `WithErrorMessage(...)` assertions:

| Field | Rule | Message |
|-------|------|---------|
| ContentType.Name | NotEmpty | "Content type name is required." |
| ContentType.Name | MaxLength(30) | "Content type name must not exceed 30 characters." |
| ContentType.Id | NotEmpty | "Content type ID is required." |
| PricingTier.Name | NotEmpty | "Pricing tier name is required." |
| PricingTier.Name | MaxLength(40) | "Pricing tier name must not exceed 40 characters." |
| PricingTier.Description | MaxLength(200) | "Pricing tier description must not exceed 200 characters." |
| PricingTier.Id | NotEmpty | "Pricing tier ID is required." |
| PromotionLevel.Name | NotEmpty | "Promotion level name is required." |
| PromotionLevel.Name | MaxLength(40) | "Promotion level name must not exceed 40 characters." |
| PromotionLevel.Id | NotEmpty | "Promotion level ID is required." |
| PromotionLevel.DurationDays | GreaterThan(0) | "Promotion level duration must be greater than zero." |
| PromotionLevel.PriceUsd | GreaterThanOrEqualTo(0) | "Promotion level price must be zero or greater." |
| Tag.Name | NotEmpty | "Tag name is required." |
| Tag.Name | MaxLength(50) | "Tag name must not exceed 50 characters." |
| Tag.Slug | NotEmpty | "Tag slug is required." |
| Tag.Slug | MaxLength(60) | "Tag slug must not exceed 60 characters." |
| Tag.Slug | Matches | "Tag slug must be lowercase and contain only letters, numbers, and hyphens." |
| Category.Name | NotEmpty | "Category name is required." |
| Category.Name | MaxLength(60) | "Category name must not exceed 60 characters." |
| Category.Slug | NotEmpty | "Category slug is required." |
| Category.Slug | MaxLength(80) | "Category slug must not exceed 80 characters." |
| Category.Slug | Matches | "Category slug must be lowercase and contain only letters, numbers, and hyphens." |
| Category.Description | MaxLength(300) | "Category description must not exceed 300 characters." |
| Category.Id | NotEmpty | "Category ID is required." |
| Customer.FullName | NotEmpty | "Customer full name is required." |
| Customer.FullName | MaxLength(100) | "Customer full name must not exceed 100 characters." |
| Customer.Email | NotEmpty | "Customer email is required." |
| Customer.Email | EmailAddress | "Customer email must be a valid email address." |
| Customer.Email | MaxLength(200) | "Customer email must not exceed 200 characters." |
| Customer.Phone | MaxLength(30) | "Customer phone must not exceed 30 characters." |
| Customer.Company | MaxLength(100) | "Customer company must not exceed 100 characters." |
| Customer.Notes | MaxLength(500) | "Customer notes must not exceed 500 characters." |
| Customer.Id | NotEmpty | "Customer ID is required." |
| Package.Name | NotEmpty | "Package name is required." |
| Package.Name | MaxLength(100) | "Package name must not exceed 100 characters." |
| Package.Description | MaxLength(500) | "Package description must not exceed 500 characters." |
| Package.FlatPriceUsd | GreaterThanOrEqualTo(0) | "Package price must be zero or greater." |
| Package.Id | NotEmpty | "Package ID is required." |
| PackageSlot.Quantity | GreaterThan(0) | "Slot quantity must be greater than zero." |
| CategoryPricing.PriceUsd | GreaterThanOrEqualTo(0) | "Category price must be zero or greater." |

---

## Infrastructure

### `ContentDbContext`
- Inherits DbContext
- Constructor: (DbContextOptions<ContentDbContext> options)
- DbSets: ContentTypes, PricingTiers, PromotionLevels, Tags, Categories, CategoryPricing, Customers, Packages, PackageSlots
- HasDefaultSchema("content")
- ApplyConfigurationsFromAssembly

### `ContentUnitOfWork`
- Constructor: (ContentDbContext context)
- CommitAsync → context.SaveChangesAsync(ct)

### `ContentModule` (AddContentModule / UseContentModule)
Services registered:
- IContentUnitOfWork → ContentUnitOfWork (Scoped)
- ILookupRepository → LookupRepository (Scoped)
- ICategoryRepository → CategoryRepository (Scoped)
- ICustomerRepository → CustomerRepository (Scoped)
- IPackageRepository → PackageRepository (Scoped)
- ContentTypeSeeder (Scoped)
- TypeAdapterConfig (Singleton) — Mapster config
- IMapper → Mapper (Scoped)
