using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing dynamic video queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class VideoQueryBuilder : IVideoQueryBuilder
{
    private Specification<VideoEntity>? _specification;

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
    public Specification<VideoEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<VideoEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
