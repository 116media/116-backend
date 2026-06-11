using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Api.Commerce;

/// <summary>
/// Integration tests for admin payment command endpoints (attach proof, verify, reject)
/// verifying authorization, validation, and successful operations against a real
/// PostgreSQL database through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminPaymentCommandEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AttachPaymentProof_AsSuperAdmin_WithValidFile_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var payment = ContentPaymentFactory.Create(order.Id);
        seedContext.ContentPayments.Add(payment);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        var response = await Client.PostAsync(
            $"{ApiRoutes.Admin.Orders}/{order.Id}/payment/proof?paymentMethod=BankTransfer",
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AttachPaymentProof_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        var response = await Client.PostAsync(
            $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/payment/proof?paymentMethod=BankTransfer",
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AttachPaymentProof_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        var response = await Client.PostAsync(
            $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/payment/proof?paymentMethod=BankTransfer",
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

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
}
