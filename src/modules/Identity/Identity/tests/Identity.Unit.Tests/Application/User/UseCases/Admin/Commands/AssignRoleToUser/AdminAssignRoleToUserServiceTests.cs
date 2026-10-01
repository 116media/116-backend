using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Unit tests for <see cref="AdminAssignRoleToUserService"/>: the role gates and the grant through
/// the user aggregate.
/// </summary>
public class AdminAssignRoleToUserServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly AdminAssignRoleToUserService _service;

    public AdminAssignRoleToUserServiceTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _authRepositoryMock = MockAuthRepository.Create();

        _service = new AdminAssignRoleToUserService(
            _roleRepositoryMock.Object,
            _authRepositoryMock.Object,
            TestErrorsFactory.CreateIdentityI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task GrantAsync_ShouldGrantTheRoleThroughTheUserAggregate()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        RoleGrantData grant = await _service.GrantAsync(user.Id, role.Id, CancellationToken.None);

        // Assert
        grant.User.Should().BeSameAs(user);
        grant.Role.Should().BeSameAs(role);
        user.UserRoles.Should().ContainSingle(ur => ur.RoleId == role.Id && ur.UserId == user.Id);
    }

    [Fact]
    public async Task GrantAsync_WithCancellationToken_ShouldPassToTheRepositories()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _service.GrantAsync(user.Id, role.Id, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
        _authRepositoryMock.Verify(x => x.GetUserWithRolesByIdOrThrow(user.Id, cts.Token), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task GrantAsync_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(roleId);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(Guid.NewGuid(), roleId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GrantAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesByIdNotFound(userId);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(userId, role.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GrantAsync_WhenRoleIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateInactive();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(Guid.NewGuid(), role.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GrantAsync_WhenRoleIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDeleted();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(Guid.NewGuid(), role.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GrantAsync_WhenRoleAlreadyAssigned_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = UserFactory.Create("test@example.com");
        user.GrantInitialRole(role.Id);
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(user.Id, role.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
