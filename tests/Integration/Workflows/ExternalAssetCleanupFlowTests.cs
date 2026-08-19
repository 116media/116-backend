using System.Net.Http.Headers;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Failure-injection regressions for post-commit external-asset cleanup.
/// Each test queues a one-shot Cloudinary delete failure on the singleton
/// stub and proves the business operation still commits over real HTTP: the
/// content rows are gone (or replaced) and the cleanup failure is tolerated,
/// so a storage outage can orphan a remote asset but can never block a
/// mutation or leave dangling database rows.
/// </summary>
[Collection("Database")]
public class ExternalAssetCleanupFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    private StubCloudinaryService CloudinaryStub => Api.Services.GetRequiredService<StubCloudinaryService>();

    private async Task<ArticleEntity> SeedArticleAsync(Action<ContentDbContext, ArticleEntity>? extend = null)
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity article = ArticleFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(article);
            extend?.Invoke(ctx, article);
            return article;
        });
    }

    private async Task<FileEntity> SeedFileAsync(string storageKey)
    {
        return await SeedAsync<CoreDbContext, FileEntity>(ctx =>
        {
            FileEntity file = FileFactory.CreateWithStorageKey(storageKey);
            ctx.Files.Add(file);
            return file;
        });
    }

    [Fact]
    public async Task DeleteArticle_WhenCloudinaryDeleteFails_StillDeletesArticleAndImageRows()
    {
        FileEntity coverFile = await SeedFileAsync("content/articles/failing-cover");
        ArticleEntity article = await SeedArticleAsync(
            (ctx, a) =>
            {
                a.UpdateCoverImage(coverFile.Id);
                ctx.ArticleImages.AddRange(ArticleImageFactory.CreateMany(a.Id, 2));
            }
        );

        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextDeleteFailure = new InvalidOperationException("cloudinary down");

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{article.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext contentCtx = CreateDbContext<ContentDbContext>();
        (await contentCtx.Articles.FindAsync(article.Id)).Should().BeNull();
        (await contentCtx.ArticleImages.CountAsync(img => img.ArticleId == article.Id)).Should().Be(0);

        // The cover row commits its soft delete before the remote call, so the
        // failing storage delete cannot undo it.
        await using CoreDbContext coreCtx = CreateDbContext<CoreDbContext>();
        FileEntity? cover = await coreCtx.Files.FindAsync(coverFile.Id);
        cover!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateArticle_WhenCloudinaryDeleteFails_LeavesNoOrphanedImageRows()
    {
        const string orphanedKey = "content/articles/orphaned-body-image";
        ArticleEntity article = await SeedArticleAsync(
            (ctx, a) =>
                ctx.ArticleImages.Add(
                    ArticleImageFactory.CreateBody(
                        a.Id,
                        orphanedKey,
                        "https://res.cloudinary.com/test-cloud/image/upload/orphaned-body-image.jpg"
                    )
                )
        );

        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextDeleteFailure = new InvalidOperationException("cloudinary down");

        var request = new AdminUpdateArticleRequestBuilder()
            .WithCategoryId(article.CategoryId)
            .WithSlug(article.Slug)
            .WithBody("<p>New body without the previous image.</p>")
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{article.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The orphaned rows are removed and committed before the remote delete,
        // so the Cloudinary failure no longer leaves them dangling.
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        (await ctx.ArticleImages.CountAsync(img => img.StorageKey == orphanedKey)).Should().Be(0);
    }

    [Fact]
    public async Task DeleteVideo_WhenCloudinaryDeleteFails_StillDeletesVideo()
    {
        FileEntity thumbnailFile = await SeedFileAsync("content/video-thumbnails/failing-thumb");
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity entity = VideoFactory.Create(category.Id);
            entity.SetThumbnailFileId(thumbnailFile.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextDeleteFailure = new InvalidOperationException("cloudinary down");

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Videos}/{video.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext contentCtx = CreateDbContext<ContentDbContext>();
        (await contentCtx.Videos.FindAsync(video.Id)).Should().BeNull();

        await using CoreDbContext coreCtx = CreateDbContext<CoreDbContext>();
        FileEntity? thumbnail = await coreCtx.Files.FindAsync(thumbnailFile.Id);
        thumbnail!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteShortVideo_WhenCloudinaryDeleteFails_StillDeletesShortVideo()
    {
        FileEntity videoFile = await SeedFileAsync("content/shorts/failing-video");
        FileEntity thumbnailFile = await SeedFileAsync("content/shorts/failing-thumb");
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            entity.ReplaceVideoFile(videoFile.Id);
            entity.SetThumbnailFileId(thumbnailFile.Id);
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextDeleteFailure = new InvalidOperationException("cloudinary down");

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext contentCtx = CreateDbContext<ContentDbContext>();
        (await contentCtx.ShortVideos.FindAsync(shortVideo.Id)).Should().BeNull();

        await using CoreDbContext coreCtx = CreateDbContext<CoreDbContext>();
        (await coreCtx.Files.FindAsync(videoFile.Id))!.IsDeleted.Should().BeTrue();
        (await coreCtx.Files.FindAsync(thumbnailFile.Id))!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task AttachYoutubeUrl_WhenOldThumbnailCleanupFails_StillAttachesNewThumbnail()
    {
        FileEntity oldThumbnail = await SeedFileAsync("content/video-thumbnails/old-thumb");
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity entity = VideoFactory.Create(category.Id);
            entity.SetThumbnailFileId(oldThumbnail.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        CloudinaryStub.NextDeleteFailure = new InvalidOperationException("cloudinary down");

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Youtube(EditorialRouteConstants.Videos, video.Id),
            new { YoutubeVideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The post-commit thumbnail handler replaced the old file and attached
        // the new one; the failing remote delete of the old asset is tolerated.
        await using ContentDbContext contentCtx = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await contentCtx.Videos.FindAsync(video.Id);
        persisted!.YoutubeVideoUrl.Should().Be("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        persisted.ThumbnailFileId.Should().NotBeNull();
        persisted.ThumbnailFileId.Should().NotBe(oldThumbnail.Id);

        await using CoreDbContext coreCtx = CreateDbContext<CoreDbContext>();
        (await coreCtx.Files.FindAsync(oldThumbnail.Id))!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAvatar_WhenOldAvatarCleanupFails_StillReplacesAvatarAndSoftDeletesOldRow()
    {
        var sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
        });
        Client.AuthenticateAs(TestUser.VisitorId, "Visitor", sessionId);

        // First upload establishes the avatar file row that the second upload replaces.
        var firstResponse = await Client.PatchAsync(Routes.Public.Me.Avatar(), BuildAvatarContent());
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Guid firstAvatarFileId;
        await using (IdentityDbContext identityCtx = CreateDbContext<IdentityDbContext>())
        {
            UserEntity? user = await identityCtx.Users.FindAsync(TestUser.VisitorId);
            firstAvatarFileId = user!.AvatarFileId!.Value;
        }

        CloudinaryStub.NextDeleteFailure = new InvalidOperationException("cloudinary down");

        var secondResponse = await Client.PatchAsync(Routes.Public.Me.Avatar(), BuildAvatarContent());
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // The old row rides the unified replacement path: soft-deleted, never
        // hard-deleted, and the failing remote delete is tolerated.
        await using IdentityDbContext verifyIdentityCtx = CreateDbContext<IdentityDbContext>();
        UserEntity? updatedUser = await verifyIdentityCtx.Users.FindAsync(TestUser.VisitorId);
        updatedUser!.AvatarFileId.Should().NotBeNull();
        updatedUser.AvatarFileId.Should().NotBe(firstAvatarFileId);

        await using CoreDbContext coreCtx = CreateDbContext<CoreDbContext>();
        FileEntity? oldAvatar = await coreCtx.Files.FindAsync(firstAvatarFileId);
        oldAvatar!.IsDeleted.Should().BeTrue();
    }

    private static MultipartFormDataContent BuildAvatarContent()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "avatarFile", "avatar.jpg");
        return content;
    }
}
