using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos.V1;

/// <summary>
/// Integration tests for the PublicGetPopularVideos endpoint. Proves the weighted
/// engagement ordering computed in the SQL ORDER BY (rating volume weighted by quality,
/// plus shares), the published/category/exclude filters, the limit clamp, and the cache
/// invalidation triggered by an engagement mutation.
/// </summary>
[Collection("Database")]
public class PublicGetPopularVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    private async Task<VideoEntity> SeedVideoAsync(
        Guid categoryId,
        decimal ratingAverage = 0m,
        int ratingCount = 0,
        int shares = 0
    )
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.CreatePublished(categoryId);

            if (ratingCount > 0)
            {
                video.UpdateRating(average: ratingAverage, count: ratingCount);
            }

            for (int i = 0; i < shares; i++)
            {
                video.IncrementShareCount();
            }

            ctx.Videos.Add(video);
            return video;
        });
    }

    [Fact]
    public async Task GetPopularVideos_AsAnonymous_OrdersByWeightedEngagementScore()
    {
        Guid categoryId = await SeedCategoryAsync();

        // score = rating(3) * (average * count) + share(5) * shareCount
        VideoEntity high = await SeedVideoAsync(categoryId, ratingAverage: 5m, ratingCount: 10); // 150
        VideoEntity mid = await SeedVideoAsync(categoryId, shares: 20); // 100
        VideoEntity low = await SeedVideoAsync(categoryId, ratingAverage: 4m, ratingCount: 4); // 48

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularVideosResponse body = await response.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Select(v => v.Id).Should().ContainInOrder(high.Id, mid.Id, low.Id);
    }

    [Fact]
    public async Task GetPopularVideos_WhenScoresAreEqual_TieBreaksByPublishedAtDescending()
    {
        Guid categoryId = await SeedCategoryAsync();

        VideoEntity older = await SeedVideoAsync(categoryId, shares: 2);
        await Task.Delay(50);
        VideoEntity newer = await SeedVideoAsync(categoryId, shares: 2);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");

        PublicGetPopularVideosResponse body = await response.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Select(v => v.Id).Should().ContainInOrder(newer.Id, older.Id);
    }

    [Fact]
    public async Task GetPopularVideos_ExcludesGivenId()
    {
        Guid categoryId = await SeedCategoryAsync();

        VideoEntity top = await SeedVideoAsync(categoryId, ratingAverage: 5m, ratingCount: 5); // 75
        VideoEntity other = await SeedVideoAsync(categoryId, shares: 1); // 5

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10&excludeId={top.Id}");

        PublicGetPopularVideosResponse body = await response.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Should().NotContain(v => v.Id == top.Id);
        body.Videos.Should().Contain(v => v.Id == other.Id);
    }

    [Fact]
    public async Task GetPopularVideos_FiltersByCategory()
    {
        Guid categoryA = await SeedCategoryAsync();
        Guid categoryB = await SeedCategoryAsync();

        VideoEntity inA = await SeedVideoAsync(categoryA, shares: 3);
        await SeedVideoAsync(categoryB, shares: 9);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?categoryId={categoryA}");

        PublicGetPopularVideosResponse body = await response.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Should().OnlyContain(v => v.CategoryId == categoryA);
        body.Videos.Should().Contain(v => v.Id == inA.Id);
    }

    [Fact]
    public async Task GetPopularVideos_ReturnsOnlyPublished()
    {
        Guid categoryId = await SeedCategoryAsync();

        await SeedVideoAsync(categoryId, shares: 1);
        VideoEntity draft = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            VideoEntity video = VideoFactory.Create(categoryId);

            for (int i = 0; i < 50; i++)
            {
                video.IncrementShareCount();
            }

            ctx.Videos.Add(video);
            return video;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");

        PublicGetPopularVideosResponse body = await response.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Should().NotContain(v => v.Id == draft.Id);
        body.Videos.Should().OnlyContain(v => v.Status == EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetPopularVideos_RespectsLimit()
    {
        Guid categoryId = await SeedCategoryAsync();

        await SeedVideoAsync(categoryId, shares: 1);
        await SeedVideoAsync(categoryId, shares: 2);
        await SeedVideoAsync(categoryId, shares: 3);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=2");

        PublicGetPopularVideosResponse body = await response.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetPopularVideos_WithOutOfRangeLimit_ClampsAndReturnsOk()
    {
        Guid categoryId = await SeedCategoryAsync();
        await SeedVideoAsync(categoryId, shares: 1);

        Client.ClearAuthentication();

        var oversized = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=500");
        var undersized = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=0");

        oversized.StatusCode.Should().Be(HttpStatusCode.OK);
        undersized.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPopularVideosResponse body = await undersized.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetPopularVideos_WithCategoryExcludeAndLimit_AppliesAllFilters()
    {
        Guid categoryA = await SeedCategoryAsync();
        Guid categoryB = await SeedCategoryAsync();

        VideoEntity excluded = await SeedVideoAsync(categoryA, shares: 9); // 45
        VideoEntity topKept = await SeedVideoAsync(categoryA, shares: 5); // 25
        await SeedVideoAsync(categoryA, shares: 1); // 5
        await SeedVideoAsync(categoryB, shares: 8); // 40, other category

        Client.ClearAuthentication();

        var response = await Client.GetAsync(
            $"{Routes.Public.Videos.Popular()}?categoryId={categoryA}&excludeId={excluded.Id}&limit=1"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularVideosResponse body = await response.ReadAsAsync<PublicGetPopularVideosResponse>();
        body.Videos.Should().ContainSingle();
        body.Videos.Should().OnlyContain(v => v.CategoryId == categoryA);
        body.Videos.Should().NotContain(v => v.Id == excluded.Id);
        body.Videos.First().Id.Should().Be(topKept.Id);
    }

    [Fact]
    public async Task GetPopularVideos_AfterEngagementChange_ReflectsNewRanking()
    {
        Guid categoryId = await SeedCategoryAsync();

        VideoEntity quiet = await SeedVideoAsync(categoryId); // 0
        VideoEntity leader = await SeedVideoAsync(categoryId, ratingAverage: 1m, ratingCount: 1); // 3

        Client.ClearAuthentication();

        var first = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");
        PublicGetPopularVideosResponse firstBody = await first.ReadAsAsync<PublicGetPopularVideosResponse>();
        firstBody.Videos.First().Id.Should().Be(leader.Id);

        await Client.PostAsync(Routes.Public.Videos.Shares(quiet.Id), null); // quiet -> 5, now leads

        var second = await Client.GetAsync($"{Routes.Public.Videos.Popular()}?limit=10");
        PublicGetPopularVideosResponse secondBody = await second.ReadAsAsync<PublicGetPopularVideosResponse>();
        secondBody.Videos.First().Id.Should().Be(quiet.Id);
    }
}
