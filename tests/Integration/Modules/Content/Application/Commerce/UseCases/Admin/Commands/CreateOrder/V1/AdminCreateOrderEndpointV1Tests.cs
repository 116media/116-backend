using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;

/// <summary>
/// Integration tests for the AdminCreateOrder endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateOrderEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateOrder_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            CustomerEntity entity = CustomerFactory.Create();
            ctx.Customers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder()
            .WithCustomerId(customer.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateOrderResponse>();
        body.Order.Id.Should().NotBeEmpty();
        body.Order.CustomerName.Should().Be(customer.FullName);
        body.Order.Status.Should().Be(EnumOrderStatus.Draft);
        body.Order.ItemCount.Should().Be(0);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(body.Order.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(customer.Id);
        persisted.Status.Should().Be(EnumOrderStatus.Draft);
    }

    [Fact]
    public async Task CreateOrder_AsAdmin_WithValidData_ReturnsCreated()
    {
        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            CustomerEntity entity = CustomerFactory.Create();
            ctx.Customers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder()
            .WithCustomerId(customer.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateOrderResponse>();
        body.Order.Id.Should().NotBeEmpty();
        body.Order.CustomerName.Should().Be(customer.FullName);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(body.Order.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public async Task CreateOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentCustomer_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Customer"))
        );
    }
}
