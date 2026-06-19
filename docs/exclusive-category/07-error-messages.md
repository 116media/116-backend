# 07 — Error Messages

## CategoryErrors

**File:** `src/Modules/Content/Content/Application/Shared/Errors/CategoryErrors.cs`

No new error factory methods are strictly needed for the exclusive mutex — the handler silently unsets the previous exclusive category without throwing. The unique filtered index on `is_exclusive` provides database-level protection as a last resort.

However, if we want to surface a user-facing message when a category cannot be made exclusive (e.g., inactive categories), add:

```csharp
/// <summary>
/// Throws when an inactive category is being set as exclusive.
/// </summary>
public BadRequestException CannotMakeInactiveExclusive()
{
    return new BadRequestException(i18n.CannotMakeInactiveExclusive());
}
```

## CategoryErrorMessage

**File:** `src/Modules/Content/Content/Application/Shared/Errors/Messages/CategoryErrorMessage.cs`

Add the new i18n message key:

```csharp
/// <summary>
/// Message when trying to set an inactive category as exclusive.
/// </summary>
public string CannotMakeInactiveExclusive()
{
    return _localizer["Category.CannotMakeInactiveExclusive"];
}
```

## Resource Files

Add the translation key to all locale resource files:

| Key | EN | FR |
|-----|----|----|
| `Category.CannotMakeInactiveExclusive` | `An inactive category cannot be set as exclusive.` | `Une catégorie inactive ne peut pas être définie comme exclusive.` |

## Validation Rules

The exclusive-inactive guard should be added in the `AdminUpdateCategoryHandler` and `AdminCreateCategoryHandler`:

```csharp
if (command.IsExclusive && !category.IsActive)
{
    throw i18n.Category.CannotMakeInactiveExclusive();
}
```

For `Create`, since new categories default to `IsActive = true`, this guard is only relevant if someone sends `IsExclusive = true` and immediately deactivates. The guard in `Update` is the important one.
