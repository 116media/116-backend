# Writing Specification Tests

Specification tests verify that a specification's predicate correctly matches or excludes entities. They do not require mocks, a base class, or a database.

---

## Two Testing Approaches

### Approach 1: `ToExpression().Compile()(entity)` — preferred

Compiles the specification's expression into a delegate and invokes it directly. Use this for all specifications that do not use EF.Functions extensions.

```csharp
var spec = new ContentOrderByIdSpecification(orderId);
bool result = spec.ToExpression().Compile()(order);
result.Should().BeTrue();
```

### Approach 2: `spec.IsSatisfiedBy(entity)` — for simple specs

Works identically for specifications whose `ToExpression()` and `IsSatisfiedBy()` are equivalent. Used in some Identity specification tests.

```csharp
var spec = new ArticleByIdSpecification(articleId);
bool result = spec.IsSatisfiedBy(article);
result.Should().BeTrue();
```

### Approach 3: Compile-only test — for PostgreSQL-specific specs

Specifications that use `EF.Functions.ILike` or other server-side functions cannot be executed in memory. Test only that the expression compiles without throwing.

```csharp
[Fact]
public void ArticleBySlugSpecification_ShouldCompileExpression()
{
    var spec = new ArticleBySlugSpecification("my-slug");

    // Cannot invoke — ILike requires PostgreSQL. Just verify it compiles.
    Expression<Func<ArticleEntity, bool>> expression = spec.ToExpression();
    expression.Should().NotBeNull();
}
```

---

## Class Setup

```csharp
public class ContentOrderSpecificationTests
{
    // Shared IDs for consistent test data
    private static readonly Guid CustomerId = Guid.NewGuid();
}
```

---

## Simple Specification Tests (Match / No Match)

Always write both the match case and the no-match case.

```csharp
[Fact]
public void ByIdSpec_WhenIdMatches_ShouldReturnTrue()
{
    ContentOrderEntity order = ContentOrderFactory.Create();
    var spec = new ContentOrderByIdSpecification(order.Id);

    bool result = spec.ToExpression().Compile()(order);

    result.Should().BeTrue();
}

[Fact]
public void ByIdSpec_WhenIdDoesNotMatch_ShouldReturnFalse()
{
    ContentOrderEntity order = ContentOrderFactory.Create();
    var spec = new ContentOrderByIdSpecification(Guid.NewGuid()); // Different ID

    bool result = spec.ToExpression().Compile()(order);

    result.Should().BeFalse();
}
```

---

## Status Specification Tests

```csharp
[Fact]
public void ByStatusSpec_WhenStatusMatches_ShouldReturnTrue()
{
    ContentOrderEntity order = ContentOrderFactory.Create(); // Draft

    var spec = new ContentOrderByStatusSpecification(OrderStatus.Draft);
    bool result = spec.ToExpression().Compile()(order);

    result.Should().BeTrue();
}

[Fact]
public void ByStatusSpec_WhenStatusDoesNotMatch_ShouldReturnFalse()
{
    ContentOrderEntity order = ContentOrderFactory.CreateSubmitted(); // PendingPayment

    var spec = new ContentOrderByStatusSpecification(OrderStatus.Draft);
    bool result = spec.ToExpression().Compile()(order);

    result.Should().BeFalse();
}
```

---

## Boolean Field Specifications

```csharp
[Fact]
public void FreeCategorySpecification_WithFreeCategory_ShouldReturnTrue()
{
    CategoryEntity category = CategoryFactory.CreateFree(ContentTypeId);
    var spec = new FreeCategorySpecification();

    bool result = spec.ToExpression().Compile()(category);

    result.Should().BeTrue();
}

[Fact]
public void FreeCategorySpecification_WithPaidCategory_ShouldReturnFalse()
{
    CategoryEntity category = CategoryFactory.CreatePaid(ContentTypeId);
    var spec = new FreeCategorySpecification();

    bool result = spec.ToExpression().Compile()(category);

    result.Should().BeFalse();
}
```

---

## Composite Key Specification Tests

When a specification checks multiple fields, test all combinations:

```csharp
[Fact]
public void ContentOrderItemByIdAndOrderIdSpec_WhenBothMatch_ShouldReturnTrue()
{
    ContentOrderEntity order = ContentOrderFactory.Create();
    ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());

    var spec = new ContentOrderItemByIdAndOrderIdSpecification(item.Id, order.Id);
    bool result = spec.ToExpression().Compile()(item);

    result.Should().BeTrue();
}

[Fact]
public void ContentOrderItemByIdAndOrderIdSpec_WhenItemIdDoesNotMatch_ShouldReturnFalse()
{
    ContentOrderEntity order = ContentOrderFactory.Create();
    ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());

    var spec = new ContentOrderItemByIdAndOrderIdSpecification(Guid.NewGuid(), order.Id); // Wrong item ID

    bool result = spec.ToExpression().Compile()(item);

    result.Should().BeFalse();
}

[Fact]
public void ContentOrderItemByIdAndOrderIdSpec_WhenOrderIdDoesNotMatch_ShouldReturnFalse()
{
    ContentOrderEntity order = ContentOrderFactory.Create();
    ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());

    var spec = new ContentOrderItemByIdAndOrderIdSpecification(item.Id, Guid.NewGuid()); // Wrong order ID

    bool result = spec.ToExpression().Compile()(item);

    result.Should().BeFalse();
}
```

---

## Time-Based Specification Tests

```csharp
[Fact]
public void PromotedArticleSpecification_WithPromotedPublishedArticle_ShouldReturnTrue()
{
    ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId); // Published + PromotedUntil in future
    var spec = new PromotedArticleSpecification();

    bool result = spec.ToExpression().Compile()(article);

    result.Should().BeTrue();
}

[Fact]
public void PromotedArticleSpecification_WithPromotedDraftArticle_ShouldReturnFalse()
{
    // StampPromotion on a draft — spec requires Published status too
    ArticleEntity article = ArticleFactory.Create(CategoryId); // Draft
    article.StampPromotion(DateTimeOffset.UtcNow.AddDays(7));

    var spec = new PromotedArticleSpecification();
    bool result = spec.ToExpression().Compile()(article);

    result.Should().BeFalse();
}
```

---

## Reflection for Private Property Specs (e.g., CreatedAt)

Some specs filter on `CreatedAt` which is set by EF Core. Use reflection to set it in tests.

```csharp
[Fact]
public void AbandonedDraftSpecification_WithDraftCreatedBeforeCutoff_ShouldReturnTrue()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId); // Draft

    // Set CreatedAt to 48 hours ago via reflection
    typeof(ArticleEntity)
        .GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance)
        ?.SetValue(article, DateTimeOffset.UtcNow.AddHours(-48));

    var spec = new AbandonedDraftSpecification();
    bool result = spec.ToExpression().Compile()(article);

    result.Should().BeTrue();
}

[Fact]
public void AbandonedDraftSpecification_WithDraftCreatedAfterCutoff_ShouldReturnFalse()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId); // Draft

    // Recent draft — still within the retention window
    typeof(ArticleEntity)
        .GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance)
        ?.SetValue(article, DateTimeOffset.UtcNow.AddHours(-1));

    var spec = new AbandonedDraftSpecification();
    bool result = spec.ToExpression().Compile()(article);

    result.Should().BeFalse();
}
```

---

## Test Coverage Target

For every specification class, write:
- 1 test where the predicate returns `true`
- 1 test where the predicate returns `false`
- For composite specs: 1 test per mismatching field
- 1 compile-only test if the spec uses EF.Functions

---

## Real Test Files to Reference

| File | Key Pattern |
|------|-------------|
| `tests/Unit/Modules/Content/Application/Commerce/Specifications/ContentOrderSpecificationTests.cs` | ToExpression().Compile(), composite key, payment spec |
| `tests/Unit/Modules/Content/Application/Editorial/Specifications/ArticleSpecificationsTests.cs` | IsSatisfiedBy, compile-only (ILike), PromotedArticle, AbandonedDraft with reflection |
| `tests/Unit/Modules/Content/Application/Catalog/Specifications/CatalogSpecificationsTests.cs` | Boolean field specs (IsFree, IsActive), CategoryPricingByIds composite |
