# Claude Prompt Patterns for SDD

How to phrase prompts when handing specs to Claude. The prompt is as important as the spec.
A vague prompt wastes the precision of a good spec.

---

## Pattern 1: Implement a complete use case from spec

Use when: You have a complete command or query spec and want all files generated at once.

```
Implement the following spec. Produce all files described.
Follow all conventions in CLAUDE.md and the test patterns in projects/how-to-tests/.
Do not invent any behavior not explicitly stated in the spec.
If a new error factory method or domain method is listed in "New Artifacts Required",
create or modify those files first, then create the use case files.

---
[paste the full spec here]
```

---

## Pattern 2: Add domain methods to an existing entity

Use when: You only need to add `Restore()` + `EnsureCancelled()` to `ContentOrderEntity`.

```
Add the following domain methods to ContentOrderEntity.
File: src/Modules/Content/Content/Domain/Entities/ContentOrderEntity.cs

Do not modify any other files.
Follow the patterns in the existing methods of that file.

Methods to add:

EnsureCancelled():
  - Purpose: guard — verifies Status == Cancelled
  - Throws: BadRequestException using ContentOrderErrors.NotInCancelled(Id)
  - Returns: void

Restore():
  - Precondition: call EnsureCancelled() (throws if not Cancelled)
  - State change: Status = EnumOrderStatus.Draft
  - Returns: void

Then add these test cases to tests/Unit/Modules/Content/Domain/ContentOrderEntityTests.cs:
  - Restore_WhenCancelled_ShouldTransitionToDraft
  - Restore_WhenNotCancelled_ShouldThrowBadRequestException [Theory, InlineData for Draft/PendingPayment/Paid]
  - EnsureCancelled_WhenCancelled_ShouldNotThrow
  - EnsureCancelled_WhenNotCancelled_ShouldThrowBadRequestException [Theory]
```

---

## Pattern 3: Write tests only (implementation already exists)

Use when: Production code is done but tests are missing or incomplete.

```
Write unit tests for AdminVerifyPaymentFactory.
The implementation is at:
  src/Modules/Content/Content/Application/Commerce/UseCases/Admin/Commands/VerifyPayment/AdminVerifyPaymentFactory.cs

Use the test patterns in projects/how-to-tests/11-writing-factory-tests.md.
Test file goes in:
  tests/Unit/Modules/Content/Application/Commerce/UseCases/Admin/Commands/VerifyPayment/AdminVerifyPaymentFactoryTests.cs

Test cases to write:
  VerifyAsync_WhenAllValid_ShouldVerifyPaymentMarkOrderPaidAndCommit
    Arrange: ContentOrderFactory.CreateSubmitted(), ContentPaymentFactory.Create(orderId)
    Verify: VerifyUpdateCalled (order + payment), VerifyCommitCalled

  VerifyAsync_WhenOrderNotPendingPayment_ShouldThrowBadRequestException
    Use: ContentOrderFactory.Create() (Draft)

  VerifyAsync_WhenPaymentAlreadyVerified_ShouldThrowConflictException
    Use: ContentPaymentFactory.CreateVerified(orderId, adminId)
```

---

## Pattern 4: Write a validator and its tests

Use when: The handler exists but the validator is missing.

```
Write a FluentValidation validator for AdminAttachPaymentProofCommand.

Command record (already exists):
  public record AdminAttachPaymentProofCommand(
      string OrderId,
      IFormFile ProofFile,
      string PaymentMethod
  ) : ICommand<AdminAttachPaymentProofResult>;

Rules:
  OrderId:       IsValidGuid("Order ID")
  ProofFile:     NotNull with message "Payment proof file is required."
  PaymentMethod: NotEmpty with message "Payment method is required."
                 Must(m => Enum.TryParse<EnumPaymentMethod>(m, out _))
                 with message "Invalid payment method."

Validator file: AdminAttachPaymentProofValidator.cs (same folder as command)

Tests (AdminAttachPaymentProofValidatorTests.cs):
  - Validate_WhenAllValid_ShouldPass
  - Validate_WhenOrderIdIsEmpty_ShouldFail_OnOrderId
  - Validate_WhenOrderIdIsInvalidGuid_ShouldFail_OnOrderId
  - Validate_WhenProofFileIsNull_ShouldFail_OnProofFile
  - Validate_WhenPaymentMethodIsEmpty_ShouldFail_OnPaymentMethod
  - Validate_WhenPaymentMethodIsInvalid_ShouldFail_OnPaymentMethod
```

---

## Pattern 5: Extend an existing query with a new filter

Use when: A query exists and you want to add filtering without breaking the existing tests.

```
Extend AdminGetAllOrdersQuery to support filtering by Status.

Current query:
  public record AdminGetAllOrdersQuery(int Page, int PageSize)
    : IQuery<PagedResult<AdminGetAllOrdersResult>>;

New query (add optional Status filter):
  public record AdminGetAllOrdersQuery(
    int Page,
    int PageSize,
    string? Status = null    // filter: "Draft" | "PendingPayment" | "Paid" | "Cancelled"
  ) : IQuery<PagedResult<AdminGetAllOrdersResult>>;

Handler change:
  If Status is provided and valid, parse to EnumOrderStatus and apply
  ContentOrderByStatusSpecification to the queryable.
  If Status is null, no filter applied (return all).
  If Status is provided but invalid, throw BadRequestException("Invalid order status value.").

Specification: ContentOrderByStatusSpecification already exists.

New test cases to add to AdminGetAllOrdersHandlerTests.cs:
  - Handle_WhenStatusFilterIsNull_ShouldReturnAllOrders
  - Handle_WhenStatusFilterIsValid_ShouldReturnFilteredOrders
  - Handle_WhenStatusFilterIsInvalid_ShouldThrowBadRequestException

Do NOT modify existing passing tests.
```

---

## Pattern 6: Generate the error factory class for a new module

Use when: You're starting a new module and need the error class set up.

```
Create a static error factory class for the Subscription module.

File: src/Modules/Subscription/Subscription/Application/Shared/Errors/SubscriptionErrors.cs

Pattern to follow:
  src/Modules/Content/Content/Application/Commerce/Shared/Errors/ContentOrderErrors.cs

Include these error methods:
  NotFound(Guid id)         → NotFoundException("Subscription {id} was not found.")
  AlreadyActive(Guid id)    → ConflictException("Subscription {id} is already active.")
  NotActive(Guid id)        → BadRequestException("Subscription {id} is not active.")
  AlreadyCancelled(Guid id) → ConflictException("Subscription {id} has already been cancelled.")
  PaymentFailed(Guid id)    → BadRequestException("Payment failed for subscription {id}.")
```

---

## Pattern 7: Implement a spec in stages

Use when: The feature is large and you want to review each part before the next.

**Stage 1 — Domain:**
```
Implement only the domain layer from this spec.
Create or modify the entity, add domain methods, add domain tests.
Stop before creating any use case files.

[paste domain spec section]
```

**Stage 2 — Use case (after reviewing Stage 1):**
```
The domain changes from Stage 1 are complete and reviewed.
Now implement the use case layer: command, handler, validator, factory (if any).
Do not create tests yet.

[paste use case spec sections]
```

**Stage 3 — Tests (after reviewing Stage 2):**
```
Production code is complete and reviewed.
Now write the test cases listed in the spec.

[paste test cases section]
```

**Stage 4 — Endpoint:**
```
Handler and tests are complete. Now create the Carter endpoint and MetaField.

[paste endpoint and metafield spec sections]
```

---

## Do's and Don'ts

| Do | Don't |
|----|-------|
| Paste the full spec | Summarize the spec ("it's like Submit but reversed") |
| Name the exact files to create | Say "create the necessary files" |
| Reference existing factories and mocks by name | Say "mock the repository" |
| List exact test method names | Say "write tests for this" |
| Specify which test file to write to | Say "add tests" |
| Ask for one stage at a time for large features | Dump a 200-line spec and expect perfection |
| Specify "Do not modify existing tests" when extending | Let Claude guess what it can touch |