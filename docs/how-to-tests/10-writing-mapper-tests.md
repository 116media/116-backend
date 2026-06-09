# Writing Mapper Tests

Mapper tests verify that extension methods (or Mapster profiles) correctly map entities to DTOs. Some mapper tests need only the mapper instance; others need a real in-memory database to load navigation properties.

---

## When to Use Mapper Tests

- Testing extension methods like `entity.ToDto()`, `entity.ToSummaryDto()`, `entity.ToDetailDto()`
- Testing null propagation (`null` input → `null` output)
- Testing that navigation properties are correctly included in mapped output

---

## Class Setup (with Mapper only)

Extend `BaseContentHandlerTest` (or `BaseHandlerTest` for Identity/Core) to get a pre-configured `IMapper`.

```csharp
public class ContentOrderMapperTests : BaseContentHandlerTest
{
    // Mapper is available as 'Mapper' from the base class
}
```

---

## Class Setup (with InMemory Database)

When navigation properties need to be loaded (e.g., `order.Customer`, `order.Items`), use an in-memory EF Core database. Implement `IDisposable` to clean up.

```csharp
public class ContentOrderMapperTests : BaseContentHandlerTest, IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ContentOrderRepository _repository;

    public ContentOrderMapperTests()
    {
        DbContextOptions<ContentDbContext> options =
            new DbContextOptionsBuilder<ContentDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test class
                .Options;

        _context = new ContentDbContext(options);
        _repository = new ContentOrderRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

**Rule:** Always use `Guid.NewGuid().ToString()` as the database name. This ensures each test class gets an isolated in-memory database that cannot bleed state between test classes.

---

## Null Propagation Test

```csharp
[Fact]
public void ToFileDto_WhenEntityIsNull_ShouldReturnNull()
{
    FileEntity? entity = null;

    FileDto? result = entity.ToFileDto();

    result.Should().BeNull();
}
```

---

## Simple Mapping Test

```csharp
[Fact]
public void ToFileDto_WhenEntityIsNotNull_ShouldMapToFileDto()
{
    FileEntity entity = FileFactory.CreateWithTestValues();

    FileDto result = entity.ToFileDto();

    result.Should().NotBeNull();
    result.Id.Should().Be(entity.Id);
    result.FileName.Should().Be(entity.FileName);
    result.OriginalFileName.Should().Be(entity.OriginalFileName);
    result.MimeType.Should().Be(entity.MimeType);
    result.StorageUrl.Should().Be(entity.StorageUrl);
    result.SizeInBytes.Should().Be(entity.SizeInBytes);
}
```

---

## Conditional Mapping Test (Null Optional)

```csharp
[Fact]
public void ToPaymentDto_WithNullProofFile_ShouldReturnDtoWithNullProof()
{
    ContentPaymentEntity payment = ContentPaymentFactory.Create(Guid.NewGuid());

    // Extension method signature: payment.ToPaymentDto(FileDto? proofFile)
    ContentPaymentDto result = payment.ToPaymentDto(proofFile: null);

    result.Should().NotBeNull();
    result.ProofFile.Should().BeNull();
}

[Fact]
public void ToPaymentDto_WithProofFile_ShouldInjectProofFile()
{
    ContentPaymentEntity payment = ContentPaymentFactory.Create(Guid.NewGuid());
    var proofFile = new FileDto(
        Id: Guid.NewGuid(),
        FileName: "proof.jpg",
        OriginalFileName: "proof.jpg",
        MimeType: "image/jpeg",
        StorageUrl: TestConstants.File.ValidStorageUrl,
        SizeInBytes: 1024
    );

    ContentPaymentDto result = payment.ToPaymentDto(proofFile: proofFile);

    result.ProofFile.Should().NotBeNull();
    result.ProofFile!.FileName.Should().Be("proof.jpg");
}
```

---

## InMemory Database Test (Navigation Properties)

When the mapper calls `.Include()` to load navigation properties, seed the in-memory database first.

```csharp
[Fact]
public async Task ToContentOrderSummaryDto_ShouldMapCustomerNameAndStatus()
{
    // Arrange — seed related entities
    CustomerEntity customer = CustomerFactory.Create();
    await _context.Customers.AddAsync(customer);

    ContentOrderEntity order = ContentOrderFactory.Create(customer.Id);
    await _context.ContentOrders.AddAsync(order);

    await _context.SaveChangesAsync();

    // Act — load with navigation properties
    ContentOrderEntity? loaded = await _context.ContentOrders
        .Include(o => o.Customer)
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == order.Id);

    ContentOrderSummaryDto result = loaded!.ToSummaryDto();

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(order.Id);
    result.CustomerName.Should().Be(customer.Name);
    result.ItemCount.Should().Be(0);
}
```

---

## Complex InMemory Seeding

For deeply nested navigation (order → items → tiers → category):

```csharp
[Fact]
public async Task ToOrderItemDto_ShouldMapCategoryName()
{
    // Seed all required entities in dependency order
    ContentTypeEntity contentType = ContentTypeFactory.Create();
    CategoryEntity category = CategoryFactory.Create(contentType.Id);
    CustomerEntity customer = CustomerFactory.Create();
    ContentOrderEntity order = ContentOrderFactory.Create(customer.Id);
    ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, category.Id);

    await _context.ContentTypes.AddAsync(contentType);
    await _context.Categories.AddAsync(category);
    await _context.Customers.AddAsync(customer);
    await _context.ContentOrders.AddAsync(order);
    await _context.ContentOrderItems.AddAsync(item);
    await _context.SaveChangesAsync();

    // Load with all navigations
    ContentOrderItemEntity? loaded = await _context.ContentOrderItems
        .Include(i => i.Category)
        .Include(i => i.Tiers)
        .FirstOrDefaultAsync(i => i.Id == item.Id);

    ContentOrderItemDto result = loaded!.ToOrderItemDto();

    result.CategoryName.Should().Be(category.Name);
}
```

---

## Key Rules

1. **Use `Guid.NewGuid().ToString()` for in-memory database name** — never reuse a fixed name between test classes
2. **Implement `IDisposable` whenever you create a `DbContext`** — always call `EnsureDeleted()` and `Dispose()` in `Dispose()`
3. **Seed entities in FK dependency order** — parent before child (ContentType before Category, Customer before Order, etc.)
4. **Call `SaveChangesAsync()` before querying** — in-memory DB is not populated until saved
5. **Always load with explicit `.Include()`** — InMemory does not lazy-load navigation properties

---

## Real Test Files to Reference

| File | Key Pattern |
|------|-------------|
| `tests/Unit/Modules/Content/Application/Commerce/Mappers/ContentOrderMapperTests.cs` | Full example: null test, conditional mapping, InMemory seeding, IDisposable |
