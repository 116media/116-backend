using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Infrastructure.Repositories;

/// <summary>
/// Integration tests proving the <c>(RevisionId, UserId)</c> unique index genuinely rejects a
/// duplicate vote insert at the database level for both <see cref="LyricsTranslationVoteEntity" />
/// and <see cref="LyricsRevisionVoteEntity" />, against a real PostgreSQL database — the real,
/// DB-level backstop enforcement behind the handler-level "already voted" pre-check.
/// </summary>
[Collection("Database")]
public class LyricsReviewVoteConstraintTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task InsertingDuplicateTranslationVote_ForSameRevisionAndUser_ThrowsDbUpdateException()
    {
        Guid userId = Guid.NewGuid();

        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.CreatePublished(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var translation = LyricsTranslationFactory.Create(lyrics.Id, "es");
        context.LyricsTranslations.Add(translation);
        await context.SaveChangesAsync();

        var revision = LyricsTranslationRevisionFactory.Create(translation.Id);
        context.LyricsTranslationRevisions.Add(revision);
        await context.SaveChangesAsync();

        context.LyricsTranslationVotes.Add(LyricsTranslationVoteFactory.CreateApprove(revision.Id, userId));
        await context.SaveChangesAsync();

        await using var duplicateContext = CreateDbContext<ContentDbContext>();
        duplicateContext.LyricsTranslationVotes.Add(LyricsTranslationVoteFactory.CreateApprove(revision.Id, userId));

        Func<Task> act = async () => await duplicateContext.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task InsertingDuplicateLyricsRevisionVote_ForSameRevisionAndUser_ThrowsDbUpdateException()
    {
        Guid userId = Guid.NewGuid();

        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.CreatePublished(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var revision = LyricsRevisionFactory.Create(lyrics.Id);
        context.LyricsRevisions.Add(revision);
        await context.SaveChangesAsync();

        context.LyricsRevisionVotes.Add(LyricsRevisionVoteFactory.CreateApprove(revision.Id, userId));
        await context.SaveChangesAsync();

        await using var duplicateContext = CreateDbContext<ContentDbContext>();
        duplicateContext.LyricsRevisionVotes.Add(LyricsRevisionVoteFactory.CreateApprove(revision.Id, userId));

        Func<Task> act = async () => await duplicateContext.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
