using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePermissionHandler"/>.
/// </summary>
public class AdminDeactivatePermissionHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeactivatePermissionHandler _handler;

    public AdminDeactivatePermissionHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminDeactivatePermissionHandler(
            _permissionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithActivePermission_ShouldDeactivateAndReturnResult()
    {
        // Arrange
        PermissionEntity activePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminDeactivatePermissionCommand command = new(PermissionId: activePermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        AdminDeactivatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Id.Should().Be(activePermission.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithActivePermission_ShouldSetIsActiveToFalse()
    {
        // Arrange
        PermissionEntity activePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminDeactivatePermissionCommand command = new(PermissionId: activePermission.Id);

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
        AdminDeactivatePermissionCommand command = new(PermissionId: nonExistentPermissionId);

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyInactive_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity inactivePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        inactivePermission.Deactivate();

        AdminDeactivatePermissionCommand command = new(PermissionId: inactivePermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyInactive_ShouldNotCommit()
    {
        // Arrange
        PermissionEntity inactivePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        inactivePermission.Deactivate();

        AdminDeactivatePermissionCommand command = new(PermissionId: inactivePermission.Id);

        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

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
        PermissionEntity activePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminDeactivatePermissionCommand command = new(PermissionId: activePermission.Id);

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
