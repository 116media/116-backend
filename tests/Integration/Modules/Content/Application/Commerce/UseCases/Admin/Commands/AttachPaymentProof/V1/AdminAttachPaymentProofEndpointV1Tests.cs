using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(order.Id)}?paymentMethod=BankTransfer",
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminAttachPaymentProofResponse>();
        body.Proof.Id.Should().NotBeEmpty();
        body.Proof.MimeType.Should().NotBeNullOrEmpty();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentPaymentEntity? persisted = await db.ContentPayments.FindAsync(payment.Id);
        persisted.Should().NotBeNull();
        persisted!.PaymentProofFileId.Should().Be(body.Proof.Id);
        persisted.PaymentMethod.Should().Be(EnumPaymentMethod.BankTransfer);
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
            $"{Routes.Admin.Orders.PaymentProof(Guid.NewGuid())}?paymentMethod=BankTransfer",
            content
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
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
            $"{Routes.Admin.Orders.PaymentProof(Guid.NewGuid())}?paymentMethod=BankTransfer",
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
