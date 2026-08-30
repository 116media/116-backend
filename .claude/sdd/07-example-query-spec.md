# Example Query Spec: AdminGetOrderPayment

This is a **fully worked spec** for the `AdminGetOrderPayment` use case.
This use case already exists — use this to see what a complete query spec looks like.

---

# Spec: AdminGetOrderPaymentQuery

## Intent

Allows an Admin to retrieve the payment record associated with a specific order.
Used by the frontend to display payment status, proof attachment, verification details,
and rejection notes for a given order.

---

## Query Shape

| Field | Type | Source | Constraints |
|-------|------|--------|-------------|
| `OrderId` | `Guid` | Route param `{id}` | Route constraint (GET, no Guid.Parse needed) |

```csharp
public record AdminGetOrderPaymentQuery(Guid OrderId) : IQuery<AdminGetOrderPaymentResult>;
```

---

## Business Rules

1. Payment must exist for the given `OrderId`
   (`IOrderPaymentFactory.GetByOrderIdOrThrowAsync` returns the payment or throws)

Note: No order existence check — the payment is the primary resource.
If the order doesn't exist, there will be no payment and the factory throws `NotFoundException`.

---

## Error Cases

| Trigger | Exception class | Error factory |
|---------|----------------|---------------|
| Payment not found for orderId | `NotFoundException` | thrown by `IOrderPaymentFactory.GetByOrderIdOrThrowAsync` |

---

## Data Loading

1. Load `ContentPaymentEntity` via `IOrderPaymentFactory.GetByOrderIdOrThrowAsync(query.OrderId, ct)`
2. Map to `AdminGetOrderPaymentResult` using `payment.ToAdminGetOrderPaymentResult()` extension

---

## Response Shape

```csharp
public record AdminGetOrderPaymentResult(
    Guid Id,
    Guid OrderId,
    decimal AmountUsd,
    string Status,              // EnumPaymentStatus.ToString(): "Pending" | "Verified" | "Rejected"
    string? PaymentMethod,      // EnumPaymentMethod?.ToString(): "BankTransfer" | "MobileMoney" | "Cash" | null
    Guid? PaymentProofFileId,
    Guid? VerifiedById,
    string? ReceiptUrl,
    string? Notes,
    DateTimeOffset? VerifiedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
```

---

## Mapper / Extension

```csharp
// In Commerce/Mappers/ContentPaymentMapper.cs
public static class ContentPaymentMapper
{
    public static AdminGetOrderPaymentResult ToAdminGetOrderPaymentResult(
        this ContentPaymentEntity payment) => new(
            Id:                 payment.Id,
            OrderId:            payment.OrderId,
            AmountUsd:          payment.AmountUsd,
            Status:             payment.Status.ToString(),
            PaymentMethod:      payment.PaymentMethod?.ToString(),
            PaymentProofFileId: payment.PaymentProofFileId,
            VerifiedById:       payment.VerifiedById,
            ReceiptUrl:         payment.ReceiptUrl,
            Notes:              payment.Notes,
            VerifiedAt:         payment.VerifiedAt,
            CreatedAt:          payment.CreatedAt,
            UpdatedAt:          payment.UpdatedAt
        );
}
```

---

## Endpoint

```
Method:       GET
Route:        /api/v1/admin/orders/{id}/payment
Response:     AdminGetOrderPaymentResult
Auth:         AccountStatusPolicies.RequireActiveUser
              UserRolePolicies.RequireAdminOrSuperAdmin
Rate limit:   RateLimitPolicies.ContentBrowsing
Route group:  ContentConstants.Admin + "/" + CommerceRouteConstants.Orders
Produces:
  200 OK      AdminGetOrderPaymentResult
  401         ProblemDetails
  403         ProblemDetails
  404         ProblemDetails (payment not found)
  429         ProblemDetails
```

No request body. `Guid id` in route (route constraint acceptable for GET).

---

## Dependencies

**Handler:**
- `IOrderPaymentFactory`

---

## MetaField

```csharp
public static class AdminGetOrderPaymentMetaField
{
    public static readonly RouteMetadata AdminGetOrderPayment = new(
        "AdminGetOrderPayment",
        "Get the payment record for an order",
        """
        Returns the full payment record associated with the specified order, including
        payment status, method, proof file reference, receipt URL, and verification details.

        **Authentication Requirements:**
        - User must be authenticated with a valid access token
        - User must have Admin or SuperAdmin role

        **Response Codes:**
        - Returns 200 OK with the payment record
        - Returns 401 Unauthorized if access token is invalid or expired
        - Returns 403 Forbidden if user lacks Admin role
        - Returns 404 Not Found if no payment exists for this order
        """
    );
}
```

---

## Test Cases

**Handler tests (`AdminGetOrderPaymentHandlerTests`):**

```
[Happy path]
- Handle_WhenPaymentExists_ShouldReturnMappedResult
  Arrange: ContentPaymentFactory.CreateVerified(orderId, adminId)
  Setup: MockOrderPaymentFactory.SetupGetByOrderId(orderId, payment)
  Assert:
    result.Should().NotBeNull()
    result.Id.Should().Be(payment.Id)
    result.OrderId.Should().Be(payment.OrderId)
    result.AmountUsd.Should().Be(payment.AmountUsd)
    result.Status.Should().Be("Verified")
    result.VerifiedById.Should().Be(payment.VerifiedById)

[Failure paths]
- Handle_WhenPaymentNotFound_ShouldThrowNotFoundException
  Setup: MockOrderPaymentFactory throws NotFoundException
```

**Mapper tests (`AdminGetOrderPaymentMapperTests`):**

```
- ToAdminGetOrderPaymentResult_WhenPaymentIsPending_ShouldMapStatusAndNullFields
  payment.Status = Pending, PaymentMethod = null, ReceiptUrl = null
  Assert: result.Status == "Pending", result.PaymentMethod == null

- ToAdminGetOrderPaymentResult_WhenPaymentIsVerified_ShouldMapAllFields
  Use: ContentPaymentFactory.CreateVerified(orderId, adminId)
  Assert: result.Status == "Verified", result.VerifiedById != null, result.ReceiptUrl != null

- ToAdminGetOrderPaymentResult_WhenPaymentIsRejected_ShouldMapNotes
  Use: ContentPaymentFactory.CreateRejected(orderId, "Insufficient proof")
  Assert: result.Status == "Rejected", result.Notes == "Insufficient proof"

- ToAdminGetOrderPaymentResult_WhenPaymentMethodIsSet_ShouldMapPaymentMethodString
  Use: ContentPaymentFactory.CreateWithProof(orderId, proofFileId)
  Assert: result.PaymentMethod == "BankTransfer" (or whichever was set)
```