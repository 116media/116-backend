using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Unit tests for <see cref="AdminAssignPermissionToRoleHandler"/>: the grant call, the commit,
/// the token bump and the response. The gates are covered by
/// <c>AdminAssignPermissionToRoleServiceTests</c>.
/// </summary>
public class AdminAssignPermissionToRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminAssignPermissionToRoleService> _assignPermissionServiceMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminAssignPermissionToRoleHandler _handler;

    public AdminAssignPermissionToRoleHandlerTests()
    {
        _assignPermissionServiceMock = new Mock<IAdminAssignPermissionToRoleService>();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminAssignPermissionToRoleHandler(
            _assignPermissionServiceMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    private (RoleEntity Role, PermissionEntity Permission, AdminAssignPermissionToRoleCommand Command) ArrangeGrant()
    {
        PermissionEntity permission = PermissionFactory.CreateDefault();
        RoleEntity role = RoleFactory.CreateDefault();
        role.GrantPermission(permission.Id);

        _assignPermissionServiceMock
            .Setup(x => x.GrantAsync(role.Id, permission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissionGrantData(Role: role, Permission: permission));

        return (role, permission, new AdminAssignPermissionToRoleCommand(role.Id.ToString(), permission.Id));
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnTheRoleWithTheFreshPermission()
    {
        // Arrange
        (RoleEntity role, PermissionEntity permission, AdminAssignPermissionToRoleCommand command) = ArrangeGrant();

        // Act
        AdminAssignPermissionToRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Id.Should().Be(role.Id);
        result.Role.Permissions.Should().ContainSingle(p => p.Id == permission.Id && p.Resource == permission.Resource);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        (_, _, AdminAssignPermissionToRoleCommand command) = ArrangeGrant();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldBumpTheRoleHoldersTokenVersionAfterCommitting()
    {
        // Arrange
        (RoleEntity role, _, AdminAssignPermissionToRoleCommand command) = ArrangeGrant();

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
    public async Task Handle_WhenTheGrantIsRefused_ShouldNotCommitOrBump()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        _assignPermissionServiceMock
            .Setup(x => x.GrantAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(TestErrorsFactory.CreateIdentityI18n().User.PermissionAlreadyAssignedToRole());

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new AdminAssignPermissionToRoleCommand(roleId.ToString(), permissionId),
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
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
        (RoleEntity role, PermissionEntity permission, AdminAssignPermissionToRoleCommand command) = ArrangeGrant();
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _assignPermissionServiceMock.Verify(x => x.GrantAsync(role.Id, permission.Id, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
