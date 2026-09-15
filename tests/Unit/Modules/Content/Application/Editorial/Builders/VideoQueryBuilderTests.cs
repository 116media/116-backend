using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Specifications;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="VideoQueryBuilder"/>.
/// </summary>
public class VideoQueryBuilderTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    /// <summary>
    /// Attaches a tag to a video through the junction entity, populating the Tag
    /// navigation EF Core would load via Include.
    /// </summary>
    private static void AttachTag(VideoEntity video, TagEntity tag)
    {
        VideoTagEntity videoTag = VideoTagEntity.Create(Guid.NewGuid(), video.Id, tag.Id);
        typeof(VideoTagEntity).GetProperty(nameof(VideoTagEntity.Tag))!.SetValue(videoTag, tag);
        video.Tags.Add(videoTag);
    }

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
    public void WithSearch_WithTerm_ShouldMatchTitleCaseInsensitively()
    {
        VideoEntity match = new VideoBuilder(CategoryId)
            .WithTitle("116 Le Focus Fally Ipupa")
            .WithDescription("Interview intégrale")
            .Build();
        VideoEntity noMatch = new VideoBuilder(CategoryId)
            .WithTitle("116 Le Focus Koffi Olomide")
            .WithDescription("Interview intégrale")
            .Build();
        var builder = new VideoQueryBuilder();
        builder.WithSearch("FALLY");

        Specification<VideoEntity> spec = builder.Build()!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(noMatch).Should().BeFalse();
    }

    [Fact]
    public void WithSearch_WithTerm_ShouldMatchDescriptionAsWellAsTitle()
    {
        VideoEntity match = new VideoBuilder(CategoryId)
            .WithTitle("116 Le Focus Koffi Olomide")
            .WithDescription("Un hommage à Fally Ipupa")
            .Build();
        var builder = new VideoQueryBuilder();
        builder.WithSearch("fally");

        Specification<VideoEntity> spec = builder.Build()!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
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

    #region WithTag

    [Fact]
    public void WithTag_WithNull_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithTag(null);
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithTag_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithTag("   ");
        builder.Build().Should().BeNull();
    }

    [Fact]
    public void WithTag_WithSlug_ShouldMatchOnlyVideosCarryingTheTag()
    {
        VideoEntity taggedVideo = VideoFactory.Create(CategoryId);
        AttachTag(taggedVideo, TagFactory.Create("Rumba", "rumba"));
        VideoEntity untaggedVideo = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithTag("RUMBA");

        Specification<VideoEntity> spec = builder.Build()!;

        spec.IsSatisfiedInMemoryBy(taggedVideo).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(untaggedVideo).Should().BeFalse();
    }

    [Fact]
    public void WithTag_AndWithStatus_Combined_ShouldMatchOnlyVideosSatisfyingBoth()
    {
        VideoEntity match = VideoFactory.CreatePublished(CategoryId);
        AttachTag(match, TagFactory.Create("Rumba", "rumba"));
        VideoEntity draftTagged = VideoFactory.Create(CategoryId);
        AttachTag(draftTagged, TagFactory.Create("Rumba", "rumba"));
        VideoEntity publishedUntagged = VideoFactory.CreatePublished(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published).WithTag("rumba");

        Specification<VideoEntity> spec = builder.Build()!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(draftTagged).Should().BeFalse();
        spec.IsSatisfiedInMemoryBy(publishedUntagged).Should().BeFalse();
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
