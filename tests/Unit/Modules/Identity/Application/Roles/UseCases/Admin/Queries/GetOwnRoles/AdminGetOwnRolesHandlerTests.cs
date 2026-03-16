using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles;

/// <summary>
/// Unit tests for <see cref="AdminGetOwnRolesHandler"/>.
/// </summary>
public class AdminGetOwnRolesHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly AdminGetOwnRolesHandler _handler;

    public AdminGetOwnRolesHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _handler = new AdminGetOwnRolesHandler(_authRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WithUserHavingRoles_ShouldReturnRolesWithPermissions()
    {
        // Arrange
        UserEntity user = UserFactory.CreateAdmin();
        AdminGetOwnRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        // Act
        AdminGetOwnRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().NotBeNull();
        result.Roles.Should().HaveCount(user.UserRoles.Count);
    }

    [Fact]
    public async Task Handle_WithUserHavingRoles_ShouldMapRolesCorrectly()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateAdmin();
        UserEntity user = UserFactory.CreateWithRole(role);
        AdminGetOwnRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        // Act
        AdminGetOwnRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        RoleWithPermissionsDto roleDto = result.Roles.First();
        roleDto.Id.Should().Be(role.Id);
        roleDto.Name.Should().Be(role.Name);
        roleDto.Description.Should().Be(role.Description);
        roleDto.IsActive.Should().Be(role.IsActive);
    }

    [Fact]
    public async Task Handle_WithUserHavingNoRoles_ShouldReturnEmptyList()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        AdminGetOwnRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        // Act
        AdminGetOwnRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AdminGetOwnRolesQuery query = new(UserId: userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsByIdNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        UserEntity user = UserFactory.CreateAdmin();
        AdminGetOwnRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByIdOrThrow(user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithRoleIncludingPermissions_ShouldMapPermissionsCorrectly()
    {
        // Arrange
        List<PermissionEntity> permissions =
        [
            PermissionFactory.Create("articles", "read"),
            PermissionFactory.Create("videos", "read"),
        ];
        RoleEntity roleWithPermissions = RoleFactory.Create("Admin", permissions);
        UserEntity user = UserFactory.CreateWithRole(roleWithPermissions);

        AdminGetOwnRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);

        // Act
        AdminGetOwnRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        result.Roles.First().Permissions.Should().HaveCount(2);
    }
}
