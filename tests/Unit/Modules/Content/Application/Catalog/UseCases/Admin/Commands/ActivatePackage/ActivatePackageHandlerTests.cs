using _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Unit tests for <see cref="ActivatePackageHandler"/>.
/// </summary>
public class ActivatePackageHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ActivatePackageHandler _handler;

    public ActivatePackageHandlerTests()
    {
        _packageRepositoryMock = MockPackageRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new ActivatePackageHandler(_packageRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldActivateAndReturnDto()
    {
        // Arrange
        PackageEntity inactive = PackageFactory.CreateInactive();
        var command = new ActivatePackageCommand(Id: inactive.Id.ToString());

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(inactive);

        // Act
        ActivatePackageResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Package.Should().NotBeNull();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldReloadAfterCommit()
    {
        // Arrange
        PackageEntity inactive = PackageFactory.CreateInactive();
        var command = new ActivatePackageCommand(Id: inactive.Id.ToString());

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(inactive);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _packageRepositoryMock.Verify(
            x => x.GetByIdWithSlotsOrThrowAsync(inactive.Id, It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldThrowConflictException()
    {
        // Arrange
        PackageEntity active = PackageFactory.Create();
        var command = new ActivatePackageCommand(Id: active.Id.ToString());

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(active);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyActive_ShouldNotCommit()
    {
        // Arrange
        PackageEntity active = PackageFactory.Create();
        var command = new ActivatePackageCommand(Id: active.Id.ToString());

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(active);

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
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new ActivatePackageCommand(Id: nonExistentId.ToString());

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
