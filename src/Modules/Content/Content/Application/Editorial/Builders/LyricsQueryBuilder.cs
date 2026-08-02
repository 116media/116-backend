using _116.Content.Application.Editorial.Builders.Contracts;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing dynamic lyrics queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class LyricsQueryBuilder : ILyricsQueryBuilder
{
    private Specification<LyricsEntity>? _specification;

    /// <inheritdoc />
    public ILyricsQueryBuilder WithSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(value: search))
        {
            return this;
        }

        var searchSpec = new LyricsSearchSpecification(search: search);
        CombineSpecification(spec: searchSpec);
        return this;
    }

    /// <inheritdoc />
    public ILyricsQueryBuilder WithStatus(EnumContentStatus? status)
    {
        if (!status.HasValue)
        {
            return this;
        }

        var statusSpec = new LyricsByStatusSpecification(status: status.Value);
        CombineSpecification(spec: statusSpec);
        return this;
    }

    /// <inheritdoc />
    public ILyricsQueryBuilder WithCategory(Guid? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return this;
        }

        var categorySpec = new LyricsByCategorySpecification(categoryId: categoryId.Value);
        CombineSpecification(spec: categorySpec);
        return this;
    }

    /// <inheritdoc />
    public ILyricsQueryBuilder WithLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(value: language))
        {
            return this;
        }

        var languageSpec = new LyricsByLanguageSpecification(language: language);
        CombineSpecification(spec: languageSpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<LyricsEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<LyricsEntity> spec)
    {
        _specification = _specification is null ? spec : _specification.And(other: spec);
    }
}
