using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="ArticleQueryBuilder"/>.
/// </summary>
public class ArticleQueryBuilderTests
{
    /// <summary>
    /// The tag rows the slug filter probes; empty for the tests that do not filter by tag.
    /// </summary>
    private static readonly IQueryable<TagEntity> NoTags = Array.Empty<TagEntity>().AsQueryable();

    /// <summary>
    /// The category rows the slug filter probes; empty for the tests that do not filter by category slug.
    /// </summary>
    private static readonly IQueryable<CategoryEntity> NoCategories = Array.Empty<CategoryEntity>().AsQueryable();

    private static readonly Guid CategoryId = Guid.NewGuid();

    /// <summary>
    /// Attaches a tag to an article through the junction entity, which carries the tag id only;
    /// the tag row itself reaches the specification as the probed source.
    /// </summary>
    private static void AttachTag(ArticleEntity article, TagEntity tag)
    {
        article.Tags.Add(ArticleTagFactory.Create(article.Id, tag.Id));
    }

    /// <summary>
    /// The category rows a slug filter probes.
    /// </summary>
    private static IQueryable<CategoryEntity> CategorySource(params CategoryEntity[] categories) =>
        categories.AsQueryable();

    /// <summary>
    /// The tag rows a slug filter probes.
    /// </summary>
    private static IQueryable<TagEntity> TagSource(params TagEntity[] tags) => tags.AsQueryable();

    #region Build — no filters

    [Fact]
    public void Build_WithNoFilters_ShouldReturnNull()
    {
        var builder = new ArticleQueryBuilder();
        Specification<ArticleEntity>? spec = builder.Build(NoTags, NoCategories);
        spec.Should().BeNull();
    }

    #endregion

    #region WithSearch

    [Fact]
    public void WithSearch_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithSearch(null);
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithSearch("   ");
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithTerm_ShouldMatchTitleCaseInsensitively()
    {
        ArticleEntity match = ArticleFactory.Create(CategoryId, "Fally Ipupa Portrait", "fally-ipupa-portrait");
        ArticleEntity noMatch = ArticleFactory.Create(CategoryId, "Koffi Olomide Live", "koffi-olomide-live");
        var builder = new ArticleQueryBuilder();
        builder.WithSearch("FALLY");

        Specification<ArticleEntity> spec = builder.Build(NoTags, NoCategories)!;

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
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithStatus_WithDraft_ShouldMatchDraftArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft);

        Specification<ArticleEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_WithPublished_ShouldNotMatchDraftArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published);

        Specification<ArticleEntity> spec = builder.Build(NoTags, NoCategories)!;
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
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithCategory_WithMatchingId_ShouldMatchArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithCategory(CategoryId);

        Specification<ArticleEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeTrue();
    }

    [Fact]
    public void WithCategory_WithDifferentId_ShouldNotMatchArticle()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithCategory(Guid.NewGuid());

        Specification<ArticleEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeFalse();
    }

    #endregion

    #region Build — category slug filter

    [Fact]
    public void WithCategorySlug_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithCategorySlug(null);
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithCategorySlug_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithCategorySlug("   ");
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithCategorySlug_WithSlug_ShouldMatchOnlyArticlesInThatCategory()
    {
        CategoryEntity gossip = CategoryFactory.Create(Guid.NewGuid(), "Gossip", "gossip");
        ArticleEntity inCategory = ArticleFactory.Create(gossip.Id);
        ArticleEntity otherCategory = ArticleFactory.Create(Guid.NewGuid());
        var builder = new ArticleQueryBuilder();
        builder.WithCategorySlug("GOSSIP");

        Specification<ArticleEntity> spec = builder.Build(NoTags, CategorySource(gossip))!;

        spec.IsSatisfiedInMemoryBy(inCategory).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(otherCategory).Should().BeFalse();
    }

    #endregion

    #region Build — tag slug filter

    [Fact]
    public void WithTag_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithTag(null);
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithTag_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new ArticleQueryBuilder();
        builder.WithTag("   ");
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithTag_WithSlug_ShouldMatchOnlyArticlesCarryingTheTag()
    {
        TagEntity rumba = TagFactory.Create("Rumba", "rumba");
        ArticleEntity taggedArticle = ArticleFactory.Create(CategoryId);
        AttachTag(taggedArticle, rumba);
        ArticleEntity untaggedArticle = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithTag("RUMBA");

        Specification<ArticleEntity> spec = builder.Build(TagSource(rumba), NoCategories)!;

        spec.IsSatisfiedInMemoryBy(taggedArticle).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(untaggedArticle).Should().BeFalse();
    }

    [Fact]
    public void WithCategorySlug_AndWithTag_Combined_ShouldMatchOnlyArticlesSatisfyingBoth()
    {
        CategoryEntity gossip = CategoryFactory.Create(Guid.NewGuid(), "Gossip", "gossip");
        TagEntity rumba = TagFactory.Create("Rumba", "rumba");
        ArticleEntity match = ArticleFactory.Create(gossip.Id);
        AttachTag(match, rumba);
        ArticleEntity taggedElsewhere = ArticleFactory.Create(Guid.NewGuid());
        AttachTag(taggedElsewhere, rumba);
        ArticleEntity inCategoryUntagged = ArticleFactory.Create(gossip.Id);
        var builder = new ArticleQueryBuilder();
        builder.WithCategorySlug("gossip").WithTag("rumba");

        Specification<ArticleEntity> spec = builder.Build(TagSource(rumba), CategorySource(gossip))!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(taggedElsewhere).Should().BeFalse();
        spec.IsSatisfiedInMemoryBy(inCategoryUntagged).Should().BeFalse();
    }

    #endregion

    #region Chaining

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldMatchWhenBothMatch()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(CategoryId);

        Specification<ArticleEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldNotMatchWhenOneFails()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var builder = new ArticleQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(Guid.NewGuid());

        Specification<ArticleEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<ArticleEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(article).Should().BeFalse();
    }

    #endregion
}
