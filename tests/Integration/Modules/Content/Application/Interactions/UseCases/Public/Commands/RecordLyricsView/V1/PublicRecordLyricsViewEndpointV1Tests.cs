using System.Net.Http.Json;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView.V1;

/// <summary>
/// Integration tests for the PublicRecordLyricsView endpoint, in particular the read-time
/// view-counting algorithm (spec 05) end to end against real Postgres.
/// </summary>
[Collection("Database")]
public class PublicRecordLyricsViewEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<LyricsEntity> SeedPublishedLyricsAsync()
    {
        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    private async Task<HttpResponseMessage> RecordViewAsync(
        Guid lyricsId,
        int dwellMs,
        double scrollDepthRatio,
        string? deviceId = null
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Lyrics.Views(lyricsId))
        {
            Content = JsonContent.Create(new PublicRecordLyricsViewRequest(dwellMs, scrollDepthRatio)),
        };

        if (deviceId is not null)
        {
            request.Headers.Add("X-Device-Id", deviceId);
        }

        return await Client.SendAsync(request);
    }

    [Fact]
    public async Task RecordLyricsView_NonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await RecordViewAsync(Guid.NewGuid(), dwellMs: 30_000, scrollDepthRatio: 1.0);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A genuine full read — high dwell time, full scroll depth — counts exactly once per
    /// dedup window and persists the raw event.
    /// </summary>
    [Fact]
    public async Task RecordLyricsView_GenuineFullRead_CountsOnce()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.ClearAuthentication();

        var response = await RecordViewAsync(
            lyrics.Id,
            dwellMs: 30_000,
            scrollDepthRatio: 1.0,
            deviceId: $"device-{Guid.NewGuid():N}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicRecordLyricsViewResponse>();
        body.IsSuccess.Should().BeTrue();
        body.IsCounted.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ViewCount.Should().Be(1);

        List<LyricsViewEventEntity> events = await verifyDb
            .LyricsViewEvents.Where(e => e.LyricsId == lyrics.Id)
            .ToListAsync();
        events.Should().ContainSingle();
        events.Single().IsCounted.Should().BeTrue();
    }

    /// <summary>
    /// A bounce — negligible dwell time and scroll depth — never counts, even from a fresh
    /// anonymous identity with no dedup history, but the raw event is still persisted.
    /// </summary>
    [Fact]
    public async Task RecordLyricsView_Bounce_NeverCountsButPersistsRawEvent()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.ClearAuthentication();

        var response = await RecordViewAsync(
            lyrics.Id,
            dwellMs: 300,
            scrollDepthRatio: 0.05,
            deviceId: $"device-{Guid.NewGuid():N}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicRecordLyricsViewResponse>();
        body.IsSuccess.Should().BeTrue();
        body.IsCounted.Should().BeFalse();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ViewCount.Should().Be(0);

        List<LyricsViewEventEntity> events = await verifyDb
            .LyricsViewEvents.Where(e => e.LyricsId == lyrics.Id)
            .ToListAsync();
        events.Should().ContainSingle();
        events.Single().IsCounted.Should().BeFalse();
    }

    /// <summary>
    /// A repeat genuine read from the same identity within the dedup window does not double
    /// count, but both raw events are persisted.
    /// </summary>
    [Fact]
    public async Task RecordLyricsView_RepeatGenuineReadWithinWindow_DoesNotDoubleCount()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.ClearAuthentication();
        string deviceId = $"device-{Guid.NewGuid():N}";

        var first = await RecordViewAsync(lyrics.Id, dwellMs: 30_000, scrollDepthRatio: 1.0, deviceId: deviceId);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstBody = await first.ReadAsAsync<PublicRecordLyricsViewResponse>();
        firstBody.IsCounted.Should().BeTrue();

        var second = await RecordViewAsync(lyrics.Id, dwellMs: 30_000, scrollDepthRatio: 1.0, deviceId: deviceId);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondBody = await second.ReadAsAsync<PublicRecordLyricsViewResponse>();
        secondBody.IsSuccess.Should().BeTrue();
        secondBody.IsCounted.Should().BeFalse();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ViewCount.Should().Be(1);

        List<LyricsViewEventEntity> events = await verifyDb
            .LyricsViewEvents.Where(e => e.LyricsId == lyrics.Id)
            .ToListAsync();
        events.Should().HaveCount(2);
        events.Count(e => e.IsCounted).Should().Be(1);
    }

    /// <summary>
    /// Different identities (device ids) each get their own dedup window, so both count.
    /// </summary>
    [Fact]
    public async Task RecordLyricsView_DifferentDevices_EachCounts()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.ClearAuthentication();

        await RecordViewAsync(
            lyrics.Id,
            dwellMs: 30_000,
            scrollDepthRatio: 1.0,
            deviceId: $"device-{Guid.NewGuid():N}"
        );
        await RecordViewAsync(
            lyrics.Id,
            dwellMs: 30_000,
            scrollDepthRatio: 1.0,
            deviceId: $"device-{Guid.NewGuid():N}"
        );

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ViewCount.Should().Be(2);
    }
}
