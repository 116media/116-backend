using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics.V1;

/// <summary>
/// Integration tests for the AdminSubmitLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminSubmitLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    private async Task<EnumContentStatus> GetLyricsStatusAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? lyrics = await ctx.Lyrics.FindAsync(id);
        return lyrics!.Status;
    }

    [Fact]
    public async Task SubmitLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that submitting a free lyrics page that is already in PendingReview status
    /// returns a 409 Conflict problem and leaves the status unchanged.
    /// </summary>
    [Fact]
    public async Task SubmitLyrics_WhenAlreadySubmitted_ReturnsConflict()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreatePendingReview);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
        (await GetLyricsStatusAsync(lyrics.Id)).Should().Be(EnumContentStatus.PendingReview);
    }

    /// <summary>
    /// Verifies that submitting a free draft lyrics page succeeds, returns IsSuccess true,
    /// and transitions the persisted status from Draft to PendingReview.
    /// </summary>
    [Fact]
    public async Task SubmitLyrics_AsSuperAdmin_FreeLyrics_ReturnsOk()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.Create);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminSubmitLyricsResponse>();
        body.IsSuccess.Should().BeTrue();
        (await GetLyricsStatusAsync(lyrics.Id)).Should().Be(EnumContentStatus.PendingReview);
    }
}
