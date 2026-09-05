using _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics.V1;

/// <summary>
/// Integration tests for the PublicGetSimilarLyrics endpoint, exercising the three-way
/// similar-lyrics waterfall (spec 06) end to end against real Postgres.
/// </summary>
[Collection("Database")]
public class PublicGetSimilarLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    [Fact]
    public async Task GetSimilarLyrics_NonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Similar(Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task GetSimilarLyrics_WithNoMatchesInAnyBranch_ReturnsEmptyArray()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity source = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Similar(source.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetSimilarLyricsResponse body = await response.ReadAsAsync<PublicGetSimilarLyricsResponse>();
        body.Lyrics.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSimilarLyrics_VideoCategoryBranch_ReturnsMatchesSortedByRecency()
    {
        Guid categoryId = await SeedCategoryAsync();

        (LyricsEntity source, LyricsEntity older, LyricsEntity newer) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, LyricsEntity, LyricsEntity)
        >(ctx =>
        {
            VideoEntity sourceVideo = VideoFactory.CreatePublished(categoryId);
            VideoEntity olderVideo = VideoFactory.CreatePublished(categoryId);
            VideoEntity newerVideo = VideoFactory.CreatePublished(categoryId);
            ctx.Videos.AddRange(sourceVideo, olderVideo, newerVideo);

            LyricsEntity source = LyricsFactory.CreatePublishedForVideoWithSlug(
                categoryId,
                sourceVideo.Id,
                $"source-{Guid.NewGuid():N}"
            );
            LyricsEntity older = LyricsFactory.CreatePublishedForVideoWithSlug(
                categoryId,
                olderVideo.Id,
                $"older-{Guid.NewGuid():N}"
            );
            LyricsEntity newer = LyricsFactory.CreatePublishedForVideoWithSlug(
                categoryId,
                newerVideo.Id,
                $"newer-{Guid.NewGuid():N}"
            );
            ctx.Lyrics.AddRange(source, older, newer);
            return (source, older, newer);
        });

        // Backdate `older` so `newer` sorts first — the audit interceptor overwrites CreatedAt
        // on insert, so this must happen via a raw update after seeding.
        await using (ContentDbContext updateCtx = CreateDbContext<ContentDbContext>())
        {
            LyricsEntity? olderTracked = await updateCtx.Lyrics.FindAsync(older.Id);
            olderTracked!.CreatedAt = DateTime.UtcNow.AddDays(-1);
            await updateCtx.SaveChangesAsync();
        }

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Similar(source.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetSimilarLyricsResponse body = await response.ReadAsAsync<PublicGetSimilarLyricsResponse>();
        body.Lyrics.Select(l => l.Id).Should().Equal(newer.Id, older.Id);
    }

    [Fact]
    public async Task GetSimilarLyrics_SharedTagsBranch_RanksByTagCountThenRecency()
    {
        Guid categoryId = await SeedCategoryAsync();

        (LyricsEntity source, LyricsEntity twoTagsMatch, LyricsEntity oneTagMatch) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, LyricsEntity, LyricsEntity)
        >(ctx =>
        {
            TagEntity tagOne = TagFactory.Create();
            TagEntity tagTwo = TagFactory.Create();
            ctx.Tags.AddRange(tagOne, tagTwo);

            LyricsEntity source = LyricsFactory.CreateWithTags(categoryId, tagOne.Id, tagTwo.Id);
            source.MarkPendingReview();
            source.Approve();
            source.Publish();
            LyricsEntity oneTagMatch = LyricsFactory.CreateWithTags(categoryId, tagOne.Id);
            oneTagMatch.MarkPendingReview();
            oneTagMatch.Approve();
            oneTagMatch.Publish();
            LyricsEntity twoTagsMatch = LyricsFactory.CreateWithTags(categoryId, tagOne.Id, tagTwo.Id);
            twoTagsMatch.MarkPendingReview();
            twoTagsMatch.Approve();
            twoTagsMatch.Publish();
            ctx.Lyrics.AddRange(source, oneTagMatch, twoTagsMatch);
            return (source, twoTagsMatch, oneTagMatch);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Similar(source.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetSimilarLyricsResponse body = await response.ReadAsAsync<PublicGetSimilarLyricsResponse>();
        body.Lyrics.Select(l => l.Id).Should().Equal(twoTagsMatch.Id, oneTagMatch.Id);
    }

    [Fact]
    public async Task GetSimilarLyrics_VideoLinkedWithTagsButNoCategoryPeers_FallsThroughToSharedTagsBranch()
    {
        Guid categoryId = await SeedCategoryAsync();

        (LyricsEntity source, LyricsEntity tagMatch) = await SeedAsync<ContentDbContext, (LyricsEntity, LyricsEntity)>(
            ctx =>
            {
                TagEntity sharedTag = TagFactory.Create();
                ctx.Tags.Add(sharedTag);

                VideoEntity sourceVideo = VideoFactory.CreatePublished(categoryId);
                ctx.Videos.Add(sourceVideo);

                LyricsEntity source = LyricsFactory.CreatePublishedForVideoWithSlug(
                    categoryId,
                    sourceVideo.Id,
                    $"source-{Guid.NewGuid():N}"
                );
                source.Tags.Add(LyricsTagEntity.Create(Guid.NewGuid(), source.Id, sharedTag.Id));

                // Standalone (no video), so it is not a category peer — proves the empty category
                // branch falls through to the shared-tags branch rather than stopping empty.
                LyricsEntity tagMatch = LyricsFactory.CreateWithTags(categoryId, sharedTag.Id);
                tagMatch.MarkPendingReview();
                tagMatch.Approve();
                tagMatch.Publish();
                ctx.Lyrics.AddRange(source, tagMatch);
                return (source, tagMatch);
            }
        );

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Similar(source.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetSimilarLyricsResponse body = await response.ReadAsAsync<PublicGetSimilarLyricsResponse>();
        body.Lyrics.Should().NotBeEmpty();
        body.Lyrics.Select(l => l.Id).Should().Equal(tagMatch.Id);
    }
}
