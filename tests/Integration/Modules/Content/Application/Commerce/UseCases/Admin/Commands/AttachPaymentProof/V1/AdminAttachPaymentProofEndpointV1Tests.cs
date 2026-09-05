using System.Net.Http.Headers;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof.V1;

/// <summary>
/// Integration tests for the AdminAttachPaymentProof endpoint.
/// </summary>
[Collection("Database")]
public class AdminAttachPaymentProofEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
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
    public async Task AttachPaymentProof_WhenPaymentAlreadyVerified_ReturnsConflictAndKeepsTheOriginalProof()
    {
        // Arrange — proof on a decided payment is the evidence the decision rests on
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(order.Id);
        Guid originalProofId = payment.PaymentProofFileId!.Value;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
            ctx.ContentPayments.Add(payment);
        });

        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        // Act
        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(order.Id)}?paymentMethod=MobileMoney",
            content
        );

        // Assert
        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ContentOrderErrorMessage>(m => m.PaymentAlreadyDecided())
        );

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ContentPayments.FindAsync(payment.Id))!.PaymentProofFileId.Should().Be(originalProofId);
    }

    [Fact]
    public async Task AttachPaymentProof_WithUnknownOrderId_ReturnsOrderNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(Guid.NewGuid())}?paymentMethod=BankTransfer",
            content
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task AttachPaymentProof_WithOrderThatHasNoPayment_ReturnsPaymentNotFound()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(order.Id)}?paymentMethod=BankTransfer",
            content
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentPayment"))
        );
    }

    [Fact]
    public async Task AttachPaymentProof_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "proof.jpg");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(Guid.NewGuid())}?paymentMethod=BankTransfer",
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AttachPaymentProof_WithNoFilePart_ReturnsLocalizedValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("unused"), "note");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(Guid.NewGuid())}?paymentMethod=BankTransfer",
            content
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("File", Localized<ContentOrderErrorMessage>(m => m.PaymentProofRequired()))
        );
    }
}
