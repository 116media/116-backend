# Method and coverage

## Two passes, and why the first one was not enough

**Pass 1 — pattern matching.** A script matched four syntactic shapes across all 1,319 test files:
a single-expression `new XBuilder()….Build()` chain, `XEntity.Create(`, `new XRequest(`, and
`mock.Setup/Verify(x => x.M(`. It produced the census in [01-census.md](01-census.md), which is sound
because it only counts names, and a first list of suspect sites, which was not.

Where it failed, measured against what reading found afterwards:

| Blind spot | Consequence |
| --- | --- |
| Required `.Build()` in the same expression | Missed every builder held in a variable and configured over several statements |
| Only considered entities whose builder name it could derive (`XEntity` → `XBuilder`) | Missed all 66 direct entity constructions in Content/Editorial, because `VideoRatingEntity`, `ArticleCommentEntity` and friends have factories but no builder |
| Compared chains by exact method set | Missed every case where the hand-rolled sequence is a *transition* (`MarkPendingReview(); Approve(); Publish(...)`) that a builder step already encapsulates — 22 sites in one Lyrics shape alone |
| Counted files, not occurrences | Understated repetition inside a single file |
| Could not read intent | Could not tell a guard test from a drifted one, and excused whole files by filename |
| Blind to anonymous objects and private local helpers | Missed the six byte-identical `CreateNotification` helpers and the two byte-identical export-strategy helpers |

In Content/Editorial the first pass found **one** site out of the dozens that reading found. Treat its
output as a hint list only.

**Pass 2 — reading.** Five agents read the suites by area, each with the census and the pass-1 output
as hints they were told to correct, and each required to report its own coverage. Every load-bearing
claim was then re-verified by hand before being written down here: the broken factory body, the
discarded entity, the `Id`-unset assertion, the md5 equality of the duplicated helpers, and the missing
project reference.

## Coverage by area

| Area | Files | Read in full | Read in part | Not opened |
| --- | --- | --- | --- | --- |
| Identity `Infrastructure/` + `Domain/` | 69 | 52 | 17 | 0 |
| Content `Editorial/` + `Domain/` | 370 | 10 | 19 | ~341 (all grep-swept) |
| Storage suites | 40 | 17 | 0 | 23 |
| Mailer suites | 56 | 42 | 1 | 13 |
| `Shared.Unit.Tests` | 68 | 0 | 0 | 68 |
| `Shared.Integration.Tests` | 21 | 6 | 3 | 12 |
| `EndToEnd.Tests` | 32 | 3 | 3 | 26 |

Two of those zeroes are justified structurally rather than by sampling:

- **`Shared.Unit.Tests` (68 files, none opened).** Its project references only `Shared.Domain`, the
  `BuildingBlocks.*` projects and `tests/TestData`. No module `.TestData` is reachable from it, so a
  finding of this audit's kind cannot exist there.
- **The 13 unopened Mailer files and 23 unopened Storage files** were each confirmed by name-grep to
  contain no builder, no factory call and no entity construction: they are DI-registration,
  MetaField-initialisation, validator, localisation and EF-model tests.

## Where this audit is thin

1. **`EndToEnd.Tests` — 26 of 32 files unopened.** Its request-record inventory comes from an
   exhaustive grep, and each proposed builder was checked to exist, but further inline entity
   construction in those 26 files cannot be ruled out. This is where the next pass should go.
2. **Content/Editorial handler and endpoint tests — ~170 + ~85 files** seen only through targeted
   greps. The residual risk is concentrated in mock arrangement and in per-file request shapes inside
   the Artist/Album/Streaming-link endpoint files.
3. **Nothing here was compiled or run.** Every "behaviour-preserving" claim rests on reading the
   builder and factory bodies and the domain transitions they drive. The suites must be run before any
   of it merges, and two swaps are known not to be inert — see the warnings in
   [13-correctly-direct.md](13-correctly-direct.md).

## Reproducing the census

The scripts are not kept in the repository; the numbers above are reproducible from these definitions:

- A factory method is a `public`/`internal static` method declared in a file under a `Factories/`
  directory.
- Its callers are the distinct files, excluding its own, matching `\b<Class>\.<Method>\b`.
- A builder chain is `new (\w+Builder)\(…\)` followed by `.Method(…)` calls ending in `.Build()`; its
  signature is the builder name plus the unordered set of method names.
