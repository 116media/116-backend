using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;
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

namespace _116.Unit.Tests.Modules.Identity.Application.Permissions.Admin.Commands.UpdatePermission;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePermissionHandler"/>.
/// </summary>
public class AdminUpdatePermissionHandlerTests
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdatePermissionHandler _handler;

    public AdminUpdatePermissionHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminUpdatePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithAllFieldsUpdated_ShouldUpdatePermissionAndReturnResult()
    {
        // Arrange
        PermissionEntity existingPermission = new PermissionBuilder()
            .WithResourceAction("oldResource", "oldAction")
            .WithDescription("Old description")
            .Build();

        string newResource = "newResource";
        string newAction = "newAction";
        string newDescription = "New description";

        AdminUpdatePermissionCommand command = new(
            PermissionId: existingPermission.Id,
            Resource: newResource,
            Action: newAction,
            Description: newDescription
        );

        _permissionRepositoryMock.SetupGetByIdOrThrow(existingPermission);
        _permissionRepositoryMock.SetupExistsByResourceAndAction(newResource, newAction, exists: false);

        // Act
        AdminUpdatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Resource.Should().Be(newResource);
        result.Permission.Action.Should().Be(newAction);
        result.Permission.Description.Should().Be(newDescription);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithOnlyResourceUpdated_ShouldUpdateOnlyResource()
    {
        // Arrange
        string originalAction = "read";
        string originalDescription = "Original description";
        PermissionEntity existingPermission = new PermissionBuilder()
            .WithResourceAction("oldResource", originalAction)
            .WithDescription(originalDescription)
            .Build();

        string newResource = "newResource";

        AdminUpdatePermissionCommand command = new(
            PermissionId: existingPermission.Id,
            Resource: newResource,
            Action: null,
            Description: null
        );

        _permissionRepositoryMock.SetupGetByIdOrThrow(existingPermission);
        _permissionRepositoryMock.SetupExistsByResourceAndAction(newResource, originalAction, exists: false);

        // Act
        AdminUpdatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Permission.Resource.Should().Be(newResource);
        result.Permission.Action.Should().Be(originalAction);
        result.Permission.Description.Should().Be(originalDescription);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithOnlyDescriptionUpdated_ShouldUpdateOnlyDescription()
    {
        // Arrange
        PermissionEntity existingPermission = new PermissionBuilder()
            .WithResourceAction("users", "read")
            .WithDescription("Old description")
            .Build();

        string newDescription = "New description";

        AdminUpdatePermissionCommand command = new(
            PermissionId: existingPermission.Id,
            Resource: null,
            Action: null,
            Description: newDescription
        );

        _permissionRepositoryMock.SetupGetByIdOrThrow(existingPermission);

        // Act
        AdminUpdatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Permission.Resource.Should().Be("users");
        result.Permission.Action.Should().Be("read");
        result.Permission.Description.Should().Be(newDescription);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region No Changes Cases

    [Fact]
    public async Task Handle_WithNoChanges_ShouldNotCommit()
    {
        // Arrange
        PermissionEntity existingPermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .WithDescription(TestConstants.Permission.ValidDescription)
            .Build();

        AdminUpdatePermissionCommand command = new(
            PermissionId: existingPermission.Id,
            Resource: null,
            Action: null,
            Description: null
        );

        _permissionRepositoryMock.SetupGetByIdOrThrow(existingPermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentPermissionId = Guid.NewGuid();
        AdminUpdatePermissionCommand command = new(
            PermissionId: nonExistentPermissionId,
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNewResourceActionAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity existingPermission = new PermissionBuilder()
            .WithResourceAction("oldResource", "oldAction")
            .Build();

        string duplicateResource = "existingResource";
        string duplicateAction = "existingAction";

        AdminUpdatePermissionCommand command = new(
            PermissionId: existingPermission.Id,
            Resource: duplicateResource,
            Action: duplicateAction,
            Description: null
        );

        _permissionRepositoryMock.SetupGetByIdOrThrow(existingPermission);
        _permissionRepositoryMock.SetupExistsByResourceAndAction(duplicateResource, duplicateAction, exists: true);

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
        PermissionEntity existingPermission = new PermissionBuilder().WithResourceAction("users", "read").Build();

        AdminUpdatePermissionCommand command = new(
            PermissionId: existingPermission.Id,
            Resource: "newResource",
            Action: null,
            Description: null
        );

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetByIdOrThrow(existingPermission);
        _permissionRepositoryMock.SetupExistsByResourceAndAction("newResource", "read", exists: false);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(
            x => x.GetPermissionByIdOrThrowAsync(existingPermission.Id, cts.Token),
            Times.Once
        );
    }

    #endregion
}
