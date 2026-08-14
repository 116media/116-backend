# Spec 05 — Article → Artist Tagging

**Frontend gap 5.** Blocks the `Actualités` tab
([frontend 08](../../../../frontend/docs/artists-page/08-catalog-sections.md)).

`ArticleEntity` has no link to an artist. The profile's news tab needs "every published article
about this artist".

## A join table, not a single FK

`ArticleEntity.ArtistId` would be cheaper by one table and is wrong. Articles routinely cover
several artists — a piece about a collaboration, a chart roundup, a feature naming five people. A
single FK forces an arbitrary choice at write time, and the artist who loses the coin flip never
gets the article on their profile.

`ArticleArtistEntity` mirrors the existing `ArticleTagEntity` / `LyricsTagEntity` junctions exactly.
Same shape, same configuration style, same admin verb.

## `ArticleArtistEntity`

`Domain/Entities/ArticleArtistEntity.cs`, an `Aggregate<Guid>`:

| Property | Type |
| --- | --- |
| `ArticleId` | `Guid` |
| `ArtistId` | `Guid` |
| `Article` | `ArticleEntity` navigation |
| `Artist` | `ArtistEntity` navigation |

One factory: `Create(Guid id, Guid articleId, Guid artistId)`.

### Configuration

```csharp
builder.HasKey(x => x.Id);
builder.HasIndex(x => new { x.ArticleId, x.ArtistId }).IsUnique();
builder.HasIndex(x => x.ArtistId);
builder.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
builder.HasOne(x => x.Artist).WithMany().HasForeignKey(x => x.ArtistId).OnDelete(DeleteBehavior.Cascade);
```

The **second, non-unique index on `ArtistId` alone** is not redundant with the composite. The
composite leads with `ArticleId`, so it cannot serve "all articles for this artist" — which is the
only direction the public page ever reads. Without it, the news tab and the article term of
`contentCount` ([spec 06](06-surfaceable-content.md)) are sequential scans of the whole join table.

Cascade on both sides: the row means nothing without either parent.

## Repository

`IArticleRepository` gains:

```csharp
Task<IReadOnlyList<ArticleArtistEntity>> GetArtistsByArticleIdAsync(Guid articleId, CancellationToken ct = default);
Task ReplaceArticleArtistsAsync(Guid articleId, IReadOnlyList<Guid> artistIds, CancellationToken ct = default);
Task<(List<ArticleEntity> Articles, int TotalCount)> GetPublishedByArtistAsync(
    Guid artistId, int page, int pageSize, CancellationToken ct = default);
```

`GetPublishedByArtistAsync` is named and shaped identically to the ones already on
`ILyricsRepository` and `IVideoRepository`, because it answers the same question for the third
surface. Filtering to `EnumContentStatus.Published`, ordered by `PublishedAt` descending.

`ReplaceArticleArtistsAsync` does a set-replace inside the caller's unit of work: load the current
rows, delete those not in the new list, add those not already present. It does **not** commit —
committing is the handler's job, so the tag change is atomic with anything else in the same request.

## Admin surface

`AdminSetArticleArtistsCommand` — `PUT /api/v1/admin/articles/{articleId}/artists`, body
`{ artistIds: Guid[] }`.

Modelled on the existing `AdminSetLyricsTagsCommand`, and **set-replace rather than add/remove**:
the admin UI is a multi-select, so the natural verb is "these are the artists now". Add and remove
endpoints would make the client compute a diff and issue N calls to express one edit, and would
leave the two out of sync on a partial failure.

Handler:

1. `GetByIdOrThrowAsync(articleId)` — 404 if the article does not exist.
2. Validate every `artistId` exists; throw `i18n.Artist.NotFound(id)` naming the **first missing
   id**, not a generic message. An admin pasting five ids needs to know which one is wrong.
3. `ReplaceArticleArtistsAsync`.
4. `CommitAsync`.

Validator: `artistIds` not null (empty is valid and means "untag everything"), no duplicates, at
most `MaxArticleArtistCount = 20`.

Empty is deliberately valid. An article that turns out to be about nobody in particular must be
untaggable, and forcing at least one artist would make editors invent a tag.

`RequireAuthorization()` with the admin policy, `RateLimitPolicies.ContentBrowsing`.

## Public endpoint

`GET /api/v1/public/artists/{slug}/articles`

| Param | Type | Default |
| --- | --- | --- |
| `pageIndex` | int | 0 |
| `pageSize` | int | 12 |

Response: `PaginatedResult<ArticleSummaryDto>` — the **existing** DTO the articles feed already
returns.

Reusing `ArticleSummaryDto` unchanged is the point. The frontend renders `ArticleCard.Feed`, the
articles module's own card, over its own entity; a bespoke `ArtistNewsDto` would fork the card
([frontend 10](../../../../frontend/docs/artists-page/10-components-and-reuse.md)).

Handler resolves `slug → artist`, 404s if absent, then calls `GetPublishedByArtistAsync`.
Anonymous, `ContentBrowsing`, `Produces`/`ProducesProblem(404, 429)`.

## The backfill is editorial, and that is the real cost

The migration is one table. Making the news tab non-empty for existing artists means someone reading
the archive and tagging it. No script can do it: the artist's name appears in prose, and matching on
text produces both false positives (a mention is not a subject) and false negatives (nicknames,
misspellings, "le Roi de la Rumba").

This is stated plainly rather than buried because it changes how the feature should be scheduled:
**the endpoint can ship in an afternoon and the tab stays empty until editorial work happens.** That
is fine and by design — an empty tab is hidden, not shown empty
([frontend 09](../../../../frontend/docs/artists-page/09-loading-empty-error.md)) — but nobody
should be surprised by it in review.

New articles get tagged at publish time through the admin UI, so the archive is the only backlog and
it shrinks on its own.

## Checklist

- [x] `MaxArticleArtistCount` added to `ContentConstants`
- [x] `ArticleArtistEntity` with `Create`
- [x] `ArticleArtistConfiguration`: unique `(ArticleId, ArtistId)`, standalone `ArtistId` index, cascade both sides
- [x] `DbSet<ArticleArtistEntity>` on `ContentDbContext`
- [x] Migration generated (`AddArtistPageFeature`, shared by specs 01–07), left unapplied
- [x] `ArticleByArtistSpecification`
- [x] `IArticleRepository.GetArtistsByArticleIdAsync`, `ReplaceArticleArtistsAsync`, `GetPublishedByArtistAsync` + implementations
- [x] `ReplaceArticleArtistsAsync` does not commit
- [x] `AdminSetArticleArtistsCommand`/`Handler`/`Validator`/`MetaField`/`EndpointV1`
- [x] Handler names the first missing artist id in its `NotFound`
- [x] `PublicGetArtistArticlesQuery`/`Handler`/`MetaField`/`EndpointV1` returning `PaginatedResult<ArticleSummaryDto>`
- [x] Unit: validator rejects duplicates and over-count; accepts an empty list
- [x] Unit: set-replace adds new, removes absent, leaves unchanged rows alone
- [x] Unit: handler throws for an unknown article and for an unknown artist id
- [ ] Integration: tagging two artists then re-tagging with one leaves exactly one row
- [ ] Integration: tagging with an empty array removes every row
- [ ] Integration: an article tagged to two artists appears on both profiles
- [ ] Integration: only `Published` articles come back; draft and archived do not
- [ ] Integration: ordering is `PublishedAt` descending
- [ ] Integration: deleting the article, or the artist, cascades the join rows away
- [ ] Integration: unknown slug returns 404; an artist with no articles returns an empty page
- [ ] `dotnet build` and both test suites clean
