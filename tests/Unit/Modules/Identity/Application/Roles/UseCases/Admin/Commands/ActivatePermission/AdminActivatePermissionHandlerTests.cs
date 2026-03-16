using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;
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

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;

/// <summary>
/// Unit tests for <see cref="AdminActivatePermissionHandler"/>.
/// </summary>
public class AdminActivatePermissionHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminActivatePermissionHandler _handler;

    public AdminActivatePermissionHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminActivatePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithInactivePermission_ShouldActivateAndReturnResult()
    {
        // Arrange
        PermissionEntity inactivePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        inactivePermission.Deactivate();

        AdminActivatePermissionCommand command = new(PermissionId: inactivePermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        AdminActivatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Id.Should().Be(inactivePermission.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithInactivePermission_ShouldSetIsActiveToTrue()
    {
        // Arrange
        PermissionEntity inactivePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        inactivePermission.Deactivate();

        AdminActivatePermissionCommand command = new(PermissionId: inactivePermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        inactivePermission.IsActive.Should().BeTrue();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentPermissionId = Guid.NewGuid();
        AdminActivatePermissionCommand command = new(PermissionId: nonExistentPermissionId.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyActive_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity activePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminActivatePermissionCommand command = new(PermissionId: activePermission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(activePermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyActive_ShouldNotCommit()
    {
        // Arrange
        PermissionEntity activePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminActivatePermissionCommand command = new(PermissionId: activePermission.Id.ToString());

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

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PermissionEntity inactivePermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        inactivePermission.Deactivate();

        AdminActivatePermissionCommand command = new(PermissionId: inactivePermission.Id.ToString());

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(
            x => x.GetPermissionByIdOrThrowAsync(inactivePermission.Id, cts.Token),
            Times.Once
        );
    }

    #endregion
}
