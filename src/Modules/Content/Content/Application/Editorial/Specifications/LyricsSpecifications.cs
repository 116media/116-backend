using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
/// Specification that matches a lyrics record by its URL-safe slug (case-insensitive).
/// </summary>
public class LyricsBySlugSpecification(string slug) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => EF.Functions.ILike(lyrics.Slug, slug);
    }
}

/// <summary>
/// Specification that matches lyrics records by their content status.
/// </summary>
public class LyricsByStatusSpecification(EnumContentStatus status) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.Status == status;
    }
}

/// <summary>
/// Specification that matches lyrics records belonging to a specific category.
/// </summary>
public class LyricsByCategorySpecification(Guid categoryId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.CategoryId == categoryId;
    }
}

/// <summary>
/// Specification for full-text search across lyrics SongTitle, ArtistName, and Body fields.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class LyricsSearchSpecification(string search) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return lyrics =>
            EF.Functions.ILike(lyrics.SongTitle, pattern)
            || EF.Functions.ILike(lyrics.ArtistName, pattern)
            || EF.Functions.ILike(lyrics.LyricsText, pattern);
    }
}

/// <summary>
/// Specification that matches a lyrics record by its linked video identifier.
/// </summary>
public class LyricsByVideoIdSpecification(Guid videoId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.VideoId == videoId;
    }
}

/// <summary>
/// Specification that matches lyrics records linked to a specific artist profile.
/// </summary>
public class LyricsByArtistSpecification(Guid artistId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.ArtistId == artistId;
    }
}

/// <summary>
/// Specification that matches lyrics records by their ISO 639-1 language code (case-insensitive).
/// </summary>
public class LyricsByLanguageSpecification(string language) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => EF.Functions.ILike(lyrics.Language, language);
    }
}

/// <summary>
/// Specification that matches lyrics records belonging to a specific album.
/// </summary>
public class LyricsByAlbumSpecification(Guid albumId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.AlbumId == albumId;
    }
}

/// <summary>
/// Specification that matches a lyrics record by the order item it fulfils.
/// </summary>
public class LyricsByOrderItemIdSpecification(Guid orderItemId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.OrderItemId == orderItemId;
    }
}

/// <summary>
/// Specification that matches published lyrics records sharing the same linked video category
/// as the source lyrics page, excluding the source page itself. First branch of the
/// similar-lyrics waterfall (spec 06): video-linked pages are matched against other lyrics
/// linked to a video in the same category.
/// </summary>
public class LyricsSimilarByVideoCategorySpecification(Guid categoryId, Guid excludeId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics =>
            lyrics.Id != excludeId
            && lyrics.Status == EnumContentStatus.Published
            && lyrics.Video != null
            && lyrics.Video.CategoryId == categoryId;
    }
}

/// <summary>
/// Specification that matches published lyrics records sharing at least one tag with the
/// source lyrics page, excluding the source page itself. Second branch of the similar-lyrics
/// waterfall (spec 06), tried when the video-category branch yields no matches.
/// </summary>
public class LyricsBySharedTagsSpecification(IReadOnlyCollection<Guid> tagIds, Guid excludeId)
    : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics =>
            lyrics.Id != excludeId
            && lyrics.Status == EnumContentStatus.Published
            && lyrics.Tags.Any(t => tagIds.Contains(t.TagId));
    }
}

/// <summary>
/// Specification that matches published, standalone (not linked to any video) lyrics records,
/// excluding the source lyrics page itself. Third and final branch of the similar-lyrics
/// waterfall (spec 06), tried when neither the video-category nor shared-tags branch yields
/// any matches.
/// </summary>
public class LyricsStandaloneSpecification(Guid excludeId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics =>
            lyrics.Id != excludeId && lyrics.Status == EnumContentStatus.Published && lyrics.VideoId == null;
    }
}

/// <summary>
/// Specification that matches a lyrics like by user and lyrics page identifiers.
/// </summary>
public class LyricsLikeByUserAndLyricsSpecification(Guid userId, Guid lyricsId) : Specification<LyricsLikeEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsLikeEntity, bool>> ToExpression()
    {
        return like => like.UserId == userId && like.LyricsId == lyricsId;
    }
}
