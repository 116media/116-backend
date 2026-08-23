using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Drives the real <see cref="AbandonedDraftCleanupJob" /> once against the real database and the
/// application's own dependency injection container. A scheduled job is reachable from neither an
/// HTTP route nor a repository method, so invoking <c>Execute</c> with the container's real
/// <see cref="IServiceScopeFactory" /> is its entry point: every repository, unit of work,
/// interceptor and post-commit event handler the run touches is resolved from the live host, and
/// nothing about the job's own collaborators is substituted. Quartz keeps job instantiation inside
/// its own job factory rather than registering the type in the container, so the registration
/// itself is asserted separately against the running scheduler.
/// </summary>
[Collection("Database")]
public class AbandonedDraftCleanupJobTests(PostgresFixture db) : BaseApiTest(db)
{
    private StubCloudinaryService CloudinaryStub => Api.Services.GetRequiredService<StubCloudinaryService>();

    /// <summary>
    /// Builds the job with the host's real scope factory, mirroring how Quartz instantiates it.
    /// </summary>
    private AbandonedDraftCleanupJob CreateJob() =>
        new(Api.Services.GetRequiredService<IServiceScopeFactory>(), NullLogger<AbandonedDraftCleanupJob>.Instance);

    [Fact]
    public async Task ContentModule_SchedulesTheJobWithTheRunningScheduler()
    {
        var schedulerFactory = Api.Services.GetRequiredService<ISchedulerFactory>();
        IScheduler scheduler = await schedulerFactory.GetScheduler(TestContext.Current.CancellationToken);

        bool exists = await scheduler.CheckExists(
            new JobKey(nameof(AbandonedDraftCleanupJob)),
            TestContext.Current.CancellationToken
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_PurgesTheAbandonedDraftWithItsAssetsAndLeavesLiveArticlesAlone()
    {
        string firstBodyKey = $"content/articles/abandoned-body-a-{Guid.NewGuid():N}";
        string secondBodyKey = $"content/articles/abandoned-body-b-{Guid.NewGuid():N}";

        FileEntity coverFile = await SeedAsync<CoreDbContext, FileEntity>(ctx =>
        {
            FileEntity file = FileFactory.CreateWithStorageKey($"content/articles/abandoned-cover-{Guid.NewGuid():N}");
            ctx.Files.Add(file);
            return file;
        });

        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ArticleEntity abandonedDraft = ArticleFactory.Create(category.Id);
        abandonedDraft.UpdateCoverImage(coverFile.Id);
        ArticleEntity recentDraft = ArticleFactory.Create(category.Id);
        ArticleEntity publishedArticle = ArticleFactory.CreatePublished(category.Id);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.AddRange(abandonedDraft, recentDraft, publishedArticle);
            ctx.ArticleImages.AddRange(
                ArticleImageFactory.CreateFromStorageKeys(abandonedDraft.Id, [firstBodyKey, secondBodyKey])
            );
        });

        // The audit interceptor stamps CreatedAt on insert, so the draft is aged past the job's
        // seven-day cutoff with a separate update that leaves the insert-time stamp behind.
        await using (ContentDbContext backdateCtx = CreateDbContext<ContentDbContext>())
        {
            await backdateCtx
                .Articles.Where(article => article.Id == abandonedDraft.Id)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(article => article.CreatedAt, DateTime.UtcNow.AddDays(-30))
                );
        }

        AbandonedDraftCleanupJob job = CreateJob();

        await job.Execute(new TestJobExecutionContext(TestContext.Current.CancellationToken));

        await using ContentDbContext contentCtx = CreateDbContext<ContentDbContext>();
        (await contentCtx.Articles.FindAsync(abandonedDraft.Id)).Should().BeNull();
        (await contentCtx.ArticleImages.CountAsync(image => image.ArticleId == abandonedDraft.Id)).Should().Be(0);
        (await contentCtx.Articles.FindAsync(recentDraft.Id)).Should().NotBeNull();
        (await contentCtx.Articles.FindAsync(publishedArticle.Id)).Should().NotBeNull();

        await using CoreDbContext coreCtx = CreateDbContext<CoreDbContext>();
        FileEntity? persistedCover = await coreCtx.Files.FindAsync(coverFile.Id);
        persistedCover!.IsDeleted.Should().BeTrue();

        CloudinaryStub.DeletedPublicIds.Should().Contain([firstBodyKey, secondBodyKey]);
    }

    [Fact]
    public async Task Execute_WithNoDraftOlderThanTheCutoff_RemovesNothing()
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ArticleEntity recentDraft = ArticleFactory.Create(category.Id);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(recentDraft);
        });

        AbandonedDraftCleanupJob job = CreateJob();

        await job.Execute(new TestJobExecutionContext(TestContext.Current.CancellationToken));

        await using ContentDbContext contentCtx = CreateDbContext<ContentDbContext>();
        (await contentCtx.Articles.FindAsync(recentDraft.Id)).Should().NotBeNull();
    }
}
