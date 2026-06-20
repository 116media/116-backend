using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.V1;

/// <summary>
/// Integration tests for the AdminForceUnpromoteVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminForceUnpromoteVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ForceUnpromoteVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/non-existent-slug/unpromote",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/non-existent-slug/unpromote",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/non-existent-slug/unpromote",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsSuperAdmin_WithNonExistentSlug_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/non-existent-slug/unpromote",
            new { Reason = "Content policy violation" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsSuperAdmin_WithPromotedVideo_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var promotionLevel = PromotionLevelFactory.Create();
        var video = VideoFactory.CreatePublished(category.Id);
        video.StampPromotion(promotionLevel.Id, DateTimeOffset.UtcNow.AddDays(7));
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.PromotionLevels.Add(promotionLevel);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{video.Slug}/unpromote",
            new { Reason = "Content policy violation" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
