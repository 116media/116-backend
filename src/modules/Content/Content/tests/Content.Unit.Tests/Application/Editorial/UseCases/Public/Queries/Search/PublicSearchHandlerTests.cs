using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.Search;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Unit tests for <see cref="PublicSearchHandler" />. The handler reads one page from the
/// catalog and reports the scoped count, so these cover trimming, the paging it passes down
/// and the count it selects; ranking and the depth cap belong to the catalog.
/// </summary>
public class PublicSearchHandlerTests
{
    /// <summary>
    /// Counts for a query no provider answered.
    /// </summary>
    private static SearchCountsDto NoCounts => new(new Dictionary<EnumSearchScope, int>());

    private readonly Mock<ISearchCatalog> _catalogMock = new();
    private readonly Mock<ISearchHitDtoService> _searchHitDtoServiceMock = new();
    private readonly PublicSearchHandler _handler;

    /// <summary>
    /// Wires an empty catalog and a display-field service that returns its input untouched.
    /// </summary>
    public PublicSearchHandlerTests()
    {
        _catalogMock
            .Setup(catalog =>
                catalog.SearchAsync(
                    It.IsAny<EnumSearchScope>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        _catalogMock
            .Setup(catalog => catalog.CountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NoCounts);

        _searchHitDtoServiceMock
            .Setup(service =>
                service.ResolveDisplayFieldsAsync(
                    It.IsAny<IReadOnlyList<SearchHitDto>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((IReadOnlyList<SearchHitDto> hits, CancellationToken _) => hits);

        _handler = new PublicSearchHandler(_catalogMock.Object, _searchHitDtoServiceMock.Object);
    }

    private static SearchCountsDto Counts(int articles, int videos, int shorts) =>
        new(
            new Dictionary<EnumSearchScope, int>
            {
                [EnumSearchScope.Articles] = articles,
                [EnumSearchScope.Videos] = videos,
                [EnumSearchScope.Shorts] = shorts,
            }
        );

    [Fact]
    public async Task Handle_WithSurroundingWhitespace_ShouldSearchAndReportTheTrimmedTerm()
    {
        // Act
        PublicSearchResult result = await _handler.Handle(
            new PublicSearchQuery(new PaginatedRequest(0, 20), "  rumba  "),
            CancellationToken.None
        );

        // Assert
        result.Query.Should().Be("rumba");
        _catalogMock.Verify(
            catalog =>
                catalog.SearchAsync(
                    It.IsAny<EnumSearchScope>(),
                    "rumba",
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _catalogMock.Verify(catalog => catalog.CountAsync("rumba", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAPagedRequest_ShouldPassTheSkipAndTakeForThatPage()
    {
        // Act
        await _handler.Handle(new PublicSearchQuery(new PaginatedRequest(2, 10), "rumba"), CancellationToken.None);

        // Assert
        _catalogMock.Verify(
            catalog => catalog.SearchAsync(EnumSearchScope.All, "rumba", 20, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithAnOverflowingPageIndex_ShouldPassAClampedSkip()
    {
        // Act
        PublicSearchResult result = await _handler.Handle(
            new PublicSearchQuery(new PaginatedRequest(int.MaxValue, 100), "rumba"),
            CancellationToken.None
        );

        // Assert
        result.Results.PageIndex.Should().Be(PaginatedRequest.MaxPageIndex);
        _catalogMock.Verify(
            catalog =>
                catalog.SearchAsync(
                    It.IsAny<EnumSearchScope>(),
                    It.IsAny<string>(),
                    It.Is<int>(skip => skip > 0),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData(EnumSearchScope.Articles, 5)]
    [InlineData(EnumSearchScope.Videos, 7)]
    [InlineData(EnumSearchScope.Shorts, 3)]
    [InlineData(EnumSearchScope.All, 15)]
    public async Task Handle_ForAScope_ShouldReportThatScopesCount(EnumSearchScope scope, int expected)
    {
        // Arrange
        _catalogMock
            .Setup(catalog => catalog.CountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Counts(articles: 5, videos: 7, shorts: 3));

        // Act
        PublicSearchResult result = await _handler.Handle(
            new PublicSearchQuery(new PaginatedRequest(0, 20), "rumba", scope),
            CancellationToken.None
        );

        // Assert
        result.Results.Count.Should().Be(expected);
        result.Counts.All.Should().Be(15);
        result.Scope.Should().Be(scope);
    }

    [Fact]
    public async Task Handle_ForEveryHit_ShouldResolveTheDisplayFields()
    {
        // Act
        await _handler.Handle(new PublicSearchQuery(new PaginatedRequest(0, 20), "rumba"), CancellationToken.None);

        // Assert
        _searchHitDtoServiceMock.Verify(
            service =>
                service.ResolveDisplayFieldsAsync(
                    It.IsAny<IReadOnlyList<SearchHitDto>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNoHits_ShouldReturnAnEmptyPageAndZeroCounts()
    {
        // Act
        PublicSearchResult result = await _handler.Handle(
            new PublicSearchQuery(new PaginatedRequest(0, 20), "rumba"),
            CancellationToken.None
        );

        // Assert
        result.Results.Items.Should().BeEmpty();
        result.Counts.All.Should().Be(0);
    }
}
