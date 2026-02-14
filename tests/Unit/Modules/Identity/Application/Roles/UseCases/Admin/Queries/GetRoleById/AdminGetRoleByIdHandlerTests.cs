using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;

/// <summary>
/// Unit tests for <see cref="AdminGetRoleByIdHandler"/>.
/// </summary>
public class AdminGetRoleByIdHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly AdminGetRoleByIdHandler _handler;

    public AdminGetRoleByIdHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();

        _handler = new AdminGetRoleByIdHandler(_roleRepositoryMock.Object, Mapper);
    }

    #region Success Cases - Basic Retrieval

    [Fact]
    public async Task Handle_WithValidRoleId_ShouldReturnRoleWithPermissions()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Id.Should().Be(role.Id);
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        result.Role.Description.Should().Be(TestConstants.Role.ValidDescription);
    }

    [Fact]
    public async Task Handle_WithRoleWithoutPermissions_ShouldReturnEmptyPermissionsList()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName);

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permissions.Should().BeEmpty();
    }

    #endregion

    #region Success Cases - With Permissions

    [Fact]
    public async Task Handle_WithRoleWithPermissions_ShouldReturnPermissionsList()
    {
        // Arrange
        PermissionEntity permission1 = PermissionFactory.Create("users", "read");

        PermissionEntity permission2 = PermissionFactory.Create("users", "write");

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithPermission(permission1)
            .WithPermission(permission2)
            .Build();

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permissions.Should().HaveCount(2);
        result.Permissions.Should().Contain(p => p.Resource == "users" && p.Action == "read");
        result.Permissions.Should().Contain(p => p.Resource == "users" && p.Action == "write");
    }

    [Fact]
    public async Task Handle_WithRoleWithMultiplePermissions_ShouldMapAllPermissions()
    {
        // Arrange
        List<PermissionEntity> permissions =
        [
            PermissionFactory.Create("articles", "read"),
            PermissionFactory.Create("articles", "create"),
            PermissionFactory.Create("articles", "update"),
        ];

        RoleEntity role = new RoleBuilder().WithName(TestConstants.Role.ValidName).WithPermissions(permissions).Build();

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permissions.Should().HaveCount(3);
        result.Permissions.Should().AllSatisfy(p => p.Resource.Should().Be("articles"));
    }

    #endregion

    #region Failure Cases - Role Not Found

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminGetRoleByIdQuery query = new(RoleId: nonExistentRoleId.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public async Task Handle_WithActiveRole_ShouldMapIsActiveCorrectly()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName);

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Role.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInactiveRole_ShouldMapIsActiveCorrectly()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateInactive();

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Role.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithDeletedRole_ShouldMapIsDeletedCorrectly()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDeleted();

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Role.IsDeleted.Should().BeTrue();
        result.Role.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithPermissions_ShouldMapPermissionDtosCorrectly()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction,
            TestConstants.Permission.ValidDescription
        );

        RoleEntity role = new RoleBuilder().WithName(TestConstants.Role.ValidName).WithPermission(permission).Build();

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        PermissionDto permissionDto = result.Permissions.First();
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
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName);

        AdminGetRoleByIdQuery query = new(RoleId: role.Id.ToString());

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLowercaseGuid_ShouldParseCorrectly()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        RoleEntity role = RoleFactory.CreateWithId(roleId, TestConstants.Role.ValidName);

        AdminGetRoleByIdQuery query = new(RoleId: roleId.ToString().ToLowerInvariant());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Role.Id.Should().Be(roleId);
    }

    [Fact]
    public async Task Handle_WithUppercaseGuid_ShouldParseCorrectly()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        RoleEntity role = RoleFactory.CreateWithId(roleId, TestConstants.Role.ValidName);

        AdminGetRoleByIdQuery query = new(RoleId: roleId.ToString().ToUpperInvariant());

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminGetRoleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Role.Id.Should().Be(roleId);
    }

    #endregion
}
