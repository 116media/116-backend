using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
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
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminAssignRoleToUserHandler _handler;

    public AdminAssignRoleToUserHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _userRoleRepositoryMock = MockUserRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminAssignRoleToUserHandler(
            _roleRepositoryMock.Object,
            _userRoleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        AdminAssignRoleToUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().ContainSingle();
        result.Roles.First().Name.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_ShouldValidateRoleExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCheckIfRoleAlreadyAssigned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.Verify(
            x => x.ExistsByUserAndRoleAsync(userId, role.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldAddUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserRoleEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

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
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyAssigned_ShouldNotAddUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, true);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _userRoleRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<UserRoleEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRoleRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);
        using CancellationTokenSource cts = new();

        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToUnitOfWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        AdminAssignRoleToUserCommand command = new(UserId: userId.ToString(), RoleId: role.Id);
        using CancellationTokenSource cts = new();

        UserRoleEntity userRole = UserRoleFactory.CreateWithRole(userId, role);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }

    #endregion
}
