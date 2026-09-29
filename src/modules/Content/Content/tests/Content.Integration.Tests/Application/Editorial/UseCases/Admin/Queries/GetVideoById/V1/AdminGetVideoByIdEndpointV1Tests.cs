using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Queries.GetVideoById.V1;

/// <summary>
/// Integration tests for the AdminGetVideoById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetVideoByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync() => await SeedAsync<ContentDbContext, Guid>(ContentSeeder.AddCategory);

    [Fact]
    public async Task GetVideoById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVideoById_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetVideoById_AsAdmin_IsAllowed()
    {
        Guid categoryId = await SeedCategoryAsync();
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetVideoByIdResponse body = await response.ReadAsAsync<AdminGetVideoByIdResponse>();
        body.Video.Id.Should().Be(video.Id);
        body.Video.Title.Should().Be(video.Title);
        body.Video.Slug.Should().Be(video.Slug);
        body.Video.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetVideoById_WithNonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }
}
