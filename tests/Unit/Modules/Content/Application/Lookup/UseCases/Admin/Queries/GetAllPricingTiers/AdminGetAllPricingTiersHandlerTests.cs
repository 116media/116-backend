using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
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
    private readonly Mock<IPricingTierRepository> _pricingTierRepositoryMock;
    private readonly AdminGetAllPricingTiersHandler _handler;

    public AdminGetAllPricingTiersHandlerTests()
    {
        _pricingTierRepositoryMock = MockPricingTierRepository.Create();
        _handler = new AdminGetAllPricingTiersHandler(_pricingTierRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithNoSearch_ShouldReturnAllPricingTiers()
    {
        // Arrange
        List<PricingTierEntity> pricingTiers = PricingTierFactory.CreateMany(3);
        _pricingTierRepositoryMock.SetupGetAllPricingTiers(pricingTiers);

        var query = new AdminGetAllPricingTiersQuery(Search: null);

        // Act
        AdminGetAllPricingTiersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PricingTiers.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchToRepository()
    {
        // Arrange
        string searchTerm = TestConstants.PricingTier.ValidName;
        PricingTierEntity tier = PricingTierFactory.CreateDefault();
        _pricingTierRepositoryMock.SetupGetAllPricingTiers(new List<PricingTierEntity> { tier });

        var query = new AdminGetAllPricingTiersQuery(Search: searchTerm);

        // Act
        AdminGetAllPricingTiersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PricingTiers.Should().ContainSingle();
        _pricingTierRepositoryMock.Verify(x => x.GetAllAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _pricingTierRepositoryMock.SetupGetAllPricingTiers(new List<PricingTierEntity>());

        var query = new AdminGetAllPricingTiersQuery();

        // Act
        AdminGetAllPricingTiersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSinglePricingTier_ShouldReturnMappedDto()
    {
        // Arrange
        PricingTierEntity tier = PricingTierFactory.CreateDefault();
        _pricingTierRepositoryMock.SetupGetAllPricingTiers(new List<PricingTierEntity> { tier });

        var query = new AdminGetAllPricingTiersQuery();

        // Act
        AdminGetAllPricingTiersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PricingTiers.Should().ContainSingle();
        result.PricingTiers[0].Name.Should().Be(tier.Name);
    }

    #endregion
}
