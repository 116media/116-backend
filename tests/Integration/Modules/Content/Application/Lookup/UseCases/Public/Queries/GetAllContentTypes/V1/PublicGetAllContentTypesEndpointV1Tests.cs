using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;

/// <summary>
/// Integration tests for the PublicGetAllContentTypes endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetAllContentTypesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllContentTypes_AsAnonymous_ReturnsActiveContentTypes()
    {
        ContentTypeEntity active = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllContentTypesResponse>();
        body.ContentTypes.Should().Contain(c => c.Id == active.Id);
        body.ContentTypes.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task GetAllContentTypes_AsVisitor_ExcludesInactiveContentTypes()
    {
        ContentTypeEntity inactive = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.CreateInactive();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllContentTypesResponse>();
        body.ContentTypes.Should().NotContain(c => c.Id == inactive.Id);
        body.ContentTypes.Should().OnlyContain(c => c.IsActive);
    }
}
