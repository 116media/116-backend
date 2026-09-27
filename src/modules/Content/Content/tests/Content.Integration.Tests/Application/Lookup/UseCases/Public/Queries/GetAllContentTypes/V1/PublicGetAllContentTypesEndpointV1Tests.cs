using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;

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
    }
}
