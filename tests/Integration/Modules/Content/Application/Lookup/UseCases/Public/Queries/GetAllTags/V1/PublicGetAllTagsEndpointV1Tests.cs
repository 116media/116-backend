using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags.V1;

/// <summary>
/// Integration tests for the PublicGetAllTags endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetAllTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllTags_AsAnonymous_ReturnsSeededTag()
    {
        TagEntity tag = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create();
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Tags);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == tag.Id && t.Name == tag.Name && t.Slug == tag.Slug);
    }

    [Fact]
    public async Task GetAllTags_WithSearchParam_ReturnsMatchingTags()
    {
        TagEntity matching = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create("test tag", "test-tag");
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Tags}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetAllTagsResponse>();
        body.Tags.Should().Contain(t => t.Id == matching.Id);
        body.Tags.Should()
            .OnlyContain(t =>
                t.Name.Contains("test", StringComparison.OrdinalIgnoreCase)
                || t.Slug.Contains("test", StringComparison.OrdinalIgnoreCase)
            );
    }
}
