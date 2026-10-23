using System.Linq.Expressions;
using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Lookup.Specifications;

/// <summary>
/// Specification that matches a tag by its URL-safe slug.
/// </summary>
public class TagBySlugSpecification(string slug) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return tag => tag.Slug == slug;
    }
}

/// <summary>
/// Specification that matches a tag by its name (case-insensitive).
/// </summary>
public class TagByNameSpecification(string name) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return tag => EF.Functions.ILike(tag.Name, name);
    }
}

/// <summary>
/// Specification for fuzzy search across tag Name and Slug.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class TagSearchSpecification(string search) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return tag => EF.Functions.ILike(tag.Name, pattern) || EF.Functions.ILike(tag.Slug, pattern);
    }
}

/// <summary>
/// Specification that matches tags at least one article carries.
/// </summary>
/// <param name="articleTags">The article-tag junction the tag is matched against.</param>
public class TagUsedByArticlesSpecification(IQueryable<ArticleTagEntity> articleTags) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return tag => articleTags.Any(at => at.TagId == tag.Id);
    }
}

/// <summary>
/// Specification that matches tags at least one video carries.
/// </summary>
/// <param name="videoTags">The video-tag junction the tag is matched against.</param>
public class TagUsedByVideosSpecification(IQueryable<VideoTagEntity> videoTags) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return tag => videoTags.Any(vt => vt.TagId == tag.Id);
    }
}

/// <summary>
/// Specification that matches tags at least one lyrics page carries.
/// </summary>
/// <param name="lyricsTags">The lyrics-tag junction the tag is matched against.</param>
public class TagUsedByLyricsSpecification(IQueryable<LyricsTagEntity> lyricsTags) : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return tag => lyricsTags.Any(lt => lt.TagId == tag.Id);
    }
}

/// <summary>
/// Specification that matches no tag at all, for the content kinds that carry no tag
/// associations: asking for them yields nothing rather than the whole vocabulary.
/// </summary>
public class TagUsedByNothingSpecification : Specification<TagEntity>
{
    /// <inheritdoc />
    public override Expression<Func<TagEntity, bool>> ToExpression()
    {
        return _ => false;
    }
}
