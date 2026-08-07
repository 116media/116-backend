using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics.V1;

/// <summary>
/// Integration tests for the AdminArchiveLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminArchiveLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task ArchiveLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Archive(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ArchiveLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Archive(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ArchiveLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Archive(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ArchiveLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Archive(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task ArchiveLyrics_WhenAlreadyArchived_ReturnsConflict()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreateArchived);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Archive(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsErrorMessage>(m => m.AlreadyArchived())
        );
        (await GetLyricsStatusAsync(lyrics.Id)).Should().Be(EnumContentStatus.Archived);
    }

    [Fact]
    public async Task ArchiveLyrics_AsSuperAdmin_PublishedLyrics_ReturnsOk()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreatePublished);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Archive(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminArchiveLyricsResponse>();
        body.IsSuccess.Should().BeTrue();
        (await GetLyricsStatusAsync(lyrics.Id)).Should().Be(EnumContentStatus.Archived);
    }
}
