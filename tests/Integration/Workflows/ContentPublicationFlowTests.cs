using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(context =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Video");
            context.ContentTypes.Add(entity);
            return entity;
        });

        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(context =>
        {
            CategoryEntity entity = CategoryFactory.Create(contentType.Id);
            context.Categories.Add(entity);
            return entity;
        });

        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(context =>
        {
            VideoEntity entity = VideoFactory.CreateApprovedWithYoutubeUrl(category.Id);
            context.Videos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        HttpResponseMessage publishResponse = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Videos, video.Id),
            null
        );
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using (ContentDbContext verifyContext = CreateDbContext<ContentDbContext>())
        {
            VideoEntity published = await verifyContext.Videos.FirstAsync(v => v.Id == video.Id);
            published.Status.Should().Be(EnumContentStatus.Published);
        }

        Client.ClearAuthentication();
        HttpResponseMessage publicResponse = await Client.GetAsync(ApiRoutes.Public.Videos);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await publicResponse.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().Contain(v => v.Id == video.Id && v.Slug == video.Slug);
    }

    [Fact]
    public async Task DraftVideo_ShouldNotBeVisiblePublicly()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(context =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Video");
            context.ContentTypes.Add(entity);
            return entity;
        });

        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(context =>
        {
            CategoryEntity entity = CategoryFactory.Create(contentType.Id);
            context.Categories.Add(entity);
            return entity;
        });

        VideoEntity draft = await SeedAsync<ContentDbContext, VideoEntity>(context =>
        {
            VideoEntity entity = VideoFactory.Create(category.Id);
            context.Videos.Add(entity);
            return entity;
        });

        await using (ContentDbContext verifyContext = CreateDbContext<ContentDbContext>())
        {
            VideoEntity persisted = await verifyContext.Videos.FirstAsync(v => v.Id == draft.Id);
            persisted.Status.Should().Be(EnumContentStatus.Draft);
        }

        Client.ClearAuthentication();
        HttpResponseMessage publicResponse = await Client.GetAsync(ApiRoutes.Public.Videos);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublishedVideosResponse body = await publicResponse.ReadAsAsync<PublicGetPublishedVideosResponse>();
        body.Videos.Items.Should().NotContain(v => v.Id == draft.Id);
        body.Videos.Items.Should().NotContain(v => v.Slug == draft.Slug);
    }

    [Fact]
    public async Task CreateCategory_AsVisitor_ShouldReturnForbidden()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(context =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create("Video");
            context.ContentTypes.Add(entity);
            return entity;
        });

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
