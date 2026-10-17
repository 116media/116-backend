using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Search;

/// <summary>
/// Reads the search projection. One table means one query for a page and one aggregate for
/// every scope's count, whatever types are registered, so neither cost grows as types are
/// added. Relevance ordering cannot be served from an index, so the depth cap bounds how
/// much of a match set a single request can rank.
/// </summary>
/// <param name="context">The Content database context.</param>
public class SearchDocumentCatalog(ContentDbContext context) : ISearchCatalog
{
    /// <summary>
    /// How deep into the ranked results a request may reach. Past this the page is empty;
    /// the counts still report the real totals.
    /// </summary>
    public const int MaxResultDepth = 1_000;

    /// <summary>
    /// How long a search query may run before the provider cancels it. Far below the
    /// module's default, because this endpoint is anonymous and a pathological term must
    /// not hold a connection.
    /// </summary>
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(5);

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchHitDto>> SearchAsync(
        EnumSearchScope scope,
        string query,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        if (skip >= MaxResultDepth)
        {
            return [];
        }

        take = Math.Min(take, MaxResultDepth - skip);
        context.Database.SetCommandTimeout(QueryTimeout);

        IQueryable<Persistence.Projections.SearchDocument> matches = Matching(query);

        if (SearchScopes.ToContentType(scope) is EnumCoreContentType type)
        {
            matches = matches.Where(document => document.Type == type);
        }

        return await matches
            .Select(document => new
            {
                document.Type,
                document.EntityId,
                document.Slug,
                document.Title,
                document.Summary,
                document.CategoryId,
                document.PublishedAt,
                document.RatingAverage,
                document.ImageFileId,
                Score = ContentDbContext.RankCd(
                    document.SearchVector,
                    EF.Functions.WebSearchToTsQuery(SearchConfig.French, query)
                        .Or(EF.Functions.WebSearchToTsQuery(SearchConfig.English, query))
                ),
            })
            .OrderByDescending(document => document.Score)
            .ThenByDescending(document => document.PublishedAt)
            .Skip(skip)
            .Take(take)
            // Positional: an expression tree cannot carry named arguments.
            .Select(document => new SearchHitDto(
                document.Type,
                document.EntityId,
                document.Slug,
                document.Title,
                document.Summary,
                null,
                null,
                document.PublishedAt,
                document.Score,
                document.RatingAverage,
                document.CategoryId,
                document.ImageFileId
            ))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SearchCountsDto> CountAsync(string query, CancellationToken cancellationToken = default)
    {
        context.Database.SetCommandTimeout(QueryTimeout);

        List<(EnumCoreContentType Type, int Count)> grouped = await Matching(query)
            .GroupBy(document => document.Type)
            .Select(group => new ValueTuple<EnumCoreContentType, int>(group.Key, group.Count()))
            .ToListAsync(cancellationToken);

        // A scope the query matched nothing in still reports a zero, so the client's tabs
        // never lose a key between queries.
        Dictionary<EnumSearchScope, int> counts = SearchScopes.Narrowing.ToDictionary(scope => scope, _ => 0);

        foreach ((EnumCoreContentType type, int count) in grouped)
        {
            if (SearchScopes.ToScope(type) is EnumSearchScope scope)
            {
                counts[scope] = count;
            }
        }

        return new SearchCountsDto(counts);
    }

    // The vector carries both stemmings, so the query parses under both and matches either:
    // a French reader's "chanteurs" and an English reader's "record" both land.
    private IQueryable<Persistence.Projections.SearchDocument> Matching(string query) =>
        context.SearchDocuments.Where(document =>
            document.IsVisible
            && document.SearchVector.Matches(
                EF.Functions.WebSearchToTsQuery(SearchConfig.French, query)
                    .Or(EF.Functions.WebSearchToTsQuery(SearchConfig.English, query))
            )
        );
}
