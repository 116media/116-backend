# Writing Domain Entity Tests

Domain entity tests validate the entity's state machine, invariants, guard clauses, and business methods. They are the simplest tests: no mocks, no base class, no database.

---

## When to Use This Test Type

- Testing an entity's `Create()` factory method and its validation
- Testing every state transition method (`Submit()`, `Approve()`, `Publish()`, etc.)
- Testing guard clauses that throw exceptions on invalid transitions
- Testing counter methods (`IncrementLikeCount()`, `DecrementBookmarkCount()`)
- Testing domain-level field updates (`UpdateSeo()`, `UpdateCoverImage()`, etc.)

---

## Class Setup

```csharp
// No base class. No mocks. Static fields for shared IDs.
public class ContentOrderEntityTests
{
    // Shared IDs needed across tests — static readonly
    private static readonly Guid CustomerId = Guid.NewGuid();
}
```

---

## Create / Factory Method Tests

```csharp
[Fact]
public void Create_ShouldSetId_CustomerId_StatusDraft_TotalZero()
{
    // Arrange
    Guid customerId = Guid.NewGuid();

    // Act
    ContentOrderEntity order = ContentOrderEntity.Create(customerId);

    // Assert
    order.Id.Should().NotBeEmpty();
    order.CustomerId.Should().Be(customerId);
    order.Status.Should().Be(OrderStatus.Draft);
    order.TotalAmountUsd.Should().Be(0m);
}

[Fact]
public void Create_WithPackageId_ShouldSetPackageId()
{
    Guid packageId = Guid.NewGuid();
    ContentOrderEntity order = ContentOrderEntity.Create(Guid.NewGuid(), packageId);
    order.PackageId.Should().Be(packageId);
}
```

---

## Validation Tests with `[Theory]` + `[InlineData]`

Use `[Theory]` with `[InlineData]` to test all invalid inputs in one method.

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void CreateFree_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
{
    // Act
    Action act = () => ArticleEntity.CreateFree(
        categoryId: CategoryId,
        title: invalidTitle!,
        slug: "valid-slug",
        authorId: Guid.NewGuid()
    );

    // Assert
    act.Should().Throw<BadRequestException>();
}

// Decimal boundary testing
[Theory]
[InlineData(-0.01)]
[InlineData(-100)]
public void Create_WithNegativePrice_ShouldThrowBadRequestException(decimal invalidPrice)
{
    Action act = () => PackageEntity.Create(name: "Test", priceUsd: invalidPrice);
    act.Should().Throw<BadRequestException>();
}

// Integer boundary testing
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public void Create_WithDurationDaysLessThanOrEqualToZero_ShouldThrowBadRequestException(int invalidDuration)
{
    Action act = () => PromotionLevelEntity.Create(name: "Promo", durationDays: invalidDuration, priceUsd: 10m);
    act.Should().Throw<BadRequestException>();
}
```

---

## State Transition Tests

Test every valid and invalid transition. Use factories to reach the required starting state.

```csharp
// Valid transition
[Fact]
public void Submit_WhenDraft_ShouldTransitionToPendingPayment()
{
    // Arrange — use factory for correct starting state
    ContentOrderEntity order = ContentOrderFactory.Create();   // Draft

    // Act
    order.Submit();

    // Assert
    order.Status.Should().Be(OrderStatus.PendingPayment);
}

// Invalid transition — must throw
[Fact]
public void Submit_WhenNotDraft_ShouldThrowConflictException()
{
    ContentOrderEntity order = ContentOrderFactory.CreateSubmitted(); // Already submitted

    Action act = () => order.Submit();

    act.Should().Throw<ConflictException>();
}

// Multi-step transition
[Fact]
public void Archive_ShouldTransitionToArchived()
{
    ArticleEntity article = ArticleEntity.CreateFree(CategoryId, "Title", "slug", Guid.NewGuid());
    article.Submit();
    article.MarkPendingReview();
    article.Approve();
    article.Publish(DateTimeOffset.UtcNow);

    article.Archive();

    article.Status.Should().Be(ArticleStatus.Archived);
}
```

---

## Idempotency Tests (Boolean-Returning Methods)

Some domain methods return `bool` to indicate whether the state actually changed.

```csharp
[Fact]
public void Activate_WhenInactive_ShouldReturnTrue()
{
    CategoryEntity category = CategoryFactory.CreateInactive(ContentTypeId);

    bool result = category.Activate();

    result.Should().BeTrue();
    category.IsActive.Should().BeTrue();
}

[Fact]
public void Activate_WhenAlreadyActive_ShouldReturnFalse()
{
    CategoryEntity category = CategoryFactory.Create(ContentTypeId); // Active by default

    bool result = category.Activate();

    result.Should().BeFalse();
}
```

---

## Counter Tests

Always test both the increment path and the boundary at zero.

```csharp
[Fact]
public void IncrementLikeCount_ShouldIncrement()
{
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);

    article.IncrementLikeCount();

    article.LikeCount.Should().Be(1);
}

[Fact]
public void DecrementLikeCount_WhenAboveZero_ShouldDecrement()
{
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
    article.IncrementLikeCount();

    article.DecrementLikeCount();

    article.LikeCount.Should().Be(0);
}

[Fact]
public void DecrementLikeCount_WhenAtZero_ShouldStayAtZero()
{
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);

    article.DecrementLikeCount(); // Should not go below 0

    article.LikeCount.Should().Be(0);
}
```

---

## Field Update Tests

```csharp
[Fact]
public void UpdateSeo_ShouldSetMetaFields()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId);
    const string metaTitle = "Meta Title";
    const string metaDescription = "Meta Description";

    article.UpdateSeo(metaTitle: metaTitle, metaDescription: metaDescription);

    article.MetaTitle.Should().Be(metaTitle);
    article.MetaDescription.Should().Be(metaDescription);
}

[Fact]
public void Update_ShouldUpdateAllFields()
{
    CategoryEntity category = CategoryFactory.Create(ContentTypeId);
    Guid newContentTypeId = Guid.NewGuid();

    category.Update(
        name: "New Name",
        slug: "new-slug",
        description: "New description",
        contentTypeId: newContentTypeId,
        isFree: false
    );

    category.Name.Should().Be("New Name");
    category.Slug.Should().Be("new-slug");
    category.ContentTypeId.Should().Be(newContentTypeId);
}

[Fact]
public void Update_WithNullDescription_ShouldClearDescription()
{
    PricingTierEntity tier = PricingTierFactory.Create();

    tier.Update(name: "New Name", description: null);

    tier.Description.Should().BeNull();
}
```

---

## DateTime / DateTimeOffset Tests

```csharp
[Fact]
public void Publish_ShouldTransitionToPublished_AndSetPublishedAt()
{
    ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
    DateTimeOffset before = DateTimeOffset.UtcNow;

    article.Publish(DateTimeOffset.UtcNow);

    article.Status.Should().Be(ArticleStatus.Published);
    article.PublishedAt.Should().NotBeNull();
    article.PublishedAt.Should().BeOnOrAfter(before);
}

[Fact]
public void StampPromotion_ShouldSetIsPromotedAndPromotedUntil()
{
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
    DateTimeOffset until = DateTimeOffset.UtcNow.AddDays(7);

    article.StampPromotion(until);

    article.IsPromoted.Should().BeTrue();
    article.PromotedUntil.Should().Be(until);
}
```

---

## Reflection for Private Property Tests

When specifications or behavior depends on a private/init-only property (e.g., `CreatedAt`) that can't be set directly:

```csharp
[Fact]
public void AbandonedDraftSpecification_WithDraftArticleCreatedBeforeCutoff_ShouldReturnTrue()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId); // Draft

    // Set CreatedAt to 48 hours ago via reflection
    var prop = typeof(ArticleEntity).GetProperty("CreatedAt",
        BindingFlags.Public | BindingFlags.Instance);
    prop?.SetValue(article, DateTimeOffset.UtcNow.AddHours(-48));

    var spec = new AbandonedDraftSpecification();
    bool result = spec.ToExpression().Compile()(article);

    result.Should().BeTrue();
}
```

---

## Real Test Files to Reference

| File | What to copy from |
|------|------------------|
| `tests/Unit/Modules/Content/Domain/ContentOrderEntityTests.cs` | Multi-step state transitions, RecalculateTotal with nested factories |
| `tests/Unit/Modules/Content/Domain/ContentPaymentEntityTests.cs` | Payment state machine (Pending → Verified/Rejected) |
| `tests/Unit/Modules/Content/Domain/Entities/ArticleEntityTests.cs` | Full state machine, InlineData, counters, StampPromotion, reflection |
| `tests/Unit/Modules/Content/Domain/Entities/CategoryEntityTests.cs` | Boolean-returning Activate/Deactivate |
| `tests/Unit/Modules/Content/Domain/Entities/PackageEntityTests.cs` | Decimal boundary InlineData |
| `tests/Unit/Modules/Content/Domain/Entities/PromotionLevelEntityTests.cs` | Three-parameter creation, decimal + int boundaries |
| `tests/Unit/Modules/Content/Domain/Entities/ShortVideoEntityTests.cs` | Two creation paths (Standalone vs Teaser) |
