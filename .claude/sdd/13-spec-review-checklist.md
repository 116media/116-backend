# Spec Review Checklist

Run this checklist on every spec before handing it to Claude.
A spec that fails any item is not ready.

---

## Universal (all spec types)

- [ ] Intent names the **actor** (Admin, SuperAdmin, Public, System)
- [ ] Intent names the **action verb** (submit, verify, restore, attach, publish...)
- [ ] Intent names the **target entity** (Order, Payment, Article, Category...)
- [ ] Intent states the **business reason** (not just "updates status")
- [ ] All types referenced exist in the codebase OR are listed in "New Artifacts Required"
- [ ] All error factory methods referenced exist OR are listed in "New Artifacts Required"
- [ ] All repository/factory methods referenced exist (check `projects/how-to-tests/04-mocks-repositories.md`)
- [ ] All mock setup methods referenced exist (check `projects/how-to-tests/05-mocks-services-infrastructure.md`)

---

## Command spec

- [ ] Every field has: name, C# type, source, constraints
- [ ] Route param for mutating verb is `string id` (not `Guid id`)
- [ ] JWT-sourced fields state they come from `IClaimsProvider.GetUserIdFromClaims`
- [ ] Every business rule has a corresponding error case (same count)
- [ ] Error cases table has: trigger, exception class, error factory call with exact parameters
- [ ] Side effects list includes every `AddAsync`, `UpdateAsync`, `DeleteAsync` call
- [ ] Side effects end with `unitOfWork.CommitAsync(ct)`
- [ ] Which `IUnitOfWork` implementation is specified (`IContentUnitOfWork`, `IIdentityUnitOfWork`, etc.)
- [ ] Response shape is a typed C# record (not `void` unless using `ICommand`)
- [ ] Validator section lists every input field with its exact rule
- [ ] Validator references existing shared extension methods where applicable
- [ ] Endpoint lists HTTP verb, exact route, request body record, response record
- [ ] Endpoint lists both `WithAuthorization()` policies
- [ ] Endpoint lists rate limit policy
- [ ] Endpoint lists every `Produces<T>()` and `ProducesProblem(statusCode)` call
- [ ] Dependencies list every interface by name
- [ ] MetaField description mentions the operation, pre-conditions, and all response codes
- [ ] Test cases listed: one happy path, one per business rule
- [ ] Test names follow `MethodName_WhenCondition_ShouldExpectedBehavior`
- [ ] Test cases grouped by class (HandlerTests, ValidatorTests, FactoryTests)

---

## Query spec

- [ ] Route param for GET is `Guid id` (route constraint acceptable)
- [ ] Data loading describes which repository method and whether `.Include()` is needed
- [ ] Response shape lists every field with its C# type
- [ ] Enum fields mapped to `string` via `.ToString()`
- [ ] Nullable fields marked with `?`
- [ ] Mapper extension method named and described
- [ ] Null-safe variant specified if entity might be null
- [ ] Test cases include: found (happy path), not found, mapper field mapping tests

---

## Domain entity spec

- [ ] Entity base class specified (`Aggregate<Guid>` or `Entity<Guid>`)
- [ ] All properties with visibility, type, constraints
- [ ] State machine drawn (all valid transitions + all invalid transitions)
- [ ] Every domain method has: precondition, state change, return type, exception on failure
- [ ] Guard methods (`EnsureXxx`) listed separately
- [ ] EF Core table name, schema, column types specified
- [ ] Navigation properties listed (type, ownership direction)
- [ ] Test cases cover every valid + every invalid state transition
- [ ] Invalid transitions use `[Theory][InlineData(...)]`

---

## Factory spec

- [ ] Interface defined (in `Contracts/` folder)
- [ ] Every dependency listed with its interface
- [ ] Step-by-step logic numbered in order
- [ ] Every step that loads an entity states what happens if null
- [ ] Every step that calls a guard states what it throws
- [ ] Return type specified (void, single entity, tuple, named record)
- [ ] Side effects include the specific repository method names
- [ ] Test cases: one happy path verifying all side effects, one per error case
- [ ] Happy path arrange uses real factory-built entities (not manually constructed)
- [ ] Happy path `Verify*` calls listed (VerifyAddCalled, VerifyUpdateCalled, VerifyCommitCalled)
- [ ] Failure path tests use status-specific factory methods (`CreateSubmitted()` not `order.Submit()`)

---

## Gate: Is the spec implementation-ready?

Answer these three questions. All must be "yes":

**1. Can you write the handler constructor from the spec alone?**
→ Check: Dependencies section lists every interface by name.

**2. Can you write every `if` check in the handler/factory from the spec alone?**
→ Check: Business rules map 1:1 to error cases. Every guard method is named.

**3. Can you write every test method signature from the spec alone?**
→ Check: Test cases listed by exact `[Fact]` method name, grouped by test class.

If any answer is "no", identify the gap and fill it before proceeding.