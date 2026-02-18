using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPermissionsHandler"/>.
/// </summary>
public class AdminGetAllPermissionsHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly AdminGetAllPermissionsHandler _handler;

    public AdminGetAllPermissionsHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();

        _handler = new AdminGetAllPermissionsHandler(_permissionRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithPermissionsInRepository_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<PermissionEntity> permissions =
        [
            PermissionFactory.Create("users", "read"),
            PermissionFactory.Create("users", "write"),
            PermissionFactory.Create("articles", "read"),
        ];

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest);

        _permissionRepositoryMock.SetupGetAllWithPagination(permissions, totalCount: 3);

        // Act
        AdminGetAllPermissionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permissions.Should().NotBeNull();
        result.Permissions.Items.Should().HaveCount(3);
        result.Permissions.Count.Should().Be(3);
        result.Permissions.PageIndex.Should().Be(0);
        result.Permissions.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithEmptyRepository_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest);

        _permissionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        AdminGetAllPermissionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permissions.Items.Should().BeEmpty();
        result.Permissions.Count.Should().Be(0);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task Handle_WithDifferentPageIndex_ShouldPassCorrectPageToRepository()
    {
        // Arrange
        int pageIndex = 2;
        int pageSize = 5;

        PaginatedRequest paginatedRequest = new(PageIndex: pageIndex, PageSize: pageSize);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest);

        _permissionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Repository receives 1-based page number
        _permissionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    pageIndex + 1,
                    pageSize,
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithTotalCountGreaterThanPageSize_ShouldReturnCorrectTotalCount()
    {
        // Arrange
        int totalCount = 100;
        List<PermissionEntity> permissions = [PermissionFactory.Create("users", "read")];

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest);

        _permissionRepositoryMock.SetupGetAllWithPagination(permissions, totalCount: totalCount);

        // Act
        AdminGetAllPermissionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permissions.Count.Should().Be(totalCount);
        result.Permissions.Items.Should().ContainSingle();
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task Handle_WithSearchFilter_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = "users";

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest, Search: searchTerm);

        _permissionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _permissionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    searchTerm,
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithIsActiveFilter_ShouldPassIsActiveToRepository()
    {
        // Arrange
        bool isActive = true;

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest, IsActive: isActive);

        _permissionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _permissionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    isActive,
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldPassAllFiltersToRepository()
    {
        // Arrange
        string searchTerm = "users";
        bool isActive = true;
        bool isDeleted = false;

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(
            PaginatedRequest: paginatedRequest,
            Search: searchTerm,
            IsActive: isActive,
            IsDeleted: isDeleted
        );

        _permissionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _permissionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    searchTerm,
                    isActive,
                    isDeleted,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public async Task Handle_WithPermissions_ShouldMapToPermissionDtos()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction,
            TestConstants.Permission.ValidDescription
        );

        List<PermissionEntity> permissions = [permission];

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest);

        _permissionRepositoryMock.SetupGetAllWithPagination(permissions, totalCount: 1);

        // Act
        AdminGetAllPermissionsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        PermissionDto permissionDto = result.Permissions.Items.First();
        permissionDto.Id.Should().Be(permission.Id);
        permissionDto.Resource.Should().Be(TestConstants.Permission.ValidResource);
        permissionDto.Action.Should().Be(TestConstants.Permission.ValidAction);
        permissionDto.Description.Should().Be(TestConstants.Permission.ValidDescription);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllPermissionsQuery query = new(PaginatedRequest: paginatedRequest);

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    cts.Token
                ),
            Times.Once
        );
    }

    #endregion
}
