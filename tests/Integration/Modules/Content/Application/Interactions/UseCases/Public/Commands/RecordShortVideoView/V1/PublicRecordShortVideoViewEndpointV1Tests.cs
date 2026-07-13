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

    private async Task<HttpResponseMessage> RecordViewAsync(Guid shortVideoId, string? deviceId = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Shorts.Views(shortVideoId));
        if (deviceId is not null)
        {
            request.Headers.Add("X-Device-Id", deviceId);
        }

        return await Client.SendAsync(request);
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

    [Fact]
    public async Task RecordShortVideoView_SameDeviceWithinWindow_CountsOnceAndRecordsBothEvents()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.ClearAuthentication();

        var first = await RecordViewAsync(shortVideo.Id, deviceId: "device-int-1");
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstBody = await first.ReadAsAsync<PublicRecordShortVideoViewResponse>();
        firstBody.IsCounted.Should().BeTrue();

        var second = await RecordViewAsync(shortVideo.Id, deviceId: "device-int-1");
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondBody = await second.ReadAsAsync<PublicRecordShortVideoViewResponse>();
        secondBody.IsSuccess.Should().BeTrue();
        secondBody.IsCounted.Should().BeFalse();

        await using var verifyDb = CreateDbContext<ContentDbContext>();

        ShortVideoEntity? updated = await verifyDb.ShortVideos.FindAsync(shortVideo.Id);
        updated!.ViewCount.Should().Be(1);

        List<ShortVideoViewEventEntity> events = await verifyDb
            .ShortVideoViewEvents.Where(e => e.ShortVideoId == shortVideo.Id)
            .ToListAsync();

        events.Should().HaveCount(2);
        events.Should().OnlyContain(e => e.DedupKey == "device:device-int-1");
        events.Count(e => e.IsCounted).Should().Be(1);
    }

    [Fact]
    public async Task RecordShortVideoView_DifferentDevices_CountsEach()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.ClearAuthentication();

        await RecordViewAsync(shortVideo.Id, deviceId: "device-int-a");
        await RecordViewAsync(shortVideo.Id, deviceId: "device-int-b");

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? updated = await verifyDb.ShortVideos.FindAsync(shortVideo.Id);
        updated!.ViewCount.Should().Be(2);
    }
}
