# 01 — Domain Entity Changes

**File:** `src/Modules/Content/Content/Domain/Entities/CategoryEntity.cs`

## New Properties

Add two new properties after `IsGossip`:

```csharp
/// <summary>
/// Optional reference to a FileEntity storing the show's poster image.
/// Nullable because article categories do not need a poster — only video
/// categories (shows) use it for the exclusive homepage section.
/// Resolved to a URL at mapping time via IFileRepository.
/// </summary>
public Guid? PosterFileId { get; private set; }

/// <summary>
/// When true, this show is the currently featured exclusive.
/// Exactly one category should have this flag set at a time (mutex).
/// The handler layer enforces the mutex by unsetting the previous exclusive
/// before setting the new one.
/// </summary>
public bool IsExclusive { get; private set; }
```

## Method Changes

### `Create()`

Add two new parameters:

```csharp
public static CategoryEntity Create(
    Guid id,
    Guid contentTypeId,
    string name,
    string slug,
    string description,
    bool isFree,
    CategoryErrors errors,
    bool isGossip = false,
    bool isExclusive = false   // <-- new
)
```

Set `IsExclusive = isExclusive` in the object initializer.

`PosterFileId` is **not** set at creation time — it requires a separate upload call (same pattern as article cover images).

### `Update()`

Add `isExclusive` parameter:

```csharp
public void Update(
    string name,
    string slug,
    string description,
    bool isGossip,
    bool isExclusive,          // <-- new
    CategoryErrors errors
)
```

Set `IsExclusive = isExclusive` in the method body.

### New method: `SetPosterFileId()`

```csharp
/// <summary>
/// Sets or clears the poster image file reference.
/// </summary>
/// <param name="posterFileId">The FileEntity ID, or null to clear.</param>
public void SetPosterFileId(Guid? posterFileId)
{
    PosterFileId = posterFileId;
}
```

### New method: `SetExclusive()`

```csharp
/// <summary>
/// Marks this category as the exclusive show.
/// The handler is responsible for calling ClearExclusive() on the previously
/// exclusive category before calling this method.
/// </summary>
public void SetExclusive()
{
    IsExclusive = true;
}
```

### New method: `ClearExclusive()`

```csharp
/// <summary>
/// Removes the exclusive flag from this category.
/// Called by the handler on the previously exclusive category before setting a new one.
/// </summary>
public void ClearExclusive()
{
    IsExclusive = false;
}
```

## Constants

**File:** `src/Modules/Content/Content/Domain/Constants/ContentConstants.cs`

No new constants needed — `PosterFileId` is a `Guid?` FK (no max length), and `IsExclusive` is a `bool`.
