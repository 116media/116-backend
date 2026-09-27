using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.Builders;

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
    public void WithSearch_WithTerm_ShouldMatchTitleCaseInsensitively()
    {
        ArticleEntity match = ArticleFactory.Create(CategoryId, "Fally Ipupa Portrait", "fally-ipupa-portrait");
        ArticleEntity noMatch = ArticleFactory.Create(CategoryId, "Koffi Olomide Live", "koffi-olomide-live");
        var builder = new ArticleQueryBuilder();
        builder.WithSearch("FALLY");

        Specification<ArticleEntity> spec = builder.Build()!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(noMatch).Should().BeFalse();
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
