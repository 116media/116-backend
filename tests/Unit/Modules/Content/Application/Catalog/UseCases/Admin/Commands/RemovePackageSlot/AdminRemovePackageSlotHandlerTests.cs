using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Unit tests for <see cref="AdminRemovePackageSlotHandler"/>.
/// </summary>
public class AdminRemovePackageSlotHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemovePackageSlotHandler _handler;

    public AdminRemovePackageSlotHandlerTests()
    {
        _packageRepositoryMock = MockPackageRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRemovePackageSlotHandler(_packageRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenSlotExists_ShouldRemoveAndReturnUpdatedPackage()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = PackageSlotFactory.Create(package.Id);

        var command = new AdminRemovePackageSlotCommand(PackageId: package.Id.ToString(), SlotId: slot.Id);

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(package);
        _packageRepositoryMock.SetupGetSlotById(slot.Id, slot);

        // Act
        AdminRemovePackageSlotResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Package.Should().NotBeNull();

        _packageRepositoryMock.VerifyRemoveSlotCalled(slot);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPackageNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentPackageId = Guid.NewGuid();

        var command = new AdminRemovePackageSlotCommand(
            PackageId: nonExistentPackageId.ToString(),
            SlotId: Guid.NewGuid()
        );

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrowNotFound(nonExistentPackageId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlotNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        var nonExistentSlotId = Guid.NewGuid();

        var command = new AdminRemovePackageSlotCommand(PackageId: package.Id.ToString(), SlotId: nonExistentSlotId);

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(package);
        _packageRepositoryMock.SetupGetSlotById(nonExistentSlotId, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
