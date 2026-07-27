# Spec 01 — Artist Identity Fields

**Frontend gap 7.** Blocks the identity block in the profile hero
([frontend 06](../../../../frontend/docs/artists-page/06-artist-detail-hero.md)).

`ArtistEntity` today carries `Name`, `Slug`, `Bio`, `AvatarFileId`, `UserId`, `VerifiedAt`. The
profile hero also renders a small identity block — real name, aliases, birthdate with a derived age,
and hometown — each row hidden entirely when its value is null.

## Columns

Four new nullable columns on `content.artists`. All nullable, no backfill: the block renders
whatever exists and omits the rest, so an artist with none of them renders no block at all.

| Property | CLR type | Column | Constraint |
| --- | --- | --- | --- |
| `RealName` | `string?` | `real_name VARCHAR(150)` | `MaxArtistRealNameLength` |
| `Aliases` | `IReadOnlyList<string>` | `aliases TEXT[] NOT NULL DEFAULT '{}'` | ≤ 10 entries, each ≤ `MaxArtistNameLength` |
| `Birthdate` | `DateOnly?` | `birthdate DATE` | must be in the past |
| `Hometown` | `string?` | `hometown VARCHAR(120)` | `MaxArtistHometownLength` |

Two new constants in `ContentConstants`:

```csharp
public const int MaxArtistRealNameLength = 150;
public const int MaxArtistHometownLength = 120;
public const int MaxArtistAliasCount = 10;
```

### `Birthdate` is `DateOnly`, not `DateTimeOffset`

This is the one type decision that is not stylistic. A birthdate is a **civil date**, not an
instant. Stored as `DateTimeOffset` it becomes midnight UTC, which serialises to the previous day
for every reader west of Greenwich — Drake's 24 October reads as 23 October in Los Angeles.
`DateOnly` maps to Postgres `date`, serialises as `"1986-10-24"`, and cannot carry a timezone to be
converted by.

The frontend has the matching rule and will not run the value through `new Date()`
([frontend 18](../../../../frontend/docs/artists-page/18-domain-entities-and-mappers.md)). Both
halves are required — either one alone still moves the day.

**Age is never stored and never returned.** It is derived at render. A stored age is wrong within a
year of being written.

### `Aliases` is `text[]`, not a join table

Npgsql maps `List<string>` to `text[]` natively. The list is **display-only and never queried** — no
filter, no search, no sort ever touches it. A join table would add an entity, a configuration, a
repository method and a migration to express "render these strings middot-joined".

If aliases ever become searchable, that is the moment to normalise them, and the migration is
mechanical. Speculating now buys nothing.

The property is exposed as `IReadOnlyList<string>` over a private `List<string>` backing field so
callers cannot mutate the collection behind the aggregate's back.

## Domain changes

`ArtistEntity` gains the four properties and one private normaliser. Both `Create` and `Update` take
the identity fields as **required positional parameters, before `errors`** — never as trailing
optionals.

That is deliberate and this repo has already been bitten by the alternative: an optional trailing
parameter means every existing call site silently keeps compiling while quietly clearing the field
it does not pass. Required parameters surface all call sites as compiler errors, which is the
point.

```csharp
public static ArtistEntity Create(
    Guid id,
    string name,
    string slug,
    string? bio,
    string? realName,
    IReadOnlyList<string>? aliases,
    DateOnly? birthdate,
    string? hometown,
    ArtistErrors errors
)

public void Update(
    string name,
    string? bio,
    string? realName,
    IReadOnlyList<string>? aliases,
    DateOnly? birthdate,
    string? hometown,
    ArtistErrors errors
)
```

`aliases` is nullable at the boundary purely so a caller can pass `null` to mean "none"; it is
normalised to an empty list, never stored as null.

### Alias normalisation

One private static helper, applied identically by `Create` and `Update`:

1. `null` → empty list.
2. Trim every entry; drop entries that are empty or whitespace.
3. De-duplicate case-insensitively, keeping first occurrence and its original casing.
4. Throw `errors.TooManyAliases()` if more than `MaxArtistAliasCount` survive.
5. Throw `errors.AliasTooLong()` if any survivor exceeds `MaxArtistNameLength`.

Normalising in the domain rather than the validator means the invariant holds no matter which use
case writes the entity — including seeds and future admin bulk tools.

### Birthdate guard

`Birthdate` must be strictly in the past. A birthdate in the future is not a typo the UI should
render with a negative age; it is bad data. Throw `errors.BirthdateInFuture()`.

Compared against `DateOnly.FromDateTime(DateTime.UtcNow)`. A one-day timezone straddle at the very
edge is acceptable here — the alternative is a timezone parameter on a domain guard, which is worse
than the problem.

## Errors

Three new members on `ArtistErrors`, each with a message on `ArtistErrorMessage` and an entry in all
three `.resx` files:

| Member | Exception | Message |
| --- | --- | --- |
| `TooManyAliases()` | `BadRequestException` | *An artist can have at most {0} aliases.* |
| `AliasTooLong()` | `BadRequestException` | *An alias cannot exceed {0} characters.* |
| `BirthdateInFuture()` | `BadRequestException` | *The birthdate must be in the past.* |

French copy lands in `ArtistErrorMessage.fr.resx` in the same commit — the neutral and `.en` files
alone are a half-shipped error.

## DTO

`ArtistDto` gains the four fields. It is already returned by the profile endpoint, so no new DTO is
needed for the profile; the directory uses a separate `ArtistSummaryDto`
([spec 07](07-public-artist-list-endpoint.md)) which deliberately does **not** carry them — 30 cards
per page have no use for a birthdate.

```csharp
public record ArtistDto(
    Guid Id,
    string Name,
    string Slug,
    string? Bio,
    string? AvatarUrl,
    bool IsVerified,
    string? RealName,
    IReadOnlyList<string> Aliases,
    DateOnly? Birthdate,
    string? Hometown
);
```

`IsVerified` arrives in [spec 08](08-profile-payload-and-totals.md); it is shown here so the final
shape is visible in one place. `ArtistMapper.ToArtistDtoAsync` passes the new fields straight
through — no suppression, no age computation. The frontend decides whether to hide a real name that
duplicates the stage name; that is a render decision, not a mapping one.

## Admin surface

`AdminCreateArtistCommand` and `AdminUpdateArtistCommand` each gain the four fields, with matching
request records on their V1 endpoints and rules on their validators:

- `RealName` — `MaximumLength(MaxArtistRealNameLength)`, optional.
- `Aliases` — optional list; `Must` have at most `MaxArtistAliasCount` entries, each within
  `MaxArtistNameLength`. The validator duplicates the domain guard on purpose: the validator
  produces a clean 400 with a field path, the domain guard makes the invariant unbypassable.
- `Birthdate` — optional; `LessThan` today.
- `Hometown` — `MaximumLength(MaxArtistHometownLength)`, optional.

No new endpoint. Identity is part of the artist record, edited on the same admin form.

## Editorial accuracy is a policy gap, not a schema gap

These are factual claims about real, living people, published on profiles most of them have not
claimed, and fed to search engines through JSON-LD. The schema cannot solve:

- **sourcing** — where a birthdate came from and who checked it,
- **correction** — how a verified artist gets their own data fixed.

Flagged here because the migration will make it *possible* to publish unsourced claims about people,
and that is worth someone saying yes to. It is recorded as the one open gate in
[frontend 25](../../../../frontend/docs/artists-page/25-open-questions.md) and does not block this
spec's implementation.

## Checklist

- [x] `MaxArtistRealNameLength`, `MaxArtistHometownLength`, `MaxArtistAliasCount` added to `ContentConstants`
- [x] `ArtistEntity` gains `RealName`, `Aliases` (`IReadOnlyList<string>` over a private `List<string>`), `Birthdate` (`DateOnly?`), `Hometown`
- [x] `ArtistEntity.Create` and `Update` take all four as required parameters before `errors`
- [x] Alias normalisation helper: trim, drop blanks, case-insensitive dedupe, count and length guards
- [x] Birthdate-in-future guard
- [x] `ArtistErrors.TooManyAliases`, `AliasTooLong`, `BirthdateInFuture` + `ArtistErrorMessage` + all three `.resx`
- [x] `ArtistConfiguration`: max lengths, `aliases` as `text[]` with an empty-array default, `birthdate` as `date`
- [x] Migration generated (`AddArtistPageFeature`, shared by specs 01–07), left unapplied
- [x] `ArtistDto` gains the four fields; `ArtistMapper` passes them through
- [x] `AdminCreateArtistCommand`/`Handler`/`Validator`/`EndpointV1` request updated
- [x] `AdminUpdateArtistCommand`/`Handler`/`Validator`/`EndpointV1` request updated
- [x] Every other `ArtistEntity.Create`/`Update` call site updated (seeds, fixtures, tests)
- [x] Unit: entity guards — too many aliases, alias too long, future birthdate, blank/duplicate alias normalisation, null aliases → empty list
- [x] Unit: both validators, at and past every boundary
- [x] Unit: `ArtistErrors` members
- [ ] Integration: admin create with identity fields round-trips through real HTTP and persists
- [ ] Integration: admin update clears a field by sending null, and does not clear untouched fields
- [ ] Integration: a `DateOnly` birthdate survives the round trip as the same civil date
- [ ] `dotnet build` and both test suites clean
