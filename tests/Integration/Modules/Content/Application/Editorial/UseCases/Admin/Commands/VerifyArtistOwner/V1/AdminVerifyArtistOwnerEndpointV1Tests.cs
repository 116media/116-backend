using _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.V1;

/// <summary>
/// Integration tests for the AdminVerifyArtistOwner endpoint.
/// </summary>
[Collection("Database")]
public class AdminVerifyArtistOwnerEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task VerifyArtistOwner_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Artists.VerifyOwner(Guid.NewGuid()),
            new AdminVerifyArtistOwnerRequest(Guid.NewGuid())
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VerifyArtistOwner_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Artists.VerifyOwner(Guid.NewGuid()),
            new AdminVerifyArtistOwnerRequest(Guid.NewGuid())
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task VerifyArtistOwner_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Artists.VerifyOwner(Guid.NewGuid()),
            new AdminVerifyArtistOwnerRequest(Guid.NewGuid())
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Artist"))
        );
    }

    [Fact]
    public async Task VerifyArtistOwner_AsAdmin_WithUnclaimedArtist_ClaimsAndPersists()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Guid userId = Guid.NewGuid();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Artists.VerifyOwner(artist.Id),
            new AdminVerifyArtistOwnerRequest(userId)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.UserId.Should().Be(userId);
        persisted.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyArtistOwner_WhenAlreadyClaimed_ReturnsConflict()
    {
        Guid originalOwnerId = Guid.NewGuid();
        ArtistEntity artist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity a = ArtistFactory.CreateClaimed(originalOwnerId);
            ctx.Artists.Add(a);
            return a;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Artists.VerifyOwner(artist.Id),
            new AdminVerifyArtistOwnerRequest(Guid.NewGuid())
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArtistErrorMessage>(m => m.AlreadyClaimed())
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.UserId.Should().Be(originalOwnerId);
    }
}
