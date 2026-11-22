using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Application.Editorial.Builders.Contracts;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing dynamic video queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class VideoQueryBuilder : IVideoQueryBuilder
{
    private string? _categorySlug;
    private Specification<VideoEntity>? _specification;
    private string? _tagSlug;

    /// <inheritdoc />
    public IVideoQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(value: search))
        {
            return this;
        }

        var searchSpec = new VideoSearchSpecification(search: search);
        CombineSpecification(spec: searchSpec);
        return this;
    }

    /// <inheritdoc />
    public IVideoQueryBuilder WithStatus(EnumContentStatus? status)
    {
        if (!status.HasValue)
        {
            return this;
        }

        var statusSpec = new VideoByStatusSpecification(status: status.Value);
        CombineSpecification(spec: statusSpec);
        return this;
    }

    /// <inheritdoc />
    public IVideoQueryBuilder WithCategory(Guid? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return this;
        }

        var categorySpec = new VideoByCategorySpecification(categoryId: categoryId.Value);
        CombineSpecification(spec: categorySpec);
        return this;
    }

    /// <inheritdoc />
    public IVideoQueryBuilder WithTag(string? tagSlug)
    {
        if (string.IsNullOrWhiteSpace(value: tagSlug))
        {
            return this;
        }

        _tagSlug = tagSlug;
        return this;
    }

    /// <inheritdoc />
    public IVideoQueryBuilder WithCategorySlug(string? categorySlug)
    {
        if (string.IsNullOrWhiteSpace(value: categorySlug))
        {
            return this;
        }

        _categorySlug = categorySlug;
        return this;
    }

    /// <inheritdoc />
    public Specification<VideoEntity>? Build(IQueryable<TagEntity> tags, IQueryable<CategoryEntity> categories)
    {
        if (_categorySlug is not null)
        {
            var categorySlugSpec = new VideoByCategorySlugSpecification(
                categorySlug: _categorySlug,
                categories: categories
            );
            CombineSpecification(spec: categorySlugSpec);
        }

        if (_tagSlug is not null)
        {
            var tagSpec = new VideoByTagSlugSpecification(tagSlug: _tagSlug, tags: tags);
            CombineSpecification(spec: tagSpec);
        }

        return _specification;
    }

    private void CombineSpecification(Specification<VideoEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
