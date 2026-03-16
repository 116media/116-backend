using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="ArticleQueryBuilder"/>.
/// </summary>
public class ArticleQueryBuilderTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    #region Build — no filters

    [Fact]
    public void Build_WithNoFilters_ShouldReturnNull()
    {
        var builder = new ArticleQueryBuilder();
        Specification<ArticleEntity>? spec = builder.Build();
        spec.Should().BeNull();
    }

    #endregion

    #region WithSearch

    [Fact]
    public void WithSearch_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithSearch(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithSearch("   ");
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithTerm_ShouldReturnNonNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithSearch("some search");
        // ArticleSearchSpecification uses ILike — only verify spec is non-null
        builder.Build().Should().NotBeNull();
    }

    #endregion

    #region WithStatus

    [Fact]
    public void WithStatus_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithStatus_WithDraft_ShouldMatchDraftArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft);

        Specification<ArticleEntity> spec = builder.Build()!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_WithPublished_ShouldNotMatchDraftArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published);

        Specification<ArticleEntity> spec = builder.Build()!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeFalse();
    }

    #endregion

    #region WithCategory

    [Fact]
    public void WithCategory_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithCategory(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithCategory_WithMatchingId_ShouldMatchArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithCategory(CategoryId);

        Specification<ArticleEntity> spec = builder.Build()!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeTrue();
    }

    [Fact]
    public void WithCategory_WithDifferentId_ShouldNotMatchArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithCategory(Guid.NewGuid());

        Specification<ArticleEntity> spec = builder.Build()!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeFalse();
    }

    #endregion

    #region Chaining

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldMatchWhenBothMatch()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(CategoryId);

        Specification<ArticleEntity> spec = builder.Build()!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldNotMatchWhenOneFails()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(Guid.NewGuid());

        Specification<ArticleEntity> spec = builder.Build()!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeFalse();
    }

    #endregion
}
