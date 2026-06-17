using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags.V1;

/// <summary>
/// Integration tests for the AdminUpdateVideoTags endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateVideoTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<VideoEntity> SeedVideoAsync()
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = VideoFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);
            return video;
        });
    }

    [Fact]
    public async Task UpdateVideoTags_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVideoTags_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVideoTags_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        AdminUpdateVideoTagsRequest request = new AdminUpdateVideoTagsRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Videos, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that submitting tag names for an existing video creates the tags,
    /// links them to the video, and persists the associations.
    /// </summary>
    [Fact]
    public async Task UpdateVideoTags_AsSuperAdmin_WithValidTags_ReturnsOkAndPersists()
    {
        VideoEntity video = await SeedVideoAsync();
        Client.AuthenticateAsSuperAdmin();
        string[] tagNames = [$"jazz-{Guid.NewGuid():N}"[..12], $"live-{Guid.NewGuid():N}"[..12]];
        AdminUpdateVideoTagsRequest request = new AdminUpdateVideoTagsRequestBuilder().WithTagNames(tagNames).Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Videos, video.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateVideoTagsResponse body = await response.ReadAsAsync<AdminUpdateVideoTagsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        List<Guid> linkedTagIds = await verifyContext
            .VideoTags.Where(vt => vt.VideoId == video.Id)
            .Select(vt => vt.TagId)
            .ToListAsync();
        linkedTagIds.Should().HaveCount(tagNames.Length);

        List<string> linkedTagNames = await verifyContext
            .Tags.Where(t => linkedTagIds.Contains(t.Id))
            .Select(t => t.Name)
            .ToListAsync();
        linkedTagNames.Should().BeEquivalentTo(tagNames);
    }
}
