using _116.Content.Application.Catalog.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.Specifications;

/// <summary>
/// Unit tests for <see cref="PinnedToFeedCategorySpecification"/>.
/// </summary>
public class CategorySpecificationTests
{
    private static bool Matches(PinnedToFeedCategorySpecification spec, CategoryEntity category) =>
        spec.ToExpression().Compile()(category);

    [Fact]
    public void Matches_PinnedActiveCategory()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity pinned = CategoryFactory.CreatePinned(videoType);

        Matches(new PinnedToFeedCategorySpecification(), pinned).Should().BeTrue();
    }

    [Fact]
    public void DoesNotMatch_UnpinnedCategory()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity unpinned = CategoryFactory.Create(videoType);

        Matches(new PinnedToFeedCategorySpecification(), unpinned).Should().BeFalse();
    }

    [Fact]
    public void DoesNotMatch_InactivePinnedCategory()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity pinned = CategoryFactory.CreatePinned(videoType);
        pinned.Deactivate();

        Matches(new PinnedToFeedCategorySpecification(), pinned).Should().BeFalse();
    }

    [Fact]
    public void WithContentTypeFilter_MatchesOnlyThatType()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity pinned = CategoryFactory.CreatePinned(videoType);

        Matches(new PinnedToFeedCategorySpecification(videoType.Id), pinned).Should().BeTrue();
        Matches(new PinnedToFeedCategorySpecification(Guid.NewGuid()), pinned).Should().BeFalse();
    }

    [Fact]
    public void WithNullContentTypeFilter_MatchesAnyType()
    {
        ContentTypeEntity articleType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Article));
        CategoryEntity pinned = CategoryFactory.CreatePinned(articleType);

        Matches(new PinnedToFeedCategorySpecification(contentTypeId: null), pinned).Should().BeTrue();
    }
}
