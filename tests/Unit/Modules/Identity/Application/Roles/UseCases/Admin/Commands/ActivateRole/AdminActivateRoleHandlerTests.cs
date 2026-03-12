using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;

/// <summary>
/// Unit tests for <see cref="AdminActivateRoleHandler"/>.
/// </summary>
public class AdminActivateRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminActivateRoleHandler _handler;

    public AdminActivateRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminActivateRoleHandler(_roleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithInactiveRole_ShouldActivateAndReturnResult()
    {
        // Arrange
        RoleEntity inactiveRole = RoleFactory.CreateInactive(TestConstants.Role.ValidName);

        AdminActivateRoleCommand command = new(RoleId: inactiveRole.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

        // Act
        AdminActivateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Id.Should().Be(inactiveRole.Id);
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithInactiveRole_ShouldSetIsActiveToTrue()
    {
        // Arrange
        RoleEntity inactiveRole = RoleFactory.CreateInactive();

        AdminActivateRoleCommand command = new(RoleId: inactiveRole.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        inactiveRole.IsActive.Should().BeTrue();
    }

    #endregion

    #region Failure Cases - Role Not Found

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentRoleId = Guid.NewGuid();
        AdminActivateRoleCommand command = new(RoleId: nonExistentRoleId.ToString());

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
        var nonExistentRoleId = Guid.NewGuid();
        AdminActivateRoleCommand command = new(RoleId: nonExistentRoleId.ToString());

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

    #region Failure Cases - Role Already Active

    [Fact]
    public async Task Handle_WhenRoleAlreadyActive_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity activeRole = RoleFactory.Create(TestConstants.Role.ValidName);

        AdminActivateRoleCommand command = new(RoleId: activeRole.Id.ToString());

        _roleRepositoryMock.SetupGetByIdOrThrow(activeRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyActive_ShouldNotCommit()
    {
        // Arrange
        RoleEntity activeRole = RoleFactory.Create(TestConstants.Role.ValidName);

        AdminActivateRoleCommand command = new(RoleId: activeRole.Id.ToString());

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

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        RoleEntity inactiveRole = RoleFactory.CreateInactive();

        AdminActivateRoleCommand command = new(RoleId: inactiveRole.Id.ToString());

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(inactiveRole.Id, cts.Token), Times.Once);
    }

    #endregion
}
