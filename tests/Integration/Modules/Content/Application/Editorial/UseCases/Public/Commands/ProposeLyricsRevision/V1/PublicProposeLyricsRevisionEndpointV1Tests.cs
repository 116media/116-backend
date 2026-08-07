using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision.V1;

/// <summary>
/// Integration tests for the PublicProposeLyricsRevision endpoint, including the explicit
/// "no trust exemption based on origin" requirement.
/// </summary>
[Collection("Database")]
public class PublicProposeLyricsRevisionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task ProposeLyricsRevision_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Revisions(Guid.NewGuid()),
            new PublicProposeLyricsRevisionRequest("Corrected text", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProposeLyricsRevision_AsVisitor_WithEmptyProposedText_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Revisions(Guid.NewGuid()),
            new PublicProposeLyricsRevisionRequest(string.Empty, null)
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("ProposedText", Localized<LyricsRevisionErrorMessage>(m => m.ProposedTextRequired()))
        );
    }

    [Fact]
    public async Task ProposeLyricsRevision_AsVisitor_WithNonExistentLyrics_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Revisions(Guid.NewGuid()),
            new PublicProposeLyricsRevisionRequest("Corrected text", null)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task ProposeLyricsRevision_AgainstAdminCreatedLyricsPage_CreatesPendingRevision()
    {
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Revisions(lyrics.Id),
            new PublicProposeLyricsRevisionRequest("Corrected lyrics text", "Fixed a transcription error")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicProposeLyricsRevisionResponse>();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsRevisionEntity? persisted = await ctx.LyricsRevisions.FindAsync(body.RevisionId);

        persisted.Should().NotBeNull();
        persisted!.LyricsId.Should().Be(lyrics.Id);
        persisted.ProposedText.Should().Be("Corrected lyrics text");
        persisted.Status.Should().Be(EnumRevisionStatus.Pending);
    }

    [Fact]
    public async Task ProposeLyricsRevision_AgainstCommunitySubmittedLyricsPage_CreatesPendingRevisionIdentically()
    {
        (LyricsEntity lyrics, LyricsSubmissionEntity submission) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, LyricsSubmissionEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.CreateApproved(Guid.NewGuid(), lyrics.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsSubmissions.Add(submission);
            return (lyrics, submission);
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Revisions(lyrics.Id),
            new PublicProposeLyricsRevisionRequest("Corrected lyrics text", "Fixed a transcription error")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicProposeLyricsRevisionResponse>();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsRevisionEntity? persisted = await ctx.LyricsRevisions.FindAsync(body.RevisionId);

        persisted.Should().NotBeNull();
        persisted!.LyricsId.Should().Be(lyrics.Id);
        persisted.ProposedText.Should().Be("Corrected lyrics text");
        persisted.Status.Should().Be(EnumRevisionStatus.Pending);

        // The submission's own approved state is untouched by proposing a correction against
        // the lyrics page it produced — the two workflows are independent.
        LyricsSubmissionEntity? persistedSubmission = await ctx.LyricsSubmissions.FindAsync(submission.Id);
        persistedSubmission.Should().NotBeNull();
        persistedSubmission!.Status.Should().Be(EnumSubmissionStatus.Approved);
    }
}
