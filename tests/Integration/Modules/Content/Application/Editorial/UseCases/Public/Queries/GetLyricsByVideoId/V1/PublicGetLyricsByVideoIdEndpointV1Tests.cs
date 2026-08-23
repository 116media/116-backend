using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId.V1;

/// <summary>
/// Integration tests for the PublicGetLyricsByVideoId endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetLyricsByVideoIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetLyricsByVideoId_WithLinkedLyrics_ReturnsLyrics()
    {
        Guid categoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreateForVideo(categoryId, video.Id);
            entity.Publish();
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/videos/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetLyricsByVideoIdResponse body = await response.ReadAsAsync<PublicGetLyricsByVideoIdResponse>();
        body.Lyrics.Id.Should().Be(lyrics.Id);
        body.Lyrics.VideoId.Should().Be(video.Id);
    }

    [Fact]
    public async Task GetLyricsByVideoId_WithDraftLyrics_ReturnsNotFound()
    {
        Guid categoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreateForVideo(categoryId, video.Id);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/videos/{video.Id}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task GetLyricsByVideoId_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/videos/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task GetLyricsByVideoId_WhenCurrentUserHasLiked_ReturnsIsLikedTrueAndCounts()
    {
        Guid categoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreateForVideo(categoryId, video.Id);
            entity.Publish();
            entity.IncrementViewCount();
            entity.IncrementShareCount();
            entity.IncrementShareCount();
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/videos/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsByVideoIdResponse body = await response.ReadAsAsync<PublicGetLyricsByVideoIdResponse>();
        body.Lyrics.IsLiked.Should().BeTrue();
        body.Lyrics.ViewCount.Should().Be(1);
        body.Lyrics.LikeCount.Should().Be(1);
        body.Lyrics.ShareCount.Should().Be(2);
    }

    [Fact]
    public async Task GetLyricsByVideoId_WhenAnonymous_ReturnsIsLikedFalse()
    {
        Guid categoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity entity = VideoFactory.Create(categoryId);
            ctx.Videos.Add(entity);
            return entity;
        });

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreateForVideo(categoryId, video.Id);
            entity.Publish();
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/videos/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsByVideoIdResponse body = await response.ReadAsAsync<PublicGetLyricsByVideoIdResponse>();
        body.Lyrics.IsLiked.Should().BeFalse();
    }
}
