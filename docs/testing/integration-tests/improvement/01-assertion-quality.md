# 01 — Assertion Quality (the #1 problem)

**87% of endpoint tests assert only the HTTP status code.** Zero tests
deserialize into the real response DTOs. This is the single biggest weakness in
the suite: a handler can return the wrong body, skip a side-effect, or emit a
malformed error and the test still passes.

## What "weak" looks like

A status-only test (most of the suite):

```csharp
[Fact]
public async Task GetOwnRoles_AsVisitor_ReturnsOk()
{
    Client.AuthenticateAsVisitor();
    var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/roles");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

This proves only that the route exists and authorization passed. It does **not**
prove the response contains roles, the right roles, pagination metadata, or the
correct shape.

Representative offenders (status-only, no body inspection):

- `Modules/Identity/.../Roles/UseCases/Public/Queries/GetOwnRoles/V1/PublicGetOwnRolesEndpointV1Tests.cs`
- `Modules/Identity/.../User/UseCases/Admin/Commands/AssignRoleToUser/V1/AdminAssignRoleToUserEndpointV1Tests.cs`
- `Modules/Content/.../Commerce/UseCases/Admin/Queries/GetAllOrders/V1/AdminGetAllOrdersEndpointV1Tests.cs`
- `Modules/Content/.../Editorial/UseCases/Public/Queries/GetArticleBySlug/V1/PublicGetArticleBySlugEndpointV1Tests.cs`

## What "good" looks like (already in the repo)

The best existing examples to model on:

- `Modules/Identity/.../Session/UseCases/Admin/Queries/GetAllSessions/V1/AdminGetAllSessionsEndpointV1Tests.cs`
  — asserts pagination metadata and filters item-by-item.
- `Modules/Identity/.../Roles/UseCases/Admin/Commands/HardDeletePermission/V1/AdminHardDeletePermissionEndpointV1Tests.cs`
  — extracts the created ID from the body **and re-queries the DB** to prove the
  side-effect.

Even these use stringly-typed `JsonDocument.Parse(...).GetProperty("...")`, which
is verbose and brittle. The target is **typed deserialization** into the real
src response records.

## The standard: what every test must assert

Beyond `StatusCode.Should().Be(...)`:

1. **Typed body.** Deserialize into the actual src response record via the new
   `response.ReadAsAsync<T>()` helper (see
   [`specs/infrastructure/04-typed-http-helpers.md`](specs/infrastructure/04-typed-http-helpers.md)).
   Example response types: `AdminGetAllSessionsResponse`,
   `AdminGetAllOrdersResponse` (`PaginatedResult<ContentOrderSummaryDto>`),
   `PublicLoginMobileResponse`.
2. **Echoed request fields.** A create/update must return the values you sent
   (title, slug, name, …), not just an id.
3. **Returned IDs.** Created resources return a non-empty id; use it downstream.
4. **Pagination metadata** for list endpoints: `items`, `pageIndex`, `pageSize`,
   `count` — and assert the seeded entity actually appears.
5. **Filter correctness.** When a query filter is applied, assert **every**
   returned item matches it (the GetAllSessions filter tests do this well).
6. **ProblemDetails for error paths.** 400/401/403/404/409/410/423/429 must
   assert the problem body shape (`status`, `title`/`type`, and where relevant a
   stable error code/detail) — not just the status. See
   [`specs/infrastructure/04-typed-http-helpers.md`](specs/infrastructure/04-typed-http-helpers.md).
7. **DB side-effects.** For POST/PUT/PATCH/DELETE, re-query with
   `CreateDbContext<TDbContext>()` and assert the row was created/updated/soft-
   deleted/hard-deleted as expected. A 200 is not proof of persistence.
8. **Content-Type** where it matters (JSON endpoints, CSV/XLSX exports).

## After (target shape)

```csharp
[Fact]
public async Task GetOwnRoles_AsVisitor_ReturnsSeededRoles()
{
    Client.AuthenticateAsVisitor();

    var response = await Client.GetAsync(Routes.Public.Me.Roles());
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var body = await response.ReadAsAsync<PublicGetOwnRolesResponse>();
    body.Roles.Should().NotBeNull();
    body.Roles.Should().Contain(r => r.Name == CoreUserRole.Visitor.ToString());
}
```

```csharp
[Fact]
public async Task CreateCategory_AsSuperAdmin_PersistsAndEchoesRequest()
{
    Client.AuthenticateAsSuperAdmin();
    var request = CategoryRequestBuilder.Valid();          // typed builder, not anon object

    var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Categories, request);
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var body = await response.ReadAsAsync<AdminCreateCategoryResponse>();
    body.Category.Name.Should().Be(request.Name);          // echoed field
    body.Category.Id.Should().NotBeEmpty();                // returned id

    await using var db = CreateDbContext<ContentDbContext>();
    (await db.Categories.AnyAsync(c => c.Id == body.Category.Id))
        .Should().BeTrue();                                // side-effect
}
```

```csharp
[Fact]
public async Task GetArticleBySlug_WithNonExistent_ReturnsNotFoundProblem()
{
    Client.ClearAuthentication();

    var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/missing-slug");

    await response.ShouldBeProblem(HttpStatusCode.NotFound);  // typed ProblemDetails assert
}
```

## Per-module execution

The file-by-file checklists live in
[`specs/assertions/`](specs/assertions/) — one spec per module/area. Work them
top to bottom; each lists every endpoint test file and the specific assertions
to add.
