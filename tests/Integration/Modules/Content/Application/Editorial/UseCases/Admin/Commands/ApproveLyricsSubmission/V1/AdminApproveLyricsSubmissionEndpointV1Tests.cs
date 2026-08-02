using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;

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

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(Guid.NewGuid()),
            new AdminApproveLyricsSubmissionRequest("some-slug")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveLyricsSubmission_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(Guid.NewGuid()),
            new AdminApproveLyricsSubmissionRequest("some-slug")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveLyricsSubmission_AsAdmin_WithNonExistentSubmission_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(Guid.NewGuid()),
            new AdminApproveLyricsSubmissionRequest("some-slug")
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Approving a pending submission creates a real, correctly-populated
    /// <see cref="LyricsEntity" />, links it back onto the submission as
    /// <c>PublishedLyricsId</c>, and marks the submission <c>Approved</c>.
    /// </summary>
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

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(submission.Id),
            new AdminApproveLyricsSubmissionRequest("eloko-oyo-approved")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminApproveLyricsSubmissionResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persistedLyrics = await ctx.Lyrics.FindAsync(body.LyricsId);
        LyricsSubmissionEntity? persistedSubmission = await ctx.LyricsSubmissions.FindAsync(submission.Id);

        persistedLyrics.Should().NotBeNull();
        persistedLyrics!.Slug.Should().Be("eloko-oyo-approved");
        persistedLyrics.SongTitle.Should().Be("Eloko Oyo");
        persistedLyrics.ArtistName.Should().Be("Fally Ipupa");
        persistedLyrics.LyricsText.Should().Be(submission.LyricsText);

        persistedSubmission.Should().NotBeNull();
        persistedSubmission!.Status.Should().Be(EnumSubmissionStatus.Approved);
        persistedSubmission.PublishedLyricsId.Should().Be(body.LyricsId);
        persistedSubmission.ReviewedByUserId.Should().Be(TestUser.AdminId);
    }

    /// <summary>
    /// Approving a submission with a slug that collides with an existing published lyrics
    /// page's slug is rejected as a conflict rather than silently overwriting it.
    /// </summary>
    [Fact]
    public async Task ApproveLyricsSubmission_WithSlugCollidingWithExistingLyrics_ReturnsConflict()
    {
        (LyricsSubmissionEntity submission, LyricsEntity existingLyrics) = await SeedAsync<
            ContentDbContext,
            (LyricsSubmissionEntity, LyricsEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            LyricsEntity existingLyrics = LyricsFactory.CreateWithSlug(category.Id, "already-taken-slug");
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(existingLyrics);
            ctx.LyricsSubmissions.Add(submission);
            return (submission, existingLyrics);
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(submission.Id),
            new AdminApproveLyricsSubmissionRequest("already-taken-slug")
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsSubmissionEntity? persistedSubmission = await ctx.LyricsSubmissions.FindAsync(submission.Id);
        persistedSubmission.Should().NotBeNull();
        persistedSubmission!.Status.Should().Be(EnumSubmissionStatus.Pending);
    }

    /// <summary>
    /// Approving a submission that has already been decided (not <c>Pending</c>) is rejected
    /// rather than creating a second lyrics record from the same submission.
    /// </summary>
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

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(submission.Id),
            new AdminApproveLyricsSubmissionRequest("some-other-slug")
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }
}
