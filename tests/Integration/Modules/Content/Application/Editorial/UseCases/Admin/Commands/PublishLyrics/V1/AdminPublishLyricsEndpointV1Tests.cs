using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishLyrics.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Shared.Domain.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishLyrics.V1;

/// <summary>
/// Integration tests for the AdminPublishLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminPublishLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task PublishLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublishLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task PublishLyrics_WhenAlreadyPublished_ReturnsConflict()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreatePublished);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsErrorMessage>(m => m.AlreadyPublished())
        );
        (await GetLyricsAsync(lyrics.Id)).Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public async Task PublishLyrics_WhenDraft_ReturnsBadRequest()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.Create);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        await response.ShouldBeProblem<DomainRuleException>(
            HttpStatusCode.BadRequest,
            Localized<LyricsErrorMessage>(m =>
                m.InvalidStatusTransition(
                    from: nameof(EnumContentStatus.Draft),
                    to: nameof(EnumContentStatus.Published)
                )
            )
        );
        (await GetLyricsAsync(lyrics.Id)).Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task PublishLyrics_AsSuperAdmin_ApprovedLyrics_ReturnsOk()
    {
        LyricsEntity lyrics = await SeedLyricsAsync(LyricsFactory.CreateApproved);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Lyrics, lyrics.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminPublishLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        LyricsEntity persisted = await GetLyricsAsync(lyrics.Id);
        persisted.Status.Should().Be(EnumContentStatus.Published);
        persisted.PublishedAt.Should().NotBeNull();
    }
}
