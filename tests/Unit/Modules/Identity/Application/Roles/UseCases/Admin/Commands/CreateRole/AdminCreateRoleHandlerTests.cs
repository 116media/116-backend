using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Unit tests for <see cref="AdminCreateRoleHandler"/>.
/// </summary>
public class AdminCreateRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateRoleHandler _handler;

    public AdminCreateRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminCreateRoleHandler(_roleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
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
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
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
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _roleRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<_116.Identity.Domain.Entities.RoleEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenRoleNameAlreadyExists_ShouldNotCommit()
    {
        // Arrange
        AdminCreateRoleCommand command = CommandFactory.Role.CreateValidCommand();

        _roleRepositoryMock.SetupExistsByName(TestConstants.Role.ValidName, exists: true);

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
