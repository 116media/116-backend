# Factory Spec Template

Application factories encapsulate multi-step operations that are too complex for a single handler.
They load entities, apply domain logic, persist side effects, and commit.
Handlers delegate to factories — factories do the actual work.

Use this template when a use case requires more than one entity load + one persist.

---

```markdown
# Spec: [AdminMyFeatureFactory]

## Intent

[What complex operation does this factory encapsulate? Why is it a factory and not inline in the handler?
What are the multiple steps it orchestrates?]

---

## Interface

```csharp
// Contract interface in UseCases/Admin/Commands/MyFeature/Contracts/
public interface IMyFeatureFactory
{
    Task<MyFeatureResult> MyMethodAsync(
        AdminMyFeatureCommand command,
        CancellationToken ct
    );
}

// Or when result is a tuple:
public interface IMyFeatureFactory
{
    Task<(EntityOne entity, string entityName)> MyMethodAsync(
        AdminMyFeatureCommand command,
        CancellationToken ct
    );
}
```

---

## Dependencies

| Dependency | Interface | What it's used for |
|-----------|-----------|-------------------|
| Order repository | `IContentOrderRepository` | Load order with items |
| Category repository | `ICategoryRepository` | Load pricing for category |
| Lookup repository | `ILookupRepository` | Load pricing tier |
| Unit of work | `IContentUnitOfWork` | Commit transaction |

---

## Step-by-Step Logic

List every step the factory executes, in order:

```
1. Parse orderId = Guid.Parse(command.OrderId)
2. Load order = contentOrderRepository.GetByIdWithItemsAsync(orderId, ct)
   → If null: throw ContentOrderErrors.NotFound(orderId)
3. order.EnsureDraft()
   → Throws BadRequestException if not Draft
4. Parse orderItemId = Guid.Parse(command.OrderItemId)
5. Load item = contentOrderRepository.GetItemByIdAsync(orderId, orderItemId, ct)
   → If null: throw ContentOrderErrors.ItemNotFound(orderItemId)
6. Parse pricingTierId = Guid.Parse(command.PricingTierId)
7. Load pricingTier = lookupRepository.GetPricingTierByIdOrThrowAsync(pricingTierId, ct)
   → Throws NotFoundException if not found
8. Load pricing = categoryRepository.GetPricingAsync(item.CategoryId, pricingTierId, ct)
   → If null: throw ContentOrderErrors.CategoryPricingNotFound(item.CategoryId, pricingTierId)
9. Create tier entity: ContentItemTierEntity.Create(Guid.NewGuid(), item.Id, pricingTier.Id, pricing.PriceUsd)
10. order.RecalculateTotal(pricing.PriceUsd)
11. contentOrderRepository.AddItemTierAsync(tier, ct)
12. contentOrderRepository.UpdateAsync(order, ct)
13. unitOfWork.CommitAsync(ct)
14. Return (tier, pricingTier.Name)
```

---

## Side Effects

1. `ContentItemTierEntity.Create(...)` — new entity created in memory
2. `order.RecalculateTotal(priceUsd)` — TotalAmountUsd updated
3. `contentOrderRepository.AddItemTierAsync(tier, ct)` — tier persisted
4. `contentOrderRepository.UpdateAsync(order, ct)` — order total persisted
5. `unitOfWork.CommitAsync(ct)` — single commit

---

## Error Cases

| Step | Trigger | Exception class | Error factory |
|------|---------|----------------|---------------|
| 2 | Order not found | `NotFoundException` | `ContentOrderErrors.NotFound(orderId)` |
| 3 | Order not Draft | `BadRequestException` | `ContentOrderErrors.NotInDraft(orderId)` |
| 5 | Item not found | `NotFoundException` | `ContentOrderErrors.ItemNotFound(itemId)` |
| 7 | Pricing tier not found | `NotFoundException` | Thrown by `GetPricingTierByIdOrThrowAsync` |
| 8 | Category pricing not found | `NotFoundException` | `ContentOrderErrors.CategoryPricingNotFound(...)` |

---

## Return Value

```csharp
// Simple success:
Task<bool> — always true

// Tuple (when caller needs entity + metadata):
Task<(ContentItemTierEntity tier, string tierName)>

// Named result:
Task<AdminMyFeatureFactoryResult>
public record AdminMyFeatureFactoryResult(
    ContentItemTierEntity Tier,
    string TierName,
    decimal PriceUsd
);
```

---

## Test Cases

**Factory tests (`AdminMyFeatureFactoryTests`):**

```
[Happy path — verify full object graph + side effects]
- MyMethodAsync_WhenAllValid_ShouldReturnResultAndCommit
  Arrange: Create order (Draft), item, pricingTier, categoryPricing
  Setup: SetupGetByIdWithItems(order), SetupGetItemById(order.Id, item.Id, item),
         SetupGetPricingTierByIdOrThrow(tier), SetupGetPricing(categoryId, tierId, pricing)
  Assert: result.tier.PricingTierId == tier.Id, result.tierName == tier.Name
  Verify: VerifyAddItemTierCalled(), VerifyUpdateCalled(), VerifyCommitCalled()

[Failure paths — one per error case]
- MyMethodAsync_WhenOrderNotFound_ShouldThrowNotFoundException
  Setup: SetupGetByIdWithItems(null)

- MyMethodAsync_WhenOrderNotDraft_ShouldThrowBadRequestException
  Setup: Use ContentOrderFactory.CreateSubmitted() — do NOT call .Submit() manually

- MyMethodAsync_WhenItemNotFound_ShouldThrowNotFoundException
  Setup: SetupGetByIdWithItems(order), SetupGetItemById(order.Id, anyGuid, null)

- MyMethodAsync_WhenCategoryPricingNotFound_ShouldThrowNotFoundException
  Setup: SetupGetByIdWithItems(order), SetupGetItemById(..., item),
         SetupGetPricingTierByIdOrThrow(tier), SetupGetPricing(categoryId, tierId, null)
```

---

## Mock Setup Reference

```csharp
// In constructor:
_orderRepositoryMock    = MockContentOrderRepository.Create();
_categoryRepositoryMock = MockCategoryRepository.Create();
_lookupRepositoryMock   = MockLookupRepository.Create();
_unitOfWorkMock         = MockContentUnitOfWork.Create();

_factory = new AdminMyFeatureFactory(
    _orderRepositoryMock.Object,
    _categoryRepositoryMock.Object,
    _lookupRepositoryMock.Object,
    _unitOfWorkMock.Object
);

// In happy-path test:
_orderRepositoryMock.SetupGetByIdWithItems(order);
_orderRepositoryMock.SetupGetItemById(order.Id, item.Id, item);
_lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(tier);
_categoryRepositoryMock.SetupGetPricing(item.CategoryId, tier.Id, pricing);
```
```

---

## Factory vs Handler — when to use which

| Complexity | Where logic lives |
|-----------|------------------|
| 1 entity load + 1 persist | Handler directly |
| 2+ entity loads OR 2+ persists | Factory (handler delegates) |
| Shared logic used by multiple handlers | Factory |
| Returned value is used by handler for further processing | Factory returning a typed result |