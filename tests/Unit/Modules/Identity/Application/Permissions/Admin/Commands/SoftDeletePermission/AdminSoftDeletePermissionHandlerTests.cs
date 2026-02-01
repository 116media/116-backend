using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;
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

namespace _116.Unit.Tests.Modules.Identity.Application.Permissions.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeletePermissionHandler"/>.
/// </summary>
public class AdminSoftDeletePermissionHandlerTests
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminSoftDeletePermissionHandler _handler;

    public AdminSoftDeletePermissionHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminSoftDeletePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithActivePermission_ShouldSoftDeleteAndReturnResult()
    {
        // Arrange
        PermissionEntity activePermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        AdminSoftDeletePermissionCommand command = new(PermissionId: activePermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        AdminSoftDeletePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Id.Should().Be(activePermission.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithActivePermission_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        PermissionEntity activePermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        AdminSoftDeletePermissionCommand command = new(PermissionId: activePermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        activePermission.IsDeleted.Should().BeTrue();
        activePermission.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithActivePermission_ShouldAlsoDeactivate()
    {
        // Arrange
        PermissionEntity activePermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        AdminSoftDeletePermissionCommand command = new(PermissionId: activePermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        activePermission.IsActive.Should().BeFalse();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentPermissionId = Guid.NewGuid();
        AdminSoftDeletePermissionCommand command = new(PermissionId: nonExistentPermissionId);

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyDeleted_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity deletedPermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsDeleted()
            .Build();

        AdminSoftDeletePermissionCommand command = new(PermissionId: deletedPermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyDeleted_ShouldNotCommit()
    {
        // Arrange
        PermissionEntity deletedPermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsDeleted()
            .Build();

        AdminSoftDeletePermissionCommand command = new(PermissionId: deletedPermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

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

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PermissionEntity activePermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        AdminSoftDeletePermissionCommand command = new(PermissionId: activePermission.Id);

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(
            x => x.GetPermissionByIdOrThrowAsync(activePermission.Id, cts.Token),
            Times.Once
        );
    }

    #endregion
}
