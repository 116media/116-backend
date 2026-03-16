using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
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
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveRoleFromUserHandler _handler;

    public AdminRemoveRoleFromUserHandlerTests()
    {
        _userRoleRepositoryMock = MockUserRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminRemoveRoleFromUserHandler(_userRoleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnRemainingRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity roleToRemove = RoleFactory.Create("Admin", "Administrator role");
        RoleEntity remainingRole = RoleFactory.Create("User", "User role");
        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, roleToRemove);
        UserRoleEntity remainingUserRole = UserRoleFactory.CreateWithRole(userId, remainingRole);

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: roleToRemove.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [remainingUserRole]);

        // Act
        AdminRemoveRoleFromUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().ContainSingle();
        result.Roles.First().Name.Should().Be("User");
    }

    [Fact]
    public async Task Handle_ShouldGetUserRoleAssociation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.Verify(
            x => x.GetByUserAndRoleAsync(userId, role.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldDeleteUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.Verify(x => x.Delete(userRole), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNoRemainingRoles_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

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
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: roleId);

        _userRoleRepositoryMock.SetupGetByUserAndRoleReturnsNull(userId, roleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleNotAssigned_ShouldNotDeleteAnything()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: roleId);

        _userRoleRepositoryMock.SetupGetByUserAndRoleReturnsNull(userId, roleId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (BadRequestException)
        {
            // Expected
        }

        // Assert
        _userRoleRepositoryMock.Verify(x => x.Delete(It.IsAny<UserRoleEntity>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleNotAssigned_ShouldNotCommit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: roleId);

        _userRoleRepositoryMock.SetupGetByUserAndRoleReturnsNull(userId, roleId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (BadRequestException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUserRoleRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);
        using CancellationTokenSource cts = new();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _userRoleRepositoryMock.Verify(x => x.GetByUserAndRoleAsync(userId, role.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);
        using CancellationTokenSource cts = new();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
