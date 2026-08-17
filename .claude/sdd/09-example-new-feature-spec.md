# Example: Writing a Spec for a Brand-New Feature

This walkthrough shows the process of writing a spec from scratch for a feature
that **does not exist yet** in the codebase.

**Feature:** `AdminRestoreOrder` — allows an Admin to restore a Cancelled order back to Draft status
so that items can be re-added and the order re-submitted.

---

## Step 1: Write the Intent

Start with the business motivation. Who does it? What does it do? Why?

```
Allows an Admin to restore a Cancelled order back to Draft status.
This is needed when a customer cancels by mistake or when the cancel was issued
erroneously. The order can then have items re-added and be re-submitted.
```

---

## Step 2: Define the Command Shape

What inputs does the operation need?

The only input is the order ID (route parameter). No body needed.

```csharp
public record AdminRestoreOrderCommand(string OrderId) : ICommand<AdminRestoreOrderResult>;
public record AdminRestoreOrderResult(bool IsSuccess);
```

| Field | Type | Source | Constraints |
|-------|------|--------|-------------|
| `OrderId` | `string` | Route param `{id}` | Must be valid GUID |

---

## Step 3: Write the Business Rules

What must be true for the operation to succeed?

```
1. Order must exist (contentOrderRepository.GetByIdAsync returns non-null)
2. Order must be in Cancelled status (order.EnsureCancelled())
```

---

## Step 4: Map Business Rules to Error Cases

One error case per business rule:

| Trigger | Exception class | Error factory |
|---------|----------------|---------------|
| Order not found | `NotFoundException` | `ContentOrderErrors.NotFound(orderId)` |
| Order not Cancelled | `BadRequestException` | `ContentOrderErrors.NotInCancelled(orderId)` |

Note: `ContentOrderErrors.NotInCancelled` doesn't exist yet — the spec documents it as a new error to create.

---

## Step 5: Define the Side Effects

What changes in the database?

```
1. order.Restore() — sets Status = Draft (new domain method, see domain spec below)
2. contentOrderRepository.UpdateAsync(order, ct)
3. unitOfWork.CommitAsync(ct)
```

---

## Step 6: Define the New Domain Method

The `Restore()` method doesn't exist on `ContentOrderEntity`. Add it to the domain spec:

```
Method: Restore()
Precondition: Status == Cancelled (EnsureCancelled() guard)
State change: Status = Draft
Throws: BadRequestException (ContentOrderErrors.NotInCancelled) if not Cancelled
Returns: void
```

And the guard:

```
Method: EnsureCancelled()
Purpose: Guard — verifies order is in Cancelled status
Precondition: Status == Cancelled
Throws: BadRequestException if not Cancelled
Returns: void
```

---

## Step 7: Write the Validator

Only the route param needs validation:

| Field | Rule |
|-------|------|
| `OrderId` | `IsValidGuid("Order ID")` |

---

## Step 8: Define the Endpoint

```
Method:       PATCH
Route:        /api/v1/admin/orders/{id}/restore
Request body: (none)
Response:     AdminRestoreOrderResponse { IsSuccess: bool }
Auth:         AccountStatusPolicies.RequireActiveUser
              UserRolePolicies.RequireAdminOrSuperAdmin
Rate limit:   RateLimitPolicies.ContentBrowsing
Route group:  ContentConstants.Admin + "/" + CommerceRouteConstants.Orders
Produces:
  200 OK      AdminRestoreOrderResponse
  400         ProblemDetails (order not Cancelled)
  401         ProblemDetails
  403         ProblemDetails
  404         ProblemDetails (order not found)
  429         ProblemDetails
```

---

## Step 9: Write the Test Cases

**Domain tests (`ContentOrderEntityTests` — new tests added to existing file):**
```
- Restore_WhenCancelled_ShouldTransitionToDraft
- Restore_WhenDraft_ShouldThrowBadRequestException
- Restore_WhenPendingPayment_ShouldThrowBadRequestException
- Restore_WhenPaid_ShouldThrowBadRequestException
- EnsureCancelled_WhenCancelled_ShouldNotThrow
- EnsureCancelled_WhenNotCancelled_ShouldThrowBadRequestException (Theory + InlineData)
```

**Handler tests (`AdminRestoreOrderHandlerTests`):**
```
- Handle_WhenOrderIsCancelled_ShouldReturnSuccess
- Handle_WhenOrderNotFound_ShouldThrowNotFoundException
- Handle_WhenOrderNotCancelled_ShouldThrowBadRequestException (via domain — no mock needed)
```

**Validator tests (`AdminRestoreOrderValidatorTests`):**
```
- Validate_WhenAllValid_ShouldPass
- Validate_WhenOrderIdIsEmpty_ShouldFail_OnOrderId
- Validate_WhenOrderIdIsInvalidGuid_ShouldFail_OnOrderId
```

---

## Step 10: Identify New Artifacts Needed

Before handing to Claude, list everything new that needs to be created:

| Artifact | Type | Location |
|---------|------|----------|
| `AdminRestoreOrderCommand.cs` | Command + Result record | `Commerce/UseCases/Admin/Commands/RestoreOrder/` |
| `AdminRestoreOrderHandler.cs` | Handler | Same folder |
| `AdminRestoreOrderValidator.cs` | Validator | Same folder |
| `AdminRestoreOrderMetaField.cs` | MetaField | Same folder |
| `AdminRestoreOrderEndpointV1.cs` | Carter endpoint | `…/V1/` |
| `Restore()` on `ContentOrderEntity` | Domain method | `Domain/Entities/ContentOrderEntity.cs` |
| `EnsureCancelled()` on `ContentOrderEntity` | Guard method | Same file |
| `ContentOrderErrors.NotInCancelled()` | Error factory | `Commerce/Errors/ContentOrderErrors.cs` |

---

## The Complete Spec (Final Form)

```markdown
# Spec: AdminRestoreOrderCommand

## Intent
Allows an Admin to restore a Cancelled order back to Draft status so items can be
re-added and the order re-submitted. Used when a cancel is issued by mistake.

## Command Shape
| Field | Type | Source | Constraints |
|-------|------|--------|-------------|
| `OrderId` | `string` | Route param `{id}` | Must be a valid GUID |

```csharp
public record AdminRestoreOrderCommand(string OrderId) : ICommand<AdminRestoreOrderResult>;
public record AdminRestoreOrderResult(bool IsSuccess);
```

## Business Rules
1. Order must exist (GetByIdAsync returns non-null)
2. Order must be in Cancelled status (order.EnsureCancelled())

## Error Cases
| Trigger | Exception | Error factory |
|---------|-----------|---------------|
| Order not found | NotFoundException | ContentOrderErrors.NotFound(orderId) |
| Order not Cancelled | BadRequestException | ContentOrderErrors.NotInCancelled(orderId) |

## Side Effects
1. order.Restore() — Status → Draft
2. contentOrderRepository.UpdateAsync(order, ct)
3. unitOfWork.CommitAsync(ct)

## Response Shape
```csharp
public record AdminRestoreOrderResult(bool IsSuccess);
```

## Validator
| Field | Rule |
|-------|------|
| `OrderId` | `IsValidGuid("Order ID")` |

## Endpoint
Method:       PATCH
Route:        /api/v1/admin/orders/{id}/restore
Request body: (none)
Response:     AdminRestoreOrderResponse { IsSuccess: bool }
Auth:         AccountStatusPolicies.RequireActiveUser
              UserRolePolicies.RequireAdminOrSuperAdmin
Rate limit:   RateLimitPolicies.ContentBrowsing
Produces:
  200 OK, 400, 401, 403, 404, 429

## Dependencies
Handler:
  - IContentOrderRepository
  - IContentUnitOfWork

## New artifacts
- ContentOrderEntity.Restore() — transition Cancelled → Draft
- ContentOrderEntity.EnsureCancelled() — guard
- ContentOrderErrors.NotInCancelled(Guid orderId) — error factory

## Test Cases
Domain (ContentOrderEntityTests):
  - Restore_WhenCancelled_ShouldTransitionToDraft
  - Restore_WhenNotCancelled_ShouldThrowBadRequestException [Theory]
  - EnsureCancelled_WhenCancelled_ShouldNotThrow
  - EnsureCancelled_WhenNotCancelled_ShouldThrowBadRequestException [Theory]

Handler (AdminRestoreOrderHandlerTests):
  - Handle_WhenOrderIsCancelled_ShouldReturnSuccess
  - Handle_WhenOrderNotFound_ShouldThrowNotFoundException
  - Handle_WhenOrderNotCancelled_ShouldThrowBadRequestException

Validator (AdminRestoreOrderValidatorTests):
  - Validate_WhenAllValid_ShouldPass
  - Validate_WhenOrderIdIsEmpty_ShouldFail_OnOrderId
  - Validate_WhenOrderIdIsInvalidGuid_ShouldFail_OnOrderId
```

---

## What Claude does with this spec

Given this spec, Claude produces (in order, no questions asked):

1. `ContentOrderEntity.Restore()` + `EnsureCancelled()` — added to existing entity file
2. `ContentOrderErrors.NotInCancelled(Guid orderId)` — added to existing errors file
3. `AdminRestoreOrderCommand.cs`
4. `AdminRestoreOrderHandler.cs`
5. `AdminRestoreOrderValidator.cs`
6. `AdminRestoreOrderMetaField.cs`
7. `AdminRestoreOrderEndpointV1.cs`
8. `ContentOrderEntityTests.cs` — new test methods appended
9. `AdminRestoreOrderHandlerTests.cs`
10. `AdminRestoreOrderValidatorTests.cs`

No guessing. No ambiguity. Every file matches the spec exactly.