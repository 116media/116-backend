using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo.V1;

/// <summary>
/// Integration tests for the AdminDeleteVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    private async Task<EnumContentStatus?> GetVideoStatusAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? video = await ctx.Videos.FindAsync(id);
        return video?.Status;
    }

    [Fact]
    public async Task DeleteVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task DeleteVideo_AsSuperAdmin_PublishedVideo_ReturnsBadRequest()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.CreatePublished(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{video.Id}");

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<VideoErrorMessage>(m => m.CannotDeletePublishedVideo())
        );
        (await GetVideoStatusAsync(video.Id)).Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public async Task DeleteVideo_AsSuperAdmin_DraftVideo_ReturnsOk()
    {
        VideoEntity video = await SeedVideoAsync(categoryId => VideoFactory.Create(categoryId));
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminDeleteVideoResponse body = await response.ReadAsAsync<AdminDeleteVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        (await GetVideoStatusAsync(video.Id)).Should().BeNull();
    }
}
