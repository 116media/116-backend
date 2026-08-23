using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo.V1;

/// <summary>
/// Integration tests for the AdminRejectVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    private async Task<VideoEntity> GetVideoAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? video = await ctx.Videos.FindAsync(id);
        return video!;
    }

    [Fact]
    public async Task RejectVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminRejectVideoRequest request = new AdminRejectVideoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Videos, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task RejectVideo_AsSuperAdmin_AlreadyRejected_ReturnsConflict()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreateRejected(categoryId));
        Client.AuthenticateAsSuperAdmin();
        AdminRejectVideoRequest request = new AdminRejectVideoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Videos, video.Id),
            request
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<VideoErrorMessage>(m => m.AlreadyRejected())
        );
        (await GetVideoAsync(video.Id)).Status.Should().Be(EnumContentStatus.Rejected);
    }

    [Fact]
    public async Task RejectVideo_AsSuperAdmin_DraftVideo_ReturnsBadRequest()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.Create(categoryId));
        Client.AuthenticateAsSuperAdmin();
        AdminRejectVideoRequest request = new AdminRejectVideoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Videos, video.Id),
            request
        );

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<VideoErrorMessage>(m =>
                m.InvalidStatusTransition(from: nameof(EnumContentStatus.Draft), to: nameof(EnumContentStatus.Rejected))
            )
        );
        (await GetVideoAsync(video.Id)).Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task RejectVideo_AsSuperAdmin_PendingReviewVideo_ReturnsOk()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreatePendingReview(categoryId));
        Client.AuthenticateAsSuperAdmin();
        AdminRejectVideoRequest request = new AdminRejectVideoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Videos, video.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        VideoEntity persisted = await GetVideoAsync(video.Id);
        persisted.Status.Should().Be(EnumContentStatus.Rejected);
        persisted.RejectionReason.Should().Be(request.Reason);
    }
}
