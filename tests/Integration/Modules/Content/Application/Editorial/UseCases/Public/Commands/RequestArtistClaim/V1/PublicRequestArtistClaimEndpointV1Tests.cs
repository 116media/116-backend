using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim.V1;

/// <summary>
/// Integration tests for the PublicRequestArtistClaim endpoint.
/// </summary>
[Collection("Database")]
public class PublicRequestArtistClaimEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ArtistEntity> SeedArtistAsync()
    {
        return await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.Create();
            ctx.Artists.Add(artist);
            return artist;
        });
    }

    [Fact]
    public async Task RequestArtistClaim_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(Guid.NewGuid()), new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestArtistClaim_AsAuthenticatedVisitor_WithNonExistentArtist_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(Guid.NewGuid()), new { });

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Artist"))
        );
    }

    [Fact]
    public async Task RequestArtistClaim_AsAuthenticatedVisitor_WithExistingArtist_ReturnsSuccess()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(artist.Id), new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicRequestArtistClaimResponse body = await response.ReadAsAsync<PublicRequestArtistClaimResponse>();
        body.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RequestArtistClaim_SubmittedTwiceByTheSameVisitor_ReturnsConflictAndPersistsOneRequest()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsVisitor();

        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var firstResponse = await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(artist.Id), new { });
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(artist.Id), new { });

        await secondResponse.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArtistErrorMessage>(m => m.ClaimRequestAlreadyExists(), LocalizedMessage.EnglishCulture)
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<ArtistClaimRequestEntity> requests = await ctx
            .ArtistClaimRequests.Where(request => request.ArtistId == artist.Id)
            .ToListAsync();
        requests.Should().ContainSingle().Which.UserId.Should().Be(TestUser.VisitorId);
    }

    [Fact]
    public async Task RequestArtistClaim_AgainstAProfileOwnedByAnotherAccount_ReturnsConflictAndQueuesNothing()
    {
        ArtistEntity claimedArtist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateClaimed(Guid.NewGuid());
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(claimedArtist.Id), new { });

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArtistErrorMessage>(m => m.AlreadyClaimed())
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        bool anyRequestQueued = await ctx.ArtistClaimRequests.AnyAsync(request => request.ArtistId == claimedArtist.Id);
        anyRequestQueued.Should().BeFalse();
    }

    [Fact]
    public async Task RequestArtistClaim_ShouldNotMutateArtistUserId()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsVisitor();

        await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(artist.Id), new { });

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.UserId.Should().BeNull();
        persisted.VerifiedAt.Should().BeNull();
    }
}
