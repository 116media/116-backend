using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Builders.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Unit tests for <see cref="AdminRemovePermissionFromRoleHandler"/>: the revocation call, the
/// commit, the token bump and the response. The gates are covered by
/// <c>AdminRemovePermissionFromRoleServiceTests</c>.
/// </summary>
public class AdminRemovePermissionFromRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminRemovePermissionFromRoleService> _removePermissionServiceMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemovePermissionFromRoleHandler _handler;

    public AdminRemovePermissionFromRoleHandlerTests()
    {
        _removePermissionServiceMock = new Mock<IAdminRemovePermissionFromRoleService>();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminRemovePermissionFromRoleHandler(
            _removePermissionServiceMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    private (RoleEntity Role, Guid PermissionId, AdminRemovePermissionFromRoleCommand Command) ArrangeRevoke()
    {
        PermissionEntity remaining = PermissionFactory.CreateDefault();
        var permissionId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder().WithPermissions([remaining]).Build();

        _removePermissionServiceMock
            .Setup(x => x.RevokeAsync(role.Id, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        return (
            role,
            permissionId,
            new AdminRemovePermissionFromRoleCommand(role.Id.ToString(), permissionId.ToString())
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnTheRoleWithItsRemainingPermissions()
    {
        // Arrange
        (RoleEntity role, _, AdminRemovePermissionFromRoleCommand command) = ArrangeRevoke();

        // Act
        AdminRemovePermissionFromRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Role.Id.Should().Be(role.Id);
        result.Role.Permissions.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        (_, _, AdminRemovePermissionFromRoleCommand command) = ArrangeRevoke();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldBumpTheRoleHoldersTokenVersionAfterCommitting()
    {
        // Arrange
        (RoleEntity role, _, AdminRemovePermissionFromRoleCommand command) = ArrangeRevoke();

        var callOrder = new List<string>();
        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("commit"))
            .ReturnsAsync(1);
        _tokenStateRepositoryMock
            .Setup(x => x.BumpTokenVersionForRoleUsersAsync(role.Id, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("bump"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("commit", "bump");
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenTheRevocationIsRefused_ShouldNotCommitOrBump()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        _removePermissionServiceMock
            .Setup(x => x.RevokeAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(TestErrorsFactory.CreateIdentityI18n().User.PermissionNotAssignedToRole());

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new AdminRemovePermissionFromRoleCommand(roleId.ToString(), permissionId.ToString()),
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionForRoleUsersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToTheServiceAndUnitOfWork()
    {
        // Arrange
        (RoleEntity role, Guid permissionId, AdminRemovePermissionFromRoleCommand command) = ArrangeRevoke();
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _removePermissionServiceMock.Verify(x => x.RevokeAsync(role.Id, permissionId, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
