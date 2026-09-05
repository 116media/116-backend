using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Unit tests for <see cref="AdminRemoveRoleFromUserHandler"/>.
/// </summary>
public class AdminRemoveRoleFromUserHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly IdentityI18n _userErrors;
    private readonly AdminRemoveRoleFromUserHandler _handler;

    public AdminRemoveRoleFromUserHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _roleRepositoryMock = MockRoleRepository.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _userErrors = TestErrorsFactory.CreateIdentityI18n();

        _handler = new AdminRemoveRoleFromUserHandler(
            _authRepositoryMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            _userErrors,
            _roleRepositoryMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnRemainingRoles()
    {
        // Arrange
        RoleEntity roleToRemove = RoleFactory.Create("Admin", "Administrator role");
        RoleEntity remainingRole = RoleFactory.Create("User", "User role");
        UserEntity user = new UserBuilder().WithRole(roleToRemove).WithRole(remainingRole).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: roleToRemove.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(roleToRemove);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        AdminRemoveRoleFromUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        result.Roles.First().Name.Should().Be("User");
    }

    [Fact]
    public async Task Handle_ShouldRevokeTheRoleThroughTheUserAggregate()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.HasRole(role.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNoRemainingRoles_ShouldReturnEmptyList()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        AdminRemoveRoleFromUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().BeEmpty();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotAssignedToUser_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = UserFactory.Create("test@example.com");

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleNotAssigned_ShouldNotCommitOrBump()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = UserFactory.Create("test@example.com");

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesByIdNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();
        using CancellationTokenSource cts = new();

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.GetUserWithRolesByIdOrThrow(user.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();
        using CancellationTokenSource cts = new();

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion

    #region Token Invalidation

    [Fact]
    public async Task Handle_ShouldBumpTheTargetUserTokenVersionAfterCommitting()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

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
}
