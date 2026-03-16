using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPricingTiersHandler"/>.
/// </summary>
public class AdminGetAllPricingTiersHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly AdminGetAllPricingTiersHandler _handler;

    public AdminGetAllPricingTiersHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _handler = new AdminGetAllPricingTiersHandler(_lookupRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithMultiplePricingTiers_ShouldReturnAllMapped()
    {
        // Arrange
        List<PricingTierEntity> pricingTiers = PricingTierFactory.CreateMany(3);
        _lookupRepositoryMock.SetupGetAllPricingTiers(pricingTiers);

        var query = new AdminGetAllPricingTiersQuery();

        // Act
        AdminGetAllPricingTiersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PricingTiers.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _lookupRepositoryMock.SetupGetAllPricingTiers(new List<PricingTierEntity>());

        var query = new AdminGetAllPricingTiersQuery();

        // Act
        AdminGetAllPricingTiersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSinglePricingTier_ShouldReturnMappedDto()
    {
        // Arrange
        PricingTierEntity tier = PricingTierFactory.CreateDefault();
        _lookupRepositoryMock.SetupGetAllPricingTiers(new List<PricingTierEntity> { tier });

        var query = new AdminGetAllPricingTiersQuery();

        // Act
        AdminGetAllPricingTiersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PricingTiers.Should().ContainSingle();
        result.PricingTiers[0].Name.Should().Be(tier.Name);
    }

    #endregion
}
