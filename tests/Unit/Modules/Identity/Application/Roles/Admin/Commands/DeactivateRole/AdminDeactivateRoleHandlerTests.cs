using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Admin.Commands.DeactivateRole;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateRoleHandler"/>.
/// </summary>
public class AdminDeactivateRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeactivateRoleHandler _handler;

    public AdminDeactivateRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminDeactivateRoleHandler(_roleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithActiveRole_ShouldDeactivateAndReturnResult()
    {
        // Arrange
        RoleEntity activeRole = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminDeactivateRoleCommand command = new(RoleId: activeRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(activeRole);

        // Act
        AdminDeactivateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Id.Should().Be(activeRole.Id);
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithActiveRole_ShouldSetIsActiveToFalse()
    {
        // Arrange
        RoleEntity activeRole = new RoleBuilder().WithName(TestConstants.Role.ValidName).AsActive().Build();

        AdminDeactivateRoleCommand command = new(RoleId: activeRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(activeRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        activeRole.IsActive.Should().BeFalse();
    }

    #endregion

    #region Failure Cases - Role Not Found

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminDeactivateRoleCommand command = new(RoleId: nonExistentRoleId);

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
        AdminDeactivateRoleCommand command = new(RoleId: nonExistentRoleId);

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

    #region Failure Cases - Role Already Inactive

    [Fact]
    public async Task Handle_WhenRoleAlreadyInactive_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity inactiveRole = new RoleBuilder().WithName(TestConstants.Role.ValidName).AsInactive().Build();

        AdminDeactivateRoleCommand command = new(RoleId: inactiveRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyInactive_ShouldNotCommit()
    {
        // Arrange
        RoleEntity inactiveRole = new RoleBuilder().WithName(TestConstants.Role.ValidName).AsInactive().Build();

        AdminDeactivateRoleCommand command = new(RoleId: inactiveRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

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
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        RoleEntity activeRole = new RoleBuilder().WithName(TestConstants.Role.ValidName).AsActive().Build();

        AdminDeactivateRoleCommand command = new(RoleId: activeRole.Id);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(activeRole);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(activeRole.Id, cts.Token), Times.Once);
    }

    #endregion
}
