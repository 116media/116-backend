using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="VideoQueryBuilder"/>.
/// </summary>
public class VideoQueryBuilderTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    #region Build — no filters

    [Fact]
    public void Build_WithNoFilters_ShouldReturnNull()
    {
        var builder = new VideoQueryBuilder();
        Specification<VideoEntity>? spec = builder.Build();
        spec.Should().BeNull();
    }

    #endregion

    #region WithSearch

    [Fact]
    public void WithSearch_WithNull_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithSearch(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithSearch("   ");
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithTerm_ShouldReturnNonNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithSearch("some search");
        // VideoSearchSpecification uses ILike — only verify spec is non-null
        builder.Build().Should().NotBeNull();
    }

    #endregion

    #region WithStatus

    [Fact]
    public void WithStatus_WithNull_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithStatus(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithStatus_WithDraft_ShouldMatchDraftVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft);

        Specification<VideoEntity> spec = builder.Build()!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_WithPublished_ShouldNotMatchDraftVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published);

        Specification<VideoEntity> spec = builder.Build()!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeFalse();
    }

    #endregion

    #region WithCategory

    [Fact]
    public void WithCategory_WithNull_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithCategory(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithCategory_WithMatchingId_ShouldMatchVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithCategory(CategoryId);

        Specification<VideoEntity> spec = builder.Build()!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeTrue();
    }

    [Fact]
    public void WithCategory_WithDifferentId_ShouldNotMatchVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithCategory(Guid.NewGuid());

        Specification<VideoEntity> spec = builder.Build()!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeFalse();
    }

    #endregion

    #region Chaining

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldMatchWhenBothMatch()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(CategoryId);

        Specification<VideoEntity> spec = builder.Build()!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldNotMatchWhenOneFails()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(Guid.NewGuid());

        Specification<VideoEntity> spec = builder.Build()!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeFalse();
    }

    #endregion
}
