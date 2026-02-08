using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Admin.Commands.ActivatePermission;

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
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();

        _handler = new AdminActivatePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WithInactivePermission_ShouldActivateAndReturnResult()
    {
        // Arrange
        var permission = new PermissionBuilder().AsInactive().Build();
        var command = new AdminActivatePermissionCommand(PermissionId: permission.Id);

        _permissionRepositoryMock
            .Setup(x => x.GetPermissionByIdOrThrowAsync(permission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Id.Should().Be(permission.Id);
        permission.IsActive.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyActivePermission_ShouldThrowConflictException()
    {
        // Arrange
        var permission = new PermissionBuilder().Build(); // Active by default
        var command = new AdminActivatePermissionCommand(PermissionId: permission.Id);

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
        var command = new AdminActivatePermissionCommand(PermissionId: permissionId);

        _permissionRepositoryMock
            .Setup(x => x.GetPermissionByIdOrThrowAsync(permissionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Permission", permissionId));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
