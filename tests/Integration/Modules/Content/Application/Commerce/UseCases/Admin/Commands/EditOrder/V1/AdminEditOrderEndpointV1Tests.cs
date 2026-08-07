using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.V1;

/// <summary>
/// Integration tests for the AdminEditOrder endpoint.
/// </summary>
[Collection("Database")]
public class AdminEditOrderEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task EditOrder_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        CustomerEntity newCustomer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.Customers.Add(newCustomer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new { CustomerId = newCustomer.Id.ToString() };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{order.Id}")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminEditOrderResponse>();
        body.Order.Id.Should().Be(order.Id);
        body.Order.CustomerName.Should().Be(newCustomer.FullName);
        body.Order.Status.Should().Be(EnumOrderStatus.Draft);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(order.Id);
        persisted!.CustomerId.Should().Be(newCustomer.Id);
    }

    [Fact]
    public async Task EditOrder_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { CustomerId = Guid.NewGuid().ToString() };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task EditOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { CustomerId = Guid.NewGuid().ToString() };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
