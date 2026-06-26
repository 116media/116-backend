# Assertions — Content / Catalog

Categories, customers, packages, pricing, slots, exclusive-category, poster
upload.

## Key response types
- Lists: `AdminGetAllCategoriesResponse` / `...Customers` / `...Packages`
  (`PaginatedResult<…Dto>`).
- Get-by-id / create / update → typed response with the entity DTO.

## After (create category)
```csharp
var request = CreateCategoryRequestBuilder.Valid();
var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Categories, request);
response.StatusCode.Should().Be(HttpStatusCode.Created);

var body = await response.ReadAsAsync<AdminCreateCategoryResponse>();
body.Category.Name.Should().Be(request.Name);
body.Category.Slug.Should().Be(request.Slug);
body.Category.Id.Should().NotBeEmpty();

await using var db = CreateDbContext<ContentDbContext>();
(await db.Categories.AnyAsync(c => c.Id == body.Category.Id)).Should().BeTrue();
```

Activate/deactivate/set-exclusive/add-pricing/remove-pricing/add-slot/remove-slot
must re-query and assert state. Conflicts (duplicate slug, already active, pricing
exists, only-video-can-be-exclusive, etc.) → `ShouldBeProblem`. Poster upload
asserts the stubbed Cloudinary URL.

## TODO checklist
- [ ] AdminActivateCategoryEndpointV1Tests.cs
- [ ] AdminActivatePackageEndpointV1Tests.cs
- [ ] AdminAddCategoryPricingEndpointV1Tests.cs
- [ ] AdminAddPackageSlotEndpointV1Tests.cs
- [ ] AdminCreateCategoryEndpointV1Tests.cs
- [ ] AdminCreateCustomerEndpointV1Tests.cs
- [ ] AdminCreatePackageEndpointV1Tests.cs
- [ ] AdminDeactivateCategoryEndpointV1Tests.cs
- [ ] AdminDeactivatePackageEndpointV1Tests.cs
- [ ] AdminGetAllCategoriesEndpointV1Tests.cs
- [ ] AdminGetAllCustomersEndpointV1Tests.cs
- [ ] AdminGetAllPackagesEndpointV1Tests.cs
- [ ] AdminGetCategoryByIdEndpointV1Tests.cs
- [ ] AdminGetCustomerByIdEndpointV1Tests.cs
- [ ] AdminGetPackageByIdEndpointV1Tests.cs
- [ ] AdminRemoveCategoryPricingEndpointV1Tests.cs
- [ ] AdminRemovePackageSlotEndpointV1Tests.cs
- [ ] AdminSetExclusiveCategoryEndpointV1Tests.cs
- [ ] AdminUpdateCategoryEndpointV1Tests.cs
- [ ] AdminUpdateCategoryPricingEndpointV1Tests.cs
- [ ] AdminUpdateCustomerEndpointV1Tests.cs
- [ ] AdminUploadCategoryPosterEndpointV1Tests.cs
- [ ] PublicGetActiveCategoriesEndpointV1Tests.cs
- [ ] PublicGetExclusiveCategoryEndpointV1Tests.cs

## Acceptance
- Mutations verify DB; lists assert seeded item + pagination; conflicts use `ShouldBeProblem`.
