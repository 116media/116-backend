using System.Text;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed.V1;

/// <summary>
/// Integration tests for the public randomized shorts feed endpoint: active-only filtering,
/// cursor routing precedence, stable pagination without drift, and per-user flags.
/// </summary>
[Collection("Database")]
public class PublicGetShortsFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string FeedUrl = $"{ApiRoutes.Public.Shorts}/feed";

    [Fact]
    public async Task GetShortsFeed_AsAnonymous_ReturnsActiveOnly()
    {
        ShortVideoEntity active = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });
        ShortVideoEntity inactive = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateInactive();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(FeedUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetShortsFeedResponse body = await response.ReadAsAsync<PublicGetShortsFeedResponse>();
        body.Items.Should().Contain(item => item.Id == active.Id);
        body.Items.Should().NotContain(item => item.Id == inactive.Id);
    }

    [Fact]
    public async Task GetShortsFeed_PagesThroughAllShortsWithoutDriftOrDuplicates()
    {
        List<Guid> seededIds = [];
        for (var i = 0; i < 7; i++)
        {
            ShortVideoEntity seeded = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
            {
                ShortVideoEntity entity = ShortVideoFactory.Create();
                ctx.ShortVideos.Add(entity);
                return entity;
            });
            seededIds.Add(seeded.Id);
        }

        Client.ClearAuthentication();

        List<Guid> collected = [];
        string? cursor = null;
        var requests = 0;

        do
        {
            string url = cursor is null
                ? $"{FeedUrl}?pageSize=3"
                : $"{FeedUrl}?pageSize=3&cursor={Uri.EscapeDataString(cursor)}";
            var response = await Client.GetAsync(url);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            PublicGetShortsFeedResponse body = await response.ReadAsAsync<PublicGetShortsFeedResponse>();
            collected.AddRange(body.Items.Select(item => item.Id));
            cursor = body.NextCursor;
            requests++;
        } while (cursor is not null && requests < 10);

        collected.Should().OnlyHaveUniqueItems();
        collected.Should().BeEquivalentTo(seededIds);
    }

    [Fact]
    public async Task GetShortsFeed_WhenVisitorLikedOne_StampsFlag()
    {
        var userId = await SeedAuthenticatedUserAsync();
        ShortVideoEntity likedShort = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            ctx.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync($"{FeedUrl}?pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetShortsFeedResponse body = await response.ReadAsAsync<PublicGetShortsFeedResponse>();
        body.Items.Single(item => item.Id == likedShort.Id).IsLiked.Should().BeTrue();
    }

    [Theory]
    [InlineData("!!!bad!!!", false)] // not base64 at all -> FormatException branch
    [InlineData("not-a-valid-cursor", false)] // decodes but wrong component count
    [InlineData("notanumber|5", true)] // valid base64url, unparseable seed
    [InlineData("5|", true)] // single-'=' re-pad branch + unparseable after-key
    public async Task GetShortsFeed_WithMalformedCursor_StartsFreshSession(string payload, bool encode)
    {
        ShortVideoEntity seeded = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        string badCursor = encode ? ToBase64Url(payload) : payload;
        var response = await Client.GetAsync($"{FeedUrl}?pageSize=10&cursor={Uri.EscapeDataString(badCursor)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetShortsFeedResponse body = await response.ReadAsAsync<PublicGetShortsFeedResponse>();
        body.Items.Should().Contain(item => item.Id == seeded.Id);
    }

    private static string ToBase64Url(string raw) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(raw)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    [Fact]
    public async Task GetShortsFeed_ForTeaserShort_ExposesParentVideoSlug()
    {
        VideoEntity parent = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            VideoEntity video = VideoFactory.CreatePublished(category.Id);
            ctx.Videos.Add(video);
            return video;
        });

        ShortVideoEntity teaser = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateTeaser(parent.Id);
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{FeedUrl}?pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetShortsFeedResponse body = await response.ReadAsAsync<PublicGetShortsFeedResponse>();
        var item = body.Items.Single(i => i.Id == teaser.Id);
        item.VideoSlug.Should().Be(parent.Slug);
        item.HasFullVideo.Should().BeTrue();
    }

    [Fact]
    public async Task GetShortsFeed_WhenFewerThanPageSize_ReturnsNullCursor()
    {
        await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{FeedUrl}?pageSize=10");

        PublicGetShortsFeedResponse body = await response.ReadAsAsync<PublicGetShortsFeedResponse>();
        body.Items.Should().ContainSingle();
        body.NextCursor.Should().BeNull();
    }
}
