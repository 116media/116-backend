# Spec Format — Canon and Rules

A spec is a markdown file. It must be written so that any person (or Claude) reading it can produce the implementation with zero ambiguity. Every section is required. Missing a section means the spec is not ready.

---

## Golden Rule

> If you can't answer "what does Claude do when X happens?" from reading the spec alone, the spec is not done.

---

## Section-by-Section Rules

### 1. `## Intent`

One to three sentences. What does this use case do and why does it exist?

**Must include:**
- The actor (Admin, SuperAdmin, Public user, System)
- The action verb (submit, verify, reject, cancel, attach, publish)
- The target entity
- The business reason

**Bad:**
```
Updates payment status.
```

**Good:**
```
Allows an Admin to verify that a customer has paid for an order by confirming receipt of
payment and recording a receipt URL. This transitions the order from PendingPayment to
Paid and stamps any promotion levels on the linked content items.
```

---

### 2. `## Command / Query Shape`

List every field with its exact C# type, source (request body / route / claims), and validation rules.

Use a table:

| Field | Type | Source | Constraints |
|-------|------|--------|-------------|
| `OrderId` | `string` | Route param `{id}` | Must be a valid GUID |
| `ReceiptUrl` | `string` | Request body | Required, max 500 chars |
| `AdminUserId` | `Guid` | JWT claims via `IClaimsProvider` | Not empty |

**Rules:**
- Mutating endpoint route params are `string` — Guid.Parse() happens in the handler or factory
- GET route params may be `Guid` directly (route constraint acceptable)
- JWT-sourced fields are extracted in the endpoint before building the command
- Never put `IFormFile` in a command record — it goes in the endpoint and is passed as the field value

---

### 3. `## Business Rules`

Numbered list of all preconditions that must hold for the operation to succeed. Each rule maps to one `if` check or `EnsureXxx()` call in the handler or factory.

**Format:**
```
1. Order must exist (GetByIdWithItemsAsync returns non-null)
2. Order must be in PendingPayment status (order.EnsurePendingPayment())
3. Payment must exist for the order (IOrderPaymentFactory.GetByOrderIdOrThrowAsync)
4. Payment must be in Pending status (payment.EnsurePending())
```

**Rules:**
- One rule per line item
- State the entity, the check, and the guard method if known
- If you don't know the guard method, describe what it checks

---

### 4. `## Error Cases`

One entry per business rule, plus any validation errors.

**Format:**
```
| Trigger | Exception class | Error factory / message |
|---------|----------------|------------------------|
| Order not found | NotFoundException | ContentOrderErrors.NotFound(orderId) |
| Order not PendingPayment | BadRequestException | ContentOrderErrors.NotInPendingPayment(orderId) |
| Payment not found | NotFoundException | ContentOrderErrors.PaymentNotFound(orderId) |
| Payment already verified | ConflictException | ContentOrderErrors.PaymentAlreadyVerified(orderId) |
```

**Rules:**
- Always use the module's static error factory class (e.g., `ContentOrderErrors`, `IdentityErrors`)
- If the error factory method doesn't exist yet, list it in the spec and Claude will create it
- Validation errors (empty string, invalid GUID) are thrown by `ValidationDecorator` — do NOT list them in business rules, list them under the Validator section instead

---

### 5. `## Side Effects`

What happens to the database as a result of this operation?

**Format:**
```
1. payment.Verify(adminUserId, receiptUrl) — sets Status=Verified, VerifiedById, VerifiedAt, ReceiptUrl
2. contentOrderRepository.UpdateAsync(order) — persists order status change
3. contentOrderRepository.UpdatePaymentAsync(payment) — persists payment state
4. For each item tier in order.Items: stamp promotion on linked content entity
5. unitOfWork.CommitAsync() — single transaction commit
```

**Rules:**
- List every `AddAsync`, `UpdateAsync`, `DeleteAsync`, and `CommitAsync` call
- If an entity's domain method triggers state change, mention it (e.g., `payment.Verify(...)`)
- List exactly which `IUnitOfWork.CommitAsync()` is called (IContentUnitOfWork, IIdentityUnitOfWork, etc.)

---

### 6. `## Response Shape`

Exact C# record that the command/query returns.

**Format:**
```csharp
// Command result
public record AdminVerifyPaymentResult(bool IsSuccess);

// Query result (inline DTO or mapped from entity)
public record AdminGetOrderPaymentResult(
    Guid Id,
    Guid OrderId,
    decimal AmountUsd,
    string Status,         // "Pending" | "Verified" | "Rejected"
    string? PaymentMethod,
    string? ReceiptUrl,
    string? Notes,
    DateTimeOffset? VerifiedAt
);
```

**Rules:**
- Simple mutations return `(bool IsSuccess)` or nothing (`ICommand`)
- Queries return a typed result record, never the entity directly
- String enums use the enum name (`.ToString()` or `nameof()`)

---

### 7. `## Endpoint`

HTTP verb, route, request body record, response body record, auth policies, rate limiting.

**Format:**
```
Method:       PATCH
Route:        /api/v1/admin/orders/{id}/payment/verify
Request body: AdminVerifyPaymentRequest { ReceiptUrl: string }
Response:     AdminVerifyPaymentResponse { IsSuccess: bool }
Auth:         AccountStatusPolicies.RequireActiveUser
              UserRolePolicies.RequireAdminOrSuperAdmin
Rate limit:   RateLimitPolicies.ContentBrowsing
Produces:
  200 OK      AdminVerifyPaymentResponse
  400         ProblemDetails (validation)
  401         ProblemDetails (unauthenticated)
  403         ProblemDetails (insufficient role)
  404         ProblemDetails (order or payment not found)
  409         ProblemDetails (payment already verified/rejected)
  429         ProblemDetails (rate limit)
```

**Rules:**
- List all `Produces<T>()` and `ProducesProblem()` declarations
- List both `WithAuthorization()` calls if two policies apply
- Specify route group (e.g., `ContentConstants.Admin + "/" + CommerceRouteConstants.Orders`)
- JWT-sourced fields extracted via `IClaimsProvider.GetUserIdFromClaims(user)` before building command

---

### 8. `## Validator`

If the command has inputs that require FluentValidation rules, list every rule here.

**Format:**
```
OrderId  : IsValidGuid("Order ID")          // extension from shared validators
ReceiptUrl: NotEmpty + MaximumLength(500)
AdminUserId: NotEmpty
```

**Rules:**
- Use the shared validator extension methods when they exist (`IsValidGuid`, `ValidReceiptUrl`, etc.)
- If a new extension is needed, name it and describe it
- If a field needs no validation (e.g., `Guid AdminUserId` from claims), note that too

---

### 9. `## Test Cases`

Named list of every unit test to write. Each test maps to one `[Fact]` method.

**Format:**
```
Handler tests (AdminVerifyPaymentHandlerTests):
  [Happy path]
  - Handle_WhenAllValid_ShouldReturnSuccess

  [Failure paths]
  - Handle_WhenOrderNotFound_ShouldThrowNotFoundException
  - Handle_WhenPaymentNotFound_ShouldThrowNotFoundException

Validator tests (AdminVerifyPaymentValidatorTests):
  - Validate_WhenAllValid_ShouldPass
  - Validate_WhenOrderIdIsEmpty_ShouldFail_OnOrderId
  - Validate_WhenOrderIdIsInvalidGuid_ShouldFail_OnOrderId
  - Validate_WhenReceiptUrlIsEmpty_ShouldFail_OnReceiptUrl
  - Validate_WhenReceiptUrlExceedsMaxLength_ShouldFail_OnReceiptUrl

Factory tests (AdminVerifyPaymentFactoryTests):
  - VerifyAsync_WhenAllValid_ShouldVerifyPaymentAndCommit
  - VerifyAsync_WhenOrderNotPendingPayment_ShouldThrowBadRequestException
  - VerifyAsync_WhenPaymentNotPending_ShouldThrowConflictException
```

**Rules:**
- Use the naming convention: `MethodName_WhenCondition_ShouldExpectedBehavior`
- Group by test class
- Every business rule gets its own failure path test
- Every validator field gets its own failure test (empty, invalid format, exceeds max)
- Handler tests only test the handler's logic — not the factory's (that's for factory tests)

---

### 10. `## Dependencies`

List every injected dependency by interface name. Claude will use this to generate the constructor.

**Format:**
```
Handler:
  - IContentOrderRepository
  - IOrderPaymentFactory
  - IVerifyPaymentFactory

Factory:
  - IContentOrderRepository
  - IContentUnitOfWork
```

**Rules:**
- List the interface, not the implementation
- If it's a new interface that doesn't exist yet, mark it with `(NEW)` and define it in the spec
- Order matches constructor parameter order