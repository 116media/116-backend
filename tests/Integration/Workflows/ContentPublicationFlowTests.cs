using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Cross-module workflow tests for the content publication lifecycle:
/// create category → create video → publish → verify public visibility.
/// </summary>
[Collection("Database")]
public class ContentPublicationFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublishApprovedVideo_ShouldBeVisiblePublicly()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var video = VideoFactory.CreateApprovedWithYoutubeUrl(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        HttpResponseMessage publishResponse = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Videos}/{video.Id}/publish",
            null
        );
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.ClearAuthentication();
        HttpResponseMessage publicResponse = await Client.GetAsync(ApiRoutes.Public.Videos);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        string body = await publicResponse.Content.ReadAsStringAsync();
        body.Should().Contain(video.Slug);
    }

    [Fact]
    public async Task DraftVideo_ShouldNotBeVisiblePublicly()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var draftSlug = $"draft-{Guid.NewGuid():N}"[..15];
        var videoRequest = new
        {
            CategoryId = category.Id,
            Title = "Draft Video",
            Slug = draftSlug,
            Description = "Should not be visible publicly",
        };

        await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, videoRequest);

        Client.ClearAuthentication();
        HttpResponseMessage publicResponse = await Client.GetAsync(ApiRoutes.Public.Videos);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        string body = await publicResponse.Content.ReadAsStringAsync();
        body.Should().NotContain(draftSlug);
    }

    [Fact]
    public async Task CreateCategory_AsVisitor_ShouldReturnForbidden()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();
        var request = new
        {
            Name = "Forbidden",
            Slug = "forbidden",
            Description = "Should fail",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}/{contentType.Id}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
