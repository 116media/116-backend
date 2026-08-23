using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo.V1;

/// <summary>
/// Integration tests for the AdminSubmitVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminSubmitVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<VideoEntity> SeedVideoAsync(Func<Guid, VideoEntity> create)
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);
            return video;
        });
    }

    private async Task<EnumContentStatus> GetVideoStatusAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? video = await ctx.Videos.FindAsync(id);
        return video!.Status;
    }

    [Fact]
    public async Task SubmitVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task SubmitVideo_AsSuperAdmin_AlreadyPendingReview_ReturnsConflict()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreatePendingReview(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Videos, video.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<VideoErrorMessage>(m => m.AlreadyPendingReview())
        );
        (await GetVideoStatusAsync(video.Id)).Should().Be(EnumContentStatus.PendingReview);
    }

    [Fact]
    public async Task SubmitVideo_AsSuperAdmin_FreeVideo_ReturnsOk()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.Create(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Videos, video.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetVideoStatusAsync(video.Id)).Should().Be(EnumContentStatus.PendingReview);
    }

    [Fact]
    public async Task SubmitVideo_AsSuperAdmin_PaidVideoAlreadySubmitted_ReturnsConflict()
    {
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            CustomerEntity customer = CustomerFactory.Create();
            VideoEntity paidVideo = VideoFactory.CreatePaid(category.Id, customer.Id, Guid.NewGuid());
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Customers.Add(customer);
            ctx.Videos.Add(paidVideo);
            return paidVideo;
        });

        Client.AuthenticateAsSuperAdmin();
        string url = Routes.Admin.Editorial.Submit(EditorialRouteConstants.Videos, video.Id);

        var firstResponse = await Client.PatchAsync(url, null);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetVideoStatusAsync(video.Id)).Should().Be(EnumContentStatus.PendingPayment);

        var secondResponse = await Client.PatchAsync(url, null);
        await secondResponse.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<VideoErrorMessage>(m => m.AlreadySubmitted())
        );
    }
}
