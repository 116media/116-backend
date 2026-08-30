# SDD Workflow with Claude

Step-by-step process for every new feature, from idea to merged code.

---

## Phase 1: Understand Before Speccing

Before writing a single line of spec, answer these questions:

```
1. What domain concept is this? (Order, Payment, Article, Category...)
2. What entities are involved? Which is the aggregate root?
3. What is the current state of those entities?
4. What transitions happen?
5. Who calls this? (Admin, SuperAdmin, Public, System)
6. What persists as a result?
```

If you can't answer all six, you are not ready to write the spec.

---

## Phase 2: Write the Spec

Use the appropriate template:
- Command → `02-command-spec-template.md`
- Query → `03-query-spec-template.md`
- Domain entity → `04-domain-entity-spec-template.md`
- Factory → `05-factory-spec-template.md`

Write every section. Run the checklist at the end of the template.

**Spec quality check — read it aloud as the implementer:**

> "I need to implement `AdminRestoreOrder`. Looking at the spec:
> I'll parse the OrderId from the route. I'll load the order.
> If null, throw `ContentOrderErrors.NotFound`. I'll call `order.EnsureCancelled()`.
> Then `order.Restore()`. Then `UpdateAsync`. Then `CommitAsync`. Return `(IsSuccess: true)`."

If any step required a guess, the spec needs more detail.

---

## Phase 3: Hand to Claude

Give Claude the spec and a clear instruction. Nothing else.

### Prompt templates

**For a new use case:**
```
Implement this spec. Follow all patterns in CLAUDE.md and projects/how-to-tests/.
Do not invent anything not in the spec. Create all files listed in the spec.

[paste the spec here]
```

**For a new domain method only:**
```
Add the domain methods described in this spec to [EntityName]Entity.
Also add the corresponding test cases to [EntityName]Tests.

[paste the domain section of the spec]
```

**For tests only (when implementation already exists):**
```
Write all test cases listed in this spec. Use the test infrastructure in projects/how-to-tests/.
Do not modify any production code.

[paste the Test Cases section]
```

---

## Phase 4: Review Claude's Output

Review against the spec, not your intuition. Every item in the spec must be present in the output.

### Review checklist

**Command / Query:**
- [ ] Command record matches spec exactly (field names, types, source)
- [ ] Handler constructor injects exactly the dependencies listed in spec
- [ ] Every business rule has a corresponding guard call
- [ ] Every error case uses the correct exception class and error factory
- [ ] Every side effect (`UpdateAsync`, `AddAsync`, `CommitAsync`) is present
- [ ] Validator has a rule for every field listed in the Validator section
- [ ] Endpoint uses `string id` for mutating verbs, `Guid id` for GET
- [ ] Endpoint has both `WithAuthorization()` calls
- [ ] Endpoint has all `Produces<T>()` and `ProducesProblem()` from the spec
- [ ] MetaField description matches spec text

**Tests:**
- [ ] Every test case named in the spec exists as a `[Fact]` or `[Theory]`
- [ ] Happy path verifies side effects (VerifyUpdateCalled, VerifyCommitCalled)
- [ ] Failure paths use the correct factory method (`CreateSubmitted()`, not `.Submit()`)
- [ ] Async exceptions use `Func<Task> act = async () => ...` not `Action act = () => ...`
- [ ] No side effect verification on failure paths

**Domain methods:**
- [ ] Guard method throws the correct exception class
- [ ] State transition sets the correct property value
- [ ] All invalid transitions tested with `[Theory][InlineData(...)]`

---

## Phase 5: Iterate on the Spec, Not the Code

If Claude's output is wrong, **fix the spec first**, then re-run Claude.

Do not manually patch Claude's output and call it done. That produces drift between the spec and the code. The spec is the source of truth. If the code is wrong, the spec was unclear.

```
Wrong approach:
  → Claude generates wrong error factory name
  → Manually edit the file
  → Spec still says the wrong name
  → Next feature: Claude uses the wrong name again

Right approach:
  → Claude generates wrong error factory name
  → Update the spec with the correct error factory name
  → Re-run Claude with the corrected spec
  → Output is correct
  → Spec is authoritative for future features
```

---

## Phase 6: Register Dependencies

After Claude generates the files, manually verify module registration in `ContentModule.cs`:

```csharp
// Factory must be registered (if applicable)
services.AddScoped<IMyFeatureFactory, AdminMyFeatureFactory>();

// Carter modules auto-discover via assembly scan — no manual registration needed
// Handlers auto-register via Scrutor — no manual registration needed
```

---

## Phase 7: Run Tests

```bash
# Run only the new tests
dotnet test tests/Unit --filter "FullyQualifiedName~AdminMyFeature"

# Run full unit test suite
dotnet test tests/Unit

# Run domain tests only
dotnet test tests/Unit --filter "FullyQualifiedName~Domain"
```

All tests must pass before merging.

---

## Common Mistakes and How to Avoid Them

| Mistake | Consequence | Prevention |
|---------|-------------|------------|
| Writing the spec after implementation | Spec becomes documentation, not a contract | Always spec first |
| Omitting the error cases section | Claude invents error messages | List every error with its exact factory |
| Saying "validate the ID" instead of `IsValidGuid("Order ID")` | Claude uses wrong extension or wrong message | Reference the exact extension method |
| Omitting `CommitAsync` from side effects | Claude forgets to commit | Every spec ends with `unitOfWork.CommitAsync` |
| Writing test names vaguely | Claude produces wrong `[Fact]` method names | Name every test exactly as it should appear in code |
| Not listing new artifacts | Claude doesn't know to create the error factory | Section: "New artifacts needed" |
| Spec says `Guid id` in route for PATCH | Claude generates wrong endpoint | Always `string id` for mutating verbs |

---

## Velocity Pattern

Once you're comfortable with the format, SDD is faster than free-form Claude prompting:

```
Week 1: Write spec (30 min) → Claude implements (2 min) → Review (15 min) → Done
Week 1 without SDD: Brief Claude (10 min) → Output wrong (2 min) → Correct Claude (10 min)
         → Still wrong (2 min) → Fix manually (30 min) → Realize tests missing (20 min)
         → Ask for tests (10 min) → Test patterns wrong (10 min) → Fix manually (20 min)
         → Total: ~114 min
```

The spec is the investment that pays off every time Claude gets it right on the first pass.