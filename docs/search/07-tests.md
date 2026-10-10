# Tests

Follows `docs/testing/00-unit-vs-integration-rules.md`: unit tests prove a method
works, integration tests prove it is wired up. Nothing in `tests/Integration/`
constructs a handler, a validator or a specification directly.

## Checklist

- [ ] `PublicSearchValidatorTests` covering every boundary
- [ ] `PublicSearchHandlerTests` covering scope selection and merge ordering
- [ ] `PublicSearchMetaFieldTests` per the MetaField convention
- [ ] `SearchRepositoryTests` through `Resolve<ISearchRepository>()`
- [ ] `PublicSearchEndpointV1Tests` through real HTTP
- [ ] `ExcerptHelperTests`

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
