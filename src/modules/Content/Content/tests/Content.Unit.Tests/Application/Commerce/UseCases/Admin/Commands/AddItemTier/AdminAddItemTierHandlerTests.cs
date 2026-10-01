using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Services;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Unit tests for <see cref="AdminAddItemTierHandler"/>.
/// </summary>
public class AdminAddItemTierHandlerTests
{
    private readonly Mock<IAddItemTierService> _serviceMock;
    private readonly AdminAddItemTierHandler _handler;

    public AdminAddItemTierHandlerTests()
    {
        _serviceMock = MockAddItemTierService.Create();
        _handler = new AdminAddItemTierHandler(_serviceMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnItemTierDto()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        Guid pricingTierId = Guid.NewGuid();
        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(orderItemId, pricingTierId);
        const string tierName = TestConstants.PricingTier.ValidName;

        _serviceMock.SetupAttachTierAsync((tier, tierName));

        var command = new AdminAddItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            OrderItemId: orderItemId.ToString(),
            PricingTierId: pricingTierId.ToString()
        );

        // Act
        AdminAddItemTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Tier.TierName.Should().Be(tierName);
        result.Tier.PriceSnapshotUsd.Should().Be(TestConstants.Commerce.ValidTierPriceUsd);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenFactoryThrows_ShouldPropagateNotFoundException()
    {
        // Arrange
        _serviceMock.SetupAttachTierAsyncThrows(new NotFoundException("Pricing tier was not found."));

        var command = new AdminAddItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            OrderItemId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
