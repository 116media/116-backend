using System.Linq.Expressions;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Catalog.Specifications;

/// <summary>
/// Specification that matches a category by its URL-safe slug (case-insensitive).
/// </summary>
public class CategoryBySlugSpecification(string slug) : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => EF.Functions.ILike(category.Slug, slug);
    }
}

/// <summary>
/// Specification that matches only active categories.
/// Used for public-facing queries and order creation where inactive categories must be hidden.
/// </summary>
public class ActiveCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.IsActive;
    }
}

/// <summary>
/// Specification that matches only free categories (no payment required).
/// </summary>
public class FreeCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.IsFree;
    }
}

/// <summary>
/// Specification that matches only paid categories.
/// </summary>
public class PaidCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => !category.IsFree;
    }
}

/// <summary>
/// Specification that matches categories belonging to a specific content type.
/// </summary>
public class CategoryByContentTypeSpecification(Guid contentTypeId) : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category => category.ContentTypeId == contentTypeId;
    }
}

/// <summary>
/// Specification that matches the single active gossip category.
/// Published articles belonging to this category populate the homepage gossip strip
/// and serve as fallback content for empty promotion spots.
/// </summary>
public class GossipCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return new MarkedGossipSpecification().And(new ActiveCategorySpecification()).ToExpression();
    }

    /// <summary>
    /// The gossip flag alone; active-ness comes from the composed
    /// <see cref="ActiveCategorySpecification" />.
    /// </summary>
    private sealed class MarkedGossipSpecification : Specification<CategoryEntity>
    {
        /// <inheritdoc />
        public override Expression<Func<CategoryEntity, bool>> ToExpression()
        {
            return category => category.IsGossip;
        }
    }
}

/// <summary>
/// Specification that matches the single active exclusive category.
/// The exclusive show is featured on the homepage after the promotion feed section.
/// </summary>
public class ExclusiveCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return new MarkedExclusiveSpecification().And(new ActiveCategorySpecification()).ToExpression();
    }

    /// <summary>
    /// The exclusive flag alone; active-ness comes from the composed
    /// <see cref="ActiveCategorySpecification" />.
    /// </summary>
    private sealed class MarkedExclusiveSpecification : Specification<CategoryEntity>
    {
        /// <inheritdoc />
        public override Expression<Func<CategoryEntity, bool>> ToExpression()
        {
            return category => category.IsExclusive;
        }
    }
}

/// <summary>
/// Specification that matches the single active category designated as the default for lyrics
/// pages.
/// </summary>
public class DefaultLyricsCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return new MarkedDefaultForLyricsSpecification().And(new ActiveCategorySpecification()).ToExpression();
    }

    /// <summary>
    /// The default-for-lyrics flag alone; active-ness comes from the composed
    /// <see cref="ActiveCategorySpecification" />.
    /// </summary>
    private sealed class MarkedDefaultForLyricsSpecification : Specification<CategoryEntity>
    {
        /// <inheritdoc />
        public override Expression<Func<CategoryEntity, bool>> ToExpression()
        {
            return category => category.IsDefaultForLyrics;
        }
    }
}

/// <summary>
/// Specification that matches active categories currently pinned to the content feed.
/// Filters on <c>PinnedToFeedAt</c> (the mapped column), not the
/// <c>[NotMapped]</c> <c>IsPinnedToFeed</c> property, so it translates to SQL. Narrowing to a
/// content type is composed at the call site with
/// <see cref="CategoryByContentTypeSpecification" />.
/// </summary>
public class PinnedToFeedCategorySpecification : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return new MarkedPinnedToFeedSpecification().And(new ActiveCategorySpecification()).ToExpression();
    }

    /// <summary>
    /// The pinned stamp alone; active-ness comes from the composed
    /// <see cref="ActiveCategorySpecification" />.
    /// </summary>
    private sealed class MarkedPinnedToFeedSpecification : Specification<CategoryEntity>
    {
        /// <inheritdoc />
        public override Expression<Func<CategoryEntity, bool>> ToExpression()
        {
            return category => category.PinnedToFeedAt != null;
        }
    }
}
