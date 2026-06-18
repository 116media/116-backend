using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes.V1;

/// <summary>
/// Integration tests for the AdminGetAllContentTypes endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllContentTypesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllContentTypes_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllContentTypes_AsAdmin_ReturnsSeededContentType()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllContentTypesResponse>();
        body.ContentTypes.Should().Contain(c => c.Id == contentType.Id && c.Name == contentType.Name);
    }

    [Fact]
    public async Task GetAllContentTypes_AsSuperAdmin_ReturnsSeededContentType()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllContentTypesResponse>();
        body.ContentTypes.Should().Contain(c => c.Id == contentType.Id);
    }

    [Fact]
    public async Task GetAllContentTypes_WithSearchParam_ReturnsMatchingContentTypes()
    {
        ContentTypeEntity matching = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Article");
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.ContentTypes}?search=Article");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllContentTypesResponse>();
        body.ContentTypes.Should().Contain(c => c.Id == matching.Id);
        body.ContentTypes.Should().OnlyContain(c => c.Name.Contains("Article", StringComparison.OrdinalIgnoreCase));
    }
}
