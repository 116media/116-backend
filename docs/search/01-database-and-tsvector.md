# Database and `tsvector`

## Checklist

- [ ] `unaccent` extension enabled
- [ ] `immutable_unaccent` wrapper created
- [ ] `search_vector` generated column on `content.articles`
- [ ] `search_vector` generated column on `content.videos`
- [ ] GIN index on each column
- [ ] `EXPLAIN ANALYZE` confirms a bitmap index scan

---

## The `unaccent` problem

A `GENERATED ALWAYS AS … STORED` column requires an `IMMUTABLE` expression. `unaccent()`
is declared `STABLE`, because the dictionary it reads could in principle be changed at
runtime. Postgres therefore refuses:

```text
ERROR:  generation expression is not immutable
```

The wrapper below asserts immutability. It is safe **on the condition that the
`unaccent` dictionary is never modified**. If it is, every generated column must be
recomputed and every index rebuilt. That constraint is the price of accent folding and
is accepted deliberately.

```sql
CREATE EXTENSION IF NOT EXISTS unaccent;

-- Immutable wrapper so unaccent() can be used in a generated column.
-- Hard dependency: the unaccent dictionary must not be modified, or every
-- search_vector column and GIN index needs rebuilding.
CREATE OR REPLACE FUNCTION content.immutable_unaccent(text)
RETURNS text
LANGUAGE sql
IMMUTABLE
PARALLEL SAFE
STRICT
AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
```

The two-argument form is used on purpose: `unaccent(text)` resolves its dictionary at
call time, which is exactly what makes it `STABLE`. Naming the dictionary explicitly
removes that lookup.

---

## Articles

```sql
ALTER TABLE content.articles
    ADD COLUMN search_vector tsvector
    GENERATED ALWAYS AS (
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(headline, ''))), 'B') ||
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_title, ''))), 'C') ||
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_description, ''))), 'C') ||
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(body, ''))), 'D')
    ) STORED;

CREATE INDEX ix_articles_search_vector
    ON content.articles USING GIN (search_vector);
```

`coalesce(…, '')` is required on every field: `to_tsvector` of `NULL` is `NULL`, and
concatenating `NULL` into the vector would wipe the whole thing.

## Videos

```sql
ALTER TABLE content.videos
    ADD COLUMN search_vector tsvector
    GENERATED ALWAYS AS (
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(description, ''))), 'B') ||
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_title, ''))), 'C') ||
        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_description, ''))), 'C')
    ) STORED;

CREATE INDEX ix_videos_search_vector
    ON content.videos USING GIN (search_vector);
```

---

## Supporting indexes

The search predicate always filters on `status`, so a partial index keeps the posting
lists to published rows only:

```sql
CREATE INDEX ix_articles_search_vector_published
    ON content.articles USING GIN (search_vector)
    WHERE status = 'Published';

CREATE INDEX ix_videos_search_vector_published
    ON content.videos USING GIN (search_vector)
    WHERE status = 'Published';
```

Create **either** the plain index or the partial one, not both. The partial index is
smaller and matches every query this feature issues; the plain one is only needed if an
admin surface later searches drafts. Start partial.

---

## Verification

```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT a.id,
       ts_rank_cd(a.search_vector, q) AS score
FROM content.articles a,
     websearch_to_tsquery('french', content.immutable_unaccent('fally ipupa')) q
WHERE a.search_vector @@ q
  AND a.status = 'Published'
ORDER BY score DESC
LIMIT 20;
```

Expected plan fragment:

```text
->  Bitmap Heap Scan on articles a
      Recheck Cond: (search_vector @@ q)
      ->  Bitmap Index Scan on ix_articles_search_vector_published
```

A `Seq Scan` means the predicate is not sargable. Check that the query is built with
`@@` against the column and not wrapped in a function.

---

## Rollback

```sql
DROP INDEX IF EXISTS content.ix_articles_search_vector_published;
DROP INDEX IF EXISTS content.ix_videos_search_vector_published;
ALTER TABLE content.articles DROP COLUMN IF EXISTS search_vector;
ALTER TABLE content.videos  DROP COLUMN IF EXISTS search_vector;
DROP FUNCTION IF EXISTS content.immutable_unaccent(text);
```

Lossless: every dropped object is derived from columns that remain.
