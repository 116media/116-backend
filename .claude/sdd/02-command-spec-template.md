# Command Spec Template

Copy this template for every new mutation use case (POST, PUT, PATCH, DELETE).
Fill in every section. Delete nothing. If a section doesn't apply, write "N/A" — never leave it blank.

---

```markdown
# Spec: [AdminMyFeatureCommand]

## Intent

[One to three sentences describing what this use case does, who does it, and why.]

---

## Command Shape

| Field | Type | Source | Constraints |
|-------|------|--------|-------------|
| `FieldName` | `string` | Route param `{id}` | Must be a valid GUID |
| `OtherField` | `string` | Request body | Required, max N chars |
| `ActorId` | `Guid` | JWT claims via `IClaimsProvider` | Not empty |

**Record definition:**
```csharp
public record AdminMyFeatureCommand(
    string FieldName,
    string OtherField,
    Guid ActorId
) : ICommand<AdminMyFeatureResult>;

public record AdminMyFeatureResult(bool IsSuccess);
```

---

## Business Rules

1. [Entity] must exist ([repository method] returns non-null)
2. [Entity] must be in [State] status ([entity].EnsureXxx())
3. [Dependency entity] must exist ([factory or repository method])
4. [Other invariant]

---

## Error Cases

| Trigger | Exception class | Error factory / message |
|---------|----------------|------------------------|
| [Entity] not found | `NotFoundException` | `[Module]Errors.NotFound(id)` |
| [Entity] wrong state | `BadRequestException` | `[Module]Errors.NotIn[State](id)` |
| [Conflict condition] | `ConflictException` | `[Module]Errors.[ConflictName](id)` |

---

## Side Effects

1. `[entity].[DomainMethod](params)` — describe what it changes on the entity
2. `[repository].UpdateAsync([entity], ct)` — persists the updated entity
3. `[repository].AddAsync([newEntity], ct)` — persists a new entity (if any)
4. `[unitOfWork].CommitAsync(ct)` — single commit at the end

---

## Response Shape

```csharp
public record AdminMyFeatureResult(bool IsSuccess);
```

---

## Validator

| Field | Rule | Error message |
|-------|------|---------------|
| `FieldName` | `IsValidGuid("Field label")` | (set by extension) |
| `OtherField` | `NotEmpty()` + `MaximumLength(N)` | "Field is required." / "Max N chars." |

**Validator class:**
```csharp
public class AdminMyFeatureValidator : AbstractValidator<AdminMyFeatureCommand>
{
    public AdminMyFeatureValidator()
    {
        RuleFor(x => x.FieldName).IsValidGuid("Field label");
        RuleFor(x => x.OtherField).NotEmpty().WithMessage("...").MaximumLength(N);
    }
}
```

---

## Endpoint

```
Method:       PATCH
Route:        /api/v1/admin/[resource]/{id}/[action]
Request body: AdminMyFeatureRequest { OtherField: string }
Response:     AdminMyFeatureResponse { IsSuccess: bool }
Auth:         AccountStatusPolicies.RequireActiveUser
              UserRolePolicies.RequireAdminOrSuperAdmin
Rate limit:   RateLimitPolicies.ContentBrowsing
Route group:  ContentConstants.Admin + "/" + [Module]RouteConstants.[Resource]
Produces:
  200 OK      AdminMyFeatureResponse
  400         ProblemDetails (validation or business rule failure)
  401         ProblemDetails
  403         ProblemDetails
  404         ProblemDetails
  409         ProblemDetails (if conflict possible)
  429         ProblemDetails
```

**Endpoint implementation notes:**
- `string id` in route (not `Guid`) — Guid.Parse happens in the handler
- JWT fields extracted via `IClaimsProvider.GetUserIdFromClaims(user)` before building command
- No business logic in the endpoint delegate

---

## Dependencies

**Handler:**
- `I[Module]Repository`
- `I[Factory]Factory` (if applicable)

**Factory (if applicable):**
- `I[Module]Repository`
- `I[Module]UnitOfWork`

---

## MetaField

```csharp
public static class AdminMyFeatureMetaField
{
    public static readonly RouteMetadata AdminMyFeature = new(
        "AdminMyFeature",
        "[One-line summary shown in Swagger]",
        """
        [Full description shown in Swagger. Include:]
        [- What the operation does]
        [- Pre-conditions (entity state requirements)]
        [- Side effects (what gets created/updated)]

        **Authentication Requirements:**
        - User must be authenticated with a valid access token
        - User must have Admin or SuperAdmin role

        **Response Codes:**
        - Returns 200 OK on success
        - Returns 400 Bad Request if [condition]
        - Returns 401 Unauthorized if access token is invalid or expired
        - Returns 403 Forbidden if user lacks Admin role
        - Returns 404 Not Found if [entity] does not exist
        - Returns 409 Conflict if [conflict condition]
        """
    );
}
```

---

## Test Cases

**Handler tests (`AdminMyFeatureHandlerTests`):**

```
[Happy path]
- Handle_WhenAllValid_ShouldReturnSuccess

[Failure paths — one per business rule]
- Handle_WhenOrderNotFound_ShouldThrowNotFoundException
- Handle_WhenOrderNotInExpectedState_ShouldThrowBadRequestException
```

**Validator tests (`AdminMyFeatureValidatorTests`):**

```
- Validate_WhenAllValid_ShouldPass
- Validate_WhenFieldNameIsEmpty_ShouldFail_OnFieldName
- Validate_WhenFieldNameIsInvalidGuid_ShouldFail_OnFieldName
- Validate_WhenOtherFieldIsEmpty_ShouldFail_OnOtherField
- Validate_WhenOtherFieldExceedsMaxLength_ShouldFail_OnOtherField
```

**Factory tests (`AdminMyFeatureFactoryTests`) — if factory exists:**

```
[Happy path]
- MyMethodAsync_WhenAllValid_ShouldPersistAndCommit

[Failure paths — one per business rule enforced in factory]
- MyMethodAsync_WhenEntityNotFound_ShouldThrowNotFoundException
- MyMethodAsync_WhenEntityNotInExpectedState_ShouldThrowBadRequestException
```
```

---

## Checklist Before Handing to Claude

- [ ] Intent is one to three sentences, names the actor and the action
- [ ] Command shape lists every field with type, source, and constraints
- [ ] Business rules are numbered and each names the entity and the check
- [ ] Every business rule has a corresponding error case
- [ ] Side effects list every `AddAsync`, `UpdateAsync`, `DeleteAsync`, `CommitAsync` call
- [ ] Response shape is a typed C# record
- [ ] Endpoint section lists the exact route, both auth policies, rate limit, and all `Produces` declarations
- [ ] Validator section lists every field with its exact rule
- [ ] Test cases list every `[Fact]` by name, grouped by test class
- [ ] Dependencies list every injected interface