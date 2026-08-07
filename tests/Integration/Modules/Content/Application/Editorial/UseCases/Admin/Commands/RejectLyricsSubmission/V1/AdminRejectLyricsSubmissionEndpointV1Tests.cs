using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission.V1;

/// <summary>
/// Integration tests for the AdminRejectLyricsSubmission endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectLyricsSubmissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task RejectLyricsSubmission_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RejectSubmission(Guid.NewGuid()),
            new AdminRejectLyricsSubmissionRequest("Not a good fit.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectLyricsSubmission_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RejectSubmission(Guid.NewGuid()),
            new AdminRejectLyricsSubmissionRequest("Not a good fit.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectLyricsSubmission_AsAdmin_WithBlankNote_ReturnsBadRequest()
    {
        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
            ctx.LyricsSubmissions.Add(submission);
            return submission;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RejectSubmission(submission.Id),
            new AdminRejectLyricsSubmissionRequest(string.Empty)
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Note", Localized<LyricsErrorMessage>(m => m.RejectionReasonRequired()))
        );
    }

    [Fact]
    public async Task RejectLyricsSubmission_HappyPath_SetsStatusAndNoteWithoutCreatingLyrics()
    {
        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
            ctx.LyricsSubmissions.Add(submission);
            return submission;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RejectSubmission(submission.Id),
            new AdminRejectLyricsSubmissionRequest("Les paroles contiennent des erreurs de transcription.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsSubmissionEntity? persisted = await ctx.LyricsSubmissions.FindAsync(submission.Id);

        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(EnumSubmissionStatus.Rejected);
        persisted.ReviewNote.Should().Be("Les paroles contiennent des erreurs de transcription.");
        persisted.ReviewedByUserId.Should().Be(TestUser.AdminId);
        persisted.PublishedLyricsId.Should().BeNull();

        bool anyLyricsCreated = await ctx.Lyrics.AnyAsync();
        anyLyricsCreated.Should().BeFalse();
    }
}
