using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyrics.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyrics.V1;

/// <summary>
/// Integration tests for the AdminApproveLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminApproveLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task ApproveLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Approve(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Approve(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Approve(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Approve(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task ApproveLyrics_WhenAlreadyApproved_ReturnsConflict()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreateApproved);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Approve(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsErrorMessage>(m => m.AlreadyApproved())
        );
        (await GetLyricsStatusAsync(lyrics.Id)).Should().Be(EnumContentStatus.Approved);
    }

    [Fact]
    public async Task ApproveLyrics_AsSuperAdmin_PendingReviewLyrics_ReturnsOk()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreatePendingReview);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Approve(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminApproveLyricsResponse>();
        body.IsSuccess.Should().BeTrue();
        (await GetLyricsStatusAsync(lyrics.Id)).Should().Be(EnumContentStatus.Approved);
    }
}
