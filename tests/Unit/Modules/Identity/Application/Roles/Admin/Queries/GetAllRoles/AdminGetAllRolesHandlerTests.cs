using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Admin.Queries.GetAllRoles;

/// <summary>
/// Unit tests for <see cref="AdminGetAllRolesHandler"/>.
/// </summary>
public class AdminGetAllRolesHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly AdminGetAllRolesHandler _handler;

    public AdminGetAllRolesHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();

        _handler = new AdminGetAllRolesHandler(_roleRepositoryMock.Object, Mapper);
    }

    #region Success Cases - Basic Retrieval

    [Fact]
    public async Task Handle_WithRolesInRepository_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<RoleEntity> roles =
        [
            new RoleBuilder().WithName("Role1").Build(),
            new RoleBuilder().WithName("Role2").Build(),
            new RoleBuilder().WithName("Role3").Build(),
        ];

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest);

        _roleRepositoryMock.SetupGetAllWithPagination(roles, totalCount: 3);

        // Act
        AdminGetAllRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().NotBeNull();
        result.Roles.Items.Should().HaveCount(3);
        result.Roles.Count.Should().Be(3);
        result.Roles.PageIndex.Should().Be(0);
        result.Roles.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithEmptyRepository_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest);

        _roleRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        AdminGetAllRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Items.Should().BeEmpty();
        result.Roles.Count.Should().Be(0);
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
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest);

        _roleRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Repository receives 1-based page number
        _roleRepositoryMock.Verify(
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
    public async Task Handle_WithCustomPageSize_ShouldReturnCorrectPageSize()
    {
        // Arrange
        int pageSize = 25;
        List<RoleEntity> roles = [new RoleBuilder().WithName("Role1").Build()];

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: pageSize);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest);

        _roleRepositoryMock.SetupGetAllWithPagination(roles, totalCount: 1);

        // Act
        AdminGetAllRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task Handle_WithTotalCountGreaterThanPageSize_ShouldReturnCorrectTotalCount()
    {
        // Arrange
        int totalCount = 100;
        List<RoleEntity> roles = [new RoleBuilder().WithName("Role1").Build()];

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest);

        _roleRepositoryMock.SetupGetAllWithPagination(roles, totalCount: totalCount);

        // Act
        AdminGetAllRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Count.Should().Be(totalCount);
        result.Roles.Items.Should().HaveCount(1);
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task Handle_WithSearchFilter_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = "Admin";

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest, Search: searchTerm);

        _roleRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
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
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest, IsActive: isActive);

        _roleRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
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
    public async Task Handle_WithIsDeletedFilter_ShouldPassIsDeletedToRepository()
    {
        // Arrange
        bool isDeleted = true;

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest, IsDeleted: isDeleted);

        _roleRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
            x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    isDeleted,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldPassAllFiltersToRepository()
    {
        // Arrange
        string searchTerm = "Admin";
        bool isActive = true;
        bool isDeleted = false;

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(
            PaginatedRequest: paginatedRequest,
            Search: searchTerm,
            IsActive: isActive,
            IsDeleted: isDeleted
        );

        _roleRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
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
    public async Task Handle_WithRoles_ShouldMapToRoleDtos()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .Build();

        List<RoleEntity> roles = [role];

        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest);

        _roleRepositoryMock.SetupGetAllWithPagination(roles, totalCount: 1);

        // Act
        AdminGetAllRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var roleDto = result.Roles.Items.First();
        roleDto.Id.Should().Be(role.Id);
        roleDto.Name.Should().Be(TestConstants.Role.ValidName);
        roleDto.Description.Should().Be(TestConstants.Role.ValidDescription);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PaginatedRequest paginatedRequest = new(PageIndex: 0, PageSize: 10);
        AdminGetAllRolesQuery query = new(PaginatedRequest: paginatedRequest);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetAllWithPaginationEmpty();

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(
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
