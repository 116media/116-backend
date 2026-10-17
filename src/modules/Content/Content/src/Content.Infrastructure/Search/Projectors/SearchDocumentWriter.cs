using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Projections;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Search.Projectors;

/// <summary>
/// Upserts and removes search documents. Shared by every projector so the write semantics,
/// including the hide-rather-than-delete rule, are defined once.
/// </summary>
/// <param name="context">The Content database context.</param>
public class SearchDocumentWriter(ContentDbContext context)
{
    /// <summary>
    /// Writes one document, inserting it or updating the existing row in place.
    /// </summary>
    /// <param name="document">The document as its source now stands.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    public async Task UpsertAsync(SearchDocument document, CancellationToken cancellationToken = default)
    {
        SearchDocument? existing = await context.SearchDocuments.FirstOrDefaultAsync(
            candidate => candidate.Type == document.Type && candidate.EntityId == document.EntityId,
            cancellationToken
        );

        if (existing is null)
        {
            context.SearchDocuments.Add(document);
        }
        else
        {
            Copy(from: document, to: existing);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Writes many documents in one round trip, for a rebuild.
    /// </summary>
    /// <param name="documents">The documents to write.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number written.</returns>
    public async Task<int> ReplaceAllAsync(
        IReadOnlyCollection<SearchDocument> documents,
        CancellationToken cancellationToken = default
    )
    {
        if (documents.Count == 0)
        {
            return 0;
        }

        EnumCoreContentType type = documents.First().Type;

        await context.SearchDocuments.Where(document => document.Type == type).ExecuteDeleteAsync(cancellationToken);

        context.SearchDocuments.AddRange(documents);
        await context.SaveChangesAsync(cancellationToken);

        return documents.Count;
    }

    /// <summary>
    /// Deletes one document, for a source that no longer exists.
    /// </summary>
    /// <param name="type">The content type the item belonged to.</param>
    /// <param name="entityId">The source aggregate's identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    public Task RemoveAsync(EnumCoreContentType type, Guid entityId, CancellationToken cancellationToken = default) =>
        context
            .SearchDocuments.Where(document => document.Type == type && document.EntityId == entityId)
            .ExecuteDeleteAsync(cancellationToken);

    private static void Copy(SearchDocument from, SearchDocument to)
    {
        to.Slug = from.Slug;
        to.IsVisible = from.IsVisible;
        to.Title = from.Title;
        to.Summary = from.Summary;
        to.Keywords = from.Keywords;
        to.Body = from.Body;
        to.ImageFileId = from.ImageFileId;
        to.CategoryId = from.CategoryId;
        to.PublishedAt = from.PublishedAt;
        to.RatingAverage = from.RatingAverage;
    }
}
