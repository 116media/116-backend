# Phase 8: Content Module — Catalog API Tests Spec

## Tasks

### Admin Category Commands
- [ ] `AdminCreateCategoryEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_AsVisitor_ShouldReturn403
  - [ ] Post_WithoutAuth_ShouldReturn401
  - [ ] Post_WithDuplicateSlug_ShouldReturn409
  - [ ] Post_WithInvalidData_ShouldReturn422
  - [ ] Post_WithNonExistentContentType_ShouldReturn404
- [ ] `AdminUpdateCategoryEndpointTests.cs`
  - [ ] Put_AsAdmin_WithValidData_ShouldReturn200
  - [ ] Put_NonExistentCategory_ShouldReturn404
  - [ ] Put_WithDuplicateSlug_ShouldReturn409
- [ ] `AdminActivateCategoryEndpointTests.cs`
  - [ ] Patch_AsAdmin_ShouldReturn200
  - [ ] Patch_AlreadyActive_ShouldReturn409
- [ ] `AdminDeactivateCategoryEndpointTests.cs`
  - [ ] Patch_AsAdmin_ShouldReturn200
  - [ ] Patch_AlreadyInactive_ShouldReturn409
- [ ] `AdminSetExclusiveCategoryEndpointTests.cs`
  - [ ] Patch_AsAdmin_VideoCategory_ShouldReturn200
  - [ ] Patch_AsAdmin_NonVideoCategory_ShouldReturn400
  - [ ] Patch_ShouldUnsetPreviousExclusive
- [ ] `AdminUploadCategoryPosterEndpointTests.cs`
  - [ ] Post_WithValidImage_ShouldReturn200
  - [ ] Post_WithoutAuth_ShouldReturn401
- [ ] `AdminAddCategoryPricingEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_WithDuplicateTier_ShouldReturn409
- [ ] `AdminUpdateCategoryPricingEndpointTests.cs`
  - [ ] Put_AsAdmin_ShouldReturn200
- [ ] `AdminRemoveCategoryPricingEndpointTests.cs`
  - [ ] Delete_AsAdmin_ShouldReturn204

### Admin Package Commands
- [ ] `AdminCreatePackageEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_WithInvalidData_ShouldReturn422
- [ ] `AdminActivatePackageEndpointTests.cs`
- [ ] `AdminDeactivatePackageEndpointTests.cs`
- [ ] `AdminAddPackageSlotEndpointTests.cs`
  - [ ] Post_AsAdmin_ShouldReturn201
- [ ] `AdminRemovePackageSlotEndpointTests.cs`
  - [ ] Delete_AsAdmin_ShouldReturn204

### Admin Customer Commands
- [ ] `AdminCreateCustomerEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
- [ ] `AdminUpdateCustomerEndpointTests.cs`
  - [ ] Put_AsAdmin_ShouldReturn200

### Admin Catalog Queries
- [ ] `AdminGetAllCategoriesEndpointTests.cs`
  - [ ] Get_AsAdmin_ShouldReturn200WithPaginatedCategories
  - [ ] Get_WithSearchTerm_ShouldFilterResults
- [ ] `AdminGetCategoryByIdEndpointTests.cs`
  - [ ] Get_AsAdmin_ExistingCategory_ShouldReturn200
  - [ ] Get_NonExistent_ShouldReturn404
- [ ] `AdminGetAllCustomersEndpointTests.cs`
- [ ] `AdminGetCustomerByIdEndpointTests.cs`
- [ ] `AdminGetAllPackagesEndpointTests.cs`
- [ ] `AdminGetPackageByIdEndpointTests.cs`

### Public Catalog Queries
- [ ] `PublicGetActiveCategoriesEndpointTests.cs`
  - [ ] Get_Anonymous_ShouldReturn200WithActiveCategories
  - [ ] Get_ShouldNotIncludeInactiveCategories
- [ ] `PublicGetExclusiveCategoryEndpointTests.cs`
  - [ ] Get_WithExclusiveCategory_ShouldReturn200WithVideos
  - [ ] Get_WithNoExclusiveCategory_ShouldReturn404

## Seeding Requirements

Categories need a ContentType FK:
```csharp
protected override async Task SeedAsync()
{
    await using var context = CreateDbContext<ContentDbContext>();
    var videoType = ContentTypeEntity.Create(Guid.NewGuid(), "Video", "Videos");
    context.ContentTypes.Add(videoType);
    await context.SaveChangesAsync();
}
```

## File Locations

```
tests/_116.Integration.Tests/Content/Api/Catalog/
├── AdminCreateCategoryEndpointTests.cs
├── AdminUpdateCategoryEndpointTests.cs
├── AdminActivateCategoryEndpointTests.cs
├── AdminDeactivateCategoryEndpointTests.cs
├── AdminSetExclusiveCategoryEndpointTests.cs
├── AdminUploadCategoryPosterEndpointTests.cs
├── AdminAddCategoryPricingEndpointTests.cs
├── AdminUpdateCategoryPricingEndpointTests.cs
├── AdminRemoveCategoryPricingEndpointTests.cs
├── AdminCreatePackageEndpointTests.cs
├── AdminActivatePackageEndpointTests.cs
├── AdminDeactivatePackageEndpointTests.cs
├── AdminAddPackageSlotEndpointTests.cs
├── AdminRemovePackageSlotEndpointTests.cs
├── AdminCreateCustomerEndpointTests.cs
├── AdminUpdateCustomerEndpointTests.cs
├── AdminGetAllCategoriesEndpointTests.cs
├── AdminGetCategoryByIdEndpointTests.cs
├── AdminGetAllCustomersEndpointTests.cs
├── AdminGetCustomerByIdEndpointTests.cs
├── AdminGetAllPackagesEndpointTests.cs
├── AdminGetPackageByIdEndpointTests.cs
├── PublicGetActiveCategoriesEndpointTests.cs
└── PublicGetExclusiveCategoryEndpointTests.cs
```

## Acceptance Criteria

1. Every catalog endpoint has integration tests
2. CRUD lifecycle verified: Create → Read → Update → Activate/Deactivate
3. FK integrity verified (Categories → ContentTypes, CategoryPricings → PricingTiers)
4. `./scripts/run-tests-with-coverage.sh integration` passes
