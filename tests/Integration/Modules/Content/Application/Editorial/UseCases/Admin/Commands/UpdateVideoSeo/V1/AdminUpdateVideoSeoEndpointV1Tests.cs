using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo.V1;

/// <summary>
/// Integration tests for the AdminUpdateVideoSeo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateVideoSeoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task UpdateVideoSeo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVideoSeo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVideoSeo_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { }
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task UpdateVideoSeo_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        VideoEntity video = await SeedVideoAsync();
        Client.AuthenticateAsSuperAdmin();
        AdminUpdateVideoSeoRequest request = new AdminUpdateVideoSeoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Videos, video.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateVideoSeoResponse body = await response.ReadAsAsync<AdminUpdateVideoSeoResponse>();
        body.Video.Id.Should().Be(video.Id);
        body.Video.MetaTitle.Should().Be(request.MetaTitle);
        body.Video.MetaDescription.Should().Be(request.MetaDescription);

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await verifyContext.Videos.FindAsync(video.Id);
        persisted!.MetaTitle.Should().Be(request.MetaTitle);
        persisted.MetaDescription.Should().Be(request.MetaDescription);
    }
}
