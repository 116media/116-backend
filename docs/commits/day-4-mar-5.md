# Day 4 — March 5, 2026 (58 commits)
## Catalog commands (continued) + all catalog queries

**Start time:** 08:20
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreateCustomer/CreateCustomerValidator.cs`
```
feat(content): add CreateCustomer FluentValidation validator
```

### 2
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreateCustomer/V1/CreateCustomerEndpointV1.cs`
```
feat(content): add POST /api/v1/admin/customers endpoint
```

### 3
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageCommand.cs`
```
feat(content): add CreatePackage command and result records
```

### 4
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageHandler.cs`
```
feat(content): add CreatePackage command handler:

- Guard against duplicate names via ILike case-insensitive check
- Create PackageEntity with generated id and persist
- Return PackageDto mapped via Mapster
```

### 5
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageMetaField.cs`
```
feat(content): add CreatePackage route metadata
```

### 6
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageValidator.cs`
```
feat(content): add CreatePackage FluentValidation validator
```

### 7
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/V1/CreatePackageEndpointV1.cs`
```
feat(content): add POST /api/v1/admin/packages endpoint
```

### 8
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryCommand.cs`
```
feat(content): add DeactivateCategory command record
```

### 9
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryHandler.cs`
```
feat(content): add DeactivateCategory command handler:

- Fetch category by id, throw NotFoundException if missing
- Call Deactivate() and throw ConflictException if already inactive
- Persist change via unit of work
```

### 10
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryMetaField.cs`
```
feat(content): add DeactivateCategory route metadata
```

### 11
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryValidator.cs`
```
feat(content): add DeactivateCategory FluentValidation validator
```

### 12
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/V1/DeactivateCategoryEndpointV1.cs`
```
feat(content): add PATCH /api/v1/admin/categories/{id}/deactivate endpoint
```

### 13
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageCommand.cs`
```
feat(content): add DeactivatePackage command record
```

### 14
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageHandler.cs`
```
feat(content): add DeactivatePackage command handler:

- Fetch package by id, throw NotFoundException if missing
- Call Deactivate() and throw ConflictException if already inactive
- Persist change via unit of work
```

### 15
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageMetaField.cs`
```
feat(content): add DeactivatePackage route metadata
```

### 16
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageValidator.cs`
```
feat(content): add DeactivatePackage FluentValidation validator
```

### 17
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/V1/DeactivatePackageEndpointV1.cs`
```
feat(content): add PATCH /api/v1/admin/packages/{id}/deactivate endpoint
```

### 18
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/RemoveCategoryPricingCommand.cs`
```
feat(content): add RemoveCategoryPricing command record
```

### 19
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/RemoveCategoryPricingHandler.cs`
```
feat(content): add RemoveCategoryPricing command handler:

- Look up category-pricing association by composite key
- Remove entity from context and persist via unit of work
```

### 20
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/RemoveCategoryPricingMetaField.cs`
```
feat(content): add RemoveCategoryPricing route metadata
```

### 21
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/V1/RemoveCategoryPricingEndpointV1.cs`
```
feat(content): add DELETE /api/v1/admin/categories/{id}/pricings/{tierId} endpoint
```

### 22
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/RemovePackageSlotCommand.cs`
```
feat(content): add RemovePackageSlot command record
```

### 23
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/RemovePackageSlotHandler.cs`
```
feat(content): add RemovePackageSlot command handler:

- Look up package slot by composite key
- Remove entity from context and persist via unit of work
```

### 24
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/RemovePackageSlotMetaField.cs`
```
feat(content): add RemovePackageSlot route metadata
```

### 25
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/V1/RemovePackageSlotEndpointV1.cs`
```
feat(content): add DELETE /api/v1/admin/packages/{id}/slots/{slotId} endpoint
```

### 26
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryCommand.cs`
```
feat(content): add UpdateCategory command and result records
```

### 27
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryHandler.cs`
```
feat(content): add UpdateCategory command handler:

- Fetch category by id and validate name uniqueness with ILike exclusion
- Call entity Update method and persist via unit of work
```

### 28
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryMetaField.cs`
```
feat(content): add UpdateCategory route metadata
```

### 29
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryValidator.cs`
```
feat(content): add UpdateCategory FluentValidation validator
```

### 30
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/V1/UpdateCategoryEndpointV1.cs`
```
feat(content): add PUT /api/v1/admin/categories/{id} endpoint
```

### 31
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingCommand.cs`
```
feat(content): add UpdateCategoryPricing command and result records
```

### 32
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingHandler.cs`
```
feat(content): add UpdateCategoryPricing command handler:

- Fetch category-pricing by composite key
- Validate new pricing-tier exists and call entity Update method
- Persist via unit of work
```

### 33
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingMetaField.cs`
```
feat(content): add UpdateCategoryPricing route metadata
```

### 34
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingValidator.cs`
```
feat(content): add UpdateCategoryPricing FluentValidation validator
```

### 35
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/V1/UpdateCategoryPricingEndpointV1.cs`
```
feat(content): add PUT /api/v1/admin/categories/{id}/pricings/{tierId} endpoint
```

### 36
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerCommand.cs`
```
feat(content): add UpdateCustomer command and result records
```

### 37
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerHandler.cs`
```
feat(content): add UpdateCustomer command handler:

- Fetch customer by id and validate email uniqueness with ILike exclusion
- Call entity Update method and persist via unit of work
```

### 38
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerMetaField.cs`
```
feat(content): add UpdateCustomer route metadata
```

### 39
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerValidator.cs`
```
feat(content): add UpdateCustomer FluentValidation validator
```

### 40
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/V1/UpdateCustomerEndpointV1.cs`
```
feat(content): add PUT /api/v1/admin/customers/{id} endpoint
```

### 41
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCategories/GetAllCategoriesHandler.cs`
```
feat(content): add GetAllCategories query handler with pagination and filters
```

### 42
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCategories/GetAllCategoriesMetaField.cs`
```
feat(content): add GetAllCategories route metadata
```

### 43
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCategories/GetAllCategoriesQuery.cs`
```
feat(content): add GetAllCategories query record with filter parameters
```

### 44
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCategories/V1/GetAllCategoriesEndpointV1.cs`
```
feat(content): add GET /api/v1/admin/categories endpoint
```

### 45
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCustomers/GetAllCustomersHandler.cs`
```
feat(content): add GetAllCustomers query handler with pagination
```

### 46
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCustomers/GetAllCustomersMetaField.cs`
```
feat(content): add GetAllCustomers route metadata
```

### 47
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCustomers/GetAllCustomersQuery.cs`
```
feat(content): add GetAllCustomers query record with filter parameters
```

### 48
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCustomers/V1/GetAllCustomersEndpointV1.cs`
```
feat(content): add GET /api/v1/admin/customers endpoint
```

### 49
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllPackages/GetAllPackagesHandler.cs`
```
feat(content): add GetAllPackages query handler with pagination and IsActive filter
```

### 50
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllPackages/GetAllPackagesMetaField.cs`
```
feat(content): add GetAllPackages route metadata
```

### 51
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllPackages/GetAllPackagesQuery.cs`
```
feat(content): add GetAllPackages query record with filter parameters
```

### 52
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetAllPackages/V1/GetAllPackagesEndpointV1.cs`
```
feat(content): add GET /api/v1/admin/packages endpoint
```

### 53
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCategoryById/GetCategoryByIdHandler.cs`
```
feat(content): add GetCategoryById query handler with pricing eager load
```

### 54
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCategoryById/GetCategoryByIdMetaField.cs`
```
feat(content): add GetCategoryById route metadata
```

### 55
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCategoryById/GetCategoryByIdQuery.cs`
```
feat(content): add GetCategoryById query record
```

### 56
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCategoryById/V1/GetCategoryByIdEndpointV1.cs`
```
feat(content): add GET /api/v1/admin/categories/{id} endpoint
```

### 57
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCustomerById/GetCustomerByIdHandler.cs`
```
feat(content): add GetCustomerById query handler
```

### 58
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCustomerById/GetCustomerByIdMetaField.cs`
```
feat(content): add GetCustomerById route metadata
```