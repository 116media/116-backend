using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Unit tests for <see cref="AdminAssignRoleToUserHandler"/>.
/// </summary>
public class AdminAssignRoleToUserHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly IdentityI18n _userErrors;
    private readonly AdminAssignRoleToUserHandler _handler;

    public AdminAssignRoleToUserHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _authRepositoryMock = MockAuthRepository.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _userErrors = TestErrorsFactory.CreateIdentityI18n();

        _handler = new AdminAssignRoleToUserHandler(
            _roleRepositoryMock.Object,
            _authRepositoryMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            _userErrors
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnRoles()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        AdminAssignRoleToUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        user.HasRole(role.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldValidateRoleExists()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGrantTheRoleThroughTheUserAggregate()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.UserRoles.Should().ContainSingle(ur => ur.RoleId == role.Id && ur.UserId == user.Id);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: roleId);

        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(roleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesByIdNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.CreateInactive();
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.CreateDeleted();
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyAssigned_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = UserFactory.Create("test@example.com");
        user.GrantRoleBootstrap(role.Id);
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyAssigned_ShouldNotCommitOrBump()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = UserFactory.Create("test@example.com");
        user.GrantRoleBootstrap(role.Id);
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRoleRepository()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);
        using CancellationTokenSource cts = new();

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);
        using CancellationTokenSource cts = new();

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
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: user.Id.ToString(), RoleId: role.Id);

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
