using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Specifications;
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

    #endregion
}
