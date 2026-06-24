# 08 — EF Migration

## Migration Name

```
AddPinnedToFeedToCategory
```

## Generation Command

```bash
dotnet ef migrations add AddPinnedToFeedToCategory \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Expected Schema Changes

### Up

```csharp
migrationBuilder.AddColumn<DateTimeOffset>(
    name: "pinned_to_feed_at",
    schema: "content",
    table: "categories",
    type: "timestamp with time zone",
    nullable: true);

migrationBuilder.CreateIndex(
    name: "ix_categories_pinned_to_feed_at",
    schema: "content",
    table: "categories",
    column: "pinned_to_feed_at",
    filter: "pinned_to_feed_at IS NOT NULL");
```

### Down

```csharp
migrationBuilder.DropIndex(
    name: "ix_categories_pinned_to_feed_at",
    schema: "content",
    table: "categories");

migrationBuilder.DropColumn(
    name: "pinned_to_feed_at",
    schema: "content",
    table: "categories");
```

## Notes

- Only the Content module needs a migration.
- The index is a **non-unique** partial index — unlike the `is_exclusive` / `is_gossip_fallback`
  unique filtered indexes, the feed allows up to 5 pinned categories per content type.
- `pinned_to_feed_at` defaults to `null` (not pinned), so the migration is non-breaking on
  existing rows — no category is pinned until an admin explicitly pins it.
- This column is unrelated to the historical `is_featured` / `featured_until` columns on
  `videos` and `articles` (since renamed to `is_promoted` / `promoted_until`). No collision.
- Run `dotnet csharpier .` after generating the migration, then `dotnet build` to confirm
  the snapshot compiles.
