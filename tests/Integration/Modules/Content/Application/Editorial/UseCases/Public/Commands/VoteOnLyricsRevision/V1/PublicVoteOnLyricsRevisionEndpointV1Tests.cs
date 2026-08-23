using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.V1;

/// <summary>
/// Integration tests for the PublicVoteOnLyricsRevision endpoint.
/// </summary>
[Collection("Database")]
public class PublicVoteOnLyricsRevisionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task VoteOnLyricsRevision_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(Guid.NewGuid()),
            new PublicVoteOnLyricsRevisionRequest(EnumVote.Approve, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VoteOnLyricsRevision_AsVisitor_WithNonExistentRevision_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(Guid.NewGuid()),
            new PublicVoteOnLyricsRevisionRequest(EnumVote.Approve, null)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("LyricsRevision"))
        );
    }

    [Fact]
    public async Task VoteOnLyricsRevision_DuplicateVoteFromSameUser_ReturnsConflict()
    {
        LyricsRevisionEntity revision = await SeedAsync<ContentDbContext, LyricsRevisionEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsRevisionEntity revision = LyricsRevisionFactory.Create(lyrics.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsRevisions.Add(revision);
            return revision;
        });

        Client.AuthenticateAsVisitor();

        var firstVote = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(revision.Id),
            new PublicVoteOnLyricsRevisionRequest(EnumVote.Approve, null)
        );
        firstVote.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondVote = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(revision.Id),
            new PublicVoteOnLyricsRevisionRequest(EnumVote.Approve, null)
        );

        await secondVote.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsRevisionErrorMessage>(m => m.AlreadyVoted())
        );
    }

    [Fact]
    public async Task VoteOnLyricsRevision_ReachingAutoAcceptThreshold_ReplacesOnlyLyricsText()
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
                "Community-corrected lyrics text"
            );
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsRevisions.Add(revision);
            return (lyrics, revision);
        });

        string originalSlug = lyrics.Slug;
        string originalSongTitle = lyrics.SongTitle;
        string originalArtistName = lyrics.ArtistName;
        EnumContentStatus originalStatus = lyrics.Status;

        for (int i = 0; i < LyricsRevisionConstants.AutoAcceptThreshold; i++)
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
                Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(revision.Id),
                new PublicVoteOnLyricsRevisionRequest(EnumVote.Approve, null)
            );

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsRevisionEntity? persistedRevision = await ctx.LyricsRevisions.FindAsync(revision.Id);
        LyricsEntity? persistedLyrics = await ctx.Lyrics.FindAsync(lyrics.Id);

        persistedRevision.Should().NotBeNull();
        persistedRevision!.Status.Should().Be(EnumRevisionStatus.Accepted);
        persistedRevision.DecidedByUserId.Should().BeNull();

        persistedLyrics.Should().NotBeNull();
        persistedLyrics!.LyricsText.Should().Be("Community-corrected lyrics text");
        persistedLyrics.Slug.Should().Be(originalSlug);
        persistedLyrics.SongTitle.Should().Be(originalSongTitle);
        persistedLyrics.ArtistName.Should().Be(originalArtistName);
        persistedLyrics.Status.Should().Be(originalStatus);
    }
}
