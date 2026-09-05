using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;

/// <summary>
/// Unit tests for <see cref="AdminGetUserRolesHandler"/>.
/// </summary>
public class AdminGetUserRolesHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly AdminGetUserRolesHandler _handler;

    public AdminGetUserRolesHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();

        _handler = new AdminGetUserRolesHandler(_authRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithUserRoles_ShouldReturnRoleDtos()
    {
        // Arrange
        RoleEntity role1 = RoleFactory.Create("Admin", "Administrator role");
        RoleEntity role2 = RoleFactory.Create("User", "Regular user role");
        UserEntity user = new UserBuilder().WithRole(role1).WithRole(role2).Build();
        AdminGetUserRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoRoles_ShouldReturnEmptyList()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        AdminGetUserRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapRolesToDtos()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("TestRole", "Test role description");
        UserEntity user = new UserBuilder().WithRole(role).Build();
        AdminGetUserRolesQuery query = new(UserId: user.Id);

        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        AdminGetUserRolesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        result.Roles.First().Name.Should().Be("TestRole");
        result.Roles.First().Description.Should().Be("Test role description");
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AdminGetUserRolesQuery query = new(UserId: userId);

        _authRepositoryMock.SetupGetUserWithRolesByIdNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        UserEntity user = UserFactory.Create("test@example.com");
        AdminGetUserRolesQuery query = new(UserId: user.Id);
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesById(user);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.GetUserWithRolesByIdOrThrow(user.Id, cts.Token), Times.Once);
    }

    #endregion
}
