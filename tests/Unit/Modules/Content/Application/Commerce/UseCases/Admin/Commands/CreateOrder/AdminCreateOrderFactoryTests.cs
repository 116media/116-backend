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
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly AdminCreateOrderFactory _factory;

    public AdminCreateOrderFactoryTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _factory = new AdminCreateOrderFactory(_categoryRepositoryMock.Object, _contentTypeRepositoryMock.Object);
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
        PackageSlotEntity slot = new PackageSlotBuilder(package)
            .WithCategory(category)
            .WithIsRequired(true)
            .WithQuantity(1)
            .Build();

        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category, Guid.NewGuid(), 50m);

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
        PackageSlotEntity slot = new PackageSlotBuilder(package)
            .WithCategory(category)
            .WithIsRequired(false)
            .WithQuantity(1)
            .Build();

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
            package,
            categoryId: null,
            isRequired: true,
            quantity: 2
        );

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
        PackageSlotEntity slot = new PackageSlotBuilder(package)
            .WithCategory(category)
            .WithIsRequired(true)
            .WithQuantity(3)
            .Build();

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
        PackageSlotEntity slot = new PackageSlotBuilder(package)
            .WithCategory(category)
            .WithIsRequired(false)
            .WithQuantity(1)
            .Build();

        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category, Guid.NewGuid(), 100m);

        // Act
        await _factory.PopulateFromPackageAsync(order, package, CancellationToken.None);

        // Assert — bonus item's tier should not contribute to total
        order.TotalAmountUsd.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task PopulateFromPackageAsync_WithCustomContentType_ShouldUseCustomEnum()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CreateCategoryWithContentType(contentTypeId, "PhotoShoot");
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = new PackageSlotBuilder(package)
            .WithCategory(category)
            .WithIsRequired(true)
            .WithQuantity(1)
            .Build();

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
    /// <summary>
    /// Builds a category of the named content type and arranges both batch lookups to resolve it.
    /// </summary>
    private CategoryEntity CreateCategoryWithContentType(Guid contentTypeId, string contentTypeName)
    {
        CategoryEntity category = new CategoryBuilder(contentTypeId).Build();
        _contentTypeRepositoryMock.SetupGetByIds(ContentTypeEntity.Create(contentTypeId, contentTypeName));
        _categoryRepositoryMock.SetupGetByIds(category);

        return category;
    }
}
