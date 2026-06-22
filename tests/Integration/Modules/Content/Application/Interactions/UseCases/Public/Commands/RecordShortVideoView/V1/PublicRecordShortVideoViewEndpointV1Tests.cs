using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView.V1;

/// <summary>
/// Integration tests for the PublicRecordShortVideoView endpoint.
/// </summary>
[Collection("Database")]
public class PublicRecordShortVideoViewEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ShortVideoEntity> SeedShortVideoAsync()
    {
        return await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity shortVideo = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(shortVideo);
            return shortVideo;
        });
    }

    [Fact]
    public async Task RecordShortVideoView_AsAnonymous_ReturnsOk()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Shorts.Views(shortVideo.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicRecordShortVideoViewResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? updated = await verifyDb.ShortVideos.FindAsync(shortVideo.Id);
        updated!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordShortVideoView_AsVisitor_ReturnsOk()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Views(shortVideo.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicRecordShortVideoViewResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? updated = await verifyDb.ShortVideos.FindAsync(shortVideo.Id);
        updated!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordShortVideoView_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Views(Guid.NewGuid()), null);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
