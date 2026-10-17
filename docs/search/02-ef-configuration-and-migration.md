# EF Configuration and Migration

## Checklist

- [x] `SearchDocument` mapped to `content.search_documents` with a composite key
- [x] The generated vector mapped read-only, never sent on a write
- [x] The projection kept out of the model on a non-Npgsql provider
- [x] Migration carries the raw SQL for the vector, the index and the backfill
- [x] `dotnet ef migrations has-pending-model-changes` reports none
- [x] `Content.Domain` gains no persistence package reference

---

## Why the vector is not on the source entities

Two rules forbid it, and the second applies to a shadow property as much as a real one.

`tests/Architecture.Tests/LayerDependencyTests.cs` asserts
`DomainDependsOnNoPersistenceOrWebFramework`, whose forbidden list includes `Npgsql`.
`NpgsqlTsVector` is an `Npgsql` type, so it cannot sit on a `Content.Domain` entity.

And EF materialises every mapped property, shadow properties included. Every article list
path returns entities rather than projections, so a mapped vector would be selected on each
of them — and the article vector averaged **711 bytes** against a 1,123-byte row, so that is
roughly 60% added to the payload of every feed page for a column no feed reads.

The projection sidesteps both: the vector lives on a type the domain never sees, in a table
ordinary reads never touch.

---

## The mapping

```csharp
builder.ToTable("search_documents", ContentConstants.SchemaName);
builder.HasKey(document => new { document.Type, document.EntityId });

builder.Property(document => document.Type).HasColumnName("type").HasConversion<string>();

builder
    .Property(document => document.SearchVector)
    .HasColumnName("search_vector")
    .HasColumnType("tsvector")
    .ValueGeneratedOnAddOrUpdate();

builder.Property(document => document.SearchVector).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
builder.Property(document => document.SearchVector).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
```

The database owns the vector, so EF must never include it in an `INSERT` or `UPDATE`. The two
save behaviours are what enforce that.

`SearchDocumentConfiguration` implements the `IPostgresOnlyConfiguration` marker, and
`OnModelCreating` ignores `SearchDocument` entirely when the provider is not Npgsql —
`NpgsqlTsVector` has no meaning elsewhere, and a `DbSet` pulls its type into the model
whether or not a configuration ran.

### The composite key caught a latent bug

`OnModelCreating` walked every entity type calling `SingleOrDefault()` on its primary-key
properties, to mark Guid keys as never store-generated. `SearchDocument` is the first entity
with a two-property key, and that call threw `Sequence contains more than one element` at
design time. The loop now skips composite keys rather than assuming one property.

---

## Migration

```bash
dotnet ef migrations add AddSearchDocuments \
  --project src/modules/Content/Content/src/Content.Infrastructure \
  --startup-project src/host/Api \
  --context ContentDbContext
```

EF scaffolds the table but can only emit a plain `tsvector` column, so the migration drops
that column and re-adds it as `GENERATED ALWAYS AS (...) STORED`, then creates the partial
index, backfills from the three source tables, and drops what it supersedes: the three
`*_search` views, the three partial GIN indexes, and the `search_vector` column on
`articles`, `videos` and `short_videos`.

`Down()` restores all of it, so the migration is reversible.

Dropping those columns is a real reclaim, not just tidiness: the article vector alone was
711 bytes per row.
