using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing dynamic article queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class ArticleQueryBuilder : IArticleQueryBuilder
{
    private Specification<ArticleEntity>? _specification;

    /// <inheritdoc />
    public IArticleQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(value: search))
        {
            return this;
        }

        var searchSpec = new ArticleSearchSpecification(search: search);
        CombineSpecification(spec: searchSpec);
        return this;
    }

    /// <inheritdoc />
    public IArticleQueryBuilder WithStatus(EnumContentStatus? status)
    {
        if (!status.HasValue)
        {
            return this;
        }

        var statusSpec = new ArticleByStatusSpecification(status: status.Value);
        CombineSpecification(spec: statusSpec);
        return this;
    }

    /// <inheritdoc />
    public IArticleQueryBuilder WithCategory(Guid? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return this;
        }

        var categorySpec = new ArticleByCategorySpecification(categoryId: categoryId.Value);
        CombineSpecification(spec: categorySpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<ArticleEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<ArticleEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
