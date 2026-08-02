using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision.V1;

/// <summary>
/// Integration tests for the AdminDecideLyricsRevision endpoint.
/// </summary>
[Collection("Database")]
public class AdminDecideLyricsRevisionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DecideLyricsRevision_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Revision(Guid.NewGuid()),
            new AdminDecideLyricsRevisionRequest(true)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DecideLyricsRevision_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Revision(Guid.NewGuid()),
            new AdminDecideLyricsRevisionRequest(true)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DecideLyricsRevision_AsAdmin_WithNonExistentRevision_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Revision(Guid.NewGuid()),
            new AdminDecideLyricsRevisionRequest(true)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A moderator can accept a pending lyrics-text correction directly with zero community
    /// votes cast, bypassing the tally, applying the proposed text to the lyrics page.
    /// </summary>
    [Fact]
    public async Task DecideLyricsRevision_AdminAcceptsWithZeroVotes_BypassesTallyAndReplacesText()
    {
        (LyricsEntity lyrics, LyricsRevisionEntity revision) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, LyricsRevisionEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsRevisionEntity revision = LyricsRevisionFactory.Create(
                lyrics.Id,
                Guid.NewGuid(),
                "Moderator-accepted lyrics text"
            );
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsRevisions.Add(revision);
            return (lyrics, revision);
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Revision(revision.Id),
            new AdminDecideLyricsRevisionRequest(true)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsRevisionEntity? persistedRevision = await ctx.LyricsRevisions.FindAsync(revision.Id);
        LyricsEntity? persistedLyrics = await ctx.Lyrics.FindAsync(lyrics.Id);

        persistedRevision.Should().NotBeNull();
        persistedRevision!.Status.Should().Be(EnumRevisionStatus.Accepted);
        persistedRevision.DecidedByUserId.Should().Be(TestUser.AdminId);

        persistedLyrics.Should().NotBeNull();
        persistedLyrics!.LyricsText.Should().Be("Moderator-accepted lyrics text");
    }

    /// <summary>
    /// A moderator can reject a pending correction directly with votes already cast below the
    /// auto-accept threshold, bypassing the tally in the opposite direction — the lyrics page's
    /// text is left untouched.
    /// </summary>
    [Fact]
    public async Task DecideLyricsRevision_AdminRejectsWithSomeApprovalVotes_BypassesTallyAndLeavesLyricsUnchanged()
    {
        (LyricsEntity lyrics, LyricsRevisionEntity revision) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, LyricsRevisionEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsRevisionEntity revision = LyricsRevisionFactory.Create(
                lyrics.Id,
                Guid.NewGuid(),
                "Rejected proposed lyrics text"
            );
            LyricsRevisionVoteEntity vote = LyricsRevisionVoteFactory.CreateApprove(revision.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsRevisions.Add(revision);
            ctx.LyricsRevisionVotes.Add(vote);
            return (lyrics, revision);
        });

        string originalText = lyrics.LyricsText;

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Revision(revision.Id),
            new AdminDecideLyricsRevisionRequest(false)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsRevisionEntity? persistedRevision = await ctx.LyricsRevisions.FindAsync(revision.Id);
        LyricsEntity? persistedLyrics = await ctx.Lyrics.FindAsync(lyrics.Id);

        persistedRevision.Should().NotBeNull();
        persistedRevision!.Status.Should().Be(EnumRevisionStatus.Rejected);
        persistedRevision.DecidedByUserId.Should().Be(TestUser.SuperAdminId);

        persistedLyrics.Should().NotBeNull();
        persistedLyrics!.LyricsText.Should().Be(originalText);
    }
}
