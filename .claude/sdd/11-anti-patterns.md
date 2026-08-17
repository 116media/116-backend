# SDD Anti-Patterns

These are the most common ways specs go wrong and what Claude does when they're wrong.

---

## 1. The Vague Intent

**Bad:**
```
## Intent
Updates payment status.
```

**What Claude does:** Invents a `UpdatePaymentStatusCommand` that sets the `Status` property directly,
bypassing domain methods. Doesn't know whether to go through the factory or not.
Doesn't know what state is allowed.

**Good:**
```
## Intent
Allows an Admin to verify that a customer has paid for an order by confirming receipt
of payment and recording a receipt URL. Transitions the payment from Pending to Verified
and the order from PendingPayment to Paid. Stamps any promotion-level content items
purchased in the order.
```

**Rule:** Name the actor, the action verb, the target entity, and the business outcome.

---

## 2. Missing Error Cases

**Bad:**
```
## Error Cases
- Order not found → 404
```

**What Claude does:** Writes `throw new NotFoundException("Order not found.")` with a hardcoded message
instead of `throw ContentOrderErrors.NotFound(orderId)`. Future error message changes require
hunting through every handler instead of updating one error class.

**Good:**
```
## Error Cases
| Trigger | Exception class | Error factory |
|---------|----------------|---------------|
| Order not found | NotFoundException | ContentOrderErrors.NotFound(orderId) |
| Order not PendingPayment | BadRequestException | ContentOrderErrors.NotInPendingPayment(orderId) |
| Payment not found | NotFoundException | thrown by IOrderPaymentFactory.GetByOrderIdOrThrowAsync |
| Payment already processed | ConflictException | ContentOrderErrors.PaymentAlreadyProcessed(orderId) |
```

**Rule:** Every business rule has exactly one error case. List the exception class AND the factory method.

---

## 3. Omitting Side Effects

**Bad:**
```
## Side Effects
Order is updated.
```

**What Claude does:** Calls `UpdateAsync` on the order but forgets to call `UpdateAsync` on the payment,
or calls them both but forgets `CommitAsync`. Transaction is never committed.

**Good:**
```
## Side Effects
1. payment.Verify(adminUserId, receiptUrl) — sets Status=Verified, VerifiedById, VerifiedAt, ReceiptUrl
2. order.MarkPaid() — sets Status=Paid
3. contentOrderRepository.UpdateAsync(order, ct)
4. contentOrderRepository.UpdatePaymentAsync(payment, ct)
5. unitOfWork.CommitAsync(ct) — single transaction commit
```

**Rule:** List every `UpdateAsync`, `AddAsync`, `DeleteAsync`, and `CommitAsync` call in order.

---

## 4. Wrong Route Param Type

**Bad:**
```
## Endpoint
Method: PATCH
Route: /api/v1/admin/orders/{id}/verify
```
(No mention of whether `id` is `string` or `Guid`)

**What Claude does:** Generates `Guid id` in the endpoint delegate. Invalid GUIDs from the client
go through route matching, fail silently (no route match), and return 404 instead of 400.

**Good:**
```
## Endpoint
Method: PATCH
Route: /api/v1/admin/orders/{id}/verify
Note: string id in route (not Guid) — Guid.Parse happens in the handler
```

**Rule:**
- Mutating endpoints (POST, PUT, PATCH, DELETE with path param): `string id`
- GET endpoints: `Guid id` acceptable (route constraint)

---

## 5. Naming Tests Vaguely

**Bad:**
```
## Test Cases
- Happy path test
- Order not found test
- Wrong status test
```

**What Claude does:** Invents method names like `TestHappyPath()`, `TestNotFound()`, `TestWrongStatus()`.
These don't follow the `MethodName_WhenCondition_ShouldExpectedBehavior` convention,
fail code review, and are harder to find when a test breaks.

**Good:**
```
## Test Cases
- Handle_WhenAllValid_ShouldReturnSuccess
- Handle_WhenOrderNotFound_ShouldThrowNotFoundException
- Handle_WhenOrderNotPendingPayment_ShouldThrowBadRequestException
```

**Rule:** Write the exact `[Fact]` method name. Claude uses it verbatim.

---

## 6. Forgetting New Artifacts

**Bad spec for a new feature:** Doesn't mention that `ContentOrderErrors.NotInCancelled` doesn't exist yet.

**What Claude does:** References `ContentOrderErrors.NotInCancelled(orderId)` in the handler,
but the method doesn't exist. The code doesn't compile. You don't know until you run the build.

**Good:**
```
## New Artifacts Required
- ContentOrderEntity.Restore() — new domain method
- ContentOrderEntity.EnsureCancelled() — new guard
- ContentOrderErrors.NotInCancelled(Guid orderId) — new error factory method
```

**Rule:** Any type, method, or constant referenced in the spec that doesn't yet exist
must be listed in a "New Artifacts Required" section.

---

## 7. Mixing Handler Logic and Factory Logic

**Bad business rules:**
```
## Business Rules
1. Order must exist
2. Order must be in Draft status
3. Item must exist
4. Pricing tier must exist
5. Category pricing must exist
6. Create tier entity
7. Recalculate order total
8. Persist tier
9. Persist order
10. Commit
```
(This is 10 steps in a handler — it should be a factory)

**What Claude does:** Puts all 10 steps in the handler. No factory. No interface. Untestable in isolation.

**Good:**
- Handler spec: rules 1–2 (order exists, order is Draft), delegates rest to `IAddItemTierFactory`
- Factory spec: rules 3–10

**Rule:**
- More than 2 entity loads → use a factory
- More than 1 persist → use a factory
- Handler spec focuses on orchestration; factory spec focuses on the complex logic

---

## 8. Spec Written After Implementation

**Pattern:** "Let me implement this quickly and then write the spec."

**What happens:** The spec describes what was built, not what should be built.
Edge cases discovered during implementation aren't in the spec.
Error messages are copied from code, not from a design decision.
The spec is now documentation, not a contract.

**Rule:** Write the spec first, always. If implementation reveals a gap in the spec,
stop, update the spec, then continue. Never let the code lead the spec.

---

## 9. Incomplete Validator Section

**Bad:**
```
## Validator
Validate OrderId and ReceiptUrl.
```

**What Claude does:** Writes any rules it thinks make sense. Might miss `MaximumLength`.
Might use wrong error message. Might not use the shared `IsValidGuid` extension.

**Good:**
```
## Validator
| Field | Rule | Notes |
|-------|------|-------|
| `OrderId` | `IsValidGuid("Order ID")` | Uses shared extension from CommerceValidation |
| `ReceiptUrl` | `ValidReceiptUrl()` | Uses shared extension: NotEmpty + MaximumLength(500) |
| `AdminUserId` | `NotEmpty()` with message "Admin user ID is required." | |
```

**Rule:** List every field, its exact rule method, and the error message if it's not default.

---

## 10. Testing Side Effects on Failure Paths

**Bad test:**
```csharp
[Fact]
public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
{
    _orderRepositoryMock.SetupGetByIdWithItems(null);

    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
    _unitOfWorkMock.VerifyCommitCalled();  // ← WRONG: commit should NOT be called
}
```

**Rule:** On failure paths, do NOT call `Verify*` methods. The factory/handler bails before
reaching the persist/commit calls. Verifying them passes but gives false confidence.

**Good:** Failure path tests assert only the exception. Happy path tests verify side effects.

---

## Summary Table

| Anti-pattern | Impact | Fix |
|-------------|--------|-----|
| Vague intent | Wrong orchestration pattern | Name actor + action + entity + outcome |
| Missing error cases | Hardcoded strings, wrong exception types | Full table: trigger → class → factory |
| Omitting side effects | Missing commit, missing update | List every repository call + CommitAsync |
| Wrong route param type | Silent 404 on invalid GUIDs | `string id` for mutating verbs |
| Vague test names | Wrong naming convention | Write exact `[Fact]` method names |
| Forgetting new artifacts | Compile errors | "New Artifacts" section |
| Mixed handler/factory logic | Untestable handler | Handler ≤ 2 loads; factory for rest |
| Spec after implementation | Spec is docs, not contract | Spec first, always |
| Incomplete validator | Wrong rules, missing extensions | Field-by-field table |
| Verifying side effects on failures | False test confidence | Verify only on happy path |