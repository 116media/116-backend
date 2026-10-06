using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Unit tests for <see cref="AdminCreateOrderService"/>.
/// </summary>
public class AdminCreateOrderServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
    private readonly AdminCreateOrderService _service;

    public AdminCreateOrderServiceTests()
    {
        _customerRepositoryMock = MockCustomerRepository.Create();
        _packageRepositoryMock = MockPackageRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _contentTypeRepositoryMock = MockContentTypeRepository.Create();
        _service = new AdminCreateOrderService(
            _customerRepositoryMock.Object,
            _packageRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _contentTypeRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithoutPackage_ShouldStageAnEmptyOrderForTheCustomer()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.CreateDefault();
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        CreatedOrderData created = await _service.CreateAsync(customer.Id, null, CancellationToken.None);

        // Assert
        created.Customer.Should().BeSameAs(customer);
        created.Order.CustomerId.Should().Be(customer.Id);
        created.ItemCount.Should().Be(0);
        _orderRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task CreateAsync_WithAnActivePackage_ShouldSeedTheOrderFromIt()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        PackageEntity package = PackageFactory.Create();
        PackageSlotFactory.Create(package, Guid.NewGuid(), true, 2);
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);
        _categoryRepositoryMock.SetupGetByIds([]);
        _contentTypeRepositoryMock.SetupGetByIds([]);

        // Act
        CreatedOrderData created = await _service.CreateAsync(customer.Id, package.Id, CancellationToken.None);

        // Assert
        created.Order.PackageId.Should().Be(package.Id);
        created.ItemCount.Should().Be(2);
        created.Order.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_WhenCustomerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(customerId, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenPackageNotFound_ShouldThrowNotFoundExceptionWithoutStaging()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        Guid packageId = Guid.NewGuid();
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageEntity?)null);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(customer.Id, packageId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _orderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region PopulateFromPackageAsync Tests

    [Fact]
    public async Task PopulateFromPackageAsync_WithRequiredSlot_ShouldCreateNonBonusItem()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CreateCategoryWithContentType(contentTypeId, "Video");
        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity slot = PackageSlotFactory.Create(package, category.Id, true, 1);

        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category, Guid.NewGuid(), 50m);

        // Act
        int count = await _service.PopulateFromPackageAsync(order, package, CancellationToken.None);

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
        PackageSlotEntity slot = PackageSlotFactory.Create(package, category.Id, false, 1);

        // Act
        int count = await _service.PopulateFromPackageAsync(order, package, CancellationToken.None);

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
        int count = await _service.PopulateFromPackageAsync(order, package, CancellationToken.None);

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
        PackageSlotEntity slot = PackageSlotFactory.Create(package, category.Id, true, 3);

        // Act
        int count = await _service.PopulateFromPackageAsync(order, package, CancellationToken.None);

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
        PackageSlotEntity slot = PackageSlotFactory.Create(package, category.Id, false, 1);

        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category, Guid.NewGuid(), 100m);

        // Act
        await _service.PopulateFromPackageAsync(order, package, CancellationToken.None);

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
        PackageSlotEntity slot = PackageSlotFactory.Create(package, category.Id, true, 1);

        // Act
        await _service.PopulateFromPackageAsync(order, package, CancellationToken.None);

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
