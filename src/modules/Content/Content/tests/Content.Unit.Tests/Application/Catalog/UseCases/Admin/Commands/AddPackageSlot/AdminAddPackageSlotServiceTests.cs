using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Unit tests for <see cref="AdminAddPackageSlotService"/>: the category gate and the slot added
/// through the package aggregate.
/// </summary>
public class AdminAddPackageSlotServiceTests
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock = MockPackageRepository.Create();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly AdminAddPackageSlotService _service;

    public AdminAddPackageSlotServiceTests()
    {
        _service = new AdminAddPackageSlotService(
            _packageRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task AddSlotAsync_WithAnExistingCategory_ShouldAddTheSlot()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        _packageRepositoryMock.SetupGetByIdOrThrow(package);
        _categoryRepositoryMock.SetupGetByIdAsync(category.Id, category);

        // Act
        PackageEntity result = await _service.AddSlotAsync(package.Id, category.Id, true, 2, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(package);
        package.Slots.Should().ContainSingle(slot => slot.CategoryId == category.Id && slot.Quantity == 2);
    }

    [Fact]
    public async Task AddSlotAsync_WithoutACategory_ShouldAddAFreeSlotWithoutLookingOneUp()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        _packageRepositoryMock.SetupGetByIdOrThrow(package);

        // Act
        await _service.AddSlotAsync(package.Id, null, false, 1, CancellationToken.None);

        // Assert
        package.Slots.Should().ContainSingle(slot => slot.CategoryId == null);
        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AddSlotAsync_WhenTheCategoryDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        Guid categoryId = Guid.NewGuid();
        _packageRepositoryMock.SetupGetByIdOrThrow(package);
        _categoryRepositoryMock.SetupGetByIdAsync(categoryId, null);

        // Act
        Func<Task> act = async () =>
            await _service.AddSlotAsync(package.Id, categoryId, true, 1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        package.Slots.Should().BeEmpty();
    }
}
