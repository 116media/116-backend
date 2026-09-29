using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;

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
            new AdminCreateArtistRequestBuilder().WithName("Fally Ipupa").WithSlug("fally-ipupa").Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateArtist_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder().WithName("Fally Ipupa").WithSlug("fally-ipupa").Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateArtist_AsSuperAdmin_WithValidData_ReturnsCreatedAndPersists()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder()
                .WithName("Fally Ipupa")
                .WithSlug("fally-ipupa")
                .WithBio("Congolese singer.")
                .Build()
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
            new AdminCreateArtistRequestBuilder().WithName("Fally Ipupa Copy").WithSlug("fally-ipupa").Build()
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArtistErrorMessage>(m => m.SlugAlreadyExists("fally-ipupa"))
        );
    }

    [Fact]
    public async Task CreateArtist_WithEmptyName_ReturnsValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder().WithName(string.Empty).WithSlug("some-slug").Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArtist_WithDuplicateAndBlankAliases_PersistsDedupedList()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder()
                .WithName("Drake")
                .WithAliases(["Drizzy", "drizzy", " ", "Champagne Papi"])
                .Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateArtistResponse body = await response.ReadAsAsync<AdminCreateArtistResponse>();
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(body.Artist.Id);
        persisted!.Aliases.Should().Equal("Drizzy", "Champagne Papi");
    }

    #region Identity Field Validation

    [Fact]
    public async Task CreateArtist_WithMoreThanTenAliases_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        List<string> aliases = Enumerable.Range(0, 11).Select(i => $"Alias {i}").ToList();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder().WithAliases(aliases).Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArtist_WithOverlongAlias_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder().WithAliases([new string('a', 101)]).Build()
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
            new AdminCreateArtistRequestBuilder().WithBirthdate(future).Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArtist_WithOverlongRealNameOrHometown_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var longRealName = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder().WithRealName(new string('r', 151)).Build()
        );
        var longHometown = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Artists,
            new AdminCreateArtistRequestBuilder().WithHometown(new string('h', 121)).Build()
        );

        longRealName.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        longHometown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
