using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Specifications;

/// <summary>
/// The single source of truth for "does this artist have surfaceable content" — used by the
/// public directory filter, the per-card content count, and (via the profile totals) the
/// profile's 404 rule. The three must never diverge: a filter wider than the 404 rule lists
/// artists whose pages 404, and a narrower one orphans real profiles.
/// <para>
/// Every surface the public profile renders has one term here and one matching count term in
/// <c>ArtistRepository.GetPublicDirectoryAsync</c> and <c>GetTotalsAsync</c> — EF Core cannot
/// invoke a shared count expression inside a projection, so the counts live inline beside the
/// queries that apply this specification. A future surface is added to all three places in the
/// same change that ships its section.
/// </para>
/// </summary>
public class ArtistHasContentSpecification(
    IQueryable<LyricsEntity> lyrics,
    IQueryable<VideoEntity> videos,
    IQueryable<AlbumEntity> albums,
    IQueryable<ArticleArtistEntity> articleArtists
) : Specification<ArtistEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist =>
            lyrics.Any(l => l.ArtistId == artist.Id && l.Status == EnumContentStatus.Published)
            || videos.Any(v => v.ArtistId == artist.Id && v.Status == EnumContentStatus.Published)
            || albums.Any(a =>
                a.ArtistId == artist.Id
                && (a.ReleaseType == EnumReleaseType.Album || a.ReleaseType == EnumReleaseType.Mixtape)
            )
            || articleArtists.Any(aa => aa.ArtistId == artist.Id && aa.Article.Status == EnumContentStatus.Published);
    }
}
