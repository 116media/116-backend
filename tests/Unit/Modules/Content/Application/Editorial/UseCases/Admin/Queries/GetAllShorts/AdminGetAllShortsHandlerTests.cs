using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts;

/// <summary>
/// Unit tests for <see cref="AdminGetAllShortsHandler"/>.
/// </summary>
public class AdminGetAllShortsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetAllShortsHandler _handler;

    public AdminGetAllShortsHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _userLookupMock = MockUserLookupService.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetAllShortsHandler(
            _shortVideoRepositoryMock.Object,
            _userLookupMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenShortVideosExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ShortVideoEntity> shorts = ShortVideoFactory.CreateMany(3);
        var query = new AdminGetAllShortsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            IsActive: null
        );

        _shortVideoRepositoryMock.SetupGetAllAsync(shorts, shorts.Count);

        // Act
        AdminGetAllShortsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortVideos.Items.Should().HaveCount(shorts.Count);
        result.ShortVideos.Count.Should().Be((long)shorts.Count);
    }

    [Fact]
    public async Task Handle_WhenNoShortVideosExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new AdminGetAllShortsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            IsActive: null
        );

        _shortVideoRepositoryMock.SetupGetAllAsync(new List<ShortVideoEntity>(), 0);

        // Act
        AdminGetAllShortsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideos.Items.Should().BeEmpty();
        result.ShortVideos.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenIsActiveFilterProvided_ShouldReturnFilteredResults()
    {
        // Arrange
        List<ShortVideoEntity> activeShorts = ShortVideoFactory.CreateMany(2);
        var query = new AdminGetAllShortsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            IsActive: true
        );

        _shortVideoRepositoryMock.SetupGetAllAsync(activeShorts, activeShorts.Count);

        // Act
        AdminGetAllShortsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShortVideos.Items.Should().HaveCount(activeShorts.Count);
        result.ShortVideos.Count.Should().Be((long)activeShorts.Count);
    }
}
