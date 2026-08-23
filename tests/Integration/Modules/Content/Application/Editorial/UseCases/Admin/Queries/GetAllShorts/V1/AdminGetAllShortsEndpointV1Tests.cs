using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts.V1;

/// <summary>
/// Integration tests for the AdminGetAllShorts endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllShortsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllShorts_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllShorts_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllShorts_AsAdmin_IsAllowed()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllShortsResponse body = await response.ReadAsAsync<AdminGetAllShortsResponse>();
        body.ShortVideos.Items.Should().Contain(item => item.Id == shortVideo.Id);
        body.ShortVideos.Count.Should().BeGreaterThanOrEqualTo(1);
        body.ShortVideos.PageIndex.Should().Be(0);
        body.ShortVideos.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllShorts_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllShortsResponse body = await response.ReadAsAsync<AdminGetAllShortsResponse>();
        body.ShortVideos.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllShorts_WithSearchQuery_ReturnsFilteredResults()
    {
        ShortVideoEntity matchingShort = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateWithSlug("unique-search-term-short");
            ctx.ShortVideos.Add(entity);
            return entity;
        });
        ShortVideoEntity otherShort = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Shorts}?search={matchingShort.Title}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllShortsResponse body = await response.ReadAsAsync<AdminGetAllShortsResponse>();
        body.ShortVideos.Items.Should().Contain(item => item.Id == matchingShort.Id);
        body.ShortVideos.Items.Should().NotContain(item => item.Id == otherShort.Id);
    }
}
