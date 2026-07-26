using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView.V1;

/// <summary>
/// Covers how a lyrics view is attributed to a viewer: which identity signal becomes the
/// dedup key (authenticated user id or forwarded client IP) and the scroll-depth half of
/// the read-time gate.
/// </summary>
[Collection("Database")]
public class PublicRecordLyricsViewDedupTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// A forwarded client IP used to drive the IP-based dedup key.
    /// </summary>
    private const string ForwardedIp = "203.0.113.10";

    /// <summary>
    /// Dwell time comfortably above every read-time threshold.
    /// </summary>
    private const int GenuineDwellMs = 30_000;

    /// <summary>
    /// Seeds a published lyrics page together with the category and content type it needs.
    /// </summary>
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

    /// <summary>
    /// Posts a view for the given lyrics page, optionally supplying a forwarded client IP.
    /// </summary>
    private async Task<HttpResponseMessage> RecordViewAsync(
        Guid lyricsId,
        int dwellMs,
        double scrollDepthRatio,
        string? forwardedFor = null
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Lyrics.Views(lyricsId))
        {
            Content = JsonContent.Create(new PublicRecordLyricsViewRequest(dwellMs, scrollDepthRatio)),
        };

        if (forwardedFor is not null)
        {
            request.Headers.Add("X-Forwarded-For", forwardedFor);
        }

        return await Client.SendAsync(request);
    }

    /// <summary>
    /// Reads back the persisted view events for a lyrics page, oldest first.
    /// </summary>
    private async Task<List<LyricsViewEventEntity>> GetViewEventsAsync(Guid lyricsId)
    {
        await using var verifyDb = CreateDbContext<ContentDbContext>();
        return await verifyDb
            .LyricsViewEvents.Where(e => e.LyricsId == lyricsId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// An authenticated viewer is deduplicated on their user id, so a repeat read inside the
    /// window is recorded but not counted a second time.
    /// </summary>
    [Fact]
    public async Task RecordLyricsView_AuthenticatedViewer_DedupesOnUserId()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.AuthenticateAsVisitor();

        var first = await RecordViewAsync(lyrics.Id, GenuineDwellMs, scrollDepthRatio: 1.0);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (await first.ReadAsAsync<PublicRecordLyricsViewResponse>()).IsCounted.Should().BeTrue();

        var second = await RecordViewAsync(lyrics.Id, GenuineDwellMs, scrollDepthRatio: 1.0);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        (await second.ReadAsAsync<PublicRecordLyricsViewResponse>()).IsCounted.Should().BeFalse();

        List<LyricsViewEventEntity> events = await GetViewEventsAsync(lyrics.Id);
        events.Should().HaveCount(2);
        events.Should().OnlyContain(e => e.DedupKey == $"user:{TestUser.VisitorId}");
        events.Should().OnlyContain(e => e.UserId == TestUser.VisitorId);
        events.Count(e => e.IsCounted).Should().Be(1);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ViewCount.Should().Be(1);
    }

    /// <summary>
    /// An anonymous viewer with no device id falls back to the forwarded client IP as the
    /// dedup key, so a repeat read from the same IP inside the window does not count twice.
    /// </summary>
    [Fact]
    public async Task RecordLyricsView_AnonymousWithForwardedIp_DedupesOnIpAddress()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.ClearAuthentication();

        var first = await RecordViewAsync(lyrics.Id, GenuineDwellMs, scrollDepthRatio: 1.0, forwardedFor: ForwardedIp);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (await first.ReadAsAsync<PublicRecordLyricsViewResponse>()).IsCounted.Should().BeTrue();

        var second = await RecordViewAsync(lyrics.Id, GenuineDwellMs, scrollDepthRatio: 1.0, forwardedFor: ForwardedIp);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        (await second.ReadAsAsync<PublicRecordLyricsViewResponse>()).IsCounted.Should().BeFalse();

        List<LyricsViewEventEntity> events = await GetViewEventsAsync(lyrics.Id);
        events.Should().HaveCount(2);
        events.Should().OnlyContain(e => e.DedupKey == $"ip:{ForwardedIp}");
        events.Should().OnlyContain(e => e.UserId == null);
        events.Count(e => e.IsCounted).Should().Be(1);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ViewCount.Should().Be(1);
    }

    /// <summary>
    /// A long dwell time with a shallow scroll depth fails the read-time gate: the request
    /// still succeeds and the raw event is kept for tuning, but the view never counts.
    /// </summary>
    [Fact]
    public async Task RecordLyricsView_LongDwellButShallowScroll_IsNotCounted()
    {
        LyricsEntity lyrics = await SeedPublishedLyricsAsync();
        Client.ClearAuthentication();

        var response = await RecordViewAsync(
            lyrics.Id,
            GenuineDwellMs,
            scrollDepthRatio: 0.1,
            forwardedFor: ForwardedIp
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicRecordLyricsViewResponse>();
        body.IsSuccess.Should().BeTrue();
        body.IsCounted.Should().BeFalse();

        List<LyricsViewEventEntity> events = await GetViewEventsAsync(lyrics.Id);
        events.Should().ContainSingle();
        events.Single().IsCounted.Should().BeFalse();
        events.Single().ScrollDepthRatio.Should().Be(0.1);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        LyricsEntity? updated = await verifyDb.Lyrics.FindAsync(lyrics.Id);
        updated!.ViewCount.Should().Be(0);
    }
}
