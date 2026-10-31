using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Fakes;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Search;

/// <summary>
/// Unit tests for <see cref="SearchProjection" />, which routes a projection request to the
/// projector registered for the type and ignores types that have none.
/// </summary>
public class SearchProjectionTests
{
    [Fact]
    public async Task ProjectAsync_ForARegisteredType_ShouldRouteToThatProjectorOnly()
    {
        // Arrange
        FakeSearchDocumentProjector articles = new(EnumCoreContentType.Article);
        FakeSearchDocumentProjector videos = new(EnumCoreContentType.Video);
        SearchProjection projection = new([articles, videos]);
        Guid id = Guid.NewGuid();

        // Act
        await projection.ProjectAsync(EnumCoreContentType.Article, id);

        // Assert
        articles.Projected.Should().Equal(id);
        videos.Projected.Should().BeEmpty();
    }

    [Fact]
    public async Task ProjectAsync_ForATypeWithNoProjector_ShouldDoNothing()
    {
        // Arrange
        FakeSearchDocumentProjector articles = new(EnumCoreContentType.Article);
        SearchProjection projection = new([articles]);

        // Act
        Func<Task> act = async () => await projection.ProjectAsync(EnumCoreContentType.Lyrics, Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync("a type that is not searchable yet is a no-op");
        articles.Projected.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveAsync_ForATypeWithNoProjector_ShouldDoNothing()
    {
        // Arrange
        SearchProjection projection = new([]);

        // Act
        Func<Task> act = async () => await projection.RemoveAsync(EnumCoreContentType.Article, Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RebuildAllAsync_ShouldRebuildEveryRegisteredProjectorOnce()
    {
        // Arrange
        FakeSearchDocumentProjector articles = new(EnumCoreContentType.Article);
        FakeSearchDocumentProjector videos = new(EnumCoreContentType.Video);
        FakeSearchDocumentProjector shorts = new(EnumCoreContentType.Short);
        SearchProjection projection = new([articles, videos, shorts]);

        // Act
        IReadOnlyDictionary<EnumCoreContentType, int> written = await projection.RebuildAllAsync();

        // Assert
        articles.Rebuilds.Should().Be(1);
        videos.Rebuilds.Should().Be(1);
        shorts.Rebuilds.Should().Be(1);
        written
            .Keys.Should()
            .BeEquivalentTo([EnumCoreContentType.Article, EnumCoreContentType.Video, EnumCoreContentType.Short]);
    }
}
