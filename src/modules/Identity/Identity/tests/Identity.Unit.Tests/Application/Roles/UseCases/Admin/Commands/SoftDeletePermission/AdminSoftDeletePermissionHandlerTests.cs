using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeletePermissionHandler"/>: the deletion call, the commit
/// and the response. The guard is covered by <c>AdminSoftDeletePermissionServiceTests</c>.
/// </summary>
public class AdminSoftDeletePermissionHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminSoftDeletePermissionService> _softDeletePermissionServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminSoftDeletePermissionHandler _handler;

    public AdminSoftDeletePermissionHandlerTests()
    {
        _softDeletePermissionServiceMock = new Mock<IAdminSoftDeletePermissionService>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminSoftDeletePermissionHandler(
            _softDeletePermissionServiceMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    private (PermissionEntity Permission, AdminSoftDeletePermissionCommand Command) ArrangeDeletion()
    {
        PermissionEntity permission = PermissionFactory.CreateDefault();
        permission.SoftDelete(DateTime.UtcNow);

        _softDeletePermissionServiceMock
            .Setup(x => x.SoftDeleteAsync(permission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        return (permission, new AdminSoftDeletePermissionCommand(PermissionId: permission.Id.ToString()));
    }

    [Fact]
    public async Task Handle_WithActivePermission_ShouldReturnTheDeletedPermission()
    {
        // Arrange
        (PermissionEntity permission, AdminSoftDeletePermissionCommand command) = ArrangeDeletion();

        // Act
        AdminSoftDeletePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Permission.Id.Should().Be(permission.Id);
        result.Permission.Resource.Should().Be(permission.Resource);
        result.Permission.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        (_, AdminSoftDeletePermissionCommand command) = ArrangeDeletion();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTheDeletionIsRefused_ShouldNotCommit()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        _softDeletePermissionServiceMock
            .Setup(x => x.SoftDeleteAsync(permissionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(TestErrorsFactory.CreateIdentityI18n().User.PermissionAlreadyDeleted());

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new AdminSoftDeletePermissionCommand(permissionId.ToString()),
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToTheServiceAndUnitOfWork()
    {
        // Arrange
        (PermissionEntity permission, AdminSoftDeletePermissionCommand command) = ArrangeDeletion();
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _softDeletePermissionServiceMock.Verify(x => x.SoftDeleteAsync(permission.Id, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }
}
