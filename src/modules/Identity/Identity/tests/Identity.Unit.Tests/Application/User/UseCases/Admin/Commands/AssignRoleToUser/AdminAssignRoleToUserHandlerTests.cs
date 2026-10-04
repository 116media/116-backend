using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Unit tests for <see cref="AdminAssignRoleToUserHandler"/>: the grant call, the commit, the token
/// bump and the response. The gates are covered by <c>AdminAssignRoleToUserServiceTests</c>.
/// </summary>
public class AdminAssignRoleToUserHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminAssignRoleToUserService> _assignRoleServiceMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminAssignRoleToUserHandler _handler;

    public AdminAssignRoleToUserHandlerTests()
    {
        _assignRoleServiceMock = new Mock<IAdminAssignRoleToUserService>();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminAssignRoleToUserHandler(
            _assignRoleServiceMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    private (UserEntity User, RoleEntity Role, AdminAssignRoleToUserCommand Command) ArrangeGrant()
    {
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        user.GrantRole(role.Id, role.Name);

        _assignRoleServiceMock
            .Setup(x => x.GrantAsync(user.Id, role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleGrantData(User: user, Role: role));

        return (user, role, new AdminAssignRoleToUserCommand(UserId: user.Id.ToString(), RoleId: role.Id));
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnTheRolesIncludingTheFreshGrant()
    {
        // Arrange
        (_, RoleEntity role, AdminAssignRoleToUserCommand command) = ArrangeGrant();

        // Act
        AdminAssignRoleToUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle(r => r.Id == role.Id && r.Name == role.Name);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        (_, _, AdminAssignRoleToUserCommand command) = ArrangeGrant();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldBumpTheTargetUserTokenVersionAfterCommitting()
    {
        // Arrange
        (UserEntity user, _, AdminAssignRoleToUserCommand command) = ArrangeGrant();

        var callOrder = new List<string>();
        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("commit"))
            .ReturnsAsync(1);
        _tokenStateRepositoryMock
            .Setup(x => x.BumpTokenVersionAsync(user.Id, It.IsAny<CancellationToken>()))
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
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _assignRoleServiceMock
            .Setup(x => x.GrantAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(TestErrorsFactory.CreateIdentityI18n().User.RoleAlreadyAssignedToUser());

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(new AdminAssignRoleToUserCommand(userId.ToString(), roleId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToTheServiceAndUnitOfWork()
    {
        // Arrange
        (UserEntity user, RoleEntity role, AdminAssignRoleToUserCommand command) = ArrangeGrant();
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _assignRoleServiceMock.Verify(x => x.GrantAsync(user.Id, role.Id, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
