# 02 — EF Configuration

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`

## New Property Configurations

Add after the `IsGossip` block:

```csharp
builder.Property(x => x.PosterFileId).IsRequired(false);

builder
    .Property(x => x.IsExclusive)
    .HasColumnName("is_exclusive")
    .IsRequired()
    .HasDefaultValue(false);
```

## New Index: IsExclusive Mutex

Same pattern as the `IsGossip` unique filtered index:

```csharp
builder
    .HasIndex(x => x.IsExclusive)
    .IsUnique()
    .HasFilter("is_exclusive = true");
```

This guarantees at the database level that at most one row can have `is_exclusive = true`. Any race condition that bypasses the handler-level mutex will be caught by this unique constraint.

## Existing Pattern Reference

Current `IsGossip` configuration for comparison:

```csharp
builder
    .Property(x => x.IsGossip)
    .HasColumnName("is_gossip_fallback")
    .IsRequired()
    .HasDefaultValue(false);

builder
    .HasIndex(x => x.IsGossip)
    .IsUnique()
    .HasFilter("is_gossip_fallback = true");
```

## Notes

- `PosterFileId` is a logical FK to `core.files`, but since `CategoryEntity` lives in the `content` schema and `FileEntity` in the `core` schema (separate DbContexts), it is **not** configured as an EF navigation/FK. The relationship is resolved at the application layer via `IFileRepository.GetByIdAsync()` — same pattern as `ArticleEntity.CoverImageFileId`, `VideoEntity.ThumbnailFileId`, and `ShortVideoEntity.ThumbnailFileId`.
