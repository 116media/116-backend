using System.Net.Http.Headers;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Errors.Messages;
using _116.Core.Contracts.Domain.Enums;
using _116.Integration.Tests.Common.Stubs;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Drives the real provider adapter over real HTTP: every upload here runs through
/// <c>CloudinaryService</c>, <c>CloudinaryStorageClient</c>, its resilience pipeline and the
/// provider SDK, stopping only at the stubbed transport. Proves each asset is addressed under
/// the resource type its kind implies, and that a provider refusal surfaces as a localized
/// gateway problem instead of a 500.
/// </summary>
[Collection("Database")]
public class CloudStorageAdapterFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    private StubCloudinaryEndpoint CloudinaryStub => Api.Services.GetRequiredService<StubCloudinaryEndpoint>();

    private static MultipartFormDataContent FilePart(string fileName, string mimeType)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]);
        file.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        content.Add(file, "file", fileName);
        return content;
    }

    private async Task<AlbumEntity> SeedAlbumAsync()
    {
        return await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            AlbumEntity album = AlbumFactory.Create();
            ctx.Albums.Add(album);
            return album;
        });
    }

    private async Task<ShortVideoEntity> SeedShortVideoAsync()
    {
        return await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity shortVideo = ShortVideoFactory.CreateDraft();
            ctx.ShortVideos.Add(shortVideo);
            return shortVideo;
        });
    }

    private async Task<ContentOrderEntity> SeedOrderAwaitingProofAsync()
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

        return order;
    }

    [Fact]
    public async Task UploadImage_ShouldStoreItUnderTheImageResourceType()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsSuperAdmin();

        using MultipartFormDataContent content = FilePart("cover.jpg", "image/jpeg");

        var response = await Client.PostAsync(Routes.Admin.Albums.Cover(album.Id), content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CloudinaryStub.UploadedKinds.Should().Equal(EnumStoredFileKind.Image);
    }

    [Fact]
    public async Task UploadVideo_ShouldStoreItUnderTheVideoResourceType()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsSuperAdmin();

        using MultipartFormDataContent content = FilePart("clip.mp4", "video/mp4");

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, shortVideo.Id),
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CloudinaryStub.UploadedKinds.Should().Equal(EnumStoredFileKind.Video);
    }

    [Fact]
    public async Task UploadPdf_ShouldStoreItUnderTheRawResourceType()
    {
        ContentOrderEntity order = await SeedOrderAwaitingProofAsync();
        Client.AuthenticateAsSuperAdmin();

        using MultipartFormDataContent content = FilePart("proof.pdf", "application/pdf");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(order.Id)}?paymentMethod=BankTransfer",
            content
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CloudinaryStub.UploadedKinds.Should().Equal(EnumStoredFileKind.Raw);
    }

    [Fact]
    public async Task UploadImage_WhenTheProviderRefusesIt_ShouldReturnALocalizedGatewayProblem()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextUploadError = "quota exceeded";

        using MultipartFormDataContent content = FilePart("cover.jpg", "image/jpeg");

        var response = await Client.PostAsync(Routes.Admin.Albums.Cover(album.Id), content);

        await response.ShouldBeProblem<BadGatewayException>(
            HttpStatusCode.BadGateway,
            Localized<ValidationErrorMessage>(m => m.FileUploadFailed("quota exceeded"))
        );
    }

    [Fact]
    public async Task UploadImage_WhenTheProviderRefusesIt_ShouldLeaveTheAlbumWithoutACover()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextUploadError = "quota exceeded";

        using MultipartFormDataContent content = FilePart("cover.jpg", "image/jpeg");

        await Client.PostAsync(Routes.Admin.Albums.Cover(album.Id), content);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        AlbumEntity? persisted = await ctx.Albums.FindAsync(album.Id);
        persisted!.CoverImageFileId.Should().BeNull();
    }

    [Fact]
    public async Task UploadVideo_WhenTheProviderRefusesIt_ShouldReturnALocalizedGatewayProblem()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextUploadError = "transcoding unavailable";

        using MultipartFormDataContent content = FilePart("clip.mp4", "video/mp4");

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, shortVideo.Id),
            content
        );

        await response.ShouldBeProblem<BadGatewayException>(
            HttpStatusCode.BadGateway,
            Localized<ValidationErrorMessage>(m => m.FileUploadFailed("transcoding unavailable"))
        );
    }

    [Fact]
    public async Task UploadPdf_WhenTheProviderRefusesIt_ShouldReturnALocalizedGatewayProblem()
    {
        ContentOrderEntity order = await SeedOrderAwaitingProofAsync();
        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextUploadError = "raw uploads disabled";

        using MultipartFormDataContent content = FilePart("proof.pdf", "application/pdf");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Orders.PaymentProof(order.Id)}?paymentMethod=BankTransfer",
            content
        );

        await response.ShouldBeProblem<BadGatewayException>(
            HttpStatusCode.BadGateway,
            Localized<ValidationErrorMessage>(m => m.FileUploadFailed("raw uploads disabled"))
        );
    }
}
