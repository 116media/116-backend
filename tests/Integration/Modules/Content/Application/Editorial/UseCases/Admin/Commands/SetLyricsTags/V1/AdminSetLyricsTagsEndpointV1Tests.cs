using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags.V1;

/// <summary>
/// Integration tests for the AdminSetLyricsTags endpoint.
/// </summary>
[Collection("Database")]
public class AdminSetLyricsTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SetLyricsTags_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminSetLyricsTagsRequest(new List<Guid>())
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetLyricsTags_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminSetLyricsTagsRequest(new List<Guid>())
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetLyricsTags_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminSetLyricsTagsRequest(new List<Guid>())
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Assigning a tag set then replacing it with a new set removes the old association rows
    /// entirely — no leftover rows from the previous set remain.
    /// </summary>
    [Fact]
    public async Task SetLyricsTags_WithNewSet_FullyReplacesOldSetWithNoLeftoverRows()
    {
        (LyricsEntity lyrics, TagEntity oldTag, TagEntity newTag) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, TagEntity, TagEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            TagEntity oldTag = TagFactory.Create("OldLyricsTag", "old-lyrics-tag");
            TagEntity newTag = TagFactory.Create("NewLyricsTag", "new-lyrics-tag");
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Tags.AddRange(oldTag, newTag);
            ctx.Lyrics.Add(lyrics);
            return (lyrics, oldTag, newTag);
        });

        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminSetLyricsTagsRequest(new List<Guid> { oldTag.Id })
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminSetLyricsTagsRequest(new List<Guid> { newTag.Id })
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminSetLyricsTagsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<Guid> persistedTagIds = await ctx
            .LyricsTags.Where(lt => lt.LyricsId == lyrics.Id)
            .Select(lt => lt.TagId)
            .ToListAsync();

        persistedTagIds.Should().ContainSingle().Which.Should().Be(newTag.Id);
    }

    /// <summary>
    /// Passing an empty tag id array clears every tag association on the lyrics page.
    /// </summary>
    [Fact]
    public async Task SetLyricsTags_WithEmptyArray_ClearsAllTags()
    {
        (LyricsEntity lyrics, TagEntity tag) = await SeedAsync<ContentDbContext, (LyricsEntity, TagEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            TagEntity tag = TagFactory.Create("SoloLyricsTag", "solo-lyrics-tag");
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Tags.Add(tag);
            ctx.Lyrics.Add(lyrics);
            return (lyrics, tag);
        });

        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminSetLyricsTagsRequest(new List<Guid> { tag.Id })
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminSetLyricsTagsRequest(new List<Guid>())
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<Guid> persistedTagIds = await ctx
            .LyricsTags.Where(lt => lt.LyricsId == lyrics.Id)
            .Select(lt => lt.TagId)
            .ToListAsync();

        persistedTagIds.Should().BeEmpty();
    }

    /// <summary>
    /// The same <see cref="TagEntity"/> can be applied to an article, a video, and a lyrics
    /// page simultaneously without conflict — the join tables are independent per content type.
    /// </summary>
    [Fact]
    public async Task SetLyricsTags_SameTagAppliedToArticleAndVideo_NoConflict()
    {
        (ArticleEntity article, VideoEntity video, LyricsEntity lyrics, TagEntity sharedTag) = await SeedAsync<
            ContentDbContext,
            (ArticleEntity, VideoEntity, LyricsEntity, TagEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            TagEntity sharedTag = TagFactory.Create("SharedAcrossContent", "shared-across-content");
            ArticleEntity article = ArticleFactory.Create(category.Id);
            VideoEntity video = VideoFactory.Create(category.Id);
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Tags.Add(sharedTag);
            ctx.Articles.Add(article);
            ctx.Videos.Add(video);
            ctx.Lyrics.Add(lyrics);

            return (article, video, lyrics, sharedTag);
        });

        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Articles, article.Id),
            new { TagNames = new List<string> { "SharedAcrossContent" } }
        );

        var lyricsResponse = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminSetLyricsTagsRequest(new List<Guid> { sharedTag.Id })
        );

        lyricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        bool articleHasTag = await ctx.ArticleTags.AnyAsync(at =>
            at.ArticleId == article.Id && at.TagId == sharedTag.Id
        );
        bool lyricsHasTag = await ctx.LyricsTags.AnyAsync(lt => lt.LyricsId == lyrics.Id && lt.TagId == sharedTag.Id);

        articleHasTag.Should().BeTrue();
        lyricsHasTag.Should().BeTrue();
    }
}
