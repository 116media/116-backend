# Phase 9: Content Module — Commerce API Tests Spec

## Tasks

### Admin Order Commands
- [ ] `AdminCreateOrderEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_WithNonExistentCustomer_ShouldReturn404
  - [ ] Post_WithoutAuth_ShouldReturn401
  - [ ] Post_WithInvalidData_ShouldReturn422
- [ ] `AdminEditOrderEndpointTests.cs`
  - [ ] Put_AsAdmin_ExistingOrder_ShouldReturn200
  - [ ] Put_NonExistentOrder_ShouldReturn404
- [ ] `AdminAddOrderItemEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidItem_ShouldReturn201
  - [ ] Post_WithNonExistentOrder_ShouldReturn404
- [ ] `AdminEditOrderItemEndpointTests.cs`
  - [ ] Put_AsAdmin_ShouldReturn200
- [ ] `AdminRemoveOrderItemEndpointTests.cs`
  - [ ] Delete_AsAdmin_ShouldReturn204
- [ ] `AdminSubmitOrderEndpointTests.cs`
  - [ ] Patch_AsAdmin_DraftOrder_ShouldReturn200
  - [ ] Patch_AlreadySubmittedOrder_ShouldReturn409
- [ ] `AdminCancelOrderEndpointTests.cs`
  - [ ] Patch_AsAdmin_ShouldReturn200
  - [ ] Patch_AlreadyPaidOrder_ShouldReturn409

### Admin Payment Commands
- [ ] `AdminAttachPaymentProofEndpointTests.cs`
  - [ ] Post_AsAdmin_WithFile_ShouldReturn200
- [ ] `AdminVerifyPaymentEndpointTests.cs`
  - [ ] Patch_AsAdmin_PendingPayment_ShouldReturn200
  - [ ] Patch_AlreadyVerifiedPayment_ShouldReturn409
- [ ] `AdminRejectPaymentEndpointTests.cs`
  - [ ] Patch_AsAdmin_PendingPayment_ShouldReturn200

### Admin Item Tier Commands
- [ ] `AdminAddItemTierEndpointTests.cs`
  - [ ] Post_AsAdmin_ShouldReturn201
- [ ] `AdminRemoveItemTierEndpointTests.cs`
  - [ ] Delete_AsAdmin_ShouldReturn204

### Admin Commerce Queries
- [ ] `AdminGetAllOrdersEndpointTests.cs`
  - [ ] Get_AsAdmin_ShouldReturn200WithPaginatedOrders
- [ ] `AdminGetOrderByIdEndpointTests.cs`
  - [ ] Get_AsAdmin_ExistingOrder_ShouldReturn200
  - [ ] Get_NonExistent_ShouldReturn404
- [ ] `AdminGetCustomerOrdersEndpointTests.cs`
  - [ ] Get_AsAdmin_ShouldReturnOrdersForCustomer
- [ ] `AdminGetAllPaymentsEndpointTests.cs`
- [ ] `AdminGetOrderPaymentEndpointTests.cs`
- [ ] `AdminGetPendingPaymentOrdersEndpointTests.cs`
  - [ ] Get_AsAdmin_ShouldReturnOnlyPendingPaymentOrders

## Seeding Requirements

Commerce tests need a full entity chain:
```
ContentType → Category → Video/Article → Customer → Order → OrderItem
```

```csharp
protected override async Task SeedAsync()
{
    await using var context = CreateDbContext<ContentDbContext>();

    var videoType = ContentTypeEntity.Create(Guid.NewGuid(), "Video", "Videos");
    context.ContentTypes.Add(videoType);

    var category = CategoryEntity.Create(/* ... */);
    context.Categories.Add(category);

    var video = VideoEntity.Create(/* ... */);
    context.Videos.Add(video);

    var customer = CustomerEntity.Create(/* ... */);
    context.Customers.Add(customer);

    await context.SaveChangesAsync();
}
```

## File Locations

```
tests/_116.Integration.Tests/Content/Api/Commerce/
├── AdminCreateOrderEndpointTests.cs
├── AdminEditOrderEndpointTests.cs
├── AdminAddOrderItemEndpointTests.cs
├── AdminEditOrderItemEndpointTests.cs
├── AdminRemoveOrderItemEndpointTests.cs
├── AdminSubmitOrderEndpointTests.cs
├── AdminCancelOrderEndpointTests.cs
├── AdminAttachPaymentProofEndpointTests.cs
├── AdminVerifyPaymentEndpointTests.cs
├── AdminRejectPaymentEndpointTests.cs
├── AdminAddItemTierEndpointTests.cs
├── AdminRemoveItemTierEndpointTests.cs
├── AdminGetAllOrdersEndpointTests.cs
├── AdminGetOrderByIdEndpointTests.cs
├── AdminGetCustomerOrdersEndpointTests.cs
├── AdminGetAllPaymentsEndpointTests.cs
├── AdminGetOrderPaymentEndpointTests.cs
└── AdminGetPendingPaymentOrdersEndpointTests.cs
```

## Acceptance Criteria

1. Full order lifecycle tested: Create → AddItems → Submit → Pay → Verify
2. Payment state transitions verified (Pending → Verified, Pending → Rejected)
3. Order state transitions verified (Draft → Submitted → Cancelled)
4. `./scripts/run-tests-with-coverage.sh integration` passes
