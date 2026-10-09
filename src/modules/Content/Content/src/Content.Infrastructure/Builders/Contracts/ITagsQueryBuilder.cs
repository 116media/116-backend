using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Infrastructure.Builders.Contracts;

/// <summary>
/// How a tag listing is ordered.
/// </summary>
public enum EnumTagOrder
{
    /// <summary>
    /// Alphabetical by tag name, for a vocabulary the reader scans.
    /// </summary>
    Name,

    /// <summary>
    /// Most-used first, measured by the scoped content count, name settling ties.
    /// </summary>
    Usage,
}

/// <summary>
/// Interface for building tag listing queries using the builder pattern. Encapsulates the
/// optional search filter, the content-type association filter that narrows the vocabulary to
/// tags a content kind actually uses, the ordering, and the per-kind content counts every
/// listing carries.
/// </summary>
public interface ITagsQueryBuilder
{
    /// <summary>
    /// Filters by a free-text term matched against name and slug.
    /// </summary>
    /// <param name="search">The term, or null for no filter.</param>
    /// <returns>The builder, for chaining.</returns>
    ITagsQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Narrows the vocabulary to tags the given content kind uses.
    /// </summary>
    /// <param name="contentType">The content kind, or null for the whole vocabulary.</param>
    /// <returns>The builder, for chaining.</returns>
    ITagsQueryBuilder WithContentType(EnumCoreContentType? contentType);

    /// <summary>
    /// Caps how many tags come back.
    /// </summary>
    /// <param name="limit">The cap, or null for the module's reference-list ceiling.</param>
    /// <returns>The builder, for chaining.</returns>
    ITagsQueryBuilder WithLimit(int? limit);

    /// <summary>
    /// Counts published content only when true, every association when false.
    /// </summary>
    /// <param name="publishedOnly">Whether to count published content only.</param>
    /// <returns>The builder, for chaining.</returns>
    ITagsQueryBuilder WithPublishedOnly(bool publishedOnly);

    /// <summary>
    /// Sets the ordering.
    /// </summary>
    /// <param name="order">Alphabetical, or most-used first.</param>
    /// <returns>The builder, for chaining.</returns>
    ITagsQueryBuilder WithOrder(EnumTagOrder order);

    /// <summary>
    /// Builds the final filtered, ordered and limited query of tags with their counts.
    /// </summary>
    /// <param name="context">The Content module database context.</param>
    /// <returns>The query.</returns>
    IQueryable<TagCountRow> Build(ContentDbContext context);
}
