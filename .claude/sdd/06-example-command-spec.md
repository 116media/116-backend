# Example Command Spec: AdminVerifyPayment

This is a **fully worked spec** for the `AdminVerifyPayment` use case.
This use case already exists in the codebase — use this to see what a complete spec looks like
before an implementation exists.

---

# Spec: AdminVerifyPaymentCommand

## Intent

Allows an Admin to verify that a customer has paid for an order by confirming receipt
of payment and recording a receipt URL. This transitions the payment from Pending to
Verified, transitions the order from PendingPayment to Paid, and stamps any
promotion-level content items (SocialBoost / Featured) that were purchased.

---

## Command Shape

| Field | Type | Source | Constraints |
|-------|------|--------|-------------|
| `OrderId` | `string` | Route param `{id}` | Must be a valid GUID |
| `ReceiptUrl` | `string` | Request body | Required, max 500 chars |
| `AdminUserId` | `Guid` | JWT claims via `IClaimsProvider.GetUserIdFromClaims` | Not empty |

```csharp
public record AdminVerifyPaymentCommand(
    string OrderId,
    string ReceiptUrl,
    Guid AdminUserId
) : ICommand<AdminVerifyPaymentResult>;

public record AdminVerifyPaymentResult(bool IsSuccess);
```

---

## Business Rules

1. Order must exist (`contentOrderRepository.GetByIdWithItemsAsync` returns non-null)
2. Order must have an associated payment (`IOrderPaymentFactory.GetByOrderIdOrThrowAsync`)
3. Order must be in `PendingPayment` status — enforced inside `IVerifyPaymentFactory.VerifyAsync`
4. Payment must be in `Pending` status — enforced inside `IVerifyPaymentFactory.VerifyAsync`

Note: rules 3 and 4 are delegated to the factory, not checked in the handler.

---

## Error Cases

| Trigger | Exception class | Error factory |
|---------|----------------|---------------|
| Order not found | `NotFoundException` | `ContentOrderErrors.NotFound(orderId)` |
| Payment not found | `NotFoundException` | thrown by `IOrderPaymentFactory.GetByOrderIdOrThrowAsync` |
| Order not `PendingPayment` | `BadRequestException` | `ContentOrderErrors.NotInPendingPayment(orderId)` |
| Payment not `Pending` | `ConflictException` | `ContentOrderErrors.PaymentAlreadyProcessed(orderId)` |

---

## Side Effects

_(delegated to `IVerifyPaymentFactory.VerifyAsync`)_

1. `payment.Verify(adminUserId, receiptUrl)` — sets `Status=Verified`, `VerifiedById`, `VerifiedAt`, `ReceiptUrl`
2. `order.MarkPaid()` — sets `Status=Paid`
3. For each order item with a promotion level: stamp the linked content entity (`StampSocialBoost()` or `StampFeatured(until)`)
4. `contentOrderRepository.UpdateAsync(order, ct)` — persists order
5. `contentOrderRepository.UpdatePaymentAsync(payment, ct)` — persists payment
6. Repository calls to update each stamped content entity
7. `unitOfWork.CommitAsync(ct)` — single transaction commit

---

## Response Shape

```csharp
public record AdminVerifyPaymentResult(bool IsSuccess);
```

---

## Validator

| Field | Rule |
|-------|------|
| `OrderId` | `IsValidGuid("Order ID")` |
| `ReceiptUrl` | `ValidReceiptUrl()` (custom extension — `NotEmpty` + `MaximumLength(500)`) |
| `AdminUserId` | `NotEmpty()` with message "Admin user ID is required." |

```csharp
public class AdminVerifyPaymentValidator : AbstractValidator<AdminVerifyPaymentCommand>
{
    public AdminVerifyPaymentValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.ReceiptUrl).ValidReceiptUrl();
        RuleFor(x => x.AdminUserId).NotEmpty().WithMessage("Admin user ID is required.");
    }
}
```

---

## Endpoint

```
Method:       PATCH
Route:        /api/v1/admin/orders/{id}/payment/verify
Request body: AdminVerifyPaymentRequest { ReceiptUrl: string }
Response:     AdminVerifyPaymentResponse { IsSuccess: bool }
Auth:         AccountStatusPolicies.RequireActiveUser
              UserRolePolicies.RequireAdminOrSuperAdmin
Rate limit:   RateLimitPolicies.ContentBrowsing
Route group:  ContentConstants.Admin + "/" + CommerceRouteConstants.Orders
Produces:
  200 OK      AdminVerifyPaymentResponse
  400         ProblemDetails (validation or wrong order/payment state)
  401         ProblemDetails
  403         ProblemDetails
  404         ProblemDetails (order or payment not found)
  409         ProblemDetails (payment already verified/rejected)
  429         ProblemDetails
```

JWT extraction:
```csharp
Guid adminUserId = claimsProvider.GetUserIdFromClaims(user: user);
```

---

## Dependencies

**Handler:**
- `IContentOrderRepository`
- `IOrderPaymentFactory`
- `IVerifyPaymentFactory`

**Factory (`AdminVerifyPaymentFactory`):**
- `IContentOrderRepository` (UpdateAsync, UpdatePaymentAsync)
- `IArticleRepository` (UpdateAsync for stamped articles)
- `IVideoRepository` (UpdateAsync for stamped videos)
- `IContentUnitOfWork`

---

## MetaField

```csharp
public static class AdminVerifyPaymentMetaField
{
    public static readonly RouteMetadata AdminVerifyPayment = new(
        "AdminVerifyPayment",
        "Verify an order payment",
        """
        Verifies a PendingPayment order's payment, transitioning the order to Paid status.
        A receipt URL is recorded and social boost / featured promotion is stamped on any
        already-linked content items.

        **Authentication Requirements:**
        - User must be authenticated with a valid access token
        - User must have Admin or SuperAdmin role

        **Response Codes:**
        - Returns 200 OK on success
        - Returns 400 Bad Request if the order is not in PendingPayment status
        - Returns 401 Unauthorized if access token is invalid or expired
        - Returns 403 Forbidden if user lacks Admin role
        - Returns 404 Not Found if the order or payment does not exist
        - Returns 409 Conflict if the payment has already been verified or rejected
        """
    );
}
```

---

## Test Cases

**Handler tests (`AdminVerifyPaymentHandlerTests`):**

```
[Happy path]
- Handle_WhenAllValid_ShouldReturnSuccess
  Arrange: order (PendingPayment), payment (Pending)
  Setup: SetupGetByIdWithItems(order), MockOrderPaymentFactory.SetupGetByOrderId(payment),
         MockVerifyPaymentFactory.SetupVerifyAsync()
  Assert: result.IsSuccess == true
  Verify: MockVerifyPaymentFactory.VerifyVerifyAsyncCalled()

[Failure paths]
- Handle_WhenOrderNotFound_ShouldThrowNotFoundException
  Setup: SetupGetByIdWithItems(null)

- Handle_WhenPaymentNotFound_ShouldThrowNotFoundException
  Setup: SetupGetByIdWithItems(order), MockOrderPaymentFactory returns NotFoundException
```

**Validator tests (`AdminVerifyPaymentValidatorTests`):**

```
- Validate_WhenAllValid_ShouldPass
- Validate_WhenOrderIdIsEmpty_ShouldFail_OnOrderId
- Validate_WhenOrderIdIsInvalidGuid_ShouldFail_OnOrderId
- Validate_WhenReceiptUrlIsEmpty_ShouldFail_OnReceiptUrl
- Validate_WhenReceiptUrlExceedsMaxLength_ShouldFail_OnReceiptUrl
- Validate_WhenAdminUserIdIsEmpty_ShouldFail_OnAdminUserId
```

**Factory tests (`AdminVerifyPaymentFactoryTests`):**

```
[Happy path]
- VerifyAsync_WhenAllValid_ShouldVerifyPaymentMarkOrderPaidAndCommit
  Arrange: order (PendingPayment + items with promotions), payment (Pending)
  Setup: all repositories, content entity repositories
  Assert: payment.Status == Verified, order.Status == Paid
  Verify: VerifyUpdateCalled() (order + payment), VerifyCommitCalled()

[Failure paths]
- VerifyAsync_WhenOrderNotPendingPayment_ShouldThrowBadRequestException
  Use: ContentOrderFactory.Create() (Draft)

- VerifyAsync_WhenPaymentNotPending_ShouldThrowConflictException
  Use: ContentPaymentFactory.CreateVerified(orderId, adminId)
```