# Spec 07 — Public Artist List Endpoint

**Frontend gaps 1, 1b, 1c.** Blocks the entire `/artistes` directory — the one route with no
partial degradation path.

**Depends on [spec 06](06-surfaceable-content.md)** for the filter and the count.

## Endpoint

`GET /api/v1/public/artists`

| Param | Type | Default | Notes |
| --- | --- | --- | --- |
| `pageIndex` | int | 0 | zero-based, matching the module |
| `pageSize` | int | 30 | frontend grid page size |
| `letter` | `string?` | — | `A`–`Z` or `#`; **mutually exclusive** with `search` |
| `search` | `string?` | — | min 2 chars, name only |

Response:

```csharp
public record PublicGetArtistsResponse(
    PaginatedResult<ArtistSummaryDto> Artists,
    IReadOnlyList<string> AvailableLetters
);

public record ArtistSummaryDto(string Name, string Slug, string? AvatarUrl, bool IsVerified, int ContentCount);
```

`ArtistSummaryDto` carries **no `Id`** — the directory links by slug and nothing else needs it — and
no `Bio`: shipping 30 biographies for a grid that renders name and count is wasted payload
([frontend 14](../../../../frontend/docs/artists-page/14-data-requirements.md)).

Anonymous, `RateLimitPolicies.ContentBrowsing`, `Produces<PublicGetArtistsResponse>(200)`,
`ProducesProblem(400)` (both filters at once), `ProducesProblem(429)`.

### Mutual exclusion is a 400, not a silent precedence

Sending both `letter` and `search` is rejected with `ArtistErrors.LetterAndSearchExclusive()`.
Picking one silently means a client bug renders plausible-but-wrong results forever, and the UI
already enforces the exclusion — anything sending both is broken and should be told.

`search` shorter than 2 characters is rejected by the validator. A single-character search over the
whole artist table is a full scan returning a page the user cannot use.

## Accent folding — stored columns, not `unaccent`

`Élodie` must sort under `E`, bucket under `E`, and match a search for `elodie`.

### Why not the `unaccent` extension

- **`unaccent()` is not `IMMUTABLE`** — it depends on a mutable dictionary. It cannot go in a
  generated column or an expression index without a hand-written `IMMUTABLE` wrapper, which lies to
  the planner and corrupts the index if the dictionary ever changes.
- It requires `CREATE EXTENSION`, a deployment dependency this module has nowhere else.
- Sorting, bucketing and searching would each call it separately — the same rule computed in three
  places, which is the exact failure [spec 06](06-surfaceable-content.md) exists to prevent.

### What we do instead

The domain computes and stores the folded form once, on create and on rename:

| Column | Value | Drives |
| --- | --- | --- |
| `NameFolded` | `Name`, accent-stripped, uppercased, whitespace-collapsed | `ORDER BY`, search |
| `InitialLetter` | first char of `NameFolded` if `A`–`Z`, else `'#'` | `letter` filter, `availableLetters` |

Both are plain indexed columns. Sorting, bucketing, searching and `availableLetters` all read the
**same stored value**, so they cannot disagree. The folding rule is unit-testable without a
database.

### The folding algorithm

```csharp
private static string FoldName(string name)
{
    string trimmed = System.Text.RegularExpressions.Regex.Replace(name.Trim(), @"\s+", " ");
    string decomposed = trimmed.Normalize(NormalizationForm.FormD);

    var sb = new StringBuilder(decomposed.Length);
    foreach (char c in decomposed)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        {
            sb.Append(c);
        }
    }

    return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
}
```

FormD splits `É` into `E` + combining acute; dropping non-spacing marks leaves `E`. This handles the
whole Latin range — French, Portuguese, the Congolese diaspora names this site actually serves —
without a character map to maintain.

**Known limit, accepted:** it does not fold `Ø`, `Ł` or `ß`, which decompose to nothing. They bucket
under `#`. Adding a small explicit map is trivial if a real name needs it; pre-emptively building
one for names we do not have is speculation. Stated here so it is a known limit rather than a
surprise.

`InitialLetter` is `'#'` when `NameFolded` is empty or its first character is outside `A`–`Z` —
digits, punctuation, and any script that is not Latin. One bucket for everything that is not a
letter, matching the rail's single `#` chip.

Both are recomputed by a private `RecomputeNameIndexes()` called from `Create` and from `Update`
whenever `Name` changes. `Slug` is immutable so it never participates.

### Backfill

The migration's `Up()` backfills existing rows with raw SQL, using Postgres' own
`translate()` over the accented characters that actually occur in the current data, then the domain
maintains them from that point on. The migration is not correct until the backfill runs — a `NULL`
or empty `NameFolded` sorts every legacy artist to the top of the directory.

## Search

Search matches on `NameFolded`, with the search term folded by the **same** `FoldName` function
before the query:

```csharp
string folded = FoldName(search);
EF.Functions.Like(artist.NameFolded, $"%{folded}%")
```

`Like`, not `ILike`: both sides are already uppercased, so a case-insensitive operator would be
doing work twice and would not use the index.

**Name only.** The existing admin `ArtistSearchSpecification` also searches `Bio`; the public
directory deliberately does not. A user typing a name into an artist directory expects artists whose
*name* matches, not every artist whose biography mentions Kinshasa.

## Repository

```csharp
Task<(List<ArtistDirectoryRow> Artists, int TotalCount)> GetPublicDirectoryAsync(
    int page, int pageSize, string? letter, string? search, CancellationToken ct = default);

Task<IReadOnlyList<string>> GetAvailableLettersAsync(CancellationToken ct = default);
```

`ArtistDirectoryRow` is a repository-level record carrying the entity plus its `ContentCount`,
because the count comes from the same projection and there is nowhere on the entity to put it.

The query, in order:

1. `ApplySpecification(new ArtistHasContentSpecification(...))` — [spec 06](06-surfaceable-content.md).
2. `letter` → `InitialLetter == letter`, or `search` → the folded `Like`.
3. `OrderBy(a => a.NameFolded).ThenBy(a => a.Id)` — the id tie-break makes paging stable for two
   artists with identical folded names, which is otherwise a silent duplicate/drop across pages.
4. `CountAsync` for the total, then `Skip`/`Take`.
5. `Select` into `ArtistDirectoryRow` with the count expression inlined — **one statement**.

### `availableLetters`

```sql
SELECT DISTINCT initial_letter FROM content.artists WHERE <has content> ORDER BY initial_letter
```

Over the **same filtered set** as the grid. A letter that is enabled but leads to an empty page is
worse than no rail at all.

At most 27 values and it only changes when an artist gains their first published item, so it is
highly cacheable. It ships computed per request; the existing module cache is the obvious later
optimisation and is not needed to be correct.

It is returned **alongside** the paginated result rather than as a sibling endpoint: the rail and
the grid render together, and a second round trip to draw the rail would leave it disabled during
first paint. It is computed once per request regardless of which page is asked for — the rail does
not change as the user pages.

## Errors

| Member | Exception | Message |
| --- | --- | --- |
| `LetterAndSearchExclusive()` | `BadRequestException` | *A letter filter and a search term cannot be combined.* |

## Checklist

- [x] `ArtistEntity.NameFolded` and `InitialLetter` properties, private setters
- [x] `FoldName` + `RecomputeNameIndexes`, called from `Create` and from `Update` on rename
- [x] `ArtistConfiguration`: both columns non-nullable, `ix_artists_name_folded`, `ix_artists_initial_letter`
- [x] Directory columns captured in the `AddArtistPageFeature` migration with a **backfill** in `Up()`, left unapplied
- [x] `ArtistSummaryDto` — no `Id`, no `Bio`
- [x] `ArtistDirectoryRow` repository record
- [x] `IArtistRepository.GetPublicDirectoryAsync` + implementation, single-statement projection
- [x] `IArtistRepository.GetAvailableLettersAsync` over the same filtered set
- [x] Stable ordering: `NameFolded`, then `Id`
- [x] `ArtistErrors.LetterAndSearchExclusive` + message + all three `.resx`
- [x] `PublicGetArtistsQuery`/`Handler`/`Validator`/`MetaField`/`EndpointV1`
- [x] Validator: `pageSize` bounds, `search` min length 2, mutual exclusion
- [x] Unit: `FoldName` — `Élodie`→`ELODIE`, `Ferré Gola`→`FERRE GOLA`, collapsed whitespace, empty input
- [x] Unit: `InitialLetter` — letter, digit, punctuation, non-Latin, empty → `#`
- [x] Unit: renaming an artist recomputes both columns
- [x] Unit: validator rejects both filters together and a 1-char search
- [ ] Integration: `letter=E` returns `Élodie`
- [ ] Integration: `search=elodie` returns `Élodie`
- [ ] Integration: search matches name but **not** bio
- [ ] Integration: artists with no content are absent, and their letters are absent from `availableLetters`
- [ ] Integration: ordering is accent-insensitive across a mixed page
- [ ] Integration: paging is stable — page 1 and page 2 share no rows
- [ ] Integration: both filters together returns 400
- [ ] Integration: **query-count assertion** on a 30-artist page ([spec 06](06-surfaceable-content.md))
- [ ] `dotnet build` and both test suites clean
