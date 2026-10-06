using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.Services;

/// <summary>
/// Unit tests for <see cref="CategoryPricingDtoService"/>: the tier resolution behind each row.
/// </summary>
public class CategoryPricingDtoServiceTests : BaseContentHandlerTest
{
    private readonly Mock<IPricingTierRepository> _pricingTierRepositoryMock = MockPricingTierRepository.Create();
    private readonly CategoryPricingDtoService _service;

    public CategoryPricingDtoServiceTests()
    {
        _service = new CategoryPricingDtoService(Mapper, _pricingTierRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldNameTheRowsTier()
    {
        // Arrange
        PricingTierEntity tier = PricingTierFactory.Create("Gold");
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        category.SetPricing(pricingTierId: tier.Id, priceUsd: 40m);
        _pricingTierRepositoryMock.SetupGetPricingTierByIdOrThrow(tier);

        // Act
        CategoryPricingDto dto = await _service.CreateAsync(category.FindPricing(tier.Id)!, CancellationToken.None);

        // Assert
        dto.TierName.Should().Be("Gold");
        dto.PriceUsd.Should().Be(40m);
    }

    [Fact]
    public async Task CreateManyAsync_ShouldResolveEveryTierInOneBatchOrderedByTier()
    {
        // Arrange
        PricingTierEntity first = PricingTierFactory.Create("First");
        PricingTierEntity second = PricingTierFactory.Create("Second");
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        category.SetPricing(pricingTierId: first.Id, priceUsd: 10m);
        category.SetPricing(pricingTierId: second.Id, priceUsd: 20m);
        _pricingTierRepositoryMock.SetupGetByIds(first, second);

        // Act
        IReadOnlyList<CategoryPricingDto> dtos = await _service.CreateManyAsync(
            category.Pricing,
            CancellationToken.None
        );

        // Assert
        dtos.Should().HaveCount(2);
        dtos.Select(dto => dto.TierId).Should().BeInAscendingOrder();
        dtos.Should().OnlyContain(dto => dto.TierName == "First" || dto.TierName == "Second");
    }
}
