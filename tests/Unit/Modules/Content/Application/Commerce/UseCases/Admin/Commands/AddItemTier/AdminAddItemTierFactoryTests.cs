using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Unit tests for <see cref="AdminAddItemTierFactory"/>.
/// </summary>
public class AdminAddItemTierFactoryTests
{
    private readonly Mock<_116.Content.Application.Shared.Repositories.IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<_116.Content.Application.Shared.Repositories.ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<_116.Content.Application.Shared.Repositories.ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<_116.Content.Application.Shared.Persistence.IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminAddItemTierFactory _factory;

    public AdminAddItemTierFactoryTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _factory = new AdminAddItemTierFactory(
            _orderRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task AttachTierAsync_WhenAllValid_ShouldReturnTierWithTierName()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, category.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        CategoryPricingEntity categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 100m);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderRepositoryMock.SetupGetItemById(order.Id, item.Id, item);
        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);
        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, categoryPricing);

        // Act
        (ContentItemTierEntity tier, string tierName) = await _factory.AttachTierAsync(
            order.Id,
            item.Id,
            pricingTier.Id,
            CancellationToken.None
        );

        // Assert
        tier.Should().NotBeNull();
        tier.OrderItemId.Should().Be(item.Id);
        tier.PricingTierId.Should().Be(pricingTier.Id);
        tier.PriceSnapshotUsd.Should().Be(categoryPricing.PriceUsd);
        tierName.Should().Be(pricingTier.Name);
        _orderRepositoryMock.VerifyAddItemTierCalled();
        _orderRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task AttachTierAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _orderRepositoryMock.SetupGetByIdWithItems(null);

        // Act
        Func<Task> act = async () =>
            await _factory.AttachTierAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AttachTierAsync_WhenOrderNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity submittedOrder = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdWithItems(submittedOrder);

        // Act
        Func<Task> act = async () =>
            await _factory.AttachTierAsync(submittedOrder.Id, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AttachTierAsync_WhenItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderRepositoryMock.SetupGetItemById(order.Id, Guid.NewGuid(), null);

        // Act
        Func<Task> act = async () =>
            await _factory.AttachTierAsync(order.Id, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AttachTierAsync_WhenCategoryPricingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, category.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderRepositoryMock.SetupGetItemById(order.Id, item.Id, item);
        _lookupRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);
        _categoryRepositoryMock.SetupGetPricing(category.Id, pricingTier.Id, null);

        // Act
        Func<Task> act = async () =>
            await _factory.AttachTierAsync(order.Id, item.Id, pricingTier.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
