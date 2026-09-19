using _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Cache;
using _116.Core.Application.Shared.EventHandlers;
using _116.Core.Domain.Entities;
using _116.Core.Domain.Events;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Application.Services;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Covers the file projection cache: a resolved file must be served from cache rather than
/// re-queried, and the handlers that evict it on replace or delete must actually be registered.
/// Together these are the two halves that make the cache safe — one proves it caches, the other
/// proves it lets go.
/// </summary>
[Collection("Database")]
public class FileProjectionCacheFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    private const string OriginalUrl = "https://res.cloudinary.com/test-cloud/image/upload/poster-original.jpg";
    private const string PoisonedUrl = "https://res.cloudinary.com/test-cloud/image/upload/poster-poisoned.jpg";

    [Fact]
    public async Task ResolvedFileUrl_ShouldBeServedFromCacheRatherThanRequeried()
    {
        FileEntity poster = await SeedAsync<CoreDbContext, FileEntity>(ctx =>
        {
            FileEntity file = FileFactory.CreateWithStorageUrl(OriginalUrl);
            ctx.Files.Add(file);
            return file;
        });

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            category.SetPosterFileId(poster.Id);
            ctx.Categories.Add(category);
        });

        Client.ClearAuthentication();

        // Warm: the first read resolves the poster from the database.
        PublicGetActiveCategoriesResponse warmed = await ReadCategoriesAsync();
        warmed.Categories.Should().ContainSingle().Which.PosterUrl.Should().Be(OriginalUrl);

        // Poison the row directly, bypassing the event pipeline that would evict.
        await using (CoreDbContext poisonCtx = CreateDbContext<CoreDbContext>())
        {
            await poisonCtx
                .Files.Where(file => file.Id == poster.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(file => file.StorageUrl, PoisonedUrl));
        }

        // Still the original: proof the projection came from cache, not the database.
        PublicGetActiveCategoriesResponse cached = await ReadCategoriesAsync();
        cached.Categories.Should().ContainSingle().Which.PosterUrl.Should().Be(OriginalUrl);
    }

    [Fact]
    public void FileLifecycleEvents_ShouldEvictTheCacheAlongsideTheAssetCleanup()
    {
        // Both handlers are registered against the same events; resolving only one would leave
        // either the remote asset or the cached URL behind.
        using IServiceScope scope = Api.Services.CreateScope();

        IEnumerable<IDomainEventHandler<FileSoftDeletedEvent>> deletedHandlers = scope.ServiceProvider.GetServices<
            IDomainEventHandler<FileSoftDeletedEvent>
        >();
        IEnumerable<IDomainEventHandler<FileReplacedEvent>> replacedHandlers = scope.ServiceProvider.GetServices<
            IDomainEventHandler<FileReplacedEvent>
        >();

        deletedHandlers.Select(handler => handler.GetType().Name).Should().Contain(nameof(FileCacheHandler));
        replacedHandlers.Select(handler => handler.GetType().Name).Should().Contain(nameof(FileCacheHandler));
    }

    [Fact]
    public void FileCacheTag_ShouldBeNamespacedToTheOwningModule()
    {
        // The tag is the contract between the projection and its evictors; a rename in one place
        // silently stops eviction.
        CoreCacheTags.Files.Should().Be("core:files");
    }

    /// <summary>
    /// Reads the public active-categories feed anonymously.
    /// </summary>
    /// <returns>The response body.</returns>
    private async Task<PublicGetActiveCategoriesResponse> ReadCategoriesAsync()
    {
        var response = await Client.GetAsync(ApiRoutes.Public.Categories);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return await response.ReadAsAsync<PublicGetActiveCategoriesResponse>();
    }
}
