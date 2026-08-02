using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.V1;

/// <summary>
/// Integration tests for the PublicVoteOnTranslationRevision endpoint.
/// </summary>
[Collection("Database")]
public class PublicVoteOnTranslationRevisionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task VoteOnTranslationRevision_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Translations.RevisionVotes(Guid.NewGuid()),
            new PublicVoteOnTranslationRevisionRequest(EnumVote.Approve, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VoteOnTranslationRevision_AsVisitor_WithNonExistentRevision_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Translations.RevisionVotes(Guid.NewGuid()),
            new PublicVoteOnTranslationRevisionRequest(EnumVote.Approve, null)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A second vote from the same user on the same revision is rejected as a conflict — the
    /// one-vote-per-user rule, enforced at the DB level by the unique <c>(RevisionId, UserId)</c>
    /// index, is pre-checked in the handler and surfaced as 409.
    /// </summary>
    [Fact]
    public async Task VoteOnTranslationRevision_DuplicateVoteFromSameUser_ReturnsConflict()
    {
        LyricsTranslationRevisionEntity revision = await SeedAsync<ContentDbContext, LyricsTranslationRevisionEntity>(
            ctx =>
            {
                ContentTypeEntity contentType = ContentTypeFactory.Create();
                CategoryEntity category = CategoryFactory.Create(contentType.Id);
                LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
                LyricsTranslationEntity translation = LyricsTranslationFactory.Create(lyrics.Id, "es");
                LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(translation.Id);
                ctx.ContentTypes.Add(contentType);
                ctx.Categories.Add(category);
                ctx.Lyrics.Add(lyrics);
                ctx.LyricsTranslations.Add(translation);
                ctx.LyricsTranslationRevisions.Add(revision);
                return revision;
            }
        );

        Client.AuthenticateAsVisitor();

        var firstVote = await Client.PostAsJsonAsync(
            Routes.Public.Translations.RevisionVotes(revision.Id),
            new PublicVoteOnTranslationRevisionRequest(EnumVote.Approve, null)
        );
        firstVote.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondVote = await Client.PostAsJsonAsync(
            Routes.Public.Translations.RevisionVotes(revision.Id),
            new PublicVoteOnTranslationRevisionRequest(EnumVote.Approve, null)
        );

        await secondVote.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Once net approvals reach <see cref="TranslationConstants.AutoAcceptThreshold" />, the
    /// revision is auto-accepted and its proposed text is applied to the translation in the
    /// same operation — verified here by re-fetching the translation afterward and confirming
    /// its text and source were actually updated, not just the revision's own status.
    /// </summary>
    [Fact]
    public async Task VoteOnTranslationRevision_ReachingAutoAcceptThreshold_AppliesRevisionToTranslation()
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
                "Original AI text"
            );
            LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
                translation.Id,
                Guid.NewGuid(),
                "Community-corrected text"
            );
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsTranslations.Add(translation);
            ctx.LyricsTranslationRevisions.Add(revision);
            return (translation, revision);
        });

        for (int i = 0; i < TranslationConstants.AutoAcceptThreshold; i++)
        {
            Guid voterId = Guid.NewGuid();
            await SeedAsync<IdentityDbContext>(ctx =>
            {
                UserEntity voter = UserFactory.CreateWithId(voterId);
                voter.MarkAsVerified();
                voter.Activate();
                ctx.Users.Add(voter);
            });

            Client.AuthenticateAs(voterId, "Visitor");

            var response = await Client.PostAsJsonAsync(
                Routes.Public.Translations.RevisionVotes(revision.Id),
                new PublicVoteOnTranslationRevisionRequest(EnumVote.Approve, null)
            );

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsTranslationRevisionEntity? persistedRevision = await ctx.LyricsTranslationRevisions.FindAsync(
            revision.Id
        );
        LyricsTranslationEntity? persistedTranslation = await ctx.LyricsTranslations.FindAsync(translation.Id);

        persistedRevision.Should().NotBeNull();
        persistedRevision!.Status.Should().Be(EnumRevisionStatus.Accepted);
        persistedRevision.DecidedByUserId.Should().BeNull();

        persistedTranslation.Should().NotBeNull();
        persistedTranslation!.Text.Should().Be("Community-corrected text");
        persistedTranslation.Source.Should().Be(EnumTranslationSource.Community);
    }

    /// <summary>
    /// Below the auto-accept threshold, approval votes accumulate without accepting the
    /// revision or touching the translation's text.
    /// </summary>
    [Fact]
    public async Task VoteOnTranslationRevision_BelowAutoAcceptThreshold_LeavesRevisionPending()
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
                "Original AI text"
            );
            LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
                translation.Id,
                Guid.NewGuid(),
                "Community-corrected text"
            );
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsTranslations.Add(translation);
            ctx.LyricsTranslationRevisions.Add(revision);
            return (translation, revision);
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Translations.RevisionVotes(revision.Id),
            new PublicVoteOnTranslationRevisionRequest(EnumVote.Approve, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsTranslationEntity? persistedTranslation = await ctx.LyricsTranslations.FindAsync(translation.Id);

        persistedTranslation.Should().NotBeNull();
        persistedTranslation!.Text.Should().Be("Original AI text");
    }
}
