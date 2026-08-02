using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision.V1;

/// <summary>
/// Integration tests for the AdminDecideTranslationRevision endpoint.
/// </summary>
[Collection("Database")]
public class AdminDecideTranslationRevisionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DecideTranslationRevision_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Translations.Revision(Guid.NewGuid()),
            new AdminDecideTranslationRevisionRequest(true)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DecideTranslationRevision_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Translations.Revision(Guid.NewGuid()),
            new AdminDecideTranslationRevisionRequest(true)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DecideTranslationRevision_AsAdmin_WithNonExistentRevision_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Translations.Revision(Guid.NewGuid()),
            new AdminDecideTranslationRevisionRequest(true)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A moderator can accept a pending revision directly with zero community votes cast,
    /// bypassing the vote tally entirely, applying the proposed text to the translation and
    /// attributing the decision to the deciding admin.
    /// </summary>
    [Fact]
    public async Task DecideTranslationRevision_AdminAcceptsWithZeroVotes_BypassesTallyAndAppliesText()
    {
        (LyricsTranslationEntity translation, LyricsTranslationRevisionEntity revision) = await SeedAsync<
            ContentDbContext,
            (LyricsTranslationEntity, LyricsTranslationRevisionEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsTranslationEntity translation = LyricsTranslationFactory.CreateWithText(
                lyrics.Id,
                "es",
                "Original text"
            );
            LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
                translation.Id,
                Guid.NewGuid(),
                "Moderator-accepted text"
            );
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsTranslations.Add(translation);
            ctx.LyricsTranslationRevisions.Add(revision);
            return (translation, revision);
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Translations.Revision(revision.Id),
            new AdminDecideTranslationRevisionRequest(true)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsTranslationRevisionEntity? persistedRevision = await ctx.LyricsTranslationRevisions.FindAsync(
            revision.Id
        );
        LyricsTranslationEntity? persistedTranslation = await ctx.LyricsTranslations.FindAsync(translation.Id);

        persistedRevision.Should().NotBeNull();
        persistedRevision!.Status.Should().Be(EnumRevisionStatus.Accepted);
        persistedRevision.DecidedByUserId.Should().Be(TestUser.AdminId);

        persistedTranslation.Should().NotBeNull();
        persistedTranslation!.Text.Should().Be("Moderator-accepted text");
        persistedTranslation.Source.Should().Be(EnumTranslationSource.Community);
    }

    /// <summary>
    /// A moderator can reject a pending revision directly with votes already cast below the
    /// auto-accept threshold, bypassing the tally in the opposite direction — the translation's
    /// text is left untouched.
    /// </summary>
    [Fact]
    public async Task DecideTranslationRevision_AdminRejectsWithSomeApprovalVotes_BypassesTallyAndLeavesTranslationUnchanged()
    {
        (LyricsTranslationEntity translation, LyricsTranslationRevisionEntity revision) = await SeedAsync<
            ContentDbContext,
            (LyricsTranslationEntity, LyricsTranslationRevisionEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsTranslationEntity translation = LyricsTranslationFactory.CreateWithText(
                lyrics.Id,
                "es",
                "Original text"
            );
            LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
                translation.Id,
                Guid.NewGuid(),
                "Rejected proposed text"
            );
            LyricsTranslationVoteEntity vote = LyricsTranslationVoteFactory.CreateApprove(revision.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsTranslations.Add(translation);
            ctx.LyricsTranslationRevisions.Add(revision);
            ctx.LyricsTranslationVotes.Add(vote);
            return (translation, revision);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Translations.Revision(revision.Id),
            new AdminDecideTranslationRevisionRequest(false)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsTranslationRevisionEntity? persistedRevision = await ctx.LyricsTranslationRevisions.FindAsync(
            revision.Id
        );
        LyricsTranslationEntity? persistedTranslation = await ctx.LyricsTranslations.FindAsync(translation.Id);

        persistedRevision.Should().NotBeNull();
        persistedRevision!.Status.Should().Be(EnumRevisionStatus.Rejected);
        persistedRevision.DecidedByUserId.Should().Be(TestUser.SuperAdminId);

        persistedTranslation.Should().NotBeNull();
        persistedTranslation!.Text.Should().Be("Original text");
    }
}
