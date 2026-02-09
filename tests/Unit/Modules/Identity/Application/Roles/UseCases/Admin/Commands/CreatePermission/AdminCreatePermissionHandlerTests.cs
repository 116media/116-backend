using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Unit tests for <see cref="AdminCreatePermissionHandler"/>.
/// </summary>
public class AdminCreatePermissionHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreatePermissionHandler _handler;

    public AdminCreatePermissionHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminCreatePermissionHandler(_permissionRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePermissionAndReturnResult()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        _permissionRepositoryMock.SetupExistsByResourceAndAction(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction,
            exists: false
        );

        // Act
        AdminCreatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Resource.Should().Be(TestConstants.Permission.ValidResource);
        result.Permission.Action.Should().Be(TestConstants.Permission.ValidAction);
        result.Permission.Description.Should().Be(TestConstants.Permission.ValidDescription);

        _permissionRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldGenerateNewPermissionId()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        _permissionRepositoryMock.SetupExistsByResourceAndActionReturnsFalse();

        // Act
        AdminCreatePermissionResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Permission.Id.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPermissionAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        _permissionRepositoryMock.SetupExistsByResourceAndAction(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction,
            exists: true
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyExists_ShouldNotAddPermission()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        _permissionRepositoryMock.SetupExistsByResourceAndAction(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction,
            exists: true
        );

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
        _permissionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<_116.Identity.Domain.Entities.PermissionEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyExists_ShouldNotCommit()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        _permissionRepositoryMock.SetupExistsByResourceAndAction(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction,
            exists: true
        );

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
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupExistsByResourceAndActionReturnsFalse();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(
            x =>
                x.ExistsByResourceAndActionAsync(
                    TestConstants.Permission.ValidResource,
                    TestConstants.Permission.ValidAction,
                    cts.Token
                ),
            Times.Once
        );
    }

    #endregion
}
