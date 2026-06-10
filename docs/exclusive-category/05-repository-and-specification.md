# 05 — Repository and Specification Additions

## ICategoryRepository

**File:** `src/Modules/Content/Content/Application/Shared/Repositories/ICategoryRepository.cs`

Add one new method, following the `GetGossipCategoryAsync` pattern:

```csharp
/// <summary>
/// Retrieves the single active category designated as the exclusive show.
/// Returns null if no category is currently marked exclusive.
/// </summary>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>
/// The exclusive category entity if one exists, otherwise null.
/// </returns>
Task<CategoryEntity?> GetExclusiveCategoryAsync(CancellationToken cancellationToken = default);
```

## CategoryRepository Implementation

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Repositories/CategoryRepository.cs`

Implement using the new specification:

```csharp
public async Task<CategoryEntity?> GetExclusiveCategoryAsync(CancellationToken cancellationToken = default)
{
    var spec = new ExclusiveCategorySpecification();

    return await _context.Categories
        .Include(c => c.ContentType)
        .Include(c => c.Pricing)
            .ThenInclude(p => p.PricingTier)
        .FirstOrDefaultAsync(spec.ToExpression(), cancellationToken);
}
```

## ExclusiveCategorySpecification

**File:** `src/Modules/Content/Content/Application/Catalog/Specifications/CategorySpecifications.cs`

Add after `GossipCategorySpecification`:

```csharp
/// <summary>
/// Specification that matches the single active category designated as the exclusive show.
/// Used by the update handler to find and unset the current exclusive before setting a new one.
/// </summary>
public class ExclusiveCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.IsExclusive && category.IsActive;
    }
}
```

## Usage in Handlers

The `GetExclusiveCategoryAsync` method is used by `AdminCreateCategoryHandler` and `AdminUpdateCategoryHandler` to enforce the mutex:

```csharp
if (command.IsExclusive)
{
    CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
        cancellationToken: cancellationToken);

    if (currentExclusive is not null && currentExclusive.Id != category.Id)
    {
        currentExclusive.ClearExclusive();
    }
}
```

See [06-handler-changes.md](06-handler-changes.md) for the full handler modifications.
