using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById.V1;

/// <summary>
/// Integration tests for the AdminGetShortById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetShortByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetShortById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetShortById_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetShortById_AsAdmin_IsAllowed()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetShortByIdResponse body = await response.ReadAsAsync<AdminGetShortByIdResponse>();
        body.ShortVideo.Id.Should().Be(shortVideo.Id);
        body.ShortVideo.Title.Should().Be(shortVideo.Title);
        body.ShortVideo.Slug.Should().Be(shortVideo.Slug);
    }

    [Fact]
    public async Task GetShortById_WithNonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }
}
