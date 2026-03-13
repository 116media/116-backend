using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a lyrics record by its unique identifier.
/// </summary>
public class LyricsByIdSpecification(Guid id) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.Id == id;
    }
}

/// <summary>
/// Specification that matches a lyrics record by song title and artist name (case-insensitive).
/// Used to enforce the uniqueness constraint at the application layer before insert.
/// </summary>
public class LyricsBySongAndArtistSpecification(string songTitle, string artistName) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics =>
            EF.Functions.ILike(lyrics.SongTitle, songTitle) && EF.Functions.ILike(lyrics.ArtistName, artistName);
    }
}
