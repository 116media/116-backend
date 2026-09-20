using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// Specification that matches artists with at least one published lyrics page.
/// </summary>
/// <param name="lyrics">The lyrics source the artist is matched against.</param>
public class ArtistHasPublishedLyricsSpecification(IQueryable<LyricsEntity> lyrics) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist => lyrics.Any(l => l.ArtistId == artist.Id && l.Status == EnumContentStatus.Published);
    }
}

/// <summary>
/// Specification that matches artists with at least one published video.
/// </summary>
/// <param name="videos">The video source the artist is matched against.</param>
public class ArtistHasPublishedVideosSpecification(IQueryable<VideoEntity> videos) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist => videos.Any(v => v.ArtistId == artist.Id && v.Status == EnumContentStatus.Published);
    }
}

/// <summary>
/// Specification that matches artists with at least one full-length release — an album or a
/// mixtape; singles and EPs do not qualify.
/// </summary>
/// <param name="albums">The album source the artist is matched against.</param>
public class ArtistHasFullLengthReleaseSpecification(IQueryable<AlbumEntity> albums) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist =>
            albums.Any(a =>
                a.ArtistId == artist.Id
                && (a.ReleaseType == EnumReleaseType.Album || a.ReleaseType == EnumReleaseType.Mixtape)
            );
    }
}

/// <summary>
/// Specification that matches artists tagged in at least one published article.
/// </summary>
/// <param name="articleArtists">The article-artist junction the artist is matched against.</param>
/// <param name="articles">The articles the junction rows are probed against.</param>
public class ArtistHasPublishedArticleSpecification(
    IQueryable<ArticleArtistEntity> articleArtists,
    IQueryable<ArticleEntity> articles
) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist =>
            articleArtists.Any(link =>
                link.ArtistId == artist.Id
                && articles.Any(article =>
                    article.Id == link.ArticleId && article.Status == EnumContentStatus.Published
                )
            );
    }
}

/// <summary>
/// Specification that matches artists with any publicly visible content — the four content
/// sources composed with <see cref="Specification{T}.OrAll" />, so each disjunct stays an
/// independently reusable rule.
/// <para>
/// The per-row counts in <c>ArtistRepository.GetPublicDirectoryAsync</c> and
/// <c>GetTotalsAsync</c> stay inline beside the queries that apply this specification — EF Core
/// cannot invoke a shared count expression inside a projection — but remain term-for-term
/// aligned with the four rules composed here. A future content surface is added to the rule it
/// belongs to and to those counts in the same change.
/// </para>
/// </summary>
/// <param name="lyrics">The lyrics source consulted for published pages.</param>
/// <param name="videos">The video source consulted for published videos.</param>
/// <param name="albums">The album source consulted for full-length releases.</param>
/// <param name="articleArtists">The junction consulted for published tagged articles.</param>
/// <param name="articles">The articles the junction rows are probed against.</param>
public class ArtistHasContentSpecification(
    IQueryable<LyricsEntity> lyrics,
    IQueryable<VideoEntity> videos,
    IQueryable<AlbumEntity> albums,
    IQueryable<ArticleArtistEntity> articleArtists,
    IQueryable<ArticleEntity> articles
) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return OrAll(
                new ArtistHasPublishedLyricsSpecification(lyrics: lyrics),
                new ArtistHasPublishedVideosSpecification(videos: videos),
                new ArtistHasFullLengthReleaseSpecification(albums: albums),
                new ArtistHasPublishedArticleSpecification(articleArtists: articleArtists, articles: articles)
            )
            .ToExpression();
    }
}
