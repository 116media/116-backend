# Writing Factory Tests

Application factory tests validate the business logic inside factory classes (e.g., `AdminAddItemTierFactory`, `OrderPaymentFactory`). The factory itself is the system under test, instantiated with mocked dependencies.

---

## What is a Factory (Application Layer)

Application factories are classes that encapsulate complex multi-step operations that handlers delegate to. They:
- Call repositories to load entities
- Apply domain logic
- Persist side effects (via repository methods)
- Commit via UnitOfWork
- Return results (entities, tuples, void)

Examples: `AdminAddItemTierFactory`, `AdminAddOrderItemFactory`, `AdminSubmitOrderFactory`, `AdminVerifyPaymentFactory`, `OrderPaymentFactory`

---

## Class Setup

No base class. Instantiate the factory directly with mocked dependencies.

```csharp
public class AdminAddItemTierFactoryTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICategoryRepository>     _categoryRepositoryMock;
    private readonly Mock<ILookupRepository>       _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork>      _unitOfWorkMock;

    private readonly AdminAddItemTierFactory _factory;

    public AdminAddItemTierFactoryTests()
    {
        _orderRepositoryMock    = MockContentOrderRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lookupRepositoryMock   = MockLookupRepository.Create();
        _unitOfWorkMock         = MockContentUnitOfWork.Create();

        _factory = new AdminAddItemTierFactory(
            _orderRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }
}
```

---

## Happy Path Test (with Side-Effect Verification)

```csharp
[Fact]
public async Task AttachTierAsync_WhenAllValid_ShouldReturnTierWithTierName()
{
    // Arrange — build the complete object graph
    ContentOrderEntity order        = ContentOrderFactory.Create();           // Draft
    ContentOrderItemEntity item     = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
    PricingTierEntity pricingTier   = PricingTierFactory.Create();
    CategoryPricingEntity pricing   = CategoryPricingFactory.Create(item.CategoryId, pricingTier.Id);

    var command = new AdminAddItemTierCommand(
        OrderId:      order.Id.ToString(),
        OrderItemId:  item.Id.ToString(),
        PricingTierId: pricingTier.Id.ToString()
    );

    // Setup each dependency in the call chain
    _orderRepositoryMock.SetupGetByIdWithItems(order);
    _orderRepositoryMock.SetupGetItemById(order.Id, item.Id, item);
    _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);
    _categoryRepositoryMock.SetupGetPricing(item.CategoryId, pricingTier.Id, pricing);

    // Act
    (ContentItemTierEntity tier, string tierName) = await _factory.AttachTierAsync(command, CancellationToken.None);

    // Assert — return values
    tier.Should().NotBeNull();
    tier.PricingTierId.Should().Be(pricingTier.Id);
    tierName.Should().Be(pricingTier.Name);

    // Assert — side effects
    _orderRepositoryMock.VerifyAddItemTierCalled();
    _orderRepositoryMock.VerifyUpdateCalled();
    _unitOfWorkMock.VerifyCommitCalled();
}
```

---

## Failure Path Tests

Each failure corresponds to one dependency returning nothing or throwing.

```csharp
[Fact]
public async Task AttachTierAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
{
    Guid nonExistentId = Guid.NewGuid();
    var command = new AdminAddItemTierCommand(
        OrderId:       nonExistentId.ToString(),
        OrderItemId:   Guid.NewGuid().ToString(),
        PricingTierId: Guid.NewGuid().ToString()
    );

    _orderRepositoryMock.SetupGetByIdWithItems(null); // null = not found

    Func<Task> act = async () => await _factory.AttachTierAsync(command, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
}

[Fact]
public async Task AttachTierAsync_WhenOrderNotDraft_ShouldThrowBadRequestException()
{
    // Use a submitted order — EnsureDraft() will throw
    ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();

    _orderRepositoryMock.SetupGetByIdWithItems(order);

    var command = new AdminAddItemTierCommand(
        OrderId:       order.Id.ToString(),
        OrderItemId:   Guid.NewGuid().ToString(),
        PricingTierId: Guid.NewGuid().ToString()
    );

    Func<Task> act = async () => await _factory.AttachTierAsync(command, CancellationToken.None);

    await act.Should().ThrowAsync<BadRequestException>();
}

[Fact]
public async Task AttachTierAsync_WhenItemNotFound_ShouldThrowNotFoundException()
{
    ContentOrderEntity order = ContentOrderFactory.Create(); // Draft

    _orderRepositoryMock.SetupGetByIdWithItems(order);
    _orderRepositoryMock.SetupGetItemById(order.Id, It.IsAny<Guid>(), null); // Item not found

    var command = new AdminAddItemTierCommand(
        OrderId:       order.Id.ToString(),
        OrderItemId:   Guid.NewGuid().ToString(),
        PricingTierId: Guid.NewGuid().ToString()
    );

    Func<Task> act = async () => await _factory.AttachTierAsync(command, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
}

[Fact]
public async Task AttachTierAsync_WhenCategoryPricingNotFound_ShouldThrowNotFoundException()
{
    ContentOrderEntity order     = ContentOrderFactory.Create();
    ContentOrderItemEntity item  = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
    PricingTierEntity pricingTier = PricingTierFactory.Create();

    _orderRepositoryMock.SetupGetByIdWithItems(order);
    _orderRepositoryMock.SetupGetItemById(order.Id, item.Id, item);
    _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);
    _categoryRepositoryMock.SetupGetPricing(item.CategoryId, pricingTier.Id, null); // No pricing

    var command = new AdminAddItemTierCommand(
        OrderId:       order.Id.ToString(),
        OrderItemId:   item.Id.ToString(),
        PricingTierId: pricingTier.Id.ToString()
    );

    Func<Task> act = async () => await _factory.AttachTierAsync(command, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
}
```

---

## Simple Factory Test (Single Repository Dependency)

```csharp
public class OrderPaymentFactoryTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly OrderPaymentFactory _factory;

    public OrderPaymentFactoryTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _factory = new OrderPaymentFactory(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentExists_ShouldReturnPayment()
    {
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);

        _orderRepositoryMock.SetupGetPaymentByOrderId(orderId, payment);

        ContentPaymentEntity result = await _factory.GetByOrderIdOrThrowAsync(orderId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(payment.Id);
        result.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetByOrderIdOrThrowAsync_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        Guid orderId = Guid.NewGuid();

        _orderRepositoryMock.SetupGetPaymentByOrderId(orderId, null); // null = not found

        Func<Task> act = async () => await _factory.GetByOrderIdOrThrowAsync(orderId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
```

---

## Test Coverage Target per Factory

| Test | Description |
|------|-------------|
| Happy path | All dependencies resolve, side effects called, result asserted |
| Not found — entity 1 | First dependency fails |
| Bad state — wrong status | Entity in wrong state (e.g., not Draft) |
| Not found — entity 2 | Second dependency fails (if applicable) |
| Not found — entity 3 | Third dependency fails (if applicable) |

---

## Key Rules

1. **Build the full object graph in Arrange** — if the factory loads order → item → tier, create all three entities with matching IDs
2. **Verify side effects on the happy path** — `VerifyAddItemTierCalled()`, `VerifyUpdateCalled()`, `VerifyCommitCalled()`
3. **Do NOT verify side effects on failure paths** — the factory should bail before reaching them
4. **Use factory-created submitted/paid orders** for "wrong status" tests — do not call `.Submit()` manually in the test
5. **Use `null`** to signal "not found" for nullable repository setups

---

## Real Test Files to Reference

| File | Key Pattern |
|------|-------------|
| `tests/Unit/Modules/Content/Application/Commerce/Factories/OrderPaymentFactoryTests.cs` | Single dependency, found/not found |
| `tests/Unit/Modules/Content/Application/Commerce/UseCases/Admin/Commands/AddItemTier/AdminAddItemTierFactoryTests.cs` | 4 dependencies, object graph, tuple return, side effect verification |
