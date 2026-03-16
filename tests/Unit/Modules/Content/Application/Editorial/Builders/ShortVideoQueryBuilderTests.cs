using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="ShortVideoQueryBuilder"/>.
/// </summary>
public class ShortVideoQueryBuilderTests
{
    #region Build — no filters

    [Fact]
    public void Build_WithNoFilters_ShouldReturnNull()
    {
        var builder = new ShortVideoQueryBuilder();
        Specification<ShortVideoEntity>? spec = builder.Build();
        spec.Should().BeNull();
    }

    #endregion

    #region WithSearch

    [Fact]
    public void WithSearch_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ShortVideoQueryBuilder();
        builder.WithSearch(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new ShortVideoQueryBuilder();
        builder.WithSearch("   ");
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithTerm_ShouldReturnNonNullSpec()
    {
        var builder = new ShortVideoQueryBuilder();
        builder.WithSearch("some search");
        // ShortVideoSearchSpecification uses ILike — only verify spec is non-null
        builder.Build().Should().NotBeNull();
    }

    #endregion

    #region WithIsActive

    [Fact]
    public void WithIsActive_WithNull_ShouldReturnNullSpec()
    {
        var builder = new ShortVideoQueryBuilder();
        builder.WithIsActive(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithIsActive_True_ShouldMatchActiveShortVideo()
    {
        ShortVideoEntity active = ShortVideoFactory.Create();
        var builder = new ShortVideoQueryBuilder();
        builder.WithIsActive(true);

        Specification<ShortVideoEntity> spec = builder.Build()!;
        Func<ShortVideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(active).Should().BeTrue();
    }

    [Fact]
    public void WithIsActive_True_ShouldNotMatchInactiveShortVideo()
    {
        ShortVideoEntity inactive = ShortVideoFactory.CreateInactive();
        var builder = new ShortVideoQueryBuilder();
        builder.WithIsActive(true);

        Specification<ShortVideoEntity> spec = builder.Build()!;
        Func<ShortVideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(inactive).Should().BeFalse();
    }

    [Fact]
    public void WithIsActive_False_ShouldMatchInactiveShortVideo()
    {
        ShortVideoEntity inactive = ShortVideoFactory.CreateInactive();
        var builder = new ShortVideoQueryBuilder();
        builder.WithIsActive(false);

        Specification<ShortVideoEntity> spec = builder.Build()!;
        Func<ShortVideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(inactive).Should().BeTrue();
    }

    [Fact]
    public void WithIsActive_False_ShouldNotMatchActiveShortVideo()
    {
        ShortVideoEntity active = ShortVideoFactory.Create();
        var builder = new ShortVideoQueryBuilder();
        builder.WithIsActive(false);

        Specification<ShortVideoEntity> spec = builder.Build()!;
        Func<ShortVideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(active).Should().BeFalse();
    }

    #endregion

    #region Chaining

    [Fact]
    public void WithSearch_AndWithIsActive_Combined_ShouldReturnNonNullSpec()
    {
        var builder = new ShortVideoQueryBuilder();
        builder.WithSearch("clip").WithIsActive(true);
        builder.Build().Should().NotBeNull();
    }

    #endregion
}
