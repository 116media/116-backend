using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Unit tests for <see cref="AdminAddPackageSlotHandler"/>.
/// </summary>
public class AdminAddPackageSlotHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminAddPackageSlotHandler _handler;

    public AdminAddPackageSlotHandlerTests()
    {
        _packageRepositoryMock = MockPackageRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminAddPackageSlotHandler(
            _packageRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithCategoryId_ShouldAddSlotAndReturnPackage()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);

        var command = new AdminAddPackageSlotCommand(
            PackageId: package.Id.ToString(),
            CategoryId: category.Id,
            IsRequired: true,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(package);
        _categoryRepositoryMock.SetupGetByIdAsync(category.Id, category);

        // Act
        AdminAddPackageSlotResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Package.Should().NotBeNull();

        _packageRepositoryMock.VerifyAddSlotCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithNullCategoryId_ShouldAddOpenSlotSuccessfully()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();

        var command = new AdminAddPackageSlotCommand(
            PackageId: package.Id.ToString(),
            CategoryId: null,
            IsRequired: false,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(package);

        // Act
        AdminAddPackageSlotResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Package.Should().NotBeNull();

        _packageRepositoryMock.VerifyAddSlotCalled();
        _unitOfWorkMock.VerifyCommitCalled();

        // Category lookup should not be called when CategoryId is null
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPackageNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentPackageId = Guid.NewGuid();

        var command = new AdminAddPackageSlotCommand(
            PackageId: nonExistentPackageId.ToString(),
            CategoryId: null,
            IsRequired: false,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrowNotFound(nonExistentPackageId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryIdProvidedButNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        var nonExistentCategoryId = Guid.NewGuid();

        var command = new AdminAddPackageSlotCommand(
            PackageId: package.Id.ToString(),
            CategoryId: nonExistentCategoryId,
            IsRequired: true,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(package);
        _categoryRepositoryMock.SetupGetByIdAsync(nonExistentCategoryId, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
