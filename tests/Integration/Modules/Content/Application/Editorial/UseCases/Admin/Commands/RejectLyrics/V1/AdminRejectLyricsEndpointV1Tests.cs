using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics.V1;

/// <summary>
/// Integration tests for the AdminRejectLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<LyricsEntity> SeedLyricsAsync(Func<Guid, LyricsEntity> create)
    {
        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    private async Task<LyricsEntity> GetLyricsAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? lyrics = await ctx.Lyrics.FindAsync(id);
        return lyrics!;
    }

    [Fact]
    public async Task RejectLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminRejectLyricsRequest(TestConstants.Content.Editorial.Lyrics.ValidRejectionReason);

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that rejecting a lyrics page that is already in Rejected status
    /// returns a 409 Conflict problem and leaves the lyrics page rejected.
    /// </summary>
    [Fact]
    public async Task RejectLyrics_WhenAlreadyRejected_ReturnsConflict()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreateRejected);
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminRejectLyricsRequest(TestConstants.Content.Editorial.Lyrics.ValidRejectionReason);

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Lyrics, lyrics.Id),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
        (await GetLyricsAsync(lyrics.Id)).Status.Should().Be(EnumContentStatus.Rejected);
    }

    /// <summary>
    /// Verifies that rejecting a PendingReview lyrics page succeeds, returns IsSuccess true,
    /// transitions the persisted status to Rejected, and records the rejection reason.
    /// </summary>
    [Fact]
    public async Task RejectLyrics_AsSuperAdmin_PendingReviewLyrics_ReturnsOk()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreatePendingReview);
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminRejectLyricsRequest(TestConstants.Content.Editorial.Lyrics.ValidRejectionReason);

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Lyrics, lyrics.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminRejectLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        LyricsEntity persisted = await GetLyricsAsync(lyrics.Id);
        persisted.Status.Should().Be(EnumContentStatus.Rejected);
        persisted.RejectionReason.Should().Be(request.Reason);
    }
}
