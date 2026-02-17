using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;
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

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;

/// <summary>
/// Unit tests for <see cref="AdminUpdateRoleHandler"/>.
/// </summary>
public class AdminUpdateRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateRoleHandler _handler;

    public AdminUpdateRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminUpdateRoleHandler(_roleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases - Update Both Name and Description

    [Fact]
    public async Task Handle_WithBothNameAndDescription_ShouldUpdateRoleAndReturnResult()
    {
        // Arrange
        RoleEntity existingRole = RoleFactory.Create("OldName", "Old description");

        string newName = "NewRoleName";
        string newDescription = "New role description";

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, newName, newDescription);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);
        _roleRepositoryMock.SetupExistsByName(newName, exists: false);

        // Act
        AdminUpdateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Name.Should().Be(newName);
        result.Role.Description.Should().Be(newDescription);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Success Cases - Update Only Name

    [Fact]
    public async Task Handle_WithOnlyNewName_ShouldUpdateOnlyName()
    {
        // Arrange
        string originalDescription = "Original description";
        RoleEntity existingRole = RoleFactory.Create("OldName", originalDescription);

        string newName = "NewRoleName";

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, newName, null);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);
        _roleRepositoryMock.SetupExistsByName(newName, exists: false);

        // Act
        AdminUpdateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Name.Should().Be(newName);
        result.Role.Description.Should().Be(originalDescription);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Success Cases - Update Only Description

    [Fact]
    public async Task Handle_WithOnlyNewDescription_ShouldUpdateOnlyDescription()
    {
        // Arrange
        string originalName = "OriginalName";
        RoleEntity existingRole = RoleFactory.Create(originalName, "Old description");

        string newDescription = "New role description";

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, null, newDescription);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);

        // Act
        AdminUpdateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Name.Should().Be(originalName);
        result.Role.Description.Should().Be(newDescription);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region No Changes Cases

    [Fact]
    public async Task Handle_WithNoChanges_ShouldNotCommit()
    {
        // Arrange
        RoleEntity existingRole = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, null, null);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);

        // Act
        AdminUpdateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        result.Role.Description.Should().Be(TestConstants.Role.ValidDescription);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WithSameNameAndDescription_ShouldNotCommit()
    {
        // Arrange
        RoleEntity existingRole = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateValidCommand(existingRole.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);

        // Act
        AdminUpdateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldTreatAsNoChange()
    {
        // Arrange
        RoleEntity existingRole = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, string.Empty, null);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);

        // Act
        AdminUpdateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Failure Cases - Role Not Found

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateValidCommand(nonExistentRoleId);

        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Failure Cases - Duplicate Name

    [Fact]
    public async Task Handle_WhenNewNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity existingRole = RoleFactory.Create("OldName", "Old description");

        string duplicateName = "ExistingRole";

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, duplicateName, null);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);
        _roleRepositoryMock.SetupExistsByName(duplicateName, exists: true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNewNameAlreadyExists_ShouldNotCommit()
    {
        // Arrange
        RoleEntity existingRole = RoleFactory.Create("OldName", "Old description");

        string duplicateName = "ExistingRole";

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, duplicateName, null);

        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);
        _roleRepositoryMock.SetupExistsByName(duplicateName, exists: true);

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
        RoleEntity existingRole = RoleFactory.Create("OldName", "Old description");

        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(existingRole.Id, "NewName", null);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(existingRole);
        _roleRepositoryMock.SetupExistsByName("NewName", exists: false);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(existingRole.Id, cts.Token), Times.Once);
    }

    #endregion
}
