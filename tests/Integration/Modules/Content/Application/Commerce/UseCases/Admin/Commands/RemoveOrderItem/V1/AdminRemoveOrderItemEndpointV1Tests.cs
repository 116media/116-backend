using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem.V1;

/// <summary>
/// Integration tests for the AdminRemoveOrderItem endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveOrderItemEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemoveOrderItem_AsSuperAdmin_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Orders.Item(order.Id, orderItem.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminRemoveOrderItemResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderItemEntity? persisted = await db.ContentOrderItems.FindAsync(orderItem.Id);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task RemoveOrderItem_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Orders.Item(Guid.NewGuid(), Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that an unknown item identifier on an existing Draft order returns the
    /// <c>ItemNotFound</c> 404 raised by <c>GetItemByIdOrThrowAsync</c> rather than the order
    /// lookup's own 404. The order exists and is in Draft, so both earlier guards in
    /// <c>AdminRemoveOrderItemHandler</c> pass and the response names the order item.
    /// </summary>
    [Fact]
    public async Task RemoveOrderItem_ExistingOrderWithUnknownItem_ReturnsNotFoundNamingTheOrderItem()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var response = await Client.DeleteAsync(Routes.Admin.Orders.Item(order.Id, Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound, "Could not find the requested order item.");
    }

    [Fact]
    public async Task RemoveOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Admin.Orders.Item(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
