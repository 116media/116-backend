using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Projections;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Integration.Tests.Infrastructure.Search;

/// <summary>
/// Integration tests for the search projection, driven through the real domain transitions
/// so the domain events, their handlers and the written documents are all exercised.
/// </summary>
[Collection("Database")]
public class SearchProjectionTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();

        ContentTypeEntity contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    private async Task<SearchDocument?> FindAsync(EnumCoreContentType type, Guid entityId)
    {
        await using var context = CreateDbContext<ContentDbContext>();

        return await context.SearchDocuments.FirstOrDefaultAsync(document =>
            document.Type == type && document.EntityId == entityId
        );
    }

    [Fact]
    public async Task PublishingAnArticle_ShouldWriteAVisibleDocument()
    {
        // Arrange
        CategoryEntity category = await SeedCategoryAsync();
        ArticleEntity article = new ArticleBuilder(category.Id)
            .WithTitle("Rumba au Grand Hotel")
            .WithSlug("rumba-grand-hotel")
            .AsApproved()
            .Build();

        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        }

        // Act — the real transition raises the event the projection handler listens on
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            ArticleEntity tracked = await context.Articles.FirstAsync(candidate => candidate.Id == article.Id);
            tracked.Publish(DateTimeOffset.UtcNow);
            await context.SaveChangesAsync();
        }

        // Assert
        SearchDocument? document = await FindAsync(EnumCoreContentType.Article, article.Id);

        document.Should().NotBeNull();
        document!.IsVisible.Should().BeTrue();
        document.Title.Should().Be("Rumba au Grand Hotel");
        document.Slug.Should().Be("rumba-grand-hotel");
        document.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task ArchivingAPublishedArticle_ShouldHideItsDocumentRatherThanDeleteIt()
    {
        // Arrange
        CategoryEntity category = await SeedCategoryAsync();
        ArticleEntity article = new ArticleBuilder(category.Id)
            .WithTitle("Sebene au crepuscule")
            .WithSlug("sebene-crepuscule")
            .AsPublished()
            .Build();

        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        }

        await Resolve<ISearchProjection>().ProjectAsync(EnumCoreContentType.Article, article.Id);

        // Act
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            ArticleEntity tracked = await context.Articles.FirstAsync(candidate => candidate.Id == article.Id);
            tracked.Archive();
            await context.SaveChangesAsync();
        }

        // Assert
        SearchDocument? document = await FindAsync(EnumCoreContentType.Article, article.Id);

        document.Should().NotBeNull("a republish updates the row rather than reinserting it");
        document!.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task RenamingAShortVideo_ShouldReprojectItsNewTitle()
    {
        // Arrange
        ShortVideoEntity shortVideo = new ShortVideoBuilder()
            .WithTitle("Teaser avant montage")
            .WithSlug("teaser-avant-montage")
            .Build();

        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.ShortVideos.Add(shortVideo);
            await context.SaveChangesAsync();
        }

        // Act — shorts have no editorial workflow, so their title stays mutable for life
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            ShortVideoEntity tracked = await context.ShortVideos.FirstAsync(candidate => candidate.Id == shortVideo.Id);
            tracked.Update("Teaser apres montage", videoId: null);
            await context.SaveChangesAsync();
        }

        // Assert
        SearchDocument? document = await FindAsync(EnumCoreContentType.Short, shortVideo.Id);

        document.Should().NotBeNull();
        document!.Title.Should().Be("Teaser apres montage");
    }

    [Fact]
    public async Task DeactivatingAShortVideo_ShouldHideItsDocument()
    {
        // Arrange
        ShortVideoEntity shortVideo = new ShortVideoBuilder()
            .WithTitle("Teaser retire")
            .WithSlug("teaser-retire")
            .Build();

        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.ShortVideos.Add(shortVideo);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            ShortVideoEntity tracked = await context.ShortVideos.FirstAsync(candidate => candidate.Id == shortVideo.Id);
            tracked.Deactivate();
            await context.SaveChangesAsync();
        }

        // Assert
        SearchDocument? document = await FindAsync(EnumCoreContentType.Short, shortVideo.Id);

        document.Should().NotBeNull();
        document!.IsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task RebuildAllAsync_AfterTheProjectionDrifts_ShouldRestoreEveryDocument()
    {
        // Arrange
        CategoryEntity category = await SeedCategoryAsync();

        await using (var context = CreateDbContext<ContentDbContext>())
        {
            context.Articles.Add(
                new ArticleBuilder(category.Id).WithTitle("Kwassa un").WithSlug("kwassa-un").AsPublished().Build()
            );
            context.Videos.Add(
                new VideoBuilder(category.Id).WithTitle("Kwassa deux").WithSlug("kwassa-deux").AsPublished().Build()
            );
            context.ShortVideos.Add(new ShortVideoBuilder().WithTitle("Kwassa trois").WithSlug("kwassa-trois").Build());
            await context.SaveChangesAsync();
        }

        // Simulate drift: the documents are gone while the sources remain
        await using (var context = CreateDbContext<ContentDbContext>())
        {
            await context.SearchDocuments.ExecuteDeleteAsync();
        }

        // Act
        IReadOnlyDictionary<EnumCoreContentType, int> written = await Resolve<ISearchProjection>().RebuildAllAsync();

        // Assert
        written[EnumCoreContentType.Article].Should().Be(1);
        written[EnumCoreContentType.Video].Should().Be(1);
        written[EnumCoreContentType.Short].Should().Be(1);

        await using var verification = CreateDbContext<ContentDbContext>();
        (await verification.SearchDocuments.CountAsync()).Should().Be(3);
    }
}
