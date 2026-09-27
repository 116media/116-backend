using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags.V1;
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

namespace _116.Content.Integration.Tests.Application.Lookup.UseCases.Admin.Queries.GetAllTags.V1;

/// <summary>
/// Integration tests for the AdminGetAllTags endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllTags_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Tags);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllTags_AsAdmin_ReturnsSeededTag()
    {
        TagEntity tag = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create();
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Tags);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == tag.Id && t.Name == tag.Name && t.Slug == tag.Slug);
    }

    [Fact]
    public async Task GetAllTags_AsSuperAdmin_WithSearch_ReturnsMatchingTags()
    {
        TagEntity matching = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create("test tag", "test-tag");
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Tags}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == matching.Id);
        body.Tags.Should()
            .OnlyContain(t =>
                t.Name.Contains("test", StringComparison.OrdinalIgnoreCase)
                || t.Slug.Contains("test", StringComparison.OrdinalIgnoreCase)
            );
    }
}
