using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Unit tests for <see cref="AdminAssignPermissionToRoleHandler"/>.
/// </summary>
public class AdminAssignPermissionToRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly IdentityI18n _userErrors;
    private readonly AdminAssignPermissionToRoleHandler _handler;

    public AdminAssignPermissionToRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _userErrors = TestErrorsFactory.CreateIdentityI18n();

        _handler = new AdminAssignPermissionToRoleHandler(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            _userErrors
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRoleAndPermission_ShouldAssignAndReturnResult()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminAssignPermissionToRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Id.Should().Be(role.Id);
        role.HasPermission(permission.Id).Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionForRoleUsersAsync(role.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldGrantThroughTheRoleAggregate()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.RolePermissions.Should().ContainSingle(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentRoleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        AdminAssignPermissionToRoleCommand command = new(
            RoleId: nonExistentRoleId.ToString(),
            PermissionId: permissionId
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        var nonExistentPermissionId = Guid.NewGuid();
        AdminAssignPermissionToRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: nonExistentPermissionId
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity inactiveRole = RoleFactory.CreateInactive();

        AdminAssignPermissionToRoleCommand command = new(
            RoleId: inactiveRole.Id.ToString(),
            PermissionId: permission.Id
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(inactiveRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity deletedRole = RoleFactory.CreateDeleted();

        AdminAssignPermissionToRoleCommand command = new(
            RoleId: deletedRole.Id.ToString(),
            PermissionId: permission.Id
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(deletedRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity inactivePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        inactivePermission.Deactivate();

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminAssignPermissionToRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: inactivePermission.Id
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity deletedPermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        deletedPermission.SoftDelete();

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminAssignPermissionToRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: deletedPermission.Id
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyAssigned_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);
        role.GrantPermission(permission.Id);

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyAssigned_ShouldNotCommit()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);
        role.GrantPermission(permission.Id);

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionForRoleUsersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
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

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id.ToString(), PermissionId: permission.Id);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(
            x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, cts.Token),
            Times.AtLeastOnce
        );
    }

    #endregion
}
