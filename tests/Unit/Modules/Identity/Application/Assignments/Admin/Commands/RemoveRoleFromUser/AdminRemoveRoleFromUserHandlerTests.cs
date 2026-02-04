using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Assignments.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Unit tests for <see cref="AdminRemoveRoleFromUserHandler"/>.
/// </summary>
public class AdminRemoveRoleFromUserHandlerTests
{
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveRoleFromUserHandler _handler;

    public AdminRemoveRoleFromUserHandlerTests()
    {
        _userRoleRepositoryMock = MockUserRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminRemoveRoleFromUserHandler(_userRoleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidUserAndRole_ShouldRemoveAndReturnResult()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        AdminRemoveRoleFromUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().NotBeNull();
        _userRoleRepositoryMock.VerifyDeleteCalled(userRole);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteUserRoleAssociation()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRoleRepositoryMock.VerifyDeleteCalled();
    }

    [Fact]
    public async Task Handle_WithRemainingRoles_ShouldReturnRemainingRoles()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity roleToRemove = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        RoleEntity remainingRole = new RoleBuilder()
            .WithName(TestConstants.Role.AdminName)
            .WithDescription(TestConstants.Role.AdminDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRoleToRemove = new UserRoleBuilder().WithUserId(userId).WithRole(roleToRemove).Build();

        UserRoleEntity remainingUserRole = new UserRoleBuilder().WithUserId(userId).WithRole(remainingRole).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: roleToRemove.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRoleToRemove);
        _userRoleRepositoryMock.SetupGetUserRolesWithRole(userId, [remainingUserRole]);

        // Act
        AdminRemoveRoleFromUserResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().HaveCount(1);
        result.Roles.First().Name.Should().Be(TestConstants.Role.AdminName);
    }

    [Fact]
    public async Task Handle_RemovingLastRole_ShouldReturnEmptyRoleList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: role.Id);

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
    public async Task Handle_WhenRoleNotAssigned_ShouldThrowBadRequestException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: roleId);

        _userRoleRepositoryMock.SetupGetByUserAndRoleReturnsNull(userId, roleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleNotAssigned_ShouldNotCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: roleId);

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

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: role.Id);

        using CancellationTokenSource cts = new();
        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
        _userRoleRepositoryMock.SetupGetUserRolesWithRoleEmpty(userId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _userRoleRepositoryMock.Verify(x => x.GetByUserAndRoleAsync(userId, role.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFetchUpdatedUserRolesAfterRemoval()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

        AdminRemoveRoleFromUserCommand command = new(UserId: userId, RoleId: role.Id);

        _userRoleRepositoryMock.SetupGetByUserAndRole(userRole);
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
