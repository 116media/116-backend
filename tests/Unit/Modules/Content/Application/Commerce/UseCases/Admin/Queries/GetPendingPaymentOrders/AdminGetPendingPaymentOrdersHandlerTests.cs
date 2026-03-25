using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;

/// <summary>
/// Unit tests for <see cref="AdminGetPendingPaymentOrdersHandler"/>.
/// </summary>
public class AdminGetPendingPaymentOrdersHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminGetPendingPaymentOrdersHandler _handler;

    public AdminGetPendingPaymentOrdersHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminGetPendingPaymentOrdersHandler(_orderRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedPendingPaymentOrders()
    {
        // Arrange
        List<ContentOrderEntity> orders = ContentOrderFactory.CreateMany(2);
        CustomerEntity customer = CustomerFactory.Create();
        foreach (ContentOrderEntity order in orders)
        {
            typeof(ContentOrderEntity).GetProperty(nameof(ContentOrderEntity.Customer))!.SetValue(order, customer);
        }

        _orderRepositoryMock.SetupGetAllAsync(orders, orders.Count);

        var query = new AdminGetPendingPaymentOrdersQuery(PaginatedRequest: new PaginatedRequest());

        // Act
        AdminGetPendingPaymentOrdersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().NotBeNull();
    }

    #endregion
}
