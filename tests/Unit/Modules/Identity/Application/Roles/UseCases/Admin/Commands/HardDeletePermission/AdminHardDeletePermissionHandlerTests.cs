using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission;

/// <summary>
/// Unit tests for <see cref="AdminHardDeletePermissionHandler"/>.
/// </summary>
public class AdminHardDeletePermissionHandlerTests
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminHardDeletePermissionHandler _handler;

    public AdminHardDeletePermissionHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminHardDeletePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidPermission_ShouldHardDeleteAndReturnSuccess()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .Build();

        AdminHardDeletePermissionCommand command = new(PermissionId: permission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminHardDeletePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _permissionRepositoryMock.VerifyDeleteCalled(permission);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithSoftDeletedPermission_ShouldHardDelete()
    {
        // Arrange
        PermissionEntity deletedPermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsDeleted()
            .Build();

        AdminHardDeletePermissionCommand command = new(PermissionId: deletedPermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        AdminHardDeletePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _permissionRepositoryMock.VerifyDeleteCalled(deletedPermission);
    }

    [Fact]
    public async Task Handle_WithInactivePermission_ShouldHardDelete()
    {
        // Arrange
        PermissionEntity inactivePermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsInactive()
            .Build();

        AdminHardDeletePermissionCommand command = new(PermissionId: inactivePermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        AdminHardDeletePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _permissionRepositoryMock.VerifyDeleteCalled(inactivePermission);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentPermissionId = Guid.NewGuid();
        AdminHardDeletePermissionCommand command = new(PermissionId: nonExistentPermissionId);

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldNotCommit()
    {
        // Arrange
        Guid nonExistentPermissionId = Guid.NewGuid();
        AdminHardDeletePermissionCommand command = new(PermissionId: nonExistentPermissionId);

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .Build();

        AdminHardDeletePermissionCommand command = new(PermissionId: permission.Id);

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(x => x.GetPermissionByIdOrThrowAsync(permission.Id, cts.Token), Times.Once);
    }

    #endregion
}
