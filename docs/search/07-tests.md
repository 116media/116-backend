# Tests

Follows `docs/testing/00-unit-vs-integration-rules.md`: unit tests prove a method
works, integration tests prove it is wired up. Nothing in `tests/Integration/`
constructs a handler, a validator or a specification directly.

## Checklist

- [x] `PublicSearchValidatorTests` covering every boundary
- [x] `PublicSearchHandlerTests` covering scope selection and merge ordering
- [ ] `PublicSearchMetaFieldTests` per the MetaField convention (the Content module has
      no MetaField test file yet; the convention is one file per area and actor, so this
      belongs to an `EditorialPublicMetaFieldTests` covering the whole area)
- [x] `SearchRepositoryTests` through `Resolve<ISearchRepository>()`
- [x] `PublicSearchEndpointV1Tests` through real HTTP
- [x] `ExcerptHelperTests`

---

## Unit — validator

`tests/Unit/.../Search/PublicSearchValidatorTests.cs`

| Case | Expectation |
| --- | --- |
| `q` empty | invalid |
| `q` one character | invalid |
| `q` two characters | valid |
| `q` 120 characters | valid |
| `q` 121 characters | invalid |
| `scope` outside the enum | invalid |
| `pageSize` 0 | invalid |
| `pageSize` 51 | invalid |
| `pageSize` 50 | valid |

```csharp
[Theory]
[InlineData("", false)]
[InlineData("a", false)]
[InlineData("ab", true)]
public void Validate_ForQueryLength_ShouldMatchExpectation(string term, bool expected)
{
    // Arrange
    var query = new PublicSearchQuery(new PaginatedRequest(0, 20), term);

    // Act
    TestValidationResult<PublicSearchQuery> result = _validator.TestValidate(query);

    // Assert
    result.IsValid.Should().Be(expected);
}
```

---

## Unit — handler

`tests/Unit/.../Search/PublicSearchHandlerTests.cs`, with `MockSearchRepository`.

| Case | Expectation |
| --- | --- |
| Scope `Articles` | only `SearchArticlesAsync` is called |
| Scope `Videos` | only `SearchVideosAsync` is called |
| Scope `All` | both are called, with `take = skip + pageSize` |
| Scope `All` with mixed scores | items ordered by score descending across types |
| Equal scores | ordered by `PublishedAt` descending |
| Scope `Articles` | `Results.Count` equals `Counts.Articles`, not `Counts.All` |
| Query with surrounding whitespace | repository receives the trimmed term |
| Zero hits | empty items, counts all zero, no throw |

```csharp
[Fact]
public async Task Handle_WithAllScope_ShouldOrderAcrossTypesByScore()
{
    // Arrange
    _searchRepositoryMock
        .SetupSearchArticles([Hit(EnumCoreContentType.Article, score: 0.40)])
        .SetupSearchVideos([Hit(EnumCoreContentType.Video, score: 0.90)]);

    // Act
    PublicSearchResult result = await _handler.Handle(
        new PublicSearchQuery(new PaginatedRequest(0, 20), "rumba"),
        CancellationToken.None
    );

    // Assert
    result.Results.Items.Select(hit => hit.Type).Should()
        .ContainInOrder(EnumCoreContentType.Video, EnumCoreContentType.Article);
}
```

---

## Unit — excerpt helper

| Input | Expectation |
| --- | --- |
| `null` | empty string |
| whitespace | empty string |
| under 200 chars | returned unchanged, trimmed |
| over 200 chars with a space before 200 | cut at that space, no trailing space |
| over 200 chars with no space | hard cut at 200 |

---

## Integration — repository

`tests/Integration/.../Repositories/SearchRepositoryTests.cs`, `BaseRepositoryTest`,
`Resolve<ISearchRepository>()`. These are the only tests that prove the generated
column and the GIN index actually work, because they run against real Postgres.

| Case | Expectation |
| --- | --- |
| Seed `"Mélodies de Kinshasa"`, search `melodie` | one hit — **the accent-folding proof** |
| Seed the same, search `MÉLODIE` | one hit |
| Term in the title vs the same term in the body | the title row scores higher |
| Draft article matching the term | no hit |
| Two-word query matching one row | one hit |
| Query matching nothing | empty, no throw |
| `websearch_to_tsquery` syntax `"fally ipupa"` | phrase match only |
| Malformed input `fally "` | no exception |
| `CountAsync` | counts match the two search methods' totals |

The accent case is the single most valuable test in this feature: it is the defect the
whole migration exists to fix, and it cannot be caught by a unit test.

---

## Integration — endpoint

`tests/Integration/.../Search/V1/PublicSearchEndpointV1Tests.cs`, `BaseApiTest`.

| Case | Expectation |
| --- | --- |
| `GET ?q=ab` | `200` |
| `GET ?q=a` | `400` |
| `GET` with no `q` | `400` |
| `GET ?q=…&scope=articles` | only `Article` hits |
| `GET ?q=…` matching nothing | `200`, empty items, `counts.all = 0` |
| Any successful call | `counts` sums to `articles + videos` |
| Any successful call | `Cache-Control` contains `no-store` |
| Anonymous caller | `200`, no auth required |

Zero hits returning `200` and not `404` is asserted explicitly: it is a contract
decision in `../../../../docs/search/03-api-contract.md`, and the kind of thing a
later refactor breaks silently.

---

## Not tested, deliberately

| Not tested | Why |
| --- | --- |
| The exact `score` value | An implementation detail of `ts_rank_cd`; asserting it pins us to a Postgres version |
| GIN index presence via a query plan assertion | Plans are environment-sensitive; verified manually per `01-database-and-tsvector.md` |
| Ordering across more than two types | Only two types exist; revisit when a third lands |

---

## Shorts coverage

Shorts were added after the first release of the endpoint, with the same split:

| Level | Case |
| --- | --- |
| Unit | `Shorts` scope queries only the shorts arm |
| Unit | `Shorts` scope reports `counts.Shorts`, not the total |
| Unit | `All` orders a short between a higher-scoring video and a lower-scoring article |
| Unit | `All` asks all three arms for the whole `skip + pageSize` window |
| Integration | a term in a short's title returns it with a positive score |
| Integration | a short hit carries no category, excerpt or rating |
| Integration | an inactive short is never returned |
| Integration | an unaccented term matches an accented short title |
| Integration | `CountAsync` counts active shorts and sums all three scopes |
| Integration | `scope=Shorts` over HTTP returns only active shorts |
| Integration | a short hit serialises `categoryId` and `categoryName` as null |

---

## After the projection rewrite

The depth cap and the cross-type ordering moved out of the handler into
`SearchDocumentCatalog`, which needs a database, so they moved from unit to integration.
The handler kept the orchestration it still owns.

| Level | File | Covers |
| --- | --- | --- |
| Unit | `PublicSearchHandlerTests` | trimming, the skip/take it passes down, the scoped count it selects |
| Unit | `SearchProjectionTests` | routing to the projector for a type, no-op for a type with none, rebuild-all |
| Unit | `SearchScopesTests` | the scope/content-type map, including the round trip and the types no scope exposes |
| Integration | `SearchCatalogTests` | ranked matching, accent folding, visibility, paging, merged `All`, cached counts |
| Integration | `SearchProjectionTests` | publish writes a visible document, archive hides it, a short's rename reprojects, deactivation hides, rebuild restores after drift |
| Integration | `PublicSearchEndpointV1Tests` | real HTTP, scope narrowing, zero-filled counts, overflowing page index, `no-store`, the wire shape of `counts` |

The projection tests drive the **real domain transitions** — `Publish`, `Archive`,
`Update`, `Deactivate` — rather than calling the projector, so the domain event, its
registration and its handler are all exercised. That is the part most likely to rot, since
a missing registration fails silently.

One thing the suite does **not** cover: that the SQL backfill in the migration and the C#
projectors produce the same rows. They are kept to plain column copying so they can be read
side by side, but nothing asserts it.
