using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Unit tests for <see cref="AdminAddItemTierService"/>.
/// </summary>
public class AdminAddItemTierServiceTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IPricingTierRepository> _pricingTierRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminAddItemTierService _service;

    public AdminAddItemTierServiceTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _pricingTierRepositoryMock = MockPricingTierRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _service = new AdminAddItemTierService(
            _orderRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _pricingTierRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentOrderErrors(),
            TestErrorsFactory.CreateCategoryErrors()
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
        ContentItemTierEntity existingTier = ContentItemTierFactory.Create(item.Id, Guid.NewGuid(), 40m);
        item.Tiers.Add(existingTier);
        order.Items.Add(item);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        CategoryPricingFactory.Create(category, pricingTier.Id, 100m);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _pricingTierRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        // Act
        (ContentItemTierEntity tier, string tierName) = await _service.AttachTierAsync(
            order.Id,
            item.Id,
            pricingTier.Id,
            CancellationToken.None
        );

        // Assert
        tier.OrderItemId.Should().Be(item.Id);
        tier.PricingTierId.Should().Be(pricingTier.Id);
        tier.PriceSnapshotUsd.Amount.Should().Be(100m);
        tierName.Should().Be(pricingTier.Name);
        order.TotalAmountUsd.Should().Be(existingTier.PriceSnapshotUsd + 100m);
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
            await _service.AttachTierAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task AttachTierAsync_WhenOrderNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity submittedOrder = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdWithItems(submittedOrder);

        // Act
        Func<Task> act = async () =>
            await _service.AttachTierAsync(submittedOrder.Id, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task AttachTierAsync_WhenItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        Func<Task> act = async () =>
            await _service.AttachTierAsync(order.Id, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task AttachTierAsync_WhenCategoryPricingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, category.Id);
        order.Items.Add(item);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _pricingTierRepositoryMock.SetupGetPricingTierByIdOrThrow(pricingTier);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        // Act
        Func<Task> act = async () =>
            await _service.AttachTierAsync(order.Id, item.Id, pricingTier.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task AttachTierAsync_WhenTierAlreadyAttached_ShouldThrowConflictException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        Guid contentTypeId = Guid.NewGuid();
        CategoryEntity category = CategoryFactory.Create(contentTypeId);
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, category.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();

        ContentItemTierEntity existingTier = ContentItemTierFactory.Create(item.Id, pricingTier.Id, 100m);
        item.Tiers.Add(existingTier);
        order.Items.Add(item);

        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        Func<Task> act = async () =>
            await _service.AttachTierAsync(order.Id, item.Id, pricingTier.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        item.Tiers.Should().ContainSingle().Which.Should().Be(existingTier);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
