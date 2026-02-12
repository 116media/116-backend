using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Helpers;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;

/// <summary>
/// Unit tests for <see cref="AdminGetUserRolesHandler"/>.
/// </summary>
public class AdminGetUserRolesHandlerTests : BaseHandlerTest
{
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly AdminGetUserRolesHandler _handler;

    public AdminGetUserRolesHandlerTests()
    {
        _userRoleRepositoryMock = MockUserRoleRepository.Create();

        _handler = new AdminGetUserRolesHandler(_userRoleRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithUserRoles_ShouldReturnRoleDtos()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminGetUserRolesQuery query = new(UserId: userId);

        RoleEntity role1 = RoleFactory.Create("Admin", "Administrator role");
        RoleEntity role2 = RoleFactory.Create("User", "Regular user role");

        List<UserRoleEntity> userRoles =
        [
            UserRoleFactory.CreateWithRole(userId, role1),
            UserRoleFactory.CreateWithRole(userId, role2),
        ];

        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, userRoles);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoRoles_ShouldReturnEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminGetUserRolesQuery query = new(UserId: userId);

        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminGetUserRolesQuery query = new(UserId: userId);

        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.Verify(
            x => x.GetUserRolesWithRoleAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldMapRolesToDtos()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminGetUserRolesQuery query = new(UserId: userId);

        RoleEntity role = RoleFactory.Create("TestRole", "Test role description");
        List<UserRoleEntity> userRoles = [UserRoleFactory.CreateWithRole(userId, role)];

        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, userRoles);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        result.Roles.First().Name.Should().Be("TestRole");
        result.Roles.First().Description.Should().Be("Test role description");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminGetUserRolesQuery query = new(UserId: userId);
        using CancellationTokenSource cts = new();

        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _userRoleRepositoryMock.Verify(x => x.GetUserRolesWithRoleAsync(userId, cts.Token), Times.Once);
    }

    #endregion
}
