using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink.V1;

/// <summary>
/// Integration tests for the AdminUpsertArtistSocialLink endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpsertArtistSocialLinkEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string InstagramUrl = "https://instagram.com/fallyipupa01";
    private const string ReplacementUrl = "https://instagram.com/fallyipupa-official";

    private static string Url(Guid artistId, EnumSocialPlatform platform) =>
        Routes.Admin.Artists.SocialLink(artistId, platform.ToString());

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
    public async Task UpsertArtistSocialLink_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(InstagramUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpsertArtistSocialLink_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(InstagramUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpsertArtistSocialLink_WithNonExistentArtist_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid(), EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(InstagramUrl)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpsertArtistSocialLink_WhenSlotEmpty_CreatesLinkAndPersists()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(artist.Id, EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(InstagramUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpsertArtistSocialLinkResponse body = await response.ReadAsAsync<AdminUpsertArtistSocialLinkResponse>();
        body.SocialLinkId.Should().NotBeEmpty();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistSocialLinkEntity? persisted = await ctx.ArtistSocialLinks.FirstOrDefaultAsync(link =>
            link.Id == body.SocialLinkId
        );

        persisted.Should().NotBeNull();
        persisted!.ArtistId.Should().Be(artist.Id);
        persisted.Platform.Should().Be(EnumSocialPlatform.Instagram);
        persisted.Url.Should().Be(InstagramUrl);
    }

    /// <summary>
    /// Two upserts on one platform slot must leave exactly one row carrying the second URL —
    /// the upsert verb exists precisely so this is never a 409.
    /// </summary>
    [Fact]
    public async Task UpsertArtistSocialLink_Twice_LeavesOneRowWithTheSecondUrl()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Url(artist.Id, EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(InstagramUrl)
        );
        var response = await Client.PutAsJsonAsync(
            Url(artist.Id, EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(ReplacementUrl)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<ArtistSocialLinkEntity> rows = await ctx
            .ArtistSocialLinks.Where(link => link.ArtistId == artist.Id)
            .ToListAsync();

        rows.Should().HaveCount(1);
        rows[0].Url.Should().Be(ReplacementUrl);
    }

    [Fact]
    public async Task UpsertArtistSocialLink_OnTwoPlatforms_KeepsTwoRows()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(
            Url(artist.Id, EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(InstagramUrl)
        );
        await Client.PutAsJsonAsync(
            Url(artist.Id, EnumSocialPlatform.YouTube),
            new AdminUpsertArtistSocialLinkRequest("https://youtube.com/@fally")
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<ArtistSocialLinkEntity> rows = await ctx
            .ArtistSocialLinks.Where(link => link.ArtistId == artist.Id)
            .OrderBy(link => link.Platform)
            .ToListAsync();

        rows.Should().HaveCount(2);
        rows.Select(r => r.Platform).Should().Equal(EnumSocialPlatform.Instagram, EnumSocialPlatform.YouTube);
    }

    /// <summary>
    /// The URL becomes an href on the public page, so a non-https scheme is rejected on
    /// write — a javascript: value here would be a stored XSS vector.
    /// </summary>
    [Theory]
    [InlineData("http://instagram.com/someone")]
    [InlineData("javascript:alert(1)")]
    [InlineData("not-a-url")]
    public async Task UpsertArtistSocialLink_WithNonHttpsUrl_ReturnsBadRequest(string url)
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(artist.Id, EnumSocialPlatform.Instagram),
            new AdminUpsertArtistSocialLinkRequest(url)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
