using _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder.V1;

/// <summary>
/// Integration tests for the AdminCancelOrder endpoint.
/// </summary>
[Collection("Database")]
public class AdminCancelOrderEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CancelOrder_AsSuperAdmin_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Cancel(order.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminCancelOrderResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(order.Id);
        persisted!.Status.Should().Be(EnumOrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrder_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Cancel(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task CancelOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Cancel(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelOrder_AsSuperAdmin_AlreadyCancelled_ReturnsConflict()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateCancelled();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Cancel(order.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.AlreadyCancelled())
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(order.Id);
        persisted!.Status.Should().Be(EnumOrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrder_AsSuperAdmin_PaidOrder_ReturnsBadRequest()
    {
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        CustomerEntity customer = CustomerFactory.CreateWithId(order.CustomerId);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Orders.Cancel(order.Id), null);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ContentOrderErrorMessage>(m => m.CannotCancelPaidOrder())
        );

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(order.Id);
        persisted!.Status.Should().Be(EnumOrderStatus.Paid);
    }
}
