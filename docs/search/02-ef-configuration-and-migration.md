# EF Configuration and Migration

## Checklist

- [ ] `SearchVector` property mapped as computed and stored
- [ ] Property excluded from inserts and updates
- [ ] `NpgsqlTsVector` package reference present
- [ ] Migration generated with the raw SQL for the function and columns
- [ ] `dotnet ef migrations has-pending-model-changes` reports none

---

## Entity

The column is database-generated, so the entity exposes it as a private-set property
that EF never writes. It is not part of the domain: nothing in `Content.Domain`
reads it, and no factory or verb touches it.

```csharp
// Content.Domain/Entities/ArticleEntity.cs
/// <summary>
/// The full-text search vector, maintained by the database as a generated column.
/// Never written by the application.
/// </summary>
public NpgsqlTsVector SearchVector { get; private init; } = null!;
```

`NpgsqlTsVector` comes from `Npgsql.EntityFrameworkCore.PostgreSQL`, already referenced
by `Content.Infrastructure`.

---

## Configuration

```csharp
// Content.Infrastructure/Persistence/Configurations/ArticleConfiguration.cs
builder
    .Property(article => article.SearchVector)
    .HasColumnName("search_vector")
    .HasColumnType("tsvector")
    .ValueGeneratedOnAddOrUpdate();

builder
    .HasIndex(article => article.SearchVector)
    .HasDatabaseName("ix_articles_search_vector_published")
    .HasMethod("GIN")
    .HasFilter("status = 'Published'");
```

`ValueGeneratedOnAddOrUpdate` is what keeps the column out of `INSERT` and `UPDATE`
statements. Without it EF tries to write `NULL` and Postgres rejects the write on a
generated column.

The generation expression itself is **not** expressed in the model. EF's
`HasComputedColumnSql` cannot represent `setweight(...) || setweight(...)` with the
`immutable_unaccent` dependency in a way that round-trips through a scaffold, so the
expression lives in the migration's raw SQL and the model only declares the column's
shape. Consequence: a future `dotnet ef dbcontext scaffold` will not reproduce it.
That is recorded here so it is not rediscovered as a bug.

---

## Migration

```bash
dotnet ef migrations add AddSearchVectors \
  --project src/modules/Content/Content/src/Content.Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

The generated migration must be edited to carry the raw SQL. Expected shape:

```csharp
/// <summary>
/// Adds the full-text search vectors and their GIN indexes for articles and videos,
/// plus the immutable unaccent wrapper the generated columns depend on.
/// </summary>
public partial class AddSearchVectors : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION content.immutable_unaccent(text)
            RETURNS text LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
            AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE content.articles
                ADD COLUMN search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(headline, ''))), 'B') ||
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_title, ''))), 'C') ||
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_description, ''))), 'C') ||
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(body, ''))), 'D')
                ) STORED;
            """);

        migrationBuilder.Sql(
            """
            ALTER TABLE content.videos
                ADD COLUMN search_vector tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(description, ''))), 'B') ||
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_title, ''))), 'C') ||
                    setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_description, ''))), 'C')
                ) STORED;
            """);

        migrationBuilder.Sql(
            """
            CREATE INDEX ix_articles_search_vector_published
                ON content.articles USING GIN (search_vector)
                WHERE status = 'Published';
            """);

        migrationBuilder.Sql(
            """
            CREATE INDEX ix_videos_search_vector_published
                ON content.videos USING GIN (search_vector)
                WHERE status = 'Published';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_videos_search_vector_published;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_articles_search_vector_published;");
        migrationBuilder.Sql("ALTER TABLE content.videos DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("ALTER TABLE content.articles DROP COLUMN IF EXISTS search_vector;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS content.immutable_unaccent(text);");
    }
}
```

---

## Backfill

None needed. `GENERATED … STORED` computes every existing row as part of the
`ADD COLUMN`, which takes an `ACCESS EXCLUSIVE` lock for the duration.

On a large `articles` table that lock is the deployment risk. Measure first:

```sql
SELECT count(*), pg_size_pretty(pg_total_relation_size('content.articles'))
FROM content.articles;
```

Under roughly 100k rows this is seconds and can ship in a normal release. Above that,
plan a maintenance window or switch to a nullable column plus a trigger and a batched
backfill — a different spec, not this one.
