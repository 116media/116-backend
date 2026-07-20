using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders.Contracts;

/// <summary>
/// Interface for building dynamic lyrics queries using specifications.
/// Implements the Builder pattern to construct complex queries without conditional logic.
/// </summary>
public interface ILyricsQueryBuilder
{
    /// <summary>
    /// Adds a full-text search filter across SongTitle, ArtistName, and Body.
    /// </summary>
    ILyricsQueryBuilder WithSearch(string? search);

    /// <summary>
    /// Adds a content status filter to the query.
    /// </summary>
    ILyricsQueryBuilder WithStatus(EnumContentStatus? status);

    /// <summary>
    /// Adds a category filter to the query.
    /// </summary>
    ILyricsQueryBuilder WithCategory(Guid? categoryId);

    /// <summary>
    /// Adds a language filter to the query.
    /// </summary>
    ILyricsQueryBuilder WithLanguage(string? language);

    /// <summary>
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<LyricsEntity>? Build();
}
