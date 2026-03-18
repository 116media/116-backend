using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Unit tests for <see cref="AdminAddItemTierHandler"/>.
/// </summary>
public class AdminAddItemTierHandlerTests
{
    private readonly Mock<IAddItemTierFactory> _factoryMock;
    private readonly AdminAddItemTierHandler _handler;

    public AdminAddItemTierHandlerTests()
    {
        _factoryMock = MockAddItemTierFactory.Create();
        _handler = new AdminAddItemTierHandler(_factoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnItemTierDto()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        Guid pricingTierId = Guid.NewGuid();
        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(orderItemId, pricingTierId);
        const string tierName = TestConstants.Content.PricingTier.ValidName;

        _factoryMock.SetupAttachTierAsync((tier, tierName));

        var command = new AdminAddItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            OrderItemId: orderItemId.ToString(),
            PricingTierId: pricingTierId.ToString()
        );

        // Act
        AdminAddItemTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tier.Should().NotBeNull();
        result.Tier.TierName.Should().Be(tierName);
        result.Tier.PriceSnapshotUsd.Should().Be(TestConstants.Content.Commerce.ValidTierPriceUsd);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenFactoryThrows_ShouldPropagateNotFoundException()
    {
        // Arrange
        _factoryMock.SetupAttachTierAsyncThrows(new NotFoundException("Pricing tier was not found."));

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
