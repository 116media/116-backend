using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Unit tests for <see cref="AdminCreateRoleHandler"/>.
/// </summary>
public class AdminCreateRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly IdentityI18n _userErrors;
    private readonly AdminCreateRoleHandler _handler;

    public AdminCreateRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _userErrors = TestErrorsFactory.CreateIdentityI18n();

        _handler = new AdminCreateRoleHandler(_roleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper, _userErrors);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRoleAndReturnResult()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: false);

        // Act
        AdminCreateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        result.Role.Description.Should().Be(TestConstants.Role.ValidDescription);

        _roleRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldGenerateNewRoleId()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: false);

        // Act
        AdminCreateRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithUniqueRoleName_ShouldVerifyExistsByNameCalled()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
            x => x.ExistsByNameAsync(TestConstants.Role.ValidName, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRoleNameAlreadyExists_ShouldNotAddRole()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _roleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RoleEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleNameAlreadyExists_ShouldNotCommit()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: false);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.ExistsByNameAsync(TestConstants.Role.ValidName, cts.Token), Times.Once);
    }

    #endregion
}
