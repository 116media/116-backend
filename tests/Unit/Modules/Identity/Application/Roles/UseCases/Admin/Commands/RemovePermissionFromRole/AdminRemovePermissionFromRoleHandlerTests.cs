using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Unit tests for <see cref="AdminRemovePermissionFromRoleHandler"/>.
/// </summary>
public class AdminRemovePermissionFromRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemovePermissionFromRoleHandler _handler;

    public AdminRemovePermissionFromRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _rolePermissionRepositoryMock = MockRolePermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminRemovePermissionFromRoleHandler(
            _roleRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRoleAndPermission_ShouldRemoveAndReturnResult()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        RolePermissionEntity rolePermission = RolePermissionFactory.Create(role.Id, permission.Id);

        AdminRemovePermissionFromRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermission(rolePermission);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminRemovePermissionFromRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Id.Should().Be(role.Id);
        _rolePermissionRepositoryMock.VerifyDeleteCalled(rolePermission);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteRolePermissionAssociation()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        RolePermissionEntity rolePermission = RolePermissionFactory.Create(role.Id, permission.Id);

        AdminRemovePermissionFromRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermission(rolePermission);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _rolePermissionRepositoryMock.VerifyDeleteCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentRoleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        AdminRemovePermissionFromRoleCommand command = new(
            RoleId: nonExistentRoleId.ToString(),
            PermissionId: permissionId
        );

        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotAssigned_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        var permissionId = Guid.NewGuid();
        AdminRemovePermissionFromRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permissionId);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermissionReturnsNull(role.Id, permissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotAssigned_ShouldNotCommit()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        var permissionId = Guid.NewGuid();
        AdminRemovePermissionFromRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permissionId);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermissionReturnsNull(role.Id, permissionId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (BadRequestException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepositories()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        RolePermissionEntity rolePermission = RolePermissionFactory.Create(role.Id, permission.Id);

        AdminRemovePermissionFromRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermission(rolePermission);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReloadRoleWithPermissionsAfterRemoval()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        RolePermissionEntity rolePermission = RolePermissionFactory.Create(role.Id, permission.Id);

        AdminRemovePermissionFromRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermission(rolePermission);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
            x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion
}
