using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Unit tests for <see cref="AdminRemovePermissionFromRoleHandler"/>.
/// </summary>
public class AdminRemovePermissionFromRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly IdentityI18n _userErrors;
    private readonly AdminRemovePermissionFromRoleHandler _handler;

    public AdminRemovePermissionFromRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _userErrors = TestErrorsFactory.CreateIdentityI18n();

        _handler = new AdminRemovePermissionFromRoleHandler(
            _roleRepositoryMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            _userErrors
        );
    }

    /// <summary>
    /// Builds a role that already carries the supplied permission, mirroring the shape the
    /// repository include returns.
    /// </summary>
    private static RoleEntity CreateRoleWithPermission(PermissionEntity permission)
    {
        return new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .WithPermissions([permission])
            .Build();
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRoleAndPermission_ShouldRemoveAndReturnResult()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        RoleEntity role = CreateRoleWithPermission(permission);

        AdminRemovePermissionFromRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: permission.Id.ToString()
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminRemovePermissionFromRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Role.Id.Should().Be(role.Id);
        role.HasPermission(permission.Id).Should().BeFalse();
        _unitOfWorkMock.VerifyCommitCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionForRoleUsersAsync(role.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldRevokeThroughTheRoleAggregate()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        RoleEntity role = CreateRoleWithPermission(permission);

        AdminRemovePermissionFromRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: permission.Id.ToString()
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.RolePermissions.Should().BeEmpty();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentRoleId = Guid.NewGuid();
        AdminRemovePermissionFromRoleCommand command = new(
            RoleId: nonExistentRoleId.ToString(),
            PermissionId: Guid.NewGuid().ToString()
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotAssigned_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminRemovePermissionFromRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: Guid.NewGuid().ToString()
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotAssigned_ShouldNotCommitOrBump()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminRemovePermissionFromRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: Guid.NewGuid().ToString()
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionForRoleUsersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepositories()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        RoleEntity role = CreateRoleWithPermission(permission);

        AdminRemovePermissionFromRoleCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionId: permission.Id.ToString()
        );

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, cts.Token), Times.Once);
    }

    #endregion
}
