using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.V1;

/// <summary>
/// Integration tests for the AdminForceUnpromoteVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminForceUnpromoteVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string UnpromoteUrl(string slug) =>
        $"{ApiRoutes.Admin.Videos}/{slug}/{EditorialRouteConstants.Unpromote}";

    private async Task<VideoEntity> GetVideoAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? video = await ctx.Videos.FindAsync(id);
        return video!;
    }

    [Fact]
    public async Task ForceUnpromoteVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), new { Reason = "test" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), new { Reason = "test" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), new { Reason = "test" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsSuperAdmin_WithNonExistentSlug_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminForceUnpromoteVideoRequest request = new AdminForceUnpromoteVideoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsSuperAdmin_WithPromotedVideo_ReturnsOk()
    {
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();
            VideoEntity v = VideoFactory.CreatePublished(category.Id);
            v.StampPromotion(promotionLevel.Id, DateTimeOffset.UtcNow.AddDays(7));
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PromotionLevels.Add(promotionLevel);
            ctx.Videos.Add(v);
            return v;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminForceUnpromoteVideoRequest request = new AdminForceUnpromoteVideoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl(video.Slug), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminForceUnpromoteVideoResponse body = await response.ReadAsAsync<AdminForceUnpromoteVideoResponse>();
        body.VideoId.Should().Be(video.Id);

        VideoEntity persisted = await GetVideoAsync(video.Id);
        persisted.IsPromoted.Should().BeFalse();
        persisted.UnpromotedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ForceUnpromoteVideo_AsSuperAdmin_NotPromoted_ReturnsBadRequest()
    {
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity v = VideoFactory.CreatePublished(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(v);
            return v;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminForceUnpromoteVideoRequest request = new AdminForceUnpromoteVideoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl(video.Slug), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<VideoErrorMessage>(m => m.NotPromoted())
        );
        (await GetVideoAsync(video.Id)).IsPromoted.Should().BeFalse();
    }
}
