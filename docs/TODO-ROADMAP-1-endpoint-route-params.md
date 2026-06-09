# TODO — ROADMAP Point 1: Standardize Endpoint Route Parameter Parsing

**Branch:** `refactor-endpoint-route-params`
**Scope:** All modules — Identity, Core, Content

---

## Summary

The rule is simple:

| Verb | Approach |
|------|----------|
| `GET /{id}` | `Guid id` — route constraint is fine |
| `DELETE`, `PUT`, `PATCH`, `POST /{parentId}/...` | `string id` + `Guid.Parse(id)` inside handler |

**Current state of the codebase:**
- ~77 mutating endpoints correctly use `string id` for their primary `{id}` param ✅
- All GET endpoints incorrectly use `string id` instead of `Guid id` ❌ (15 endpoints)
- 4 mutating endpoints superficially look correct (`string id` on primary param) but are
  broken on their **secondary** param — `permissionId`, `roleId`, `tierId`, `slotId` still
  use `:guid` route constraint + `Guid param` directly, which is wrong for mutating verbs ❌
  - `AdminRemovePermissionFromRoleEndpointV1` — DELETE `{id}/permissions/{permissionId:guid}`
  - `AdminRemoveRoleFromUserEndpointV1` — DELETE `{id}/roles/{roleId:guid}`
  - `AdminRemoveCategoryPricingEndpointV1` — DELETE `{id}/pricing/{tierId:guid}`
  - `AdminRemovePackageSlotEndpointV1` — DELETE `{id}/slots/{slotId:guid}`
- No `FormatException` handler exists → `Guid.Parse()` on bad input returns 500, not 400 ❌ (BLOCKING)

---

## Step 0 — Prerequisite: Add FormatException Handler (BLOCKING)

> Must be done first. Without this, `Guid.Parse()` on an invalid input throws 500.

### 0.1 — Create `FormatExceptionStrategy`

**File to create:**
`src/Shared/Shared/Application/Exceptions/Handlers/Strategies/FormatExceptionStrategy.cs`

Follow the same pattern as `BadRequestExceptionHandler.cs`:
- Handle `FormatException`
- Return `400 Bad Request` ProblemDetails
- Title: `"InvalidFormat"`, Detail: `"The provided identifier is not a valid UUID."`

### 0.2 — Register the strategy

**File to update:**
`src/Shared/Shared/Application/Exceptions/Handlers/ExceptionStrategyRegistry.cs`
(or wherever strategies are registered — check `ExceptionHandler.cs` for the registration pattern)

Add `FormatExceptionStrategy` alongside the existing strategies.

### 0.3 — Write unit tests for the strategy

**File to create:**
`tests/Unit/Shared/Application/Exceptions/Handlers/Strategies/FormatExceptionStrategyTests.cs`

Test cases:
- `FormatException` → 400 + correct title + correct detail message
- `CanHandle` returns true for `FormatException`, false for other exceptions

---

## Step 1 — Fix GET Endpoints: `string id` → `Guid id`

Per the ROADMAP, GET endpoints must use `Guid id` directly (route constraint handles invalid input gracefully).

All 15 GET endpoints below currently declare `string id` in the handler lambda and must be updated to `Guid id`. Also update the query constructor call — remove `Guid.Parse(id)` if present, or change `id` directly (the command likely takes `string id` which gets parsed internally; check and align).

### Identity Module

- [ ] `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Queries/GetRoleById/V1/AdminGetRoleByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Queries/GetPermissionById/V1/AdminGetPermissionByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Identity/Identity/Application/User/UseCases/Admin/Queries/GetUserRoles/V1/AdminGetUserRolesEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Identity/Identity/Application/Session/UseCases/Public/Queries/GetOwnSessionById/V1/PublicGetOwnSessionByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

### Content Module — Catalog

- [ ] `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCategoryById/V1/AdminGetCategoryByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCustomerById/V1/AdminGetCustomerByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetPackageById/V1/AdminGetPackageByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

### Content Module — Editorial

- [ ] `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Queries/GetArticleById/V1/AdminGetArticleByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Queries/GetVideoById/V1/AdminGetVideoByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Queries/GetShortById/V1/AdminGetShortByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

### Content Module — Commerce

- [ ] `src/Modules/Content/Content/Application/Commerce/UseCases/Admin/Queries/GetOrderById/V1/AdminGetOrderByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Content/Content/Application/Commerce/UseCases/Admin/Queries/GetOrderPayment/V1/AdminGetOrderPaymentEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Content/Content/Application/Commerce/UseCases/Admin/Queries/GetCustomerOrders/V1/AdminGetCustomerOrdersEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

### Content Module — Interactions

- [ ] `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/V1/PublicGetArticleCommentsEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

- [ ] `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Queries/GetPlaylistById/V1/PublicGetPlaylistByIdEndpointV1.cs`
  - Change: `async (string id, ...)` → `async (Guid id, ...)`

> **Note on query constructors:** After switching to `Guid id`, check each query constructor.
> If it currently takes `string` and parses internally, update it to accept `Guid` directly
> and remove the internal parse. If it already takes `Guid`, just pass `id` directly.

---

## Step 2 — Fix Secondary Guid Params on Mutating Endpoints

These 4 DELETE endpoints have a second route segment using `:guid` constraint + `Guid param`
directly in the handler. Per the ROADMAP, mutating verbs must never use the `:guid` route
constraint — they must use `string` + `Guid.Parse()` so invalid input produces 400 not a
silent route mismatch.

### 2.1 — Identity: Remove permission from role

**File:**
`src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/RemovePermissionFromRole/V1/AdminRemovePermissionFromRoleEndpointV1.cs`

**Current route:**
```
{id}/permissions/{permissionId:guid}
```
**Handler:**
```csharp
async (string id, Guid permissionId, IDispatcher dispatcher) =>
{
    var command = new AdminRemovePermissionFromRoleCommand(RoleId: id, PermissionId: permissionId);
```

**Fix:**
- Route → `{id}/permissions/{permissionId}` (remove `:guid`)
- Handler → `async (string id, string permissionId, ...)` + `Guid parsedPermissionId = Guid.Parse(permissionId);`
- Command → `new AdminRemovePermissionFromRoleCommand(RoleId: id, PermissionId: parsedPermissionId)`

### 2.2 — Identity: Remove role from user

**File:**
`src/Modules/Identity/Identity/Application/User/UseCases/Admin/Commands/RemoveRoleFromUser/V1/AdminRemoveRoleFromUserEndpointV1.cs`

**Current route:**
```
{id}/roles/{roleId:guid}
```
**Handler:**
```csharp
async (string id, Guid roleId, IDispatcher dispatcher) =>
{
    var command = new AdminRemoveRoleFromUserCommand(UserId: id, RoleId: roleId);
```

**Fix:**
- Route → `{id}/roles/{roleId}` (remove `:guid`)
- Handler → `async (string id, string roleId, ...)` + `Guid parsedRoleId = Guid.Parse(roleId);`
- Command → `new AdminRemoveRoleFromUserCommand(UserId: id, RoleId: parsedRoleId)`

### 2.3 — Content/Catalog: Remove category pricing tier

**File:**
`src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/V1/AdminRemoveCategoryPricingEndpointV1.cs`

**Current route:**
```
{id}/pricing/{tierId:guid}
```
**Handler:**
```csharp
async (string id, Guid tierId, IDispatcher dispatcher) =>
```

**Fix:**
- Route → `{id}/pricing/{tierId}` (remove `:guid`)
- Handler → `async (string id, string tierId, ...)` + `Guid parsedTierId = Guid.Parse(tierId);`
- Update command constructor call accordingly

### 2.4 — Content/Catalog: Remove package slot

**File:**
`src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/V1/AdminRemovePackageSlotEndpointV1.cs`

**Current route:**
```
{id}/slots/{slotId:guid}
```
**Handler:**
```csharp
async (string id, Guid slotId, IDispatcher dispatcher) =>
```

**Fix:**
- Route → `{id}/slots/{slotId}` (remove `:guid`)
- Handler → `async (string id, string slotId, ...)` + `Guid parsedSlotId = Guid.Parse(slotId);`
- Update command constructor call accordingly

---

## Step 3 — Verification Checklist

After all changes:

- [ ] `dotnet build` — zero warnings, zero errors
- [ ] `dotnet test` — all tests pass
- [ ] Manual smoke test: send a DELETE/PUT/PATCH with an invalid UUID (e.g. `"not-a-guid"`) →
  confirm response is `400 Bad Request` with ProblemDetails, not 500
- [ ] Manual smoke test: send a GET with an invalid UUID → confirm route does not match
  (404 or fallback to list), not 500
- [ ] Confirm no remaining `{param:guid}` route constraints exist on mutating endpoints:
  ```bash
  grep -rn ":guid}" src/Modules/ --include="*Endpoint*.cs"
  ```
  Expected: only GET endpoints or none

---

## Execution Order

```
Step 0  →  Step 1  →  Step 2  →  Step 3
```

Step 0 is a hard prerequisite. Steps 1 and 2 can be done in parallel once Step 0 is merged.
Each endpoint file is one commit.