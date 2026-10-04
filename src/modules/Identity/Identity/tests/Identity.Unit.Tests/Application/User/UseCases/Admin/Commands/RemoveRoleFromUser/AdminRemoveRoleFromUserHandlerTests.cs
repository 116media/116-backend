using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Builders.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Unit tests for <see cref="AdminRemoveRoleFromUserHandler"/>: the revocation call, the commit,
/// the token bump and the response. The gates are covered by
/// <c>AdminRemoveRoleFromUserServiceTests</c>.
/// </summary>
public class AdminRemoveRoleFromUserHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminRemoveRoleFromUserService> _removeRoleServiceMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveRoleFromUserHandler _handler;

    public AdminRemoveRoleFromUserHandlerTests()
    {
        _removeRoleServiceMock = new Mock<IAdminRemoveRoleFromUserService>();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminRemoveRoleFromUserHandler(
            _removeRoleServiceMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    private (UserEntity User, RoleEntity Removed, AdminRemoveRoleFromUserCommand Command) ArrangeRevoke(
        params RoleEntity[] remaining
    )
    {
        RoleEntity removed = RoleFactory.Create("Admin", "Administrator role");
        var builder = new UserBuilder();
        foreach (RoleEntity role in remaining)
        {
            builder.WithRole(role);
        }
        UserEntity user = builder.Build();

        _removeRoleServiceMock
            .Setup(x => x.RevokeAsync(user.Id, removed.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        return (user, removed, new AdminRemoveRoleFromUserCommand(user.Id.ToString(), removed.Id.ToString()));
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnRemainingRoles()
    {
        // Arrange
        RoleEntity remainingRole = RoleFactory.Create("User", "User role");
        (_, _, AdminRemoveRoleFromUserCommand command) = ArrangeRevoke(remainingRole);

        // Act
        AdminRemoveRoleFromUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Roles.Should().ContainSingle(r => r.Name == "User");
    }

    [Fact]
    public async Task Handle_WhenNoRemainingRoles_ShouldReturnEmptyList()
    {
        // Arrange
        (_, _, AdminRemoveRoleFromUserCommand command) = ArrangeRevoke();

        // Act
        AdminRemoveRoleFromUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        (_, _, AdminRemoveRoleFromUserCommand command) = ArrangeRevoke();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldBumpTheTargetUserTokenVersionAfterCommitting()
    {
        // Arrange
        (UserEntity user, _, AdminRemoveRoleFromUserCommand command) = ArrangeRevoke();

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
    public async Task Handle_WhenTheRevocationIsRefused_ShouldNotCommitOrBump()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _removeRoleServiceMock
            .Setup(x => x.RevokeAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(TestErrorsFactory.CreateIdentityI18n().User.RoleNotAssignedToUser());

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(
                new AdminRemoveRoleFromUserCommand(userId.ToString(), roleId.ToString()),
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
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
        (UserEntity user, RoleEntity removed, AdminRemoveRoleFromUserCommand command) = ArrangeRevoke();
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _removeRoleServiceMock.Verify(x => x.RevokeAsync(user.Id, removed.Id, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
