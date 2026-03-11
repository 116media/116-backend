using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

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
    /// Builds and returns the final specification.
    /// Returns null if no filters were applied.
    /// </summary>
    Specification<LyricsEntity>? Build();
}
