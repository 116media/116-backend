# Interceptors & Decorators Integration Tests

## Why These Need Integration Tests

All 4 components have **0% unit test coverage** because they depend on real EF Core or real CQRS pipeline behavior that cannot be meaningfully mocked.

| Component | Type | What It Does | Why Untestable in Unit Tests |
|-----------|------|-------------|------------------------------|
| `AuditableEntityInterceptor` | EF Core `SaveChangesInterceptor` | Sets `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` on entities | Needs real `SaveChangesAsync` pipeline |
| `DispatchDomainEventsInterceptor` | EF Core `SaveChangesInterceptor` | Publishes domain events after `SaveChangesAsync` | Needs real EF Core change tracker |
| `ValidationDecorator<TCommand>` | MediatR pipeline behavior | Runs FluentValidation before handler | Needs real DI-resolved validator chain |
| `LoggingDecorator<TCommand>` | MediatR pipeline behavior | Logs command execution with timing | Needs real pipeline to measure timing |

## AuditableEntityInterceptor Tests

The interceptor hooks into `SavingChangesAsync` and sets audit fields on any entity implementing `IAuditableEntity`.

### Test Setup

Uses `BaseRepositoryTest` with a real `ContentDbContext` (or any module's DbContext). The interceptor is registered in the module's `AddDatabase<T>()` call, so it runs automatically.

```csharp
[Collection("Database")]
public class AuditableEntityInterceptorTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task SaveChanges_NewEntity_ShouldSetCreatedAtAndUpdatedAt()
    {
        // Arrange
        await using var context = CreateDbContext<ContentDbContext>();
        var category = new CategoryEntityBuilder().Build();

        // Act
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryEntity saved = await verifyContext.Categories
            .FirstAsync(c => c.Id == category.Id);

        saved.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        saved.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChanges_UpdatedEntity_ShouldUpdateOnlyUpdatedAt()
    {
        // Arrange
        await using var context = CreateDbContext<ContentDbContext>();
        var category = new CategoryEntityBuilder().Build();
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        DateTime originalCreatedAt = category.CreatedAt;

        // Act — wait to ensure timestamp differs
        await Task.Delay(50);
        category.UpdateName("Updated Name");
        await context.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryEntity saved = await verifyContext.Categories
            .FirstAsync(c => c.Id == category.Id);

        saved.CreatedAt.Should().Be(originalCreatedAt);
        saved.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

}
```

> **Note:** `CreatedBy` and `UpdatedBy` require the HTTP pipeline (`ClaimsPrincipal` from `HttpContext.User`), so they are tested via `BaseApiTest` in `AuditableEntityViaApiTests` below.

### CreatedBy/UpdatedBy via API

To test `CreatedBy` and `UpdatedBy`, use `BaseApiTest` so the `ClaimsPrincipal` is available via the HTTP pipeline:

```csharp
[Collection("Database")]
public class AuditableEntityViaApiTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateCategory_ShouldSetCreatedByToAuthenticatedUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Client.AuthenticateAs(userId, "Admin");
        var request = new CreateCategoryRequestBuilder().Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<ContentDbContext>();
        CategoryEntity category = await context.Categories
            .OrderByDescending(c => c.CreatedAt)
            .FirstAsync();

        category.CreatedBy.Should().Be(userId);
    }
}
```

## DispatchDomainEventsInterceptor Tests

The interceptor collects domain events from entities in the change tracker and publishes them via `IPublisher` (MediatR) after `SaveChangesAsync` completes.

```csharp
[Collection("Database")]
public class DispatchDomainEventsInterceptorTests(PostgresFixture postgres)
    : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task SaveChanges_EntityWithDomainEvent_ShouldDispatchAndClearEvents()
    {
        // Arrange
        await using var context = CreateDbContext<ContentDbContext>();

        // Create an entity that raises a domain event in its constructor or method
        var video = new VideoEntityBuilder()
            .WithStatus(ContentStatus.Published)
            .Build();

        // Act
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        // Assert — events should be cleared after dispatch
        video.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChanges_EntityWithNoDomainEvents_ShouldNotThrow()
    {
        // Arrange
        await using var context = CreateDbContext<ContentDbContext>();
        var category = new CategoryEntityBuilder().Build();

        // Act
        Func<Task> act = async () =>
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }
}
```

### Verifying Side Effects

To verify that domain events actually trigger their handlers, use the API pipeline where MediatR is fully wired:

```csharp
[Collection("Database")]
public class DomainEventSideEffectTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublishVideo_ShouldTriggerNotificationEvent()
    {
        // Arrange — seed a draft video
        await using var context = CreateDbContext<ContentDbContext>();
        var video = new VideoEntityBuilder()
            .WithStatus(ContentStatus.Draft)
            .Build();
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        // Act — publish the video (triggers domain event)
        HttpResponseMessage response = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Videos}/{videoId}/{EditorialRouteConstants.Publish}", null);

        // Assert — verify the side effect (e.g., notification created)
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var context = CreateDbContext<ContentDbContext>();
        // Verify whatever side effect the domain event handler produces
    }
}
```

## ValidationDecorator Tests

The `ValidationDecorator<TCommand>` runs all registered `IValidator<TCommand>` implementations before the command handler executes. If validation fails, it throws a `ValidationException` with field-level errors — the command handler never runs.

```csharp
[Collection("Database")]
public class ValidationDecoratorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Post_WithInvalidPayload_ShouldReturn422WithFieldErrors()
    {
        // Arrange
        Client.AuthenticateAsAdmin();
        var request = new { Name = "", Slug = "" }; // violates required fields

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        ProblemDetails problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public async Task Post_WithValidPayload_ShouldPassThroughToHandler()
    {
        // Arrange
        Client.AuthenticateAsAdmin();
        var request = new CreateCategoryRequestBuilder().Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_WithMultipleValidationErrors_ShouldAggregateAllErrors()
    {
        // Arrange
        Client.AuthenticateAsAdmin();
        var request = new
        {
            Name = "",
            Slug = "",
            ContentTypeId = Guid.Empty
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        ProblemDetails problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        // Multiple field errors should be present
        problem!.Extensions["errors"].Should().NotBeNull();
    }
}
```

## LoggingDecorator Tests

The `LoggingDecorator<TCommand>` logs the command type name, execution duration, and any exceptions. Testing logging output requires capturing log entries.

### Option A: Verify via ITestOutputHelper (observation)

```csharp
[Collection("Database")]
public class LoggingDecoratorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Command_ShouldCompleteWithoutLoggingErrors()
    {
        // Arrange
        Client.AuthenticateAsAdmin();
        var request = new CreateCategoryRequestBuilder().Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        // Assert — the decorator should not interfere with normal execution
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task FailedCommand_ShouldStillReturnErrorResponse()
    {
        // Arrange — no auth, should fail
        var request = new CreateCategoryRequestBuilder().Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        // Assert — decorator logs the failure but doesn't swallow it
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

### Option B: Capture logs with a test sink

If you need to assert specific log messages, register a test log sink in `ApiFixture`:

```csharp
// In ApiFixture.ConfigureWebHost
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddProvider(new TestLoggerProvider(TestOutputHelper));
});
```

Then assert that log entries contain the expected command type name and duration.

## Test File Locations

```
tests/Integration/
└── Shared/
    ├── Interceptors/
    │   ├── AuditableEntityInterceptorTests.cs
    │   └── DispatchDomainEventsInterceptorTests.cs
    └── Decorators/
        ├── ValidationDecoratorTests.cs
        └── LoggingDecoratorTests.cs
```
