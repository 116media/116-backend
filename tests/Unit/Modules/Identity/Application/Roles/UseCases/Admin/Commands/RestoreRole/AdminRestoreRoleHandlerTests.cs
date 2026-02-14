using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;

/// <summary>
/// Unit tests for <see cref="AdminRestoreRoleHandler"/>.
/// </summary>
public class AdminRestoreRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminRestoreRoleHandler _handler;

    public AdminRestoreRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminRestoreRoleHandler(_roleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithDeletedRole_ShouldRestoreAndReturnResult()
    {
        // Arrange
        RoleEntity deletedRole = RoleFactory.CreateDeleted(TestConstants.Role.ValidName);

        AdminRestoreRoleCommand command = new(RoleId: deletedRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(deletedRole);

        // Act
        AdminRestoreRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Id.Should().Be(deletedRole.Id);
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithDeletedRole_ShouldSetIsDeletedToFalse()
    {
        // Arrange
        RoleEntity deletedRole = RoleFactory.CreateDeleted();

        AdminRestoreRoleCommand command = new(RoleId: deletedRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(deletedRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        deletedRole.IsDeleted.Should().BeFalse();
        deletedRole.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithDeletedRole_ShouldNotAutomaticallyActivate()
    {
        // Arrange - Deleted roles are also inactive
        RoleEntity deletedRole = RoleFactory.CreateDeleted();

        AdminRestoreRoleCommand command = new(RoleId: deletedRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(deletedRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Restore should clear IsDeleted but IsActive remains false
        deletedRole.IsDeleted.Should().BeFalse();
        deletedRole.IsActive.Should().BeFalse();
    }

    #endregion

    #region Failure Cases - Role Not Found

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminRestoreRoleCommand command = new(RoleId: nonExistentRoleId);

        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldNotCommit()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminRestoreRoleCommand command = new(RoleId: nonExistentRoleId);

        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentRoleId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Failure Cases - Role Not Deleted

    [Fact]
    public async Task Handle_WhenRoleNotDeleted_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity activeRole = RoleFactory.Create(TestConstants.Role.ValidName);

        AdminRestoreRoleCommand command = new(RoleId: activeRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(activeRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRoleNotDeleted_ShouldNotCommit()
    {
        // Arrange
        RoleEntity activeRole = RoleFactory.Create(TestConstants.Role.ValidName);

        AdminRestoreRoleCommand command = new(RoleId: activeRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(activeRole);

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

    [Fact]
    public async Task Handle_WithInactiveNotDeletedRole_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity inactiveRole = new RoleBuilder().WithName(TestConstants.Role.ValidName).AsInactive().Build();

        AdminRestoreRoleCommand command = new(RoleId: inactiveRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        RoleEntity deletedRole = RoleFactory.CreateDeleted();

        AdminRestoreRoleCommand command = new(RoleId: deletedRole.Id);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(deletedRole);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(deletedRole.Id, cts.Token), Times.Once);
    }

    #endregion
}
