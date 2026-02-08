using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Admin.Commands.CreatePermission;

/// <summary>
/// Unit tests for <see cref="AdminCreatePermissionHandler"/>.
/// </summary>
public class AdminCreatePermissionHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreatePermissionHandler _handler;

    public AdminCreatePermissionHandlerTests()
    {
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();

        _handler = new AdminCreatePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Handle Tests

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePermission()
    {
        // Arrange
        var command = new AdminCreatePermissionCommand(
            Resource: "article",
            Action: "read",
            Description: "Permission to read articles"
        );

        _permissionRepositoryMock
            .Setup(x =>
                x.ExistsByResourceAndActionAsync(command.Resource, command.Action, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        PermissionEntity? capturedPermission = null;
        _permissionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PermissionEntity>(), It.IsAny<CancellationToken>()))
            .Callback<PermissionEntity, CancellationToken>((p, _) => capturedPermission = p)
            .Returns(Task.CompletedTask);

        // Act
        AdminCreatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Resource.Should().Be(command.Resource);
        result.Permission.Action.Should().Be(command.Action);
        result.Permission.Description.Should().Be(command.Description);

        capturedPermission.Should().NotBeNull();
        capturedPermission!.Resource.Should().Be(command.Resource);
        capturedPermission.Action.Should().Be(command.Action);
        capturedPermission.Description.Should().Be(command.Description);

        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new AdminCreatePermissionCommand(
            Resource: "article",
            Action: "read",
            Description: "Permission to read articles"
        );

        _permissionRepositoryMock
            .Setup(x =>
                x.ExistsByResourceAndActionAsync(command.Resource, command.Action, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _permissionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PermissionEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var command = new AdminCreatePermissionCommand(
            Resource: "user",
            Action: "delete",
            Description: "Permission to delete users"
        );

        _permissionRepositoryMock
            .Setup(x =>
                x.ExistsByResourceAndActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _permissionRepositoryMock.Verify(
            x => x.ExistsByResourceAndActionAsync(command.Resource, command.Action, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewGuidForPermissionId()
    {
        // Arrange
        var command = new AdminCreatePermissionCommand(
            Resource: "comment",
            Action: "create",
            Description: "Permission to create comments"
        );

        _permissionRepositoryMock
            .Setup(x =>
                x.ExistsByResourceAndActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        PermissionEntity? capturedPermission = null;
        _permissionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PermissionEntity>(), It.IsAny<CancellationToken>()))
            .Callback<PermissionEntity, CancellationToken>((p, _) => capturedPermission = p)
            .Returns(Task.CompletedTask);

        // Act
        AdminCreatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Permission.Id.Should().NotBe(Guid.Empty);
        capturedPermission!.Id.Should().NotBe(Guid.Empty);
        result.Permission.Id.Should().Be(capturedPermission.Id);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        var command = new AdminCreatePermissionCommand(
            Resource: "file",
            Action: "upload",
            Description: "Permission to upload files"
        );

        _permissionRepositoryMock
            .Setup(x =>
                x.ExistsByResourceAndActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var command = new AdminCreatePermissionCommand(
            Resource: "report",
            Action: "export",
            Description: "Permission to export reports"
        );

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _permissionRepositoryMock
            .Setup(x =>
                x.ExistsByResourceAndActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _permissionRepositoryMock.Verify(
            x => x.ExistsByResourceAndActionAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken),
            Times.Once
        );
        _permissionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PermissionEntity>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
