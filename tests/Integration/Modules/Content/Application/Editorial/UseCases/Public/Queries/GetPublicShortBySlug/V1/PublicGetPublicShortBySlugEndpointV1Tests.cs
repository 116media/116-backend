using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetPublicShortBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublicShortBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetShortBySlug_WithActiveShort_ReturnsShort()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublicShortBySlugResponse body = await response.ReadAsAsync<PublicGetPublicShortBySlugResponse>();
        body.ShortVideo.Id.Should().Be(shortVideo.Id);
        body.ShortVideo.Slug.Should().Be(shortVideo.Slug);
        body.ShortVideo.Title.Should().Be(shortVideo.Title);
        body.ShortVideo.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an inactive short video is excluded from public reads
    /// and the endpoint reports it as not found.
    /// </summary>
    [Fact]
    public async Task GetShortBySlug_WithInactiveShort_ReturnsNotFound()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateInactive();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Slug}");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetShortBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/non-existent-slug");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetShortBySlug_WhenVisitorLikedAndBookmarked_ReturnsTrueFlags()
    {
        var userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            ctx.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, entity.Id));
            ctx.ShortVideoBookmarks.Add(ShortVideoBookmarkEntity.Create(Guid.NewGuid(), userId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPublicShortBySlugResponse body = await response.ReadAsAsync<PublicGetPublicShortBySlugResponse>();
        body.ShortVideo.IsLiked.Should().BeTrue();
        body.ShortVideo.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task GetShortBySlug_WhenAnonymous_ReturnsFalseFlags()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            ctx.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), entity.Id));
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Slug}");

        PublicGetPublicShortBySlugResponse body = await response.ReadAsAsync<PublicGetPublicShortBySlugResponse>();
        body.ShortVideo.IsLiked.Should().BeFalse();
        body.ShortVideo.IsBookmarked.Should().BeFalse();
    }

    [Fact]
    public async Task GetShortBySlug_WhenDifferentUserLiked_ReturnsFalseFlagsForCaller()
    {
        var likerId = Guid.NewGuid();
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            ctx.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), likerId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(Guid.NewGuid(), "Visitor");

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Slug}");

        PublicGetPublicShortBySlugResponse body = await response.ReadAsAsync<PublicGetPublicShortBySlugResponse>();
        body.ShortVideo.IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task GetShortBySlug_ResolvesAuthorProfile()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateWithAuthorId(TestUser.AdminId);
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{shortVideo.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPublicShortBySlugResponse body = await response.ReadAsAsync<PublicGetPublicShortBySlugResponse>();
        body.ShortVideo.Author.Should().NotBeNull();
        body.ShortVideo.Author!.UserName.Should().NotBeNullOrEmpty();
    }
}
