using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Builders.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Unit tests for <see cref="AdminRemoveRoleFromUserService"/>: the revocation through the user
/// aggregate.
/// </summary>
public class AdminRemoveRoleFromUserServiceTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly AdminRemoveRoleFromUserService _service;

    public AdminRemoveRoleFromUserServiceTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _roleRepositoryMock = MockRoleRepository.Create();

        _service = new AdminRemoveRoleFromUserService(
            _authRepositoryMock.Object,
            _roleRepositoryMock.Object,
            TestErrorsFactory.CreateIdentityI18n()
        );
    }

    [Fact]
    public async Task RevokeAsync_ShouldRevokeTheRoleThroughTheUserAggregate()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        UserEntity result = await _service.RevokeAsync(user.Id, role.Id, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(user);
        user.HasRole(role.Id).Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAsync_WhenRoleNotAssignedToUser_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = UserFactory.Create("test@example.com");
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        Func<Task> act = async () => await _service.RevokeAsync(user.Id, role.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task RevokeAsync_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(roleId);

        // Act
        Func<Task> act = async () => await _service.RevokeAsync(Guid.NewGuid(), roleId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RevokeAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesByIdNotFound(userId);

        // Act
        Func<Task> act = async () => await _service.RevokeAsync(userId, role.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RevokeAsync_WithCancellationToken_ShouldPassToTheRepositories()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin", "Administrator role");
        UserEntity user = new UserBuilder().WithRole(role).Build();
        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _service.RevokeAsync(user.Id, role.Id, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
        _authRepositoryMock.Verify(x => x.GetUserWithRolesByIdOrThrow(user.Id, cts.Token), Times.Once);
    }
}
