using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;

/// <summary>
/// Integration tests for the AdminDeleteLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeleteLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that deleting an existing standalone lyrics page succeeds, returns
    /// IsSuccess true, and removes the row from the database.
    /// </summary>
    [Fact]
    public async Task DeleteLyrics_AsSuperAdmin_RemovesLyrics()
    {
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity l = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminDeleteLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().BeNull();
    }

    /// <summary>
    /// Verifies that deleting a lyrics page linked to a video clears the parent video's
    /// <c>HasLyrics</c> flag through <c>VideoEntity.UnmarkHasLyrics</c>, so the video page stops
    /// advertising a lyrics companion that no longer exists.
    /// </summary>
    [Fact]
    public async Task DeleteLyrics_AsSuperAdmin_LinkedToVideo_ClearsVideoHasLyricsFlag()
    {
        Guid categoryId = Guid.Empty;
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity parentVideo = VideoFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(parentVideo);
            categoryId = category.Id;
            return parentVideo;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest createRequest = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(categoryId)
            .WithSlug($"delete-video-linked-{Guid.NewGuid().ToString("N")[..8]}")
            .WithVideoId(video.Id)
            .Build();

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.ReadAsAsync<AdminCreateLyricsResponse>();

        await using (ContentDbContext linkedCtx = CreateDbContext<ContentDbContext>())
        {
            VideoEntity? linkedVideo = await linkedCtx.Videos.FindAsync(video.Id);
            linkedVideo!.HasLyrics.Should().BeTrue();
        }

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{created.Lyrics.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? persistedVideo = await ctx.Videos.FindAsync(video.Id);
        persistedVideo.Should().NotBeNull();
        persistedVideo!.HasLyrics.Should().BeFalse();
        (await ctx.Lyrics.FindAsync(created.Lyrics.Id)).Should().BeNull();
    }
}
