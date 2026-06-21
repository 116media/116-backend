using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment.V1;

/// <summary>
/// Integration tests for the AdminRejectPayment endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectPaymentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RejectPayment_AsSuperAdmin_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var order = ContentOrderFactory.CreateSubmitted();
        var customer = CustomerFactory.CreateWithId(order.CustomerId);
        seedContext.Customers.Add(customer);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var proofFileId = Guid.NewGuid();
        var payment = ContentPaymentFactory.CreateWithProof(order.Id, proofFileId);
        seedContext.ContentPayments.Add(payment);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Payment proof is unclear, please resubmit" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{order.Id}/payment/reject")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RejectPayment_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Invalid payment" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/payment/reject")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectPayment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Notes = "Invalid payment" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/payment/reject")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that rejecting an already-rejected payment returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task RejectPayment_WhenAlreadyRejected_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var order = ContentOrderFactory.CreateSubmitted();
        var customer = CustomerFactory.CreateWithId(order.CustomerId);
        seedContext.Customers.Add(customer);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var payment = ContentPaymentFactory.CreateRejected(order.Id);
        seedContext.ContentPayments.Add(payment);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Notes = "Rejecting again" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{order.Id}/payment/reject")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
