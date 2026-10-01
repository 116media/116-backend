using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AdminAddCategoryPricingService"/>: the tier and duplicate gates and the
/// price set through the category aggregate.
/// </summary>
public class AdminAddCategoryPricingServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<IPricingTierRepository> _pricingTierRepositoryMock = MockPricingTierRepository.Create();
    private readonly AdminAddCategoryPricingService _service;

    public AdminAddCategoryPricingServiceTests()
    {
        _service = new AdminAddCategoryPricingService(
            _categoryRepositoryMock.Object,
            _pricingTierRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task AddAsync_ShouldSetThePriceAndReturnTheRowWithItsTier()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        PricingTierEntity tier = PricingTierFactory.Create();
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _pricingTierRepositoryMock.SetupGetPricingTierByIdOrThrow(tier);

        // Act
        CategoryPricingData added = await _service.AddAsync(category.Id, tier.Id, 25m, CancellationToken.None);

        // Assert
        added.Tier.Should().BeSameAs(tier);
        added.Pricing.PricingTierId.Should().Be(tier.Id);
        added.Pricing.PriceUsd.Amount.Should().Be(25m);
        category.FindPricing(tier.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WhenTierIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        PricingTierEntity tier = PricingTierFactory.CreateInactive();
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _pricingTierRepositoryMock.SetupGetPricingTierByIdOrThrow(tier);

        // Act
        Func<Task> act = async () => await _service.AddAsync(category.Id, tier.Id, 25m, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        category.FindPricing(tier.Id).Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WhenTheRowAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        PricingTierEntity tier = PricingTierFactory.Create();
        category.SetPricing(pricingTierId: tier.Id, priceUsd: 10m);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _pricingTierRepositoryMock.SetupGetPricingTierByIdOrThrow(tier);

        // Act
        Func<Task> act = async () => await _service.AddAsync(category.Id, tier.Id, 25m, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        category.FindPricing(tier.Id)!.PriceUsd.Amount.Should().Be(10m);
    }
}
