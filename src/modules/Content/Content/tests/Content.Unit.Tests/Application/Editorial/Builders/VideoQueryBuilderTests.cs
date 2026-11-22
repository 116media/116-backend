using _116.BuildingBlocks.Domain.Specifications;
using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="VideoQueryBuilder"/>.
/// </summary>
public class VideoQueryBuilderTests
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
    /// Attaches a tag to a video through the junction entity, which carries the tag id only;
    /// the tag row itself reaches the specification as the probed source.
    /// </summary>
    private static void AttachTag(VideoEntity video, TagEntity tag)
    {
        video.Tags.Add(VideoTagFactory.Create(video.Id, tag.Id));
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
        var builder = new VideoQueryBuilder();
        Specification<VideoEntity>? spec = builder.Build(NoTags, NoCategories);
        spec.Should().BeNull();
    }

    #endregion

    #region WithSearch

    [Fact]
    public void WithSearch_WithNull_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithSearch(null);
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithSearch_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithSearch("   ");
        builder.Build(NoTags, NoCategories).Should().BeNull();
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

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;

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

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
    }

    #endregion

    #region WithStatus

    [Fact]
    public void WithStatus_WithNull_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithStatus(null);
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithStatus_WithDraft_ShouldMatchDraftVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft);

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_WithPublished_ShouldNotMatchDraftVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published);

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;
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
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithCategory_WithMatchingId_ShouldMatchVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithCategory(CategoryId);

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeTrue();
    }

    [Fact]
    public void WithCategory_WithDifferentId_ShouldNotMatchVideo()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithCategory(Guid.NewGuid());

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;
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
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithTag_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithTag("   ");
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithTag_WithSlug_ShouldMatchOnlyVideosCarryingTheTag()
    {
        TagEntity rumba = TagFactory.Create("Rumba", "rumba");
        VideoEntity taggedVideo = VideoFactory.Create(CategoryId);
        AttachTag(taggedVideo, rumba);
        VideoEntity untaggedVideo = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithTag("RUMBA");

        Specification<VideoEntity> spec = builder.Build(TagSource(rumba), NoCategories)!;

        spec.IsSatisfiedInMemoryBy(taggedVideo).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(untaggedVideo).Should().BeFalse();
    }

    [Fact]
    public void WithTag_AndWithStatus_Combined_ShouldMatchOnlyVideosSatisfyingBoth()
    {
        TagEntity rumba = TagFactory.Create("Rumba", "rumba");
        VideoEntity match = VideoFactory.CreatePublished(CategoryId);
        AttachTag(match, rumba);
        VideoEntity draftTagged = VideoFactory.Create(CategoryId);
        AttachTag(draftTagged, rumba);
        VideoEntity publishedUntagged = VideoFactory.CreatePublished(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Published).WithTag("rumba");

        Specification<VideoEntity> spec = builder.Build(TagSource(rumba), NoCategories)!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(draftTagged).Should().BeFalse();
        spec.IsSatisfiedInMemoryBy(publishedUntagged).Should().BeFalse();
    }

    #endregion

    #region Build — category slug filter

    [Fact]
    public void WithCategorySlug_WithNull_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithCategorySlug(null);
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithCategorySlug_WithWhitespace_ShouldReturnNullSpec()
    {
        var builder = new VideoQueryBuilder();
        builder.WithCategorySlug("   ");
        builder.Build(NoTags, NoCategories).Should().BeNull();
    }

    [Fact]
    public void WithCategorySlug_WithSlug_ShouldMatchOnlyVideosInThatCategory()
    {
        CategoryEntity gospel = CategoryFactory.Create(Guid.NewGuid(), "Gospel", "gospel");
        VideoEntity inCategory = VideoFactory.Create(gospel.Id);
        VideoEntity otherCategory = VideoFactory.Create(Guid.NewGuid());
        var builder = new VideoQueryBuilder();
        builder.WithCategorySlug("GOSPEL");

        Specification<VideoEntity> spec = builder.Build(NoTags, CategorySource(gospel))!;

        spec.IsSatisfiedInMemoryBy(inCategory).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(otherCategory).Should().BeFalse();
    }

    [Fact]
    public void WithCategorySlug_AndWithTag_Combined_ShouldMatchOnlyVideosSatisfyingBoth()
    {
        CategoryEntity gospel = CategoryFactory.Create(Guid.NewGuid(), "Gospel", "gospel");
        TagEntity rumba = TagFactory.Create("Rumba", "rumba");
        VideoEntity match = VideoFactory.Create(gospel.Id);
        AttachTag(match, rumba);
        VideoEntity taggedElsewhere = VideoFactory.Create(Guid.NewGuid());
        AttachTag(taggedElsewhere, rumba);
        VideoEntity inCategoryUntagged = VideoFactory.Create(gospel.Id);
        var builder = new VideoQueryBuilder();
        builder.WithCategorySlug("gospel").WithTag("rumba");

        Specification<VideoEntity> spec = builder.Build(TagSource(rumba), CategorySource(gospel))!;

        spec.IsSatisfiedInMemoryBy(match).Should().BeTrue();
        spec.IsSatisfiedInMemoryBy(taggedElsewhere).Should().BeFalse();
        spec.IsSatisfiedInMemoryBy(inCategoryUntagged).Should().BeFalse();
    }

    #endregion

    #region Chaining

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldMatchWhenBothMatch()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(CategoryId);

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeTrue();
    }

    [Fact]
    public void WithStatus_AndWithCategory_Combined_ShouldNotMatchWhenOneFails()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        var builder = new VideoQueryBuilder();
        builder.WithStatus(EnumContentStatus.Draft).WithCategory(Guid.NewGuid());

        Specification<VideoEntity> spec = builder.Build(NoTags, NoCategories)!;
        Func<VideoEntity, bool> predicate = spec.ToExpression().Compile();

        predicate(video).Should().BeFalse();
    }

    #endregion
}
