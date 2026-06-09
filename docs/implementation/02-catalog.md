# Catalog Sub-Module — Implementation Plan

> Depends on: Lookup (content types, pricing tiers, promotion levels must exist).
> All other sub-modules depend on categories and customers existing.

## Scope

| Entity | SQL Table | Repository |
|---|---|---|
| `CategoryEntity` | `content.categories` | `ICategoryRepository` |
| `CategoryPricingEntity` | `content.category_pricing` | `ICategoryRepository` |
| `CustomerEntity` | `content.customers` | `ICustomerRepository` |
| `PackageEntity` | `content.packages` | `IPackageRepository` |
| `PackageSlotEntity` | `content.package_slots` | `IPackageRepository` |

---

## 🔴 CRUCIAL — Blocks editorial creation and all orders

---

### POST /api/v1/admin/categories

> Creates a new content category that editors will use when creating articles and videos (e.g.
> "Artist Profile", "116 Le Focus", "Chronique Sale"). Every content item must belong to exactly
> one category. Without categories the editorial team cannot create any content, and since all
> orders are tied to a category, no B2B revenue can flow through the system. Categories also
> determine whether content is free or paid — a `IsFree = true` category skips the payment flow
> entirely and goes straight to editorial review.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateCategoryCommand(ContentTypeId, Name, Slug, Description?, IsFree)` |
| **Response** | `201` + `CategoryDto(Id, ContentTypeId, ContentTypeName, Name, Slug, IsFree, IsActive, Pricing[])` |

**TODOs**
- [x] `CategoryEntity.Create(id, contentTypeId, name, slug, description, isFree)` — validate name max 60, slug max 80, slug format
- [x] `CreateCategoryCommand(Guid ContentTypeId, string Name, string Slug, string? Description, bool IsFree) : ICommand<CategoryDto>`
- [x] `CreateCategoryCommandValidator` — all required fields, max lengths, slug uniqueness checked in handler
- [x] `CreateCategoryCommandHandler` — verifies `ContentTypeId` exists (`ILookupRepository.GetContentTypeByIdAsync()`), checks slug not taken (`ICategoryRepository.GetBySlugAsync()`), creates entity, calls `ICategoryRepository.AddAsync()`, commits `IContentUnitOfWork`
- [x] `CategoryRepository.AddAsync(category)` and `CategoryRepository.GetBySlugAsync(slug)` implementations
- [x] `CreateCategoryEndpointV1` Carter module

---

### POST /api/v1/admin/categories/{id}/pricing

> Attaches a pricing tier to a category and sets the price for that add-on (e.g. "Artist Profile +
> base_upload = $25"). A paid category can only accept orders once it has at least one pricing tier
> configured. Without pricing rows, the admin cannot add tiers to order items, meaning no order
> total can be computed and no payment can be triggered. This endpoint is the bridge between the
> content catalogue and the revenue model.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `AddCategoryPricingCommand(CategoryId, PricingTierId, PriceUsd)` |
| **Response** | `201` + `CategoryPricingDto(TierId, TierName, PriceUsd)` |

> A category can only accept orders once it has at least one pricing tier configured (unless `IsFree = true`).

**TODOs**
- [x] `CategoryPricingEntity.Create(id, categoryId, pricingTierId, priceUsd)` — validate `priceUsd >= 0`
- [x] `AddCategoryPricingCommand(Guid CategoryId, Guid PricingTierId, decimal PriceUsd) : ICommand<CategoryPricingDto>`
- [x] `AddCategoryPricingCommandValidator` — `PriceUsd >= 0`, both IDs required
- [x] `AddCategoryPricingCommandHandler` — verifies category exists, verifies pricing tier exists and is active, checks combination not already present (`ICategoryRepository.GetPricingByCategoryAsync()`), creates entity, calls `ICategoryRepository.AddPricingAsync()`, commits UoW
- [x] `CategoryRepository.GetPricingByCategoryAsync(categoryId)` and `CategoryRepository.AddPricingAsync(pricing)`
- [x] `AddCategoryPricingEndpointV1` Carter module

---

### POST /api/v1/admin/customers

> Creates a B2B client record for an artist, music label, or brand that commissions paid content.
> A customer account must exist before an order can be opened for them — it is the entity that
> links payment history, receipts, and commissioned content to the same business contact. This is
> entirely separate from platform visitor accounts (B2C users who read articles and watch videos).
> Without a customer record the admin cannot start the revenue flow for a new client.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateCustomerCommand(FullName, Email, Phone?, Company?, Notes?)` |
| **Response** | `201` + `CustomerDto(Id, FullName, Email, Phone, Company)` |

> Customers are B2B clients (artists, labels, brands). Must exist before an order can be created.

**TODOs**
- [x] `CustomerEntity.Create(id, fullName, email, phone, company, notes)` — validate email format, fullName max 100, email max 200
- [x] `CreateCustomerCommand(string FullName, string Email, string? Phone, string? Company, string? Notes) : ICommand<CustomerDto>`
- [x] `CreateCustomerCommandValidator` — `FullName` required, `Email` required and valid format, max lengths
- [x] `CreateCustomerCommandHandler` — checks email not already used (`ICustomerRepository.GetByEmailAsync()`), creates entity, calls `ICustomerRepository.AddAsync()`, commits UoW
- [x] `CustomerRepository.AddAsync(customer)` and `CustomerRepository.GetByEmailAsync(email)`
- [x] `CreateCustomerEndpointV1` Carter module

---

## 🟡 IMPORTANT — Core admin management

---

### GET /api/v1/admin/categories

> Returns the paginated list of all categories so the admin team can review what content formats
> are currently configured, check which ones are active or free, and identify categories that still
> need pricing configured before they can accept orders. The `IsActive` and `IsFree` filters allow
> the admin to quickly isolate, for example, all active paid categories before configuring pricing
> tiers for a new client deal.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllCategoriesQuery(Page, PageSize, IsActive?, IsFree?)` |
| **Response** | `200` + `PagedResponse<CategoryDto>` |

**TODOs**
- [x] `GetAllCategoriesQuery(int Page, int PageSize, bool? IsActive, bool? IsFree) : IQuery<PagedResponse<CategoryDto>>`
- [x] `GetAllCategoriesQueryHandler` — calls `ICategoryRepository.GetAllAsync(page, pageSize, isActive, isFree)`
- [x] `CategoryRepository.GetAllAsync(page, pageSize, isActive, isFree)` — applies `IsActive` and `IsFree` filters conditionally, includes `ContentType` navigation for `ContentTypeName`
- [x] `GetAllCategoriesEndpointV1` Carter module

---

### GET /api/v1/admin/categories/{id}

> Returns the full details of a single category, including its complete pricing configuration
> (all tiers with current prices). Used by the admin when setting up an order — they need to know
> exactly what pricing tiers are available for a category before adding items and tiers to an order.
> Also used by editors to confirm the category settings before creating content.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetCategoryByIdQuery(Id)` |
| **Response** | `200` + `CategoryDto` (includes `Pricing[]`) |

**TODOs**
- [x] `GetCategoryByIdQuery(Guid Id) : IQuery<CategoryDto>`
- [x] `GetCategoryByIdQueryHandler` — calls `ICategoryRepository.GetByIdAsync(id)`, throws `ResourceNotFoundException` if null
- [x] `CategoryRepository.GetByIdAsync(id)` — includes `ContentType` and `Pricing` with `PricingTier` navigation
- [x] `GetCategoryByIdEndpointV1` Carter module

---

### PUT /api/v1/admin/categories/{id}

> Updates the category's display name, URL slug, or description without touching its pricing
> configuration. Needed when a show is rebranded (e.g. "Interview" → "116 Interview" to match the
> platform's naming convention where all shows are prefixed with "116"). Slug changes take effect
> immediately on public category URLs, so this should only be done when the old URL can be
> redirected at the frontend.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateCategoryCommand(Id, Name, Slug, Description?)` |
| **Response** | `200` + `CategoryDto` |

**TODOs**
- [x] `UpdateCategoryCommand(Guid Id, string Name, string Slug, string? Description) : ICommand<CategoryDto>`
- [x] `UpdateCategoryCommandValidator`
- [x] `UpdateCategoryCommandHandler` — fetches category, checks new slug not taken by another category, updates fields inline, calls `ICategoryRepository.UpdateAsync()`, commits UoW
- [x] `CategoryRepository.UpdateAsync(category)`
- [x] `UpdateCategoryEndpointV1` Carter module

---

### PUT /api/v1/admin/categories/{id}/pricing/{tierId}

> Updates the price for a specific tier within a category when market rates change (e.g. raising
> the base_upload fee for "116 Le Focus" from $200 to $250). The change applies only to future
> orders — existing order items have their price frozen at snapshot time and are never retroactively
> affected. This allows the admin to keep pricing current without invalidating committed deals.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateCategoryPricingCommand(CategoryId, PricingTierId, PriceUsd)` |
| **Response** | `200` + `CategoryPricingDto` |

**TODOs**
- [x] `UpdateCategoryPricingCommand(Guid CategoryId, Guid PricingTierId, decimal PriceUsd) : ICommand<CategoryPricingDto>`
- [x] `UpdateCategoryPricingCommandValidator` — `PriceUsd >= 0`
- [x] `UpdateCategoryPricingCommandHandler` — fetches pricing row, calls `CategoryPricingEntity.UpdatePrice(priceUsd)`, calls `ICategoryRepository.UpdatePricingAsync()`, commits UoW
- [x] `CategoryRepository.UpdatePricingAsync(pricing)`
- [x] `UpdateCategoryPricingEndpointV1` Carter module

---

### DELETE /api/v1/admin/categories/{id}/pricing/{tierId}

> Removes a pricing tier from a category when that add-on service is no longer offered for that
> content type. For example, if the team decides that the "extended_featured" tier will not be
> offered for "Album Review" articles, removing the pricing row ensures editors cannot accidentally
> add it to new orders. Existing orders that already contain this tier are unaffected — the price
> snapshot is preserved on the order item tier record.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RemoveCategoryPricingCommand(CategoryId, PricingTierId)` |
| **Response** | `204 No Content` |

**TODOs**
- [x] `RemoveCategoryPricingCommand(Guid CategoryId, Guid PricingTierId) : ICommand`
- [x] `RemoveCategoryPricingCommandHandler` — fetches pricing row, calls `ICategoryRepository.RemovePricingAsync()`, commits UoW
- [x] `CategoryRepository.RemovePricingAsync(pricing)` — `context.CategoryPricing.Remove(pricing)`
- [x] `RemoveCategoryPricingEndpointV1` Carter module

---

### GET /api/v1/admin/customers

> Returns the paginated list of all B2B customers so the admin can search for an existing client
> before creating an order or look up contact details when following up on payment. This is the
> primary customer directory used by the sales and admin team. Ordered by most recently created
> first so new clients appear at the top.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllCustomersQuery(Page, PageSize)` |
| **Response** | `200` + `PagedResponse<CustomerDto>` |

**TODOs**
- [x] `GetAllCustomersQuery(int Page, int PageSize) : IQuery<PagedResponse<CustomerDto>>`
- [x] `GetAllCustomersQueryHandler` — calls `ICustomerRepository.GetAllAsync(page, pageSize)`
- [x] `CustomerRepository.GetAllAsync(page, pageSize)` — ordered by `CreatedAt DESC`
- [x] `GetAllCustomersEndpointV1` Carter module

---

### GET /api/v1/admin/customers/{id}

> Returns the full details for a single customer. Used when the admin is about to create an order
> and wants to confirm the client's contact information, or when a customer calls in and the team
> needs to pull up their profile to answer questions about previous commissions. Provides the
> foundation for the customer detail view in the admin dashboard.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetCustomerByIdQuery(Id)` |
| **Response** | `200` + `CustomerDto` |

**TODOs**
- [x] `GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto>`
- [x] `GetCustomerByIdQueryHandler` — calls `ICustomerRepository.GetByIdAsync(id)`, throws `ResourceNotFoundException` if null
- [x] `CustomerRepository.GetByIdAsync(id)`
- [x] `GetCustomerByIdEndpointV1` Carter module

---

### PUT /api/v1/admin/customers/{id}

> Updates the customer's contact information (full name, phone, company, admin notes) when their
> details change. Email is intentionally excluded from this update — it is the unique identifier
> used to link payment history and receipts, so changing it would break the audit trail. Notes
> are particularly useful for recording payment preferences or special instructions that the team
> needs to reference when dealing with a returning client.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdateCustomerCommand(Id, FullName, Phone?, Company?, Notes?)` |
| **Response** | `200` + `CustomerDto` |

**TODOs**
- [x] `UpdateCustomerCommand(Guid Id, string FullName, string? Phone, string? Company, string? Notes) : ICommand<CustomerDto>`
- [x] `UpdateCustomerCommandValidator`
- [x] `UpdateCustomerCommandHandler` — fetches entity, calls `CustomerEntity.Update(fullName, phone, company, notes)`, calls `ICustomerRepository.UpdateAsync()`, commits UoW
- [x] `CustomerRepository.UpdateAsync(customer)`
- [x] `UpdateCustomerEndpointV1` Carter module

---

### POST /api/v1/admin/packages

> Creates a named bundle deal that the admin can offer to clients who want multiple content pieces
> at a flat rate (e.g. "Artist Starter Pack — $300: includes 1 × Artist Profile + 1 × 116
> Interview"). Packages are optional — individual à-la-carte orders remain the primary model —
> but they simplify the sales conversation for returning clients and allow the team to offer
> attractive discount bundles without manual price calculations.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreatePackageCommand(Name, Description?, FlatPriceUsd)` |
| **Response** | `201` + `PackageDto(Id, Name, Description, FlatPriceUsd, IsActive, Slots[])` |

**TODOs**
- [x] `PackageEntity.Create(id, name, description, flatPriceUsd)` — validate `flatPriceUsd >= 0`, name max 100
- [x] `CreatePackageCommand(string Name, string? Description, decimal FlatPriceUsd) : ICommand<PackageDto>`
- [x] `CreatePackageCommandValidator` — `Name` required, `FlatPriceUsd >= 0`
- [x] `CreatePackageCommandHandler` — creates entity, calls `IPackageRepository.AddAsync()`, commits UoW
- [x] `PackageRepository.AddAsync(package)`
- [x] `CreatePackageEndpointV1` Carter module

---

### POST /api/v1/admin/packages/{id}/slots

> Adds a content slot to a package definition — either a fixed category slot (e.g. "1 × 116 Le
> Focus video") or an open slot where the client picks any category. Required slots must be
> fulfilled before the package order is considered complete. Optional slots are bonus entries the
> client can claim. Slots define the exact composition of a bundle deal and determine how many
> articles or videos the admin must eventually create to fulfill the package.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `AddPackageSlotCommand(PackageId, CategoryId?, IsRequired, Quantity)` |
| **Response** | `201` + `PackageSlotDto(Id, CategoryId, CategoryName?, IsRequired, Quantity)` |

> `CategoryId` is nullable — a null value means an open slot where the client chooses any category.

**TODOs**
- [x] `PackageSlotEntity.Create(id, packageId, categoryId, isRequired, quantity)` — validate `quantity > 0`
- [x] `AddPackageSlotCommand(Guid PackageId, Guid? CategoryId, bool IsRequired, int Quantity) : ICommand<PackageSlotDto>`
- [x] `AddPackageSlotCommandValidator` — `Quantity > 0`, if `CategoryId` provided verify it exists
- [x] `AddPackageSlotCommandHandler` — verifies package exists, optionally verifies category exists, creates slot entity, calls `IPackageRepository.AddSlotAsync()`, commits UoW
- [x] `PackageRepository.AddSlotAsync(slot)`
- [x] `AddPackageSlotEndpointV1` Carter module

---

## 🟢 MODERATE — Status management and package CRUD

---

### GET /api/v1/admin/packages

> Lists all packages so the admin can see what bundle deals are currently configured. Used when
> opening a new order and deciding whether to apply a package discount, or when reviewing the
> package catalogue before proposing a deal to a prospective client. The `IsActive` filter
> separates current offerings from retired bundles.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllPackagesQuery(Page, PageSize, IsActive?)` |
| **Response** | `200` + `PagedResponse<PackageDto>` |

**TODOs**
- [x] `GetAllPackagesQuery(int Page, int PageSize, bool? IsActive) : IQuery<PagedResponse<PackageDto>>`
- [x] `GetAllPackagesQueryHandler` — calls `IPackageRepository.GetAllAsync(page, pageSize, isActive)`
- [x] `PackageRepository.GetAllAsync(page, pageSize, isActive)`
- [x] `GetAllPackagesEndpointV1` Carter module

---

### GET /api/v1/admin/packages/{id}

> Returns the full details of a package including all its slots, their required/optional status,
> quantities, and linked category names. Used when the admin needs to review exactly what a package
> includes before assigning it to a new client order, or when editing the bundle composition by
> adding or removing slots.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetPackageByIdQuery(Id)` |
| **Response** | `200` + `PackageDto` (includes `Slots[]`) |

**TODOs**
- [x] `GetPackageByIdQuery(Guid Id) : IQuery<PackageDto>`
- [x] `GetPackageByIdQueryHandler` — calls `IPackageRepository.GetByIdWithSlotsAsync(id)`, throws `ResourceNotFoundException` if null
- [x] `PackageRepository.GetByIdWithSlotsAsync(id)` — includes `Slots` with `Category` navigation
- [x] `GetPackageByIdEndpointV1` Carter module

---

### PATCH /api/v1/admin/categories/{id}/activate
### PATCH /api/v1/admin/categories/{id}/deactivate

> Activating a category makes it selectable again when creating content or adding items to an
> order — useful for reinstating a temporarily suspended format. Deactivating prevents editors
> from creating new content in that category and stops admins from adding it to new orders,
> effectively sunsetting the format without losing historical data or breaking existing content
> items that already belong to it.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ActivateCategoryCommand(Id)` / `DeactivateCategoryCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [x] `ActivateCategoryCommand(Guid Id) : ICommand` → calls `CategoryEntity.Activate()`
- [x] `DeactivateCategoryCommand(Guid Id) : ICommand` → calls `CategoryEntity.Deactivate()`
- [x] `CategoryRepository.UpdateAsync(category)` (reused from PUT endpoint)
- [x] `ActivateCategoryEndpointV1` and `DeactivateCategoryEndpointV1`

---

### PATCH /api/v1/admin/packages/{id}/activate
### PATCH /api/v1/admin/packages/{id}/deactivate

> Activating a package makes it available to assign to new client orders. Deactivating removes it
> from the catalogue of available bundles so the admin cannot attach it to new orders — useful when
> retiring a deal that is no longer offered without permanently deleting its historical order data.
> Existing orders that already reference this package are completely unaffected.

**TODOs**
- [x] `ActivatePackageCommand(Guid Id) : ICommand` → calls `PackageEntity.Activate()`
- [x] `DeactivatePackageCommand(Guid Id) : ICommand` → calls `PackageEntity.Deactivate()`
- [x] `PackageRepository.UpdateAsync(package)` — add `UpdateAsync` to `IPackageRepository`
- [x] `ActivatePackageEndpointV1` and `DeactivatePackageEndpointV1`

---

### DELETE /api/v1/admin/packages/{id}/slots/{slotId}

> Removes a single content slot from a package when the bundle is being restructured. For example,
> if "Artist Starter Pack" was defined with 2 × Article slots but the team decides to replace one
> with an open-choice slot, this endpoint removes the unwanted slot cleanly. Only affects future
> orders — existing orders already linked to the package retain their committed slot structure.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `RemovePackageSlotCommand(PackageId, SlotId)` |
| **Response** | `204 No Content` |

**TODOs**
- [x] `RemovePackageSlotCommand(Guid PackageId, Guid SlotId) : ICommand`
- [x] `RemovePackageSlotCommandHandler` — fetches slot, verifies it belongs to the package, calls `IPackageRepository.RemoveSlotAsync()`, commits UoW
- [x] `PackageRepository.RemoveSlotAsync(slot)` — `context.PackageSlots.Remove(slot)`
- [x] `RemovePackageSlotEndpointV1` Carter module

---

## ⚪ TRIVIAL — Public-facing category browser

---

### GET /api/v1/public/categories

> Returns the list of active categories to anonymous visitors. Powers the public-facing catalogue
> page so potential B2B clients can see what content formats 116 offers (e.g. "Artist Profile",
> "116 Le Focus", "Chronique Sale") before getting in touch. Also used by the frontend to build
> the category filter tabs on the article and video feed pages. Only active categories are returned
> — deactivated formats are hidden from the public.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetPublicCategoriesQuery(ContentTypeId?)` |
| **Response** | `200` + `IReadOnlyList<CategoryDto>` (only `IsActive = true` categories, no pricing) |

> Used by the public frontend to show what content formats are available (e.g. for a catalogue page).

**TODOs**
- [x] `GetPublicCategoriesQuery(Guid? ContentTypeId) : IQuery<IReadOnlyList<CategoryDto>>`
- [x] `GetPublicCategoriesQueryHandler` — calls `ICategoryRepository.GetByContentTypeAsync(contentTypeId)` or all active if null, filters `IsActive = true`
- [x] `CategoryRepository.GetByContentTypeAsync(contentTypeId)` — `WHERE is_active = true AND content_type_id = @id`
- [x] `GetPublicCategoriesEndpointV1` Carter module (`.AllowAnonymous()`)