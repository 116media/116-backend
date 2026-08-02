using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches a streaming link by its album and platform.
/// </summary>
public class StreamingLinkByAlbumAndPlatformSpecification(Guid albumId, EnumStreamingPlatform platform)
    : Specification<StreamingLinkEntity>
{
    /// <inheritdoc />
    public override Expression<Func<StreamingLinkEntity, bool>> ToExpression()
    {
        return link => link.AlbumId == albumId && link.Platform == platform;
    }
}

/// <summary>
/// Specification that matches every curated streaming link belonging to an album.
/// </summary>
public class StreamingLinkByAlbumSpecification(Guid albumId) : Specification<StreamingLinkEntity>
{
    /// <inheritdoc />
    public override Expression<Func<StreamingLinkEntity, bool>> ToExpression()
    {
        return link => link.AlbumId == albumId;
    }
}

/// <summary>
/// Specification that matches a streaming link by its standalone single (lyrics page) and platform.
/// </summary>
public class StreamingLinkByLyricsAndPlatformSpecification(Guid lyricsId, EnumStreamingPlatform platform)
    : Specification<StreamingLinkEntity>
{
    /// <inheritdoc />
    public override Expression<Func<StreamingLinkEntity, bool>> ToExpression()
    {
        return link => link.LyricsId == lyricsId && link.Platform == platform;
    }
}

/// <summary>
/// Specification that matches every curated streaming link belonging to a standalone single
/// (a lyrics page with no <c>AlbumId</c>).
/// </summary>
public class StreamingLinkByLyricsSpecification(Guid lyricsId) : Specification<StreamingLinkEntity>
{
    /// <inheritdoc />
    public override Expression<Func<StreamingLinkEntity, bool>> ToExpression()
    {
        return link => link.LyricsId == lyricsId;
    }
}
