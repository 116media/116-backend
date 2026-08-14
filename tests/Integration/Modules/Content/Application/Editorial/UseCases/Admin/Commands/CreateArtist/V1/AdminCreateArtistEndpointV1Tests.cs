using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;

/// <summary>
/// Integration tests for the AdminCreateArtist endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateArtistEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateArtist_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa", "fally-ipupa", null, null, null, null, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateArtist_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa", "fally-ipupa", null, null, null, null, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateArtist_AsSuperAdmin_WithValidData_ReturnsCreatedAndPersists()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa", "fally-ipupa", "Congolese singer.", null, null, null, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateArtistResponse body = await response.ReadAsAsync<AdminCreateArtistResponse>();
        body.Artist.Name.Should().Be("Fally Ipupa");
        body.Artist.Slug.Should().Be("fally-ipupa");
        body.Artist.Bio.Should().Be("Congolese singer.");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(body.Artist.Id);
        persisted.Should().NotBeNull();
        persisted!.UserId.Should().BeNull();
    }

    /// <summary>
    /// Creating an artist profile with a slug that already exists returns a conflict rather
    /// than silently creating a duplicate profile.
    /// </summary>
    [Fact]
    public async Task CreateArtist_WithDuplicateSlug_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Fally Ipupa Copy", "fally-ipupa", null, null, null, null, null)
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateArtist_WithEmptyName_ReturnsValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest(string.Empty, "some-slug", null, null, null, null, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Aliases pass the validator on count and length alone; the entity is what folds out
    /// case-insensitive duplicates and blank entries. Asserted on the persisted row.
    /// </summary>
    [Fact]
    public async Task CreateArtist_WithDuplicateAndBlankAliases_PersistsDedupedList()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest(
                "Drake",
                $"slug-{Guid.NewGuid():N}",
                null,
                null,
                ["Drizzy", "drizzy", "  ", "Champagne Papi"],
                null,
                null
            )
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateArtistResponse body = await response.ReadAsAsync<AdminCreateArtistResponse>();
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(body.Artist.Id);
        persisted!.Aliases.Should().Equal("Drizzy", "Champagne Papi");
    }

    #region Identity Field Validation

    /// <summary>
    /// Each invalid identity payload is rejected at the validator with a 400 — through real
    /// HTTP, so the validation extensions and their localized messages actually execute.
    /// </summary>
    [Fact]
    public async Task CreateArtist_WithMoreThanTenAliases_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        List<string> aliases = Enumerable.Range(0, 11).Select(i => $"Alias {i}").ToList();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Name", $"slug-{Guid.NewGuid():N}", null, null, aliases, null, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArtist_WithOverlongAlias_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest(
                "Name",
                $"slug-{Guid.NewGuid():N}",
                null,
                null,
                [new string('a', 101)],
                null,
                null
            )
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArtist_WithFutureBirthdate_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        DateOnly future = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1);

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest("Name", $"slug-{Guid.NewGuid():N}", null, null, null, future, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArtist_WithOverlongRealNameOrHometown_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var longRealName = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest(
                "Name",
                $"slug-{Guid.NewGuid():N}",
                null,
                new string('r', 151),
                null,
                null,
                null
            )
        );
        var longHometown = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequest(
                "Name",
                $"slug-{Guid.NewGuid():N}",
                null,
                null,
                null,
                null,
                new string('h', 121)
            )
        );

        longRealName.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        longHometown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
