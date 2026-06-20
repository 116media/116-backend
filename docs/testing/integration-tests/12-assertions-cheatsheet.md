# Integration Test Assertions Cheatsheet

## Golden Rule: Assert Exact Values, Not Existence

Every assertion must verify the **correct** value, not merely that a value exists. Weak assertions like `.Should().NotBeNull()`, `.Should().NotBeEmpty()`, or `.Should().NotBe(Guid.Empty)` prove almost nothing — they pass even when the code returns completely wrong data.

| Weak (avoid) | Strong (use) |
|--------------|-------------|
| `name.Should().NotBeNullOrEmpty()` | `name.Should().Be("Music Videos")` |
| `items.Should().NotBeEmpty()` | `items.Should().HaveCount(3)` |
| `id.Should().NotBe(Guid.Empty)` | `id.Should().Be(expectedId)` |
| `slug.Should().NotBeNull()` | `slug.Should().Be("music-videos")` |
| `body.Should().NotBeNull()` | `body!.Name.Should().Be("expected")` |
| `createdAt.Should().NotBeNull()` | `createdAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5))` |

You seeded the data — you know exactly what the values should be. Assert against them.

The only acceptable use of `.NotBeNull()` is as a guard before accessing properties (e.g., `result.Should().NotBeNull()` followed by exact field assertions). It should never be the **only** assertion in a test.

## HTTP Response Assertions

### Status Codes

```csharp
// Exact status code
response.StatusCode.Should().Be(HttpStatusCode.OK);
response.StatusCode.Should().Be(HttpStatusCode.Created);
response.StatusCode.Should().Be(HttpStatusCode.NoContent);
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
response.StatusCode.Should().Be(HttpStatusCode.NotFound);
response.StatusCode.Should().Be(HttpStatusCode.Conflict);
response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

// Success range (2xx)
response.IsSuccessStatusCode.Should().BeTrue();

// Not a specific code
response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
```

### Response Body

```csharp
// Deserialize and assert
var body = await response.Content.ReadFromJsonAsync<MyResponse>();
body.Should().NotBeNull();
body!.Name.Should().Be("Expected Name");
body.Items.Should().HaveCount(3);

// Raw string (for debugging)
string raw = await response.Content.ReadAsStringAsync();
raw.Should().Contain("expected");
```

### ProblemDetails (Error Responses)

```csharp
// 400 Bad Request with validation errors
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
problem.Should().NotBeNull();
problem!.Status.Should().Be(400);
problem.Title.Should().Be("Bad Request");

// Validation error details
var validation = await response.Content
    .ReadFromJsonAsync<ValidationProblemDetails>();
validation!.Errors.Should().ContainKey("Name");
validation.Errors["Name"].Should().Contain("must not be empty");
```

### Response Headers

```csharp
response.Headers.Location!.AbsolutePath.Should().StartWith("/api/v1/");
response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
```

## Database Assertions (Repository Tests)

### Entity Existence

```csharp
await using var context = CreateDbContext<ContentDbContext>();

// Entity exists — guard only, always follow with exact field assertions
var entity = await context.Categories.FindAsync(id);
entity.Should().NotBeNull();
entity!.Name.Should().Be("Music Videos");

// Entity does not exist
var missing = await context.Categories.FindAsync(nonExistentId);
missing.Should().BeNull();
```

### Entity Properties

```csharp
var category = await context.Categories
    .Include(c => c.ContentType)
    .FirstAsync(c => c.Id == id);

category.Name.Should().Be("Expected Name");
category.IsExclusive.Should().BeTrue();
category.ContentType.Should().NotBeNull();
category.ContentType.Name.Should().Be("Video");
```

### Collection Queries

```csharp
var categories = await context.Categories
    .Where(c => c.IsActive)
    .ToListAsync();

categories.Should().HaveCount(3);
categories.Should().OnlyContain(c => c.IsActive);
categories.Should().BeInDescendingOrder(c => c.CreatedAt);
```

### Pagination

```csharp
var (items, totalCount) = await repository.GetAllAsync(
    page: 2, pageSize: 5);

items.Should().HaveCount(5);
totalCount.Should().Be(15);
```

### Unique Constraint Violations

```csharp
Func<Task> act = () => context.SaveChangesAsync();
await act.Should().ThrowAsync<DbUpdateException>();
```

## Pagination Response Assertions

```csharp
var body = await response.Content
    .ReadFromJsonAsync<PaginatedResponse<CategoryDto>>();

body!.Items.Should().HaveCount(5);
body.Count.Should().Be(15);      // total count
body.PageSize.Should().Be(5);
body.PageIndex.Should().Be(0);
```

## Authentication Assertions

```csharp
// Token structure — JWT has 3 parts (header.payload.signature)
var tokens = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
tokens!.AccessToken.Split('.').Should().HaveCount(3);
tokens.RefreshToken.Should().NotBeNullOrEmpty();
```

## Collection Assertions (AwesomeAssertions)

```csharp
// Count
items.Should().HaveCount(3);
items.Should().ContainSingle();
items.Should().BeEmpty();
items.Should().NotBeEmpty();
items.Should().HaveCountGreaterThan(0);

// Content
items.Should().Contain(item => item.Name == "Expected");
items.Should().OnlyContain(item => item.IsActive);
items.Should().BeInDescendingOrder(item => item.CreatedAt);
items.Should().NotContain(item => item.IsDeleted);

// Type
items.Should().AllBeOfType<CategoryDto>();
```

## Exception Assertions (Repository Tests)

```csharp
// Async exception
Func<Task> act = async () => await repository.GetByIdOrThrowAsync(
    Guid.NewGuid());
await act.Should().ThrowAsync<NotFoundException>();

// With message
await act.Should().ThrowAsync<NotFoundException>()
    .WithMessage("*not found*");

// Specific exception type
await act.Should().ThrowAsync<DbUpdateException>();
```

## String Assertions

```csharp
// Exact match
result.Slug.Should().Be("expected-slug");

// Contains
result.Description.Should().Contain("video");

// Pattern
result.Url.Should().StartWith("https://");
result.Email.Should().EndWith("@test.com");

// Null/empty
result.Name.Should().NotBeNullOrEmpty();
result.OptionalField.Should().BeNull();
```

## DateTime Assertions

```csharp
// Approximate time (useful for CreatedAt)
result.CreatedAt.Should().BeCloseTo(
    DateTimeOffset.UtcNow, precision: TimeSpan.FromSeconds(5));

// Before/after
result.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);

// Not null
result.UpdatedAt.Should().NotBeNull();
```

## Boolean Assertions

```csharp
result.IsActive.Should().BeTrue();
result.IsDeleted.Should().BeFalse();
result.IsExclusive.Should().BeTrue();
```

## GUID Assertions

```csharp
result.Id.Should().NotBe(Guid.Empty);
result.Id.Should().Be(expectedId);
```
