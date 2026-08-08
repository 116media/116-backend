using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo.V1;

/// <summary>
/// Integration tests for the PublicShareVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicShareVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<VideoEntity> SeedVideoAsync()
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            VideoEntity video = VideoFactory.CreatePublished(category.Id);
            ctx.Videos.Add(video);
            return video;
        });
    }

    [Fact]
    public async Task ShareVideo_AsAnonymous_ReturnsOk()
    {
        VideoEntity video = await SeedVideoAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Videos.Shares(video.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.VideoShares.AnyAsync(s => s.VideoId == video.Id && s.UserId == null)).Should().BeTrue();
    }

    [Fact]
    public async Task ShareVideo_AsVisitor_ReturnsOk()
    {
        VideoEntity video = await SeedVideoAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Videos.Shares(video.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.VideoShares.AnyAsync(s => s.VideoId == video.Id && s.UserId == TestUser.VisitorId))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ShareVideo_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Videos.Shares(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task ShareVideo_WithShareChannel_PersistsChannel()
    {
        VideoEntity video = await SeedVideoAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(Routes.Public.Videos.Shares(video.Id), new { shareChannel = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        VideoShareEntity share = await verifyDb.VideoShares.SingleAsync(s => s.VideoId == video.Id);
        share.ShareChannel.Should().Be(EnumShareChannel.X);
    }
}
