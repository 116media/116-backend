using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Admin.Commands.DeactivatePermission;

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
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();

        _handler = new AdminDeactivatePermissionHandler(
            _permissionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WithActivePermission_ShouldDeactivateAndReturnResult()
    {
        // Arrange
        var permission = new PermissionBuilder().Build(); // Active by default
        var command = new AdminDeactivatePermissionCommand(PermissionId: permission.Id);

        _permissionRepositoryMock
            .Setup(x => x.GetPermissionByIdOrThrowAsync(permission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Id.Should().Be(permission.Id);
        permission.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyInactivePermission_ShouldThrowConflictException()
    {
        // Arrange
        var permission = new PermissionBuilder().AsInactive().Build();
        var command = new AdminDeactivatePermissionCommand(PermissionId: permission.Id);

        _permissionRepositoryMock
            .Setup(x => x.GetPermissionByIdOrThrowAsync(permission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var command = new AdminDeactivatePermissionCommand(PermissionId: permissionId);

        _permissionRepositoryMock
            .Setup(x => x.GetPermissionByIdOrThrowAsync(permissionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Permission", permissionId));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
