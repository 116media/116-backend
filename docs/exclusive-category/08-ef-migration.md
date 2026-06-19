# 08 — EF Migration

## Migration Name

```
AddPosterAndExclusiveToCategory
```

## Generation Command

```bash
dotnet ef migrations add AddPosterAndExclusiveToCategory \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Expected Schema Changes

The generated migration should produce:

### Up

```csharp
migrationBuilder.AddColumn<Guid>(
    name: "poster_file_id",
    schema: "content",
    table: "categories",
    type: "uuid",
    nullable: true);

migrationBuilder.AddColumn<bool>(
    name: "is_exclusive",
    schema: "content",
    table: "categories",
    type: "boolean",
    nullable: false,
    defaultValue: false);

migrationBuilder.CreateIndex(
    name: "ix_categories_is_exclusive",
    schema: "content",
    table: "categories",
    column: "is_exclusive",
    unique: true,
    filter: "is_exclusive = true");
```

### Down

```csharp
migrationBuilder.DropIndex(
    name: "ix_categories_is_exclusive",
    schema: "content",
    table: "categories");

migrationBuilder.DropColumn(
    name: "is_exclusive",
    schema: "content",
    table: "categories");

migrationBuilder.DropColumn(
    name: "poster_file_id",
    schema: "content",
    table: "categories");
```

## Notes

- Only the Content module needs a migration. The Core module (`FileEntity`) is unchanged.
- `poster_file_id` is a logical FK — no EF `HasForeignKey` since it crosses schema boundaries (`content` → `core`).
- The unique filtered index on `is_exclusive` mirrors the existing one on `is_gossip_fallback`.
- Both columns have safe defaults (`null` and `false`), so the migration is non-breaking on existing data.
