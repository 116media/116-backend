using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages;

/// <summary>
/// Unit tests for <see cref="GetAllPackagesHandler"/>.
/// </summary>
public class GetAllPackagesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly GetAllPackagesHandler _handler;

    public GetAllPackagesHandlerTests()
    {
        _packageRepositoryMock = MockPackageRepository.Create();
        _handler = new GetAllPackagesHandler(_packageRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithMultiplePackages_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<PackageEntity> packages = PackageFactory.CreateMany(3);
        int totalCount = 3;

        _packageRepositoryMock.SetupGetAllAsync(packages, totalCount);

        var query = new GetAllPackagesQuery(
            PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10),
            IsActive: null
        );

        // Act
        GetAllPackagesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Packages.Should().NotBeNull();
        result.Packages.Items.Should().HaveCount(3);
        result.Packages.Count.Should().Be(totalCount);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _packageRepositoryMock.SetupGetAllAsync(new List<PackageEntity>(), 0);

        var query = new GetAllPackagesQuery(
            PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10),
            IsActive: null
        );

        // Act
        GetAllPackagesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Packages.Items.Should().BeEmpty();
        result.Packages.Count.Should().Be(0);
    }

    #endregion
}
