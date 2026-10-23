using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Helpers;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;

/// <summary>
/// Integration tests for the AdminApproveLyricsSubmission endpoint.
/// </summary>
[Collection("Database")]
public class AdminApproveLyricsSubmissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ApproveLyricsSubmission_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsync(Routes.Admin.Lyrics.Submission(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveLyricsSubmission_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsync(Routes.Admin.Lyrics.Submission(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveLyricsSubmission_AsAdmin_WithNonExistentSubmission_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsync(Routes.Admin.Lyrics.Submission(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("LyricsSubmission"))
        );
    }

    [Fact]
    public async Task ApproveLyricsSubmission_HappyPath_CreatesLyricsAndLinksSubmission()
    {
        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create("Eloko Oyo", "Fally Ipupa");
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.LyricsSubmissions.Add(submission);
            return submission;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsync(Routes.Admin.Lyrics.Submission(submission.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminApproveLyricsSubmissionResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persistedLyrics = await ctx.Lyrics.FindAsync(body.LyricsId);
        LyricsSubmissionEntity? persistedSubmission = await ctx.LyricsSubmissions.FindAsync(submission.Id);

        persistedLyrics.Should().NotBeNull();
        persistedLyrics!.Slug.Value.Should().Be(SlugHelper.ToSlug(submission.SongTitle));
        persistedLyrics.SongTitle.Should().Be("Eloko Oyo");
        persistedLyrics.ArtistName.Should().Be("Fally Ipupa");
        persistedLyrics.LyricsText.Should().Be(submission.LyricsText);

        persistedSubmission.Should().NotBeNull();
        persistedSubmission!.Status.Should().Be(EnumSubmissionStatus.Approved);
        persistedSubmission.PublishedLyricsId.Should().Be(body.LyricsId);
        persistedSubmission.ReviewedByUserId.Should().Be(TestUser.AdminId);
    }

    [Fact]
    public async Task ApproveLyricsSubmission_AlreadyApproved_ReturnsConflict()
    {
        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.CreateApproved(Guid.NewGuid(), Guid.NewGuid());
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.LyricsSubmissions.Add(submission);
            return submission;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsync(Routes.Admin.Lyrics.Submission(submission.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<SubmissionErrorMessage>(m => m.NotPending())
        );
    }
}
