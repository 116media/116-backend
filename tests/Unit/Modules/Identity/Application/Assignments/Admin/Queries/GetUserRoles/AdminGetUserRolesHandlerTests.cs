using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Assignments.Admin.Queries.GetUserRoles;

/// <summary>
/// Unit tests for <see cref="AdminGetUserRolesHandler"/>.
/// </summary>
public class AdminGetUserRolesHandlerTests
{
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly AdminGetUserRolesHandler _handler;

    public AdminGetUserRolesHandlerTests()
    {
        _userRoleRepositoryMock = MockUserRoleRepository.Create();
        _handler = new AdminGetUserRolesHandler(_userRoleRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithUserHavingRoles_ShouldReturnRoles()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminGetUserRolesQuery query = new(UserId: userId);

        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().NotBeNull();
        result.Roles.Should().HaveCount(1);
        result.Roles.First().Name.Should().Be(TestConstants.Role.ValidName);
    }

    [Fact]
    public async Task Handle_WithUserHavingMultipleRoles_ShouldReturnAllRoles()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity adminRole = new RoleBuilder()
            .WithName(TestConstants.Role.AdminName)
            .WithDescription(TestConstants.Role.AdminDescription)
            .AsActive()
            .Build();

        RoleEntity visitorRole = new RoleBuilder()
            .WithName(TestConstants.Role.VisitorName)
            .WithDescription(TestConstants.Role.VisitorDescription)
            .AsActive()
            .Build();

        UserRoleEntity adminUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(adminRole).Build();

        UserRoleEntity visitorUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(visitorRole).Build();

        AdminGetUserRolesQuery query = new(UserId: userId);

        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [adminUserRole, visitorUserRole]);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithUserHavingNoRoles_ShouldReturnEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminGetUserRolesQuery query = new(UserId: userId);

        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().BeEmpty();
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public async Task Handle_WithValidRole_ShouldMapToRoleDto()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithId(roleId)
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminGetUserRolesQuery query = new(UserId: userId);

        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var roleDto = result.Roles.First();
        roleDto.Id.Should().Be(roleId);
        roleDto.Name.Should().Be(TestConstants.Role.ValidName);
        roleDto.Description.Should().Be(TestConstants.Role.ValidDescription);
    }

    [Fact]
    public async Task Handle_WithMultipleRoles_ShouldMapAllRolesToDtos()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity superAdminRole = new RoleBuilder()
            .WithName(TestConstants.Role.SuperAdminName)
            .WithDescription(TestConstants.Role.SuperAdminDescription)
            .AsActive()
            .Build();

        RoleEntity adminRole = new RoleBuilder()
            .WithName(TestConstants.Role.AdminName)
            .WithDescription(TestConstants.Role.AdminDescription)
            .AsActive()
            .Build();

        RoleEntity visitorRole = new RoleBuilder()
            .WithName(TestConstants.Role.VisitorName)
            .WithDescription(TestConstants.Role.VisitorDescription)
            .AsActive()
            .Build();

        UserRoleEntity superAdminUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(superAdminRole).Build();

        UserRoleEntity adminUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(adminRole).Build();

        UserRoleEntity visitorUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(visitorRole).Build();

        AdminGetUserRolesQuery query = new(UserId: userId);

        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [superAdminUserRole, adminUserRole, visitorUserRole]);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().HaveCount(3);
        result.Roles.Select(r => r.Name).Should().Contain(TestConstants.Role.SuperAdminName);
        result.Roles.Select(r => r.Name).Should().Contain(TestConstants.Role.AdminName);
        result.Roles.Select(r => r.Name).Should().Contain(TestConstants.Role.VisitorName);
    }

    #endregion

    #region Edge Cases

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

    [Fact]
    public async Task Handle_WithNewUserId_ShouldQueryCorrectUser()
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

    #endregion
}
