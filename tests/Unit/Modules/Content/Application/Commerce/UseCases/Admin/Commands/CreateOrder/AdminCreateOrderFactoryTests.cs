using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Unit tests for <see cref="AdminCreateOrderFactory"/>.
/// </summary>
public class AdminCreateOrderFactoryTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly AdminCreateOrderFactory _factory;

    public AdminCreateOrderFactoryTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _factory = new AdminCreateOrderFactory(_categoryRepositoryMock.Object);
    }

    #region PopulateFromPackageAsync Tests

    [Fact]
    public async Task PopulateFromPackageAsync_WithRequiredSlot_ShouldCreateNonBonusItem()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CreateCategoryWithContentType(contentTypeId, "Video");
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = new PackageSlotBuilder(package.Id)
            .WithCategory(category)
            .WithIsRequired(true)
            .WithQuantity(1)
            .Build();
        package.Slots.Add(slot);

        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category.Id, Guid.NewGuid(), 50m);
        _categoryRepositoryMock.SetupGetPricingByCategories(new List<CategoryPricingEntity> { pricing });

        // Act
        int count = await _factory.PopulateFromPackageAsync(order, package, CancellationToken.None);

        // Assert
        count.Should().Be(1);
        order.Items.Should().ContainSingle();
        order.Items.First().IsBonus.Should().BeFalse();
        order.Items.First().CategoryId.Should().Be(category.Id);
        order.Items.First().Tiers.Should().ContainSingle();
    }

    [Fact]
    public async Task PopulateFromPackageAsync_WithBonusSlot_ShouldCreateBonusItem()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CreateCategoryWithContentType(contentTypeId, "Article");
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = new PackageSlotBuilder(package.Id)
            .WithCategory(category)
            .WithIsRequired(false)
            .WithQuantity(1)
            .Build();
        package.Slots.Add(slot);

        _categoryRepositoryMock.SetupGetPricingByCategories(new List<CategoryPricingEntity>());

        // Act
        int count = await _factory.PopulateFromPackageAsync(order, package, CancellationToken.None);

        // Assert
        count.Should().Be(1);
        order.Items.First().IsBonus.Should().BeTrue();
    }

    [Fact]
    public async Task PopulateFromPackageAsync_WithOpenSlot_ShouldSkip()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity openSlot = PackageSlotFactory.Create(
            package.Id,
            categoryId: null,
            isRequired: true,
            quantity: 2
        );
        package.Slots.Add(openSlot);

        // Act
        int count = await _factory.PopulateFromPackageAsync(order, package, CancellationToken.None);

        // Assert
        count.Should().Be(0);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task PopulateFromPackageAsync_WithQuantityThree_ShouldCreateThreeItems()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CreateCategoryWithContentType(contentTypeId, "Video");
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = new PackageSlotBuilder(package.Id)
            .WithCategory(category)
            .WithIsRequired(true)
            .WithQuantity(3)
            .Build();
        package.Slots.Add(slot);

        _categoryRepositoryMock.SetupGetPricingByCategories(new List<CategoryPricingEntity>());

        // Act
        int count = await _factory.PopulateFromPackageAsync(order, package, CancellationToken.None);

        // Assert
        count.Should().Be(3);
        order.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task PopulateFromPackageAsync_BonusItemTiers_ShouldNotAffectTotal()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CreateCategoryWithContentType(contentTypeId, "Video");
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = new PackageSlotBuilder(package.Id)
            .WithCategory(category)
            .WithIsRequired(false)
            .WithQuantity(1)
            .Build();
        package.Slots.Add(slot);

        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category.Id, Guid.NewGuid(), 100m);
        _categoryRepositoryMock.SetupGetPricingByCategories(new List<CategoryPricingEntity> { pricing });

        // Act
        await _factory.PopulateFromPackageAsync(order, package, CancellationToken.None);

        // Assert — bonus item's tier should not contribute to total
        order.TotalAmountUsd.Should().Be(0m);
    }

    [Fact]
    public async Task PopulateFromPackageAsync_WithCustomContentType_ShouldUseCustomEnum()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CreateCategoryWithContentType(contentTypeId, "PhotoShoot");
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = new PackageSlotBuilder(package.Id)
            .WithCategory(category)
            .WithIsRequired(true)
            .WithQuantity(1)
            .Build();
        package.Slots.Add(slot);

        _categoryRepositoryMock.SetupGetPricingByCategories(new List<CategoryPricingEntity>());

        // Act
        await _factory.PopulateFromPackageAsync(order, package, CancellationToken.None);

        // Assert — unknown content type should fall back to Custom
        order.Items.First().ContentKind.Should().Be(EnumCoreContentType.Custom);
    }

    #endregion

    /// <summary>
    /// Creates a category carrying the ContentType navigation EF Core would populate, which the
    /// order factory reads to decide the content kind of each generated item.
    /// </summary>
    private static CategoryEntity CreateCategoryWithContentType(Guid contentTypeId, string contentTypeName) =>
        new CategoryBuilder(contentTypeId)
            .WithContentType(ContentTypeEntity.Create(contentTypeId, contentTypeName))
            .Build();
}
