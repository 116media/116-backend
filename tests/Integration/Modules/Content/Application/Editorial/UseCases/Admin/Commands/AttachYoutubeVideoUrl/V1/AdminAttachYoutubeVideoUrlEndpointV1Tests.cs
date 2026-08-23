using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;

/// <summary>
/// Integration tests for the AdminAttachYoutubeVideoUrl endpoint.
/// </summary>
[Collection("Database")]
public class AdminAttachYoutubeVideoUrlEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private const string ValidYoutubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

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
    public async Task AttachYoutubeVideoUrl_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { YoutubeVideoUrl = ValidYoutubeUrl }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { YoutubeVideoUrl = ValidYoutubeUrl }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl(ValidYoutubeUrl)
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.Create(categoryId));
        Client.AuthenticateAsSuperAdmin();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl(ValidYoutubeUrl)
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, video.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminAttachYoutubeVideoUrlResponse body = await response.ReadAsAsync<AdminAttachYoutubeVideoUrlResponse>();
        body.Video.Id.Should().Be(video.Id);
        body.Video.YoutubeVideoUrl.Should().Be(request.YoutubeVideoUrl);

        VideoEntity persisted = await GetVideoAsync(video.Id);
        persisted.YoutubeVideoUrl.Should().Be(request.YoutubeVideoUrl);
        persisted.ThumbnailFileId.Should().NotBeNull();
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_BeforeShootDate_ReturnsBadRequest()
    {
        VideoEntity video = await SeedVideoAsync(categoryId =>
            new VideoBuilder(categoryId).WithShootingScheduledAt(DateTimeOffset.UtcNow.AddDays(30)).Build()
        );
        Client.AuthenticateAsSuperAdmin();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl(ValidYoutubeUrl)
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, video.Id),
            request
        );

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<VideoErrorMessage>(m => m.CannotAttachYoutubeUrlBeforeShoot(video.ShootingScheduledAt!.Value))
        );
        (await GetVideoAsync(video.Id)).YoutubeVideoUrl.Should().BeNull();
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_WithInvalidYoutubeUrl_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminAttachYoutubeVideoUrlRequest request = new AdminAttachYoutubeVideoUrlRequestBuilder()
            .WithYoutubeVideoUrl("not-a-valid-url")
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, id),
            request
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("YoutubeVideoUrl", Localized<VideoErrorMessage>(m => m.YoutubeUrlInvalidFormat()))
        );
    }
}
