using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Assignments.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Unit tests for <see cref="AdminAssignRoleToUserHandler"/>.
/// </summary>
public class AdminAssignRoleToUserHandlerTests
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
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidUserAndRole_ShouldAssignAndReturnResult()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, exists: false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [userRole]);

        // Act
        AdminAssignRoleToUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().NotBeNull();
        result.Roles.Should().HaveCount(1);
        _userRoleRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserRoleAssociation()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, exists: false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.VerifyAddCalled(ur => ur.UserId == userId && ur.RoleId == role.Id);
    }

    [Fact]
    public async Task Handle_WithMultipleExistingRoles_ShouldReturnAllRoles()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity newRole = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        RoleEntity existingRole = new RoleBuilder()
            .WithName(TestConstants.Role.AdminName)
            .WithDescription(TestConstants.Role.AdminDescription)
            .AsActive()
            .Build();

        UserRoleEntity existingUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(existingRole).Build();

        UserRoleEntity newUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(newRole).Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: newRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(newRole);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, newRole.Id, exists: false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [existingUserRole, newUserRole]);

        // Act
        AdminAssignRoleToUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().HaveCount(2);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: nonExistentRoleId);

        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity inactiveRole = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsInactive()
            .Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: inactiveRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity deletedRole = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsDeleted()
            .Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: deletedRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(deletedRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyAssigned_ShouldThrowConflictException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, exists: true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyAssigned_ShouldNotCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, exists: true);

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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepositories()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: role.Id);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, exists: false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFetchUpdatedUserRolesAfterAssignment()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignRoleToUserCommand command = new(UserId: userId, RoleId: role.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _userRoleRepositoryMock.SetupExistsByUserAndRole(userId, role.Id, exists: false);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.Verify(
            x => x.GetUserRolesWithRoleAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion
}
