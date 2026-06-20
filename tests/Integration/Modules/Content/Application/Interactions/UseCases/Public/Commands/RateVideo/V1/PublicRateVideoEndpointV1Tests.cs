using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RateVideo.V1;

/// <summary>
/// Integration tests for the PublicRateVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicRateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RateVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Stars = (short)4 };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Videos}/{Guid.NewGuid()}/ratings", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RateVideo_AsVisitor_NonExistentVideo_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Stars = (short)4 };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Videos}/{Guid.NewGuid()}/ratings", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RateVideo_AsVisitor_WithValidData_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var video = VideoFactory.CreatePublished(category.Id);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();
        var request = new { Stars = (short)4 };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Videos}/{video.Id}/ratings", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
