using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions.V1;
using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.V1;

/// <summary>
/// Integration tests for the PublicSubmitLyrics endpoint — the identity-gated verified-artist
/// fast path proof is the most important coverage in this file.
/// </summary>
[Collection("Database")]
public class PublicSubmitLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task SubmitLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Submissions(),
            new PublicSubmitLyricsRequest("Eloko Oyo", "Fally Ipupa", "Some lyrics text.", "fr", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitLyrics_WithoutClaimedArtistAndValidArtistName_QueuesSubmission()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Submissions(),
            new PublicSubmitLyricsRequest("Eloko Oyo", "Fally Ipupa", "Some submitted lyrics text.", "fr", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicSubmitLyricsResponse>();

        body.WentToQueue.Should().BeTrue();
        body.SubmissionId.Should().NotBeNull();
        body.LyricsId.Should().BeNull();

        Client.AuthenticateAsAdmin();
        var listResponse = await Client.GetAsync(Routes.Admin.Lyrics.Submissions());

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listBody = await listResponse.ReadAsAsync<AdminGetLyricsSubmissionsResponse>();

        listBody.Submissions.Items.Should().Contain(s => s.Id == body.SubmissionId);
    }

    [Fact]
    public async Task SubmitLyrics_WithoutClaimedArtistAndBlankArtistName_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Submissions(),
            new PublicSubmitLyricsRequest("Eloko Oyo", "   ", "Some submitted lyrics text.", "fr", null)
        );

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<LyricsErrorMessage>(m => m.ArtistNameRequired())
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        bool anySubmissionCreated = await ctx.LyricsSubmissions.AnyAsync();
        anySubmissionCreated.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitLyrics_WithClaimedArtist_SkipsQueueAndAttributesToOwnedArtistIdentityInDraft()
    {
        Guid userId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            UserEntity user = UserFactory.CreateWithId(userId);
            user.MarkAsVerified();
            user.Activate();
            ctx.Users.Add(user);
        });

        ArtistEntity ownedArtist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            ArtistEntity artist = ArtistFactory.Create("Fally Ipupa", "fally-ipupa-real");
            artist.ClaimOwnership(userId);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Submissions(),
            new PublicSubmitLyricsRequest(
                "Eloko Oyo",
                "Some Impersonator Name",
                "Some submitted lyrics text.",
                "fr",
                "eloko-oyo-fast-path"
            )
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicSubmitLyricsResponse>();

        body.WentToQueue.Should().BeFalse();
        body.SubmissionId.Should().BeNull();
        body.LyricsId.Should().NotBeNull();

        await using ContentDbContext verifyCtx = CreateDbContext<ContentDbContext>();
        bool anySubmissionCreated = await verifyCtx.LyricsSubmissions.AnyAsync();
        anySubmissionCreated.Should().BeFalse();

        LyricsEntity? persistedLyrics = await verifyCtx.Lyrics.FindAsync(body.LyricsId);
        persistedLyrics.Should().NotBeNull();
        persistedLyrics!.ArtistId.Should().Be(ownedArtist.Id);
        persistedLyrics.ArtistName.Should().Be(ownedArtist.Name);
        persistedLyrics.ArtistName.Should().NotBe("Some Impersonator Name");
        persistedLyrics.Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task SubmitLyrics_WithClaimedArtistButNoSlug_ReturnsBadRequest()
    {
        Guid userId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            UserEntity user = UserFactory.CreateWithId(userId);
            user.MarkAsVerified();
            user.Activate();
            ctx.Users.Add(user);
        });

        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            ArtistEntity artist = ArtistFactory.CreateClaimed(userId);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Submissions(),
            new PublicSubmitLyricsRequest("Eloko Oyo", null, "Some submitted lyrics text.", "fr", null)
        );

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<LyricsErrorMessage>(m => m.SlugRequired())
        );
    }

    [Fact]
    public async Task SubmitLyrics_WithClaimedArtistAndAlreadyTakenSlug_ReturnsConflict()
    {
        string takenSlug = $"taken-slug-{Guid.NewGuid():N}";

        Guid userId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            UserEntity user = UserFactory.CreateWithId(userId);
            user.MarkAsVerified();
            user.Activate();
            ctx.Users.Add(user);
        });

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            ArtistEntity artist = ArtistFactory.CreateClaimed(userId);
            LyricsEntity existing = LyricsFactory.CreateWithSlug(category.Id, takenSlug);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(existing);
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Submissions(),
            new PublicSubmitLyricsRequest("Eloko Oyo", null, "Some submitted lyrics text.", "fr", takenSlug)
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsErrorMessage>(m => m.SlugAlreadyExists(takenSlug))
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        int withTakenSlug = await ctx.Lyrics.CountAsync(lyrics => lyrics.Slug == takenSlug);
        withTakenSlug.Should().Be(1);
    }

    [Fact]
    public async Task SubmitLyrics_WithMalformedOptionalSlug_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.LyricsSubmissionsAndRevisions.Submissions(),
            new PublicSubmitLyricsRequest(
                "Eloko Oyo",
                "Fally Ipupa",
                "Some submitted lyrics text.",
                "fr",
                "INVALID SLUG!!!"
            )
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<LyricsErrorMessage>(m => m.SlugInvalidFormat()))
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        bool anySubmissionCreated = await ctx.LyricsSubmissions.AnyAsync();
        anySubmissionCreated.Should().BeFalse();
    }
}
