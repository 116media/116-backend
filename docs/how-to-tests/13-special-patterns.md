# Special Patterns & Additional Test Types

This file covers patterns that were not in the initial documentation: helper utilities, Identity/Core-specific test patterns, Shared/BuildingBlocks tests, MetaField tests, Error tests, module registration tests, and advanced Moq usage.

---

## Test Helpers

### `AuthTestHelpers`
**File:** `tests/Fixtures/Helpers/AuthTestHelpers.cs`

Static helpers for auth-related test setup. Used in Identity handler tests.

```csharp
AuthTestHelpers.CreateDefaultSessionResult()
// Returns SessionResult with fixed refresh/access tokens and expiration times

AuthTestHelpers.CreateRoleDto(string name = "Admin", bool isActive = true, bool isDeleted = false, DateTime? deletedAt = null)
AuthTestHelpers.CreatePermissionDto(string resource = "users", string action = "read", ...)

AuthTestHelpers.CreatePublicLoginAuthData(UserEntity user)    // via AuthDataBuilder
AuthTestHelpers.CreatePublicSocialLoginAuthData(UserEntity user)
AuthTestHelpers.CreateAdminLoginAuthData(UserEntity user)
```

### `FileTestHelpers`
**File:** `tests/Fixtures/Helpers/FileTestHelpers.cs`

```csharp
FileTestHelpers.CreateMockFormFile()
// Returns IFormFile mock: FileName="test.jpg", Length=1024, ContentType="image/jpeg"

FileTestHelpers.CreateMockFormFile(string fileName, string contentType, long length)
// Custom mock IFormFile
```

### `HttpTestHelpers`
**File:** `tests/Fixtures/Helpers/HttpTestHelpers.cs`

```csharp
HttpTestHelpers.CreateDefaultHttpContext()
// Returns DefaultHttpContext with Path="/api/test", Method="GET",
// TraceIdentifier="test-trace-id", Response.Body=new MemoryStream()
```

Used in middleware and exception handler tests.

---

## FluentValidation — TestHelper Pattern (Identity Validators)

Identity validators use the `.TestValidateAsync()` extension from the `FluentValidation.TestHelper` package instead of calling `ValidateAsync()` directly. This enables the `.ShouldHaveValidationErrorFor()` fluent API.

```csharp
public class AdminLoginValidatorTests
{
    private readonly AdminLoginValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCredentials_ShouldNotHaveErrors()
    {
        var command = new AdminLoginCommand(
            Email: "admin@example.com",
            Password: "ValidPassword123"
        );

        TestValidationResult<AdminLoginCommand> result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldHaveEmailError()
    {
        var command = new AdminLoginCommand(Email: "not-an-email", Password: "ValidPass");

        TestValidationResult<AdminLoginCommand> result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WithBothInvalid_ShouldHaveMultipleErrors()
    {
        var command = new AdminLoginCommand(Email: "", Password: "");

        TestValidationResult<AdminLoginCommand> result = await _validator.TestValidateAsync(command);

        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
```

**Key difference from Content validators:**

| Pattern | Content validators | Identity validators |
|---------|-------------------|---------------------|
| Method | `await _validator.ValidateAsync(command)` | `await _validator.TestValidateAsync(command)` |
| Return type | `ValidationResult` | `TestValidationResult<T>` |
| Happy path | `.IsValid.Should().BeTrue()` | `.ShouldNotHaveAnyValidationErrors()` |
| Error path | `.Errors.Should().Contain(e => e.PropertyName == ...)` | `.ShouldHaveValidationErrorFor(x => x.Property)` |
| Error message | `.WithErrorMessage("...")` chained or via lambda | Same |

---

## Cancellation Token Tests

Identity handler tests verify that cancellation tokens are properly forwarded to dependencies.

```csharp
[Fact]
public async Task Handle_ShouldPassCancellationTokenToRepository()
{
    using var cts = new CancellationTokenSource();
    CancellationToken token = cts.Token;

    var command = new AdminLoginCommand(Email: "test@test.com", Password: "pass");
    _authFactoryMock.SetupAuthenticateAsync(authData);

    await _handler.Handle(command, token);

    // Verify specific token was passed
    _authFactoryMock.Verify(
        x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), token),
        Times.Once
    );
}
```

---

## HTTP Client Mocking (Moq Protected)

Used in `FileServiceTests` to mock `HttpMessageHandler.SendAsync`. This is needed when testing code that uses `HttpClient` internally.

```csharp
var handlerMock = new Mock<HttpMessageHandler>();

handlerMock
    .Protected()
    .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(req =>
            req.Method == HttpMethod.Head &&
            req.RequestUri == new Uri("https://example.com/file.jpg")),
        ItExpr.IsAny<CancellationToken>()
    )
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new ByteArrayContent(Array.Empty<byte>())
        {
            Headers = { ContentLength = 1024 }
        }
    });

var httpClient = new HttpClient(handlerMock.Object);
```

For sequential responses (multiple requests to the same URL):
```csharp
handlerMock
    .Protected()
    .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ...)
    .ReturnsAsync(firstResponse)
    .ReturnsAsync(secondResponse);
```

---

## IFormFile Mocking

Used in Cloudinary and file upload tests:

```csharp
var fileMock = new Mock<IFormFile>();
fileMock.Setup(f => f.FileName).Returns("photo.jpg");
fileMock.Setup(f => f.Length).Returns(1024 * 1024); // 1MB
fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

IFormFile file = fileMock.Object;
```

Or use the helper: `FileTestHelpers.CreateMockFormFile()`.

---

## Module Registration Tests

Tests that verify DI container registrations. Used in `CoreModuleTests`.

```csharp
public class CoreModuleTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly IConfiguration _configuration;

    public CoreModuleTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging();

        // Set up configuration with required settings
        var settings = new Dictionary<string, string?>
        {
            ["Cloudinary:CloudName"] = "test-cloud",
            ["Cloudinary:ApiKey"] = "test-key",
            ["Cloudinary:ApiSecret"] = "test-secret"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterFileRepository()
    {
        _services.AddCoreModule(_configuration);
        ServiceProvider provider = _services.BuildServiceProvider();

        IFileRepository repository = provider.GetRequiredService<IFileRepository>();

        repository.Should().NotBeNull();
        repository.Should().BeOfType<FileRepository>();
    }

    [Fact]
    public void AddCoreModule_ShouldReturnServiceCollection()
    {
        IServiceCollection result = _services.AddCoreModule(_configuration);

        result.Should().BeSameAs(_services);
    }

    public void Dispose() { /* cleanup if needed */ }
}
```

---

## Error Factory Tests

Direct unit tests on static error factory methods. No mocks needed.

```csharp
public class ContentOrderErrorsTests
{
    [Fact]
    public void NotFound_ShouldReturnNotFoundException()
    {
        Guid orderId = Guid.NewGuid();

        NotFoundException exception = ContentOrderErrors.NotFound(orderId);

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(orderId.ToString());
    }

    [Fact]
    public void AlreadySubmitted_ShouldReturnConflictException()
    {
        ConflictException exception = ContentOrderErrors.AlreadySubmitted();

        exception.Should().NotBeNull();
    }

    [Fact]
    public void MustHaveAtLeastOneItemWithTier_ShouldReturnBadRequestException()
    {
        BadRequestException exception = ContentOrderErrors.MustHaveAtLeastOneItemWithTier();

        exception.Should().NotBeNull();
    }
}
```

---

## Error Message Tests

Tests on static string factory methods. Verify exact return values.

```csharp
public class ContentOrderErrorMessageTests
{
    [Fact]
    public void AlreadySubmitted_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.AlreadySubmitted();

        message.Should().Be("Order has already been submitted.");
    }

    [Fact]
    public void MustHaveAtLeastOneItemWithTier_ShouldReturnExpectedMessage()
    {
        string message = ContentOrderErrorMessage.MustHaveAtLeastOneItemWithTier();

        message.Should().Be("Order must have at least one item with a pricing tier.");
    }
}
```

---

## MetaField Initialization Tests

Tests that verify static `RouteMetadata` fields are properly initialized. These are simple not-null / property checks.

```csharp
public class LookupMetaFieldTests
{
    [Fact]
    public void ActivateContentTypeMetaField_ShouldBeInitialized()
    {
        RouteMetadata meta = AdminActivateContentTypeMetaField.ActivateContentType;

        meta.Should().NotBeNull();
        meta.Name.Should().NotBeNullOrEmpty();
        meta.Summary.Should().NotBeNullOrEmpty();
        meta.Description.Should().NotBeNullOrEmpty();
    }
}
```

**Rule:** Write one test per MetaField static field. Test that Name, Summary, and Description are not null or empty.

---

## Shared Specification Composition Tests

Tests in `tests/Unit/Shared/Specifications/` verify the base `Specification<T>` class and its `And`/`Or`/`Not` operators.

```csharp
// Testing And composition
[Fact]
public void And_WhenBothSatisfied_ShouldReturnTrue()
{
    var specA = new IsActiveSpecification();  // example
    var specB = new IsVerifiedSpecification();

    var combined = specA.And(specB);

    bool result = combined.IsSatisfiedBy(activeVerifiedEntity);
    result.Should().BeTrue();
}

// Testing AndAll static method
[Fact]
public void AndAll_WithEmptyArray_ShouldThrow()
{
    Action act = () => Specification<MyEntity>.AndAll(Array.Empty<Specification<MyEntity>>());
    act.Should().Throw<ArgumentException>();
}

// Testing De Morgan's Laws (Not + Or/And)
[Fact]
public void Not_Or_ShouldEqualAnd_Not_Not()
{
    // !(A || B) == !A && !B
}
```

---

## Middleware Tests

Pattern from `SwaggerDescriptionMiddlewareTests` and `ResourceNotFoundMiddlewareTests`.

```csharp
public class ResourceNotFoundMiddlewareTests
{
    private readonly ResourceNotFoundMiddleware _middleware;

    public ResourceNotFoundMiddlewareTests()
    {
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        };

        _middleware = new ResourceNotFoundMiddleware(next);
    }

    [Fact]
    public async Task InvokeAsync_When404_ShouldThrowResourceNotFoundException()
    {
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        Func<Task> act = async () => await _middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(400)]
    [InlineData(500)]
    public async Task InvokeAsync_WithNon404Status_ShouldNotThrow(int statusCode)
    {
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        };
        var middleware = new ResourceNotFoundMiddleware(next);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        Func<Task> act = async () => await middleware.InvokeAsync(context);

        await act.Should().NotThrowAsync();
    }
}
```

---

## Exception Handler Tests

Pattern from `ConflictExceptionHandlerTests`, `BadRequestExceptionHandlerTests`, etc.

```csharp
public class ConflictExceptionHandlerTests
{
    private readonly ConflictExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnConflictExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(ConflictException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn409StatusCode()
    {
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        var exception = new ConflictException("Already exists");

        ProblemDetails details = _handler.CreateProblemDetails(context, exception);

        details.Status.Should().Be(409);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeExceptionMessage()
    {
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        var exception = new ConflictException("Already exists");

        ProblemDetails details = _handler.CreateProblemDetails(context, exception);

        details.Detail.Should().Be("Already exists");
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeTraceId()
    {
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        var exception = new ConflictException("Already exists");

        ProblemDetails details = _handler.CreateProblemDetails(context, exception);

        details.Extensions.Should().ContainKey("traceId");
        details.Extensions["traceId"].Should().Be(context.TraceIdentifier);
    }
}
```

---

## CQRS Decorator Tests

Pattern from `LoggingDecoratorTests` and `ValidationDecoratorTests`.

```csharp
public class LoggingDecoratorTests
{
    private readonly Mock<ICommandHandler<MyCommand, MyResult>> _innerHandlerMock;
    private readonly Mock<ILogger<LoggingDecorator<MyCommand, MyResult>>> _loggerMock;
    private readonly LoggingDecorator<MyCommand, MyResult> _decorator;

    public LoggingDecoratorTests()
    {
        _innerHandlerMock = new Mock<ICommandHandler<MyCommand, MyResult>>();
        _loggerMock = new Mock<ILogger<LoggingDecorator<MyCommand, MyResult>>>();
        _decorator = new LoggingDecorator<MyCommand, MyResult>(
            _innerHandlerMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCallInnerHandlerAndReturnResult()
    {
        var command = new MyCommand();
        var expected = new MyResult();
        _innerHandlerMock.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        MyResult result = await _decorator.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_WhenSlowRequest_ShouldLogPerformanceWarning()
    {
        // Tests that requests > 3 seconds produce a warning log
    }
}
```

---

## Pagination Tests

Pattern from `PaginatedRequestTests` and `PaginatedResultTests`.

```csharp
[Fact]
public void PaginatedRequest_DefaultValues_ShouldBePageZeroSize10()
{
    var request = new PaginatedRequest();
    request.PageIndex.Should().Be(0);
    request.PageSize.Should().Be(10);
}

[Fact]
public void PaginatedResult_ShouldCalculateHasMorePages()
{
    var result = new PaginatedResult<string>(
        items: new List<string> { "a", "b", "c" },
        totalCount: 10,
        pageIndex: 0,
        pageSize: 3
    );

    result.HasNextPage.Should().BeTrue();
    result.TotalPages.Should().Be(4);
}
```

---

## In-Memory SQLite (Unit of Work / Repository Integration)

Some infrastructure tests use **SQLite in-memory** instead of EF InMemory, for better FK enforcement:

```csharp
public class CoreUnitOfWorkTests : IDisposable
{
    private readonly CoreDbContext _context;
    private readonly CoreUnitOfWork _unitOfWork;

    public CoreUnitOfWorkTests()
    {
        DbContextOptions<CoreDbContext> options =
            new DbContextOptionsBuilder<CoreDbContext>()
                .UseSqlite("DataSource=:memory:")   // SQLite in-memory
                .Options;

        _context = new CoreDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _unitOfWork = new CoreUnitOfWork(_context);
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = async () => await _unitOfWork.CommitAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
```

**SQLite vs EF InMemory:**
- Use **EF InMemory** for mapper tests (navigation properties, no FK needed)
- Use **SQLite in-memory** for UnitOfWork and repository tests (real SQL execution, FK constraints, cancellation)

---

## Real Test Files to Reference

| Area | File |
|------|------|
| FluentValidation TestHelper | `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/AdminLoginValidatorTests.cs` |
| Cancellation tokens | `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/AdminLoginHandlerTests.cs` |
| HTTP Protected mock | `tests/Unit/Modules/Core/Infrastructure/Services/FileServiceTests.cs` |
| IFormFile mock | `tests/Unit/Modules/Core/Infrastructure/Services/CloudinaryServiceTests.cs` |
| Module registration | `tests/Unit/Modules/Core/CoreModuleTests.cs` |
| Error factory tests | `tests/Unit/Modules/Content/Application/Commerce/Errors/ContentOrderErrorsTests.cs` |
| Error message tests | `tests/Unit/Modules/Content/Application/Commerce/Errors/ContentOrderErrorMessageTests.cs` |
| MetaField tests | `tests/Unit/Modules/Content/Application/Lookup/MetaFields/LookupMetaFieldTests.cs` |
| Middleware tests | `tests/Unit/Shared/Middleware/ResourceNotFoundMiddlewareTests.cs` |
| Exception handler tests | `tests/Unit/Shared/Exceptions/` (any `*ExceptionHandlerTests.cs`) |
| Decorator tests | `tests/Unit/Shared/Application/LoggingDecoratorTests.cs` |
| Specification composition | `tests/Unit/Shared/Specifications/SpecificationTests.cs` |
| SQLite in-memory | `tests/Unit/Modules/Core/Infrastructure/Persistence/CoreUnitOfWorkTests.cs` |
| Pagination | `tests/Unit/Shared/Pagination/PaginatedResultTests.cs` |
