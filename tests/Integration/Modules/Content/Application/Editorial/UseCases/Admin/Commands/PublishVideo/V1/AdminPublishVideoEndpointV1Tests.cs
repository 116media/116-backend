using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo.V1;

/// <summary>
/// Integration tests for the AdminPublishVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminPublishVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task PublishVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublishVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task PublishVideo_AsSuperAdmin_AlreadyPublished_ReturnsConflict()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreatePublished(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, video.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<VideoErrorMessage>(m => m.AlreadyPublished())
        );
        (await GetVideoAsync(video.Id)).Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public async Task PublishVideo_AsSuperAdmin_DraftVideo_ReturnsBadRequest()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.Create(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, video.Id),
            null
        );

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<VideoErrorMessage>(m =>
                m.InvalidStatusTransition(
                    from: nameof(EnumContentStatus.Draft),
                    to: nameof(EnumContentStatus.Published)
                )
            )
        );
        (await GetVideoAsync(video.Id)).Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task PublishVideo_AsSuperAdmin_ApprovedWithYoutubeUrl_ReturnsOk()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreateApprovedWithYoutubeUrl(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, video.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        VideoEntity persisted = await GetVideoAsync(video.Id);
        persisted.Status.Should().Be(EnumContentStatus.Published);
        persisted.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishVideo_AsSuperAdmin_ApprovedWithoutYoutubeUrl_ReturnsBadRequest()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreateApproved(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, video.Id),
            null
        );

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<VideoErrorMessage>(m => m.CannotPublishWithoutYoutubeUrl())
        );
        (await GetVideoAsync(video.Id)).Status.Should().Be(EnumContentStatus.Approved);
    }
}
