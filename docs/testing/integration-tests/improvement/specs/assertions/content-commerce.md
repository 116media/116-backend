# Assertions — Content / Commerce

Orders, items, item-tiers, payment proof/verify/reject, submit/cancel, lists.

## Key response types
- Lists: `AdminGetAllOrdersResponse` (`PaginatedResult<ContentOrderSummaryDto>`:
  Id, CustomerName, Status, TotalAmountUsd, ItemCount), `AdminGetAllPaymentsResponse`.
- Get-by-id → full order DTO (items, tiers, totals, status).

## After (list — currently status-only)
```csharp
var body = await response.ReadAsAsync<AdminGetAllOrdersResponse>();
body.Orders.Items.Should().Contain(o => o.Id == seededOrder.Id);
var dto = body.Orders.Items.Single(o => o.Id == seededOrder.Id);
dto.CustomerName.Should().Be(customer.FullName);
dto.Status.Should().Be(EnumOrderStatus.Draft);
dto.ItemCount.Should().Be(1);
```

## After (state machine — submit/verify/cancel)
```csharp
// PATCH .../{id}/submit
await using var db = CreateDbContext<ContentDbContext>();
var order = await db.ContentOrders.FindAsync(seededOrder.Id);
order!.Status.Should().Be(EnumOrderStatus.PendingPayment);
```

Add-item / add-tier assert the item/tier persisted and totals recomputed.
Verify/reject payment assert payment status + order status. Invalid transitions
(cancel paid, add item to non-draft, submit empty, already-paid) → `ShouldBeProblem`.

## TODO checklist
- [ ] AdminAddItemTierEndpointV1Tests.cs
- [ ] AdminAddOrderItemEndpointV1Tests.cs
- [ ] AdminAttachPaymentProofEndpointV1Tests.cs
- [ ] AdminCancelOrderEndpointV1Tests.cs
- [ ] AdminCreateOrderEndpointV1Tests.cs
- [ ] AdminEditOrderEndpointV1Tests.cs
- [ ] AdminEditOrderItemEndpointV1Tests.cs
- [ ] AdminGetAllOrdersEndpointV1Tests.cs
- [ ] AdminGetAllPaymentsEndpointV1Tests.cs
- [ ] AdminGetCustomerOrdersEndpointV1Tests.cs
- [ ] AdminGetOrderByIdEndpointV1Tests.cs
- [ ] AdminGetOrderPaymentEndpointV1Tests.cs
- [ ] AdminGetPendingPaymentOrdersEndpointV1Tests.cs
- [ ] AdminRejectPaymentEndpointV1Tests.cs
- [ ] AdminRemoveItemTierEndpointV1Tests.cs
- [ ] AdminRemoveOrderItemEndpointV1Tests.cs
- [ ] AdminSubmitOrderEndpointV1Tests.cs
- [ ] AdminVerifyPaymentEndpointV1Tests.cs

## Acceptance
- Every state transition verifies the persisted status; lists assert DTO fields;
  invalid transitions use `ShouldBeProblem`.
