using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;

/// <summary>
/// Unit tests for <see cref="AdminHardDeleteRoleHandler"/>.
/// </summary>
public class AdminHardDeleteRoleHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminHardDeleteRoleHandler _handler;

    public AdminHardDeleteRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminHardDeleteRoleHandler(_roleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNonCoreRole_ShouldHardDeleteAndReturnSuccess()
    {
        // Arrange
        RoleEntity customRole = RoleFactory.Create("CustomRole", "A custom non-core role");

        AdminHardDeleteRoleCommand command = new(RoleId: customRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(customRole);

        // Act
        AdminHardDeleteRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _roleRepositoryMock.VerifyDeleteCalled(customRole);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithSoftDeletedNonCoreRole_ShouldHardDelete()
    {
        // Arrange
        RoleEntity deletedRole = RoleFactory.CreateDeleted();

        AdminHardDeleteRoleCommand command = new(RoleId: deletedRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(deletedRole);

        // Act
        AdminHardDeleteRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _roleRepositoryMock.VerifyDeleteCalled(deletedRole);
    }

    #endregion

    #region Failure Cases - Role Not Found

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminHardDeleteRoleCommand command = new(RoleId: nonExistentRoleId);

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
        AdminHardDeleteRoleCommand command = new(RoleId: nonExistentRoleId);

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

    #region Failure Cases - Core Role Protection

    [Fact]
    public async Task Handle_WithSuperAdminRole_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity superAdminRole = RoleFactory.CreateSuperAdmin();

        AdminHardDeleteRoleCommand command = new(RoleId: superAdminRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(superAdminRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithAdminRole_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity adminRole = RoleFactory.CreateAdmin();

        AdminHardDeleteRoleCommand command = new(RoleId: adminRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(adminRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithVisitorRole_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity visitorRole = RoleFactory.CreateVisitor();

        AdminHardDeleteRoleCommand command = new(RoleId: visitorRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(visitorRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithCoreRole_ShouldNotDeleteOrCommit()
    {
        // Arrange
        RoleEntity superAdminRole = RoleFactory.CreateSuperAdmin();

        AdminHardDeleteRoleCommand command = new(RoleId: superAdminRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(superAdminRole);

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
        _roleRepositoryMock.Verify(x => x.Delete(It.IsAny<RoleEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        RoleEntity customRole = RoleFactory.Create("CustomRole");

        AdminHardDeleteRoleCommand command = new(RoleId: customRole.Id);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(customRole);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(customRole.Id, cts.Token), Times.Once);
    }

    #endregion
}
