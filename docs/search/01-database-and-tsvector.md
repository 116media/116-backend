# Database and tsvector

## Checklist

- [x] `immutable_unaccent` wrapper, so a generated column may call `unaccent`
- [x] `content.search_documents` holding one row per searchable item
- [x] `search_vector` generated from the four weighted text columns
- [x] Partial GIN index covering the visible rows
- [x] Backfill from every source table in the same migration

---

## One table, not one per type

Every searchable item is a row in `content.search_documents`. The alternative — a vector on
each source table and a view over each — was built first and replaced: it cost one query per
type for a page plus one per type for the counts, merged the page in application memory, and
grew both costs with every type added.

The source tables remain the record. This table is derived and can be rebuilt from them.

```sql
CREATE TABLE content.search_documents (
    type             text        NOT NULL,
    entity_id        uuid        NOT NULL,
    slug             text        NOT NULL,
    is_visible       boolean     NOT NULL,

    title            text        NOT NULL,   -- weight A
    summary          text        NOT NULL,   -- weight B, also the display excerpt
    keywords         text        NOT NULL,   -- weight C
    body             text        NOT NULL,   -- weight D

    image_file_id    uuid,
    category_id      uuid,
    published_at     timestamptz,
    rating_average   numeric,

    PRIMARY KEY (type, entity_id)
);
```

The four text columns are the contract between the table and the types. Each projector
decides what its type puts in each weight slot; the vector definition never changes when a
type is added.

`is_visible` is the whole of public reachability, so the predicate is the same for every
type even though the rule behind it differs — published for the editorial types, active for
shorts.

---

## The vector

Every weight slot is indexed under **both** the French and the English configurations.

```sql
setweight(to_tsvector('french',  content.immutable_unaccent(coalesce(title, ''))), 'A') ||
setweight(to_tsvector('english', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
setweight(to_tsvector('french',  content.immutable_unaccent(coalesce(summary, ''))), 'B') ||
setweight(to_tsvector('english', content.immutable_unaccent(coalesce(summary, ''))), 'B') ||
…
```

The catalogue is not French-only. Titles and descriptions are regularly English, and the
French stemmer leaves English morphology alone, so under a single `french` configuration a
reader searching `record` never reached an article about a `recording`. Measured on the real
database:

| Text | `french` alone | Consequence |
| --- | --- | --- |
| `CoolBeatz Talks Hip-Hop` | identical to `english` | none |
| `The Making of the Album` | keeps `the`, `of` | harmless |
| `ON AIR with a star` | drops `on` | partial |
| `recording sessions` | `recording`, not `record` | `record` misses it |

Indexing both recovers the last two rows, and French morphology is untouched: `chanteur`
still finds `chanteurs`.

**`simple` was considered and rejected.** It would recover the remaining loss — words that
are stop words in *both* languages, like `on` — but it indexes every stop word, so `la`,
`de` and `the` become searchable tokens matching nearly every row. On an anonymous endpoint
whose cost is already driven by match-set size, that is a worse trade than losing a handful
of two-letter titles.

`coalesce` is not optional: concatenating `NULL` into a `tsvector` wipes the whole thing.

The wrapper exists because a generated column may only call `IMMUTABLE` functions, and the
one-argument `unaccent` is `STABLE` — it resolves its dictionary per call. Naming the
dictionary explicitly is what makes it immutable:

```sql
CREATE OR REPLACE FUNCTION content.immutable_unaccent(text)
RETURNS text LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
```

### The query must parse under both

A vector carrying two stemmings is only half the fix; the query parses under both and the
two are OR'd, so either stemming can match:

```csharp
EF.Functions.WebSearchToTsQuery(SearchConfig.French, query)
    .Or(EF.Functions.WebSearchToTsQuery(SearchConfig.English, query))
```

The cost is that the vector roughly doubles where the two stemmers disagree, which widens
the match set — the same dimension the ranking cost below is driven by.

---

## The index

```sql
CREATE INDEX ix_search_documents_vector_visible
    ON content.search_documents USING GIN (search_vector)
    WHERE is_visible;
```

Partial, because every search filters on visibility, so the posting lists carry only rows a
reader can reach.

---

## What the index does not do

`ORDER BY ts_rank_cd(...)` **cannot** be served from this index, or from any B-tree or GIN
index. Postgres matches through the index, then heap-fetches and ranks every matching row
before returning a page. Measured on a synthetic 200k-article corpus, a term matching 77,143
of 180,000 visible rows took 43ms warm to return 20: the index scan was 6.7ms of that and the
heap-and-rank the rest.

The unified table does not change this — it is the same corpus either way. What it fixes is
the number of round trips and the application-memory merge.

Two things would move it, neither done here:

- **Narrow what is indexed.** The article body at weight D is why a common term matches tens
  of thousands of rows. Dropping it from `body` is a one-line change to
  `ArticleSearchProjector` and a product decision about whether full-body search matters.
- **The `rum` extension**, a GIN variant that can return rows in rank order from the index.
  Non-standard, and slower on write.

Until one of those, `SearchDocumentCatalog.MaxResultDepth` bounds how much of a match set a
single request can rank.

---

## Backfill

`AddSearchDocuments` creates the table, then inserts from `articles`, `videos` and
`short_videos` in the same migration, so a deploy leaves search populated. The generated
column computes as the rows land; there is nothing to backfill afterwards.

The SQL mirrors the C# projectors, which is a real duplication: **if a projector's column
mapping changes, the backfill is stale for anyone migrating from scratch.** The C# projector
is the source of truth, and `RebuildAllAsync` is what reconciles. The mapping is kept to
plain column copying precisely so the two are easy to compare — nothing is computed on
either side.
