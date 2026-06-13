using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof.V1;

/// <summary>
/// Integration tests for the AdminAttachPaymentProof endpoint.
/// </summary>
[Collection("Database")]
public class AdminAttachPaymentProofEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
}
