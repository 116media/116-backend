# Writing Handler Tests

Handler tests validate that a command or query handler correctly orchestrates repositories, factories, services, and domain entities. They use mocks for all dependencies.

---

## Class Setup Pattern

```csharp
public class AdminPublishArticleHandlerTests  // No base class if Mapper not needed
{
    private static readonly Guid CategoryId = Guid.NewGuid();  // Shared test IDs

    // Mocks — one field per dependency
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;

    // Handler — the system under test
    private readonly AdminPublishArticleHandler _handler;

    public AdminPublishArticleHandlerTests()
    {
        // Create mocks
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        // Wire handler
        _handler = new AdminPublishArticleHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }
}
```

---

## With Mapper (extends BaseContentHandlerTest)

Use `BaseContentHandlerTest` only when the handler has `IMapper` as a dependency.

```csharp
public class AdminGetOrderPaymentHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IOrderPaymentFactory> _orderPaymentFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetOrderPaymentHandler _handler;

    public AdminGetOrderPaymentHandlerTests()
    {
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _fileRepositoryMock = MockFileRepository.Create();

        // Mapper comes from BaseContentHandlerTest
        _handler = new AdminGetOrderPaymentHandler(
            _orderPaymentFactoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }
}
```

---

## Happy Path Test

```csharp
[Fact]
public async Task Handle_WhenArticleIsApproved_ShouldPublishAndReturnSuccess()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());

    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    // Act
    AdminPublishArticleResult result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    _articleRepositoryMock.VerifyUpdateCalled();
    _unitOfWorkMock.VerifyCommitCalled();
}
```

---

## Exception Path Tests

Use `Func<Task>` for async exception assertions — never use `Action` for async code.

```csharp
[Fact]
public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
{
    // Arrange
    Guid nonExistentId = Guid.NewGuid();
    var command = new AdminPublishArticleCommand(Id: nonExistentId.ToString());

    _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

    // Act
    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<NotFoundException>();
}

[Fact]
public async Task Handle_WhenArticleAlreadyPublished_ShouldThrowConflictException()
{
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId); // Already published
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());

    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<ConflictException>();
}

[Fact]
public async Task Handle_WhenArticleInWrongStatus_ShouldThrowBadRequestException()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId); // Draft — wrong status
    var command = new AdminPublishArticleCommand(Id: article.Id.ToString());

    _articleRepositoryMock.SetupGetByIdOrThrow(article);

    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<BadRequestException>();
}
```

---

## Verify Not Called on Error Paths

When a handler should bail early on an exception, verify no side effects occurred.

```csharp
[Fact]
public async Task Handle_WhenAlreadyActive_ShouldNotCommit()
{
    CategoryEntity category = CategoryFactory.Create(ContentTypeId); // Already active

    _categoryRepositoryMock.SetupGetByIdOrThrow(category);

    try
    {
        await _handler.Handle(command, CancellationToken.None);
    }
    catch (ConflictException)
    {
        // Expected — swallow to allow verification below
    }

    _unitOfWorkMock.VerifyCommitNotCalled();
}
```

---

## Reload After Commit Pattern

Some handlers call the repository twice: once before commit (to mutate) and once after (to return fresh data). Verify this with `Times.Exactly(2)`.

```csharp
[Fact]
public async Task Handle_WhenSuccessful_ShouldReloadAfterCommit()
{
    CategoryEntity category = CategoryFactory.CreateInactive(ContentTypeId);
    var command = new AdminActivateCategoryCommand(Id: category.Id.ToString());

    _categoryRepositoryMock.SetupGetByIdOrThrow(category);

    await _handler.Handle(command, CancellationToken.None);

    // GetByIdOrThrowAsync called twice: once to load, once to reload after commit
    _categoryRepositoryMock.Verify(
        x => x.GetByIdOrThrowAsync(category.Id, It.IsAny<CancellationToken>()),
        Times.Exactly(2)
    );
}
```

---

## Multiple Factory Dependencies

When a handler depends on multiple factory interfaces:

```csharp
public class AdminVerifyPaymentHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderPaymentFactory>    _orderPaymentFactoryMock;
    private readonly Mock<IVerifyPaymentFactory>   _verifyPaymentFactoryMock;
    private readonly AdminVerifyPaymentHandler _handler;

    public AdminVerifyPaymentHandlerTests()
    {
        _orderRepositoryMock      = MockContentOrderRepository.Create();
        _orderPaymentFactoryMock  = MockOrderPaymentFactory.Create();
        _verifyPaymentFactoryMock = MockVerifyPaymentFactory.Create();

        _handler = new AdminVerifyPaymentHandler(
            _orderRepositoryMock.Object,
            _orderPaymentFactoryMock.Object,
            _verifyPaymentFactoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenOrderAndPaymentFound_ShouldVerifyAndReturnSuccess()
    {
        ContentOrderEntity order   = ContentOrderFactory.CreateSubmitted();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        var command = new AdminVerifyPaymentCommand(
            OrderId: order.Id.ToString(),
            ReceiptUrl: TestConstants.Content.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderPaymentFactoryMock.SetupGetByOrderId(order.Id, payment);
        _verifyPaymentFactoryMock.SetupVerifyAsync();

        AdminVerifyPaymentResult result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _verifyPaymentFactoryMock.VerifyVerifyCalled();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminVerifyPaymentCommand(
            OrderId: nonExistentId.ToString(),
            ReceiptUrl: TestConstants.Content.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        _orderRepositoryMock.SetupGetByIdWithItems(null); // null = not found

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderPaymentFactoryMock.SetupGetByOrderIdNotFound(order.Id);

        var command = new AdminVerifyPaymentCommand(
            OrderId: order.Id.ToString(),
            ReceiptUrl: TestConstants.Content.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
```

---

## Conditional Mock Verification (Times.Never)

When a path should NOT call a specific dependency:

```csharp
[Fact]
public async Task Handle_WithoutProofFile_ShouldReturnPaymentWithNullProof()
{
    ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId); // No proof file

    _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);
    // _fileRepositoryMock is NOT set up — it should not be called

    var query = new AdminGetOrderPaymentQuery(OrderId: orderId.ToString());
    var result = await _handler.Handle(query, CancellationToken.None);

    result.ProofFile.Should().BeNull();

    // Verify file repo was never consulted
    _fileRepositoryMock.Verify(
        x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Never
    );
}
```

---

## Query Handler with Pagination

```csharp
[Fact]
public async Task Handle_ShouldReturnPaginatedResult()
{
    List<ContentOrderEntity> orders = ContentOrderFactory.CreateMany(3);
    _orderRepositoryMock.SetupGetAllAsync(orders, orders.Count);

    var query = new AdminGetAllOrdersQuery(
        PaginatedRequest: new PaginatedRequest(Page: 0, PageSize: 10),
        Status: null,
        CustomerId: null
    );

    AdminGetAllOrdersResult result = await _handler.Handle(query, CancellationToken.None);

    result.Items.Should().HaveCount(3);
    result.TotalCount.Should().Be(3);
}
```

---

## Real Test Files to Reference

| File | Key Pattern |
|------|-------------|
| `tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/AdminPublishArticleHandlerTests.cs` | Basic handler with repo + UoW, VerifyUpdateCalled |
| `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/ActivateCategory/AdminActivateCategoryHandlerTests.cs` | Reload after commit, VerifyCommitNotCalled, Times.Exactly(2) |
| `tests/Unit/Modules/Content/Application/Commerce/UseCases/Admin/Commands/VerifyPayment/AdminVerifyPaymentHandlerTests.cs` | Three factory mocks, multiple failure points |
| `tests/Unit/Modules/Content/Application/Commerce/UseCases/Admin/Queries/GetOrderPayment/AdminGetOrderPaymentHandlerTests.cs` | BaseContentHandlerTest, Mapper, Times.Never |
| `tests/Unit/Modules/Content/Application/Commerce/UseCases/Admin/Queries/GetAllOrders/AdminGetAllOrdersHandlerTests.cs` | Pagination query |
