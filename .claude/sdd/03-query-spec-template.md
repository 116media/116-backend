# Query Spec Template

Copy this template for every new read use case (GET).
Queries have no side effects. They load data, map it, and return a DTO.

---

```markdown
# Spec: [AdminMyQueryQuery]

## Intent

[One to three sentences. Who queries, what they get back, and why it exists.]

---

## Query Shape

| Field | Type | Source | Constraints |
|-------|------|--------|-------------|
| `EntityId` | `Guid` | Route param `{id}` | Route constraint (no Guid.Parse needed) |
| `CustomerId` | `Guid?` | Query string `?customerId=` | Optional filter |
| `Page` | `int` | Query string | Default 1 |
| `PageSize` | `int` | Query string | Default 20, max 100 |

**Record definition:**
```csharp
public record AdminMyQuery(Guid EntityId) : IQuery<AdminMyQueryResult>;

// or paginated:
public record AdminMyQuery(
    Guid? CustomerId,
    int Page,
    int PageSize
) : IQuery<PagedResult<AdminMyQueryResult>>;
```

---

## Business Rules

1. [Entity] must exist (return 404 if not found)
2. [Optional filter entity] must exist if provided
3. [Any access restriction: e.g., only visible to the requesting admin's scope]

---

## Error Cases

| Trigger | Exception class | Error factory / message |
|---------|----------------|------------------------|
| [Entity] not found | `NotFoundException` | `[Module]Errors.NotFound(id)` |

---

## Data Loading

Describe what the handler loads from repositories:

1. Load `[EntityType]` via `[repository].[Method](id, ct)` — with or without `.Include()` (navigation properties)
2. Map to result using `[EntityType].ToAdminMyQueryResult()` extension (Mapster or manual)

If paginated:
1. Load `IQueryable<[EntityType]>` from repository
2. Apply specification: `[Module][Entity]Specifications.[SpecName](filters)`
3. Apply `OrderBy([field])` and `ToPagedListAsync(page, pageSize, ct)`
4. Map each item

---

## Response Shape

```csharp
// Single entity
public record AdminMyQueryResult(
    Guid Id,
    string FieldOne,
    string? OptionalField,
    string Status,           // enum .ToString()
    DateTimeOffset CreatedAt
);

// Paginated
public record PagedResult<AdminMyQueryResult>(
    IReadOnlyList<AdminMyQueryResult> Items,
    int Page,
    int PageSize,
    int TotalCount
);
```

---

## Mapper / Extension

If the result is mapped from an entity, describe the mapping:

```csharp
// Extension method on entity (in a Mappers/ file):
public static AdminMyQueryResult ToAdminMyQueryResult(this MyEntity entity)
    => new(
        Id: entity.Id,
        FieldOne: entity.FieldOne,
        OptionalField: entity.OptionalField,
        Status: entity.Status.ToString(),
        CreatedAt: entity.CreatedAt
    );

// Or null-safe (when entity may be null):
public static AdminMyQueryResult? ToAdminMyQueryResult(this MyEntity? entity)
    => entity is null ? null : new(...);
```

---

## Endpoint

```
Method:       GET
Route:        /api/v1/admin/[resource]/{id}
Response:     AdminMyQueryResult
Auth:         AccountStatusPolicies.RequireActiveUser
              UserRolePolicies.RequireAdminOrSuperAdmin
Rate limit:   RateLimitPolicies.ContentBrowsing
Route group:  ContentConstants.Admin + "/" + [Module]RouteConstants.[Resource]
Produces:
  200 OK      AdminMyQueryResult
  401         ProblemDetails
  403         ProblemDetails
  404         ProblemDetails
  429         ProblemDetails
```

**Endpoint notes:**
- `Guid id` in route is acceptable for GET (route constraint provides implicit validation)
- No JWT extraction needed unless result is scoped to the requesting user
- No request body for GET

---

## Dependencies

**Handler:**
- `I[Module]Repository`
- `IOrderPaymentFactory` (if using a factory for sub-entity lookup)

---

## MetaField

```csharp
public static class AdminMyQueryMetaField
{
    public static readonly RouteMetadata AdminMyQuery = new(
        "AdminMyQuery",
        "[One-line Swagger summary]",
        """
        [Full Swagger description. Include what is returned, filters supported, etc.]

        **Authentication Requirements:**
        - User must be authenticated with a valid access token
        - User must have Admin or SuperAdmin role

        **Response Codes:**
        - Returns 200 OK with the result
        - Returns 401 Unauthorized if access token is invalid
        - Returns 403 Forbidden if user lacks Admin role
        - Returns 404 Not Found if [entity] does not exist
        """
    );
}
```

---

## Test Cases

**Handler tests (`AdminMyQueryHandlerTests`):**

```
[Happy path]
- Handle_WhenEntityExists_ShouldReturnMappedResult

[Failure paths]
- Handle_WhenEntityNotFound_ShouldThrowNotFoundException
```

**Mapper tests (`AdminMyQueryMapperTests` or inline in handler tests):**

```
- ToAdminMyQueryResult_WhenEntityIsValid_ShouldMapAllFields
- ToAdminMyQueryResult_WhenEntityIsNull_ShouldReturnNull   (if null-safe)
```

**Specification tests (if query uses a specification):**

```
- [SpecName]_WhenMatchingEntity_ShouldReturnTrue
- [SpecName]_WhenNonMatchingEntity_ShouldReturnFalse
- [SpecName]_WhenFilterIsNull_ShouldMatchAll
```
```

---

## Key differences from Command specs

| | Command | Query |
|---|---------|-------|
| Side effects | Yes — persist, commit | None |
| Business rules | State checks, existence | Existence only |
| Validator | Usually present | Rarely needed (route constraint handles GUID) |
| Response | `(bool IsSuccess)` or simple DTO | Rich DTO or paginated list |
| Route param type | `string id` (Guid.Parse in handler) | `Guid id` (route constraint acceptable) |
| Tests | Handler + Validator + Factory | Handler + Mapper |