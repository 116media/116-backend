using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;

/// <summary>
/// Integration tests for the AdminVerifyPayment endpoint.
/// </summary>
[Collection("Database")]
public class AdminVerifyPaymentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task VerifyPayment_AsSuperAdmin_ReturnsOk()
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
        var request = new { ReceiptUrl = "https://example.com/receipt/12345" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{order.Id}/payment/verify")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task VerifyPayment_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { ReceiptUrl = "https://example.com/receipt/12345" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/payment/verify")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyPayment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { ReceiptUrl = "https://example.com/receipt/12345" };
        var msg = new HttpRequestMessage(HttpMethod.Patch, $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/payment/verify")
        {
            Content = JsonContent.Create(request),
        };

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
