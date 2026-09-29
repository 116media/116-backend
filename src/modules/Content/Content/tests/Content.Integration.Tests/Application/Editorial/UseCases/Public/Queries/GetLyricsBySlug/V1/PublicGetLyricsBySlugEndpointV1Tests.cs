using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Tests.TestData.Helpers;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetLyricsBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetLyricsBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync() => await SeedAsync<ContentDbContext, Guid>(ContentSeeder.AddCategory);

    [Fact]
    public async Task GetLyricsBySlug_WithExistingLyrics_ReturnsLyrics()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublishedWithSlug(categoryId, slug);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.Lyrics.Id.Should().Be(lyrics.Id);
        body.Lyrics.Slug.Should().Be(slug);
        body.Lyrics.SongTitle.Should().Be(lyrics.SongTitle);
        body.Lyrics.ArtistName.Should().Be(lyrics.ArtistName);
    }

    [Fact]
    public async Task GetLyricsBySlug_WithDraftLyrics_ReturnsNotFound()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity draftLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{draftLyrics.Slug}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task GetLyricsBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/non-existent-slug");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task GetLyricsBySlug_LinkedToExistingVideo_ReturnsVideoSlug()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";
        string videoSlug = $"linked-video-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.CreateWithSlug(categoryId, videoSlug);
            LyricsEntity entity = LyricsFactory.CreatePublishedForVideoWithSlug(categoryId, video.Id, slug);
            ctx.Videos.Add(video);
            ctx.Lyrics.Add(entity);
            return video;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.VideoSlug.Should().Be(videoSlug);
    }

    [Fact]
    public async Task GetLyricsBySlug_LinkedToDeletedVideo_ReturnsNullVideoSlugWithoutFailing()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.Create(categoryId);
            LyricsEntity entity = LyricsFactory.CreatePublishedForVideoWithSlug(categoryId, video.Id, slug);
            ctx.Videos.Add(video);
            ctx.Lyrics.Add(entity);
            return video;
        });

        // The FK's OnDelete(DeleteBehavior.SetNull) nulls out VideoId on the linked lyrics
        // row once the video is deleted, simulating a stale link without violating the FK.
        await using (ContentDbContext deleteCtx = CreateDbContext<ContentDbContext>())
        {
            VideoEntity? videoToDelete = await deleteCtx.Videos.FindAsync(video.Id);
            deleteCtx.Videos.Remove(videoToDelete!);
            await deleteCtx.SaveChangesAsync();
        }

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.VideoSlug.Should().BeNull();
    }

    [Fact]
    public async Task GetLyricsBySlug_Standalone_ReturnsNullVideoSlugAndArtistSlug()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublishedWithSlug(categoryId, slug);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.VideoSlug.Should().BeNull();
        body.ArtistSlug.Should().BeNull();
    }

    [Fact]
    public async Task GetLyricsBySlug_LinkedToExistingArtist_ReturnsArtistSlug()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";
        string artistSlug = $"linked-artist-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(artistSlug);
            LyricsEntity entity = LyricsFactory.CreatePublishedWithSlug(categoryId, slug);
            entity.LinkArtist(artist.Id);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(entity);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.ArtistSlug.Should().Be(artistSlug);
    }

    [Fact]
    public async Task GetLyricsBySlug_LinkedToDeletedArtist_ReturnsNullArtistSlugWithoutFailing()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";

        (_, ArtistEntity artist) = await SeedAsync<ContentDbContext, (LyricsEntity, ArtistEntity)>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.Create();
            LyricsEntity entity = LyricsFactory.CreatePublishedWithSlug(categoryId, slug);
            entity.LinkArtist(artist.Id);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(entity);
            return (entity, artist);
        });

        // The FK's OnDelete(DeleteBehavior.SetNull) nulls out ArtistId on the linked lyrics
        // row once the artist is deleted, simulating a stale link without violating the FK.
        await using (ContentDbContext deleteCtx = CreateDbContext<ContentDbContext>())
        {
            ArtistEntity? artistToDelete = await deleteCtx.Artists.FindAsync(artist.Id);
            deleteCtx.Artists.Remove(artistToDelete!);
            await deleteCtx.SaveChangesAsync();
        }

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.ArtistSlug.Should().BeNull();
    }

    [Fact]
    public async Task GetLyricsBySlug_Unlinked_ReturnsNullArtistSlug()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublishedWithSlug(categoryId, slug);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.ArtistSlug.Should().BeNull();
    }

    [Fact]
    public async Task GetLyricsBySlug_WhenCurrentUserHasLiked_ReturnsIsLikedTrueAndCounts()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublishedWithSlug(categoryId, slug);
            entity.WithViewCount(2);
            entity.WithShareCount(1);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.Lyrics.IsLiked.Should().BeTrue();
        body.Lyrics.ViewCount.Should().Be(2);
        body.Lyrics.LikeCount.Should().Be(1);
        body.Lyrics.ShareCount.Should().Be(1);
    }

    [Fact]
    public async Task GetLyricsBySlug_WhenAnonymous_ReturnsIsLikedFalse()
    {
        Guid categoryId = await SeedCategoryAsync();
        string slug = $"unique-slug-song-{Guid.NewGuid():N}";

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublishedWithSlug(categoryId, slug);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Lyrics.Likes(lyrics.Id), null);
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Lyrics}/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetLyricsBySlugResponse body = await response.ReadAsAsync<PublicGetLyricsBySlugResponse>();
        body.Lyrics.IsLiked.Should().BeFalse();
    }
}
