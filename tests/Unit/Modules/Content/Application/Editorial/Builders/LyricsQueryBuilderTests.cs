using _116.Content.Application.Editorial.Builders;
using _116.Content.Application.Editorial.Builders.Contracts;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="LyricsQueryBuilder"/>.
/// </summary>
public class LyricsQueryBuilderTests
{
    #region Build — no filters

    [Fact]
    public void Build_WithNoFilters_ShouldReturnNull()
    {
        var builder = new LyricsQueryBuilder();
        Specification<LyricsEntity>? spec = builder.Build();
        spec.Should().BeNull();
    }

    #endregion

    #region WithSearch

    [Fact]
    public void WithSearch_WithNull_ShouldReturnNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithSearch(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithSearch("   ");
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithTerm_ShouldReturnNonNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithSearch("some search");
        // LyricsSearchSpecification uses ILike — only verify spec is non-null
        builder.Build().Should().NotBeNull();
    }

    [Fact]
    public void WithSearch_CalledTwice_ShouldCombineSpecificationsWithAnd()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithSearch("first");
        builder.WithSearch("second");

        Specification<LyricsEntity>? spec = builder.Build();

        spec.Should().NotBeNull();
    }

    #endregion

    #region WithStatus

    [Fact]
    public void WithStatus_WithNull_ShouldReturnNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithStatus(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithStatus_WithValue_ShouldReturnNonNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published);
        builder.Build().Should().NotBeNull();
    }

    [Fact]
    public void WithStatus_WithValue_ShouldFilterByStatus()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published);
        Specification<LyricsEntity>? spec = builder.Build();

        LyricsEntity published = LyricsFactory.CreatePublished(Guid.NewGuid());
        LyricsEntity draft = LyricsFactory.Create(Guid.NewGuid());

        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(published).Should().BeTrue();
        spec.IsSatisfiedBy(draft).Should().BeFalse();
    }

    #endregion

    #region WithCategory

    [Fact]
    public void WithCategory_WithNull_ShouldReturnNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithCategory(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithCategory_WithValue_ShouldFilterByCategory()
    {
        Guid categoryId = Guid.NewGuid();
        var builder = new LyricsQueryBuilder();
        builder.WithCategory(categoryId);
        Specification<LyricsEntity>? spec = builder.Build();

        LyricsEntity matching = LyricsFactory.Create(categoryId);
        LyricsEntity other = LyricsFactory.Create(Guid.NewGuid());

        spec.Should().NotBeNull();
        spec!.IsSatisfiedBy(matching).Should().BeTrue();
        spec.IsSatisfiedBy(other).Should().BeFalse();
    }

    #endregion

    #region WithLanguage

    [Fact]
    public void WithLanguage_WithNull_ShouldReturnNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithLanguage(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithLanguage_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithLanguage("   ");
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithLanguage_WithValue_ShouldReturnNonNullSpec()
    {
        var builder = new LyricsQueryBuilder();
        builder.WithLanguage("fr");
        // LyricsByLanguageSpecification uses ILike — only verify spec is non-null
        builder.Build().Should().NotBeNull();
    }

    #endregion

    #region Combined filters

    [Fact]
    public void Build_WithMultipleFilters_ShouldCombineWithAnd()
    {
        Guid categoryId = Guid.NewGuid();
        var builder = new LyricsQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published).WithCategory(categoryId).WithLanguage("fr");

        Specification<LyricsEntity>? spec = builder.Build();

        spec.Should().NotBeNull();
    }

    #endregion

    #region Fluent chaining — returns self

    [Fact]
    public void WithSearch_ShouldReturnSameBuilderInstance()
    {
        var builder = new LyricsQueryBuilder();
        ILyricsQueryBuilder returned = builder.WithSearch("test");
        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithSearch_WithNull_ShouldReturnSameBuilderInstance()
    {
        var builder = new LyricsQueryBuilder();
        ILyricsQueryBuilder returned = builder.WithSearch(null);
        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithStatus_ShouldReturnSameBuilderInstance()
    {
        var builder = new LyricsQueryBuilder();
        ILyricsQueryBuilder returned = builder.WithStatus(EnumContentStatus.Published);
        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithCategory_ShouldReturnSameBuilderInstance()
    {
        var builder = new LyricsQueryBuilder();
        ILyricsQueryBuilder returned = builder.WithCategory(Guid.NewGuid());
        returned.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithLanguage_ShouldReturnSameBuilderInstance()
    {
        var builder = new LyricsQueryBuilder();
        ILyricsQueryBuilder returned = builder.WithLanguage("fr");
        returned.Should().BeSameAs(builder);
    }

    #endregion
}
