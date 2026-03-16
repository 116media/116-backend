using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;
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

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;

/// <summary>
/// Unit tests for <see cref="AdminRestorePermissionHandler"/>.
/// </summary>
public class AdminRestorePermissionHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminRestorePermissionHandler _handler;

    public AdminRestorePermissionHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminRestorePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithDeletedPermission_ShouldRestoreAndReturnResult()
    {
        // Arrange
        PermissionEntity deletedPermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        deletedPermission.SoftDelete();

        AdminRestorePermissionCommand command = new(PermissionId: deletedPermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        AdminRestorePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Id.Should().Be(deletedPermission.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithDeletedPermission_ShouldSetIsDeletedToFalse()
    {
        // Arrange
        PermissionEntity deletedPermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        deletedPermission.SoftDelete();

        AdminRestorePermissionCommand command = new(PermissionId: deletedPermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        deletedPermission.IsDeleted.Should().BeFalse();
        deletedPermission.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithDeletedPermission_ShouldNotAutomaticallyActivate()
    {
        // Arrange
        PermissionEntity deletedPermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        deletedPermission.SoftDelete();

        AdminRestorePermissionCommand command = new(PermissionId: deletedPermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        deletedPermission.IsDeleted.Should().BeFalse();
        deletedPermission.IsActive.Should().BeFalse();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentPermissionId = Guid.NewGuid();
        AdminRestorePermissionCommand command = new(PermissionId: nonExistentPermissionId.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotDeleted_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity activePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminRestorePermissionCommand command = new(PermissionId: activePermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotDeleted_ShouldNotCommit()
    {
        // Arrange
        PermissionEntity activePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminRestorePermissionCommand command = new(PermissionId: activePermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WithInactiveNotDeletedPermission_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity inactivePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        inactivePermission.Deactivate();

        AdminRestorePermissionCommand command = new(PermissionId: inactivePermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PermissionEntity deletedPermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        deletedPermission.SoftDelete();

        AdminRestorePermissionCommand command = new(PermissionId: deletedPermission.Id.ToString());

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(
            x => x.GetPermissionByIdOrThrowAsync(deletedPermission.Id, cts.Token),
            Times.Once
        );
    }

    #endregion
}
