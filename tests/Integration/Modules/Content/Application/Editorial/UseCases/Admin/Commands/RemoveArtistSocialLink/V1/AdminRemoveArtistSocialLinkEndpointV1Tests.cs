using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RemoveArtistSocialLink.V1;

/// <summary>
/// Integration tests for the AdminRemoveArtistSocialLink endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveArtistSocialLinkEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string Url(Guid artistId, EnumSocialPlatform platform) =>
        Routes.Admin.Artists.SocialLink(artistId, platform.ToString());

    private async Task<(ArtistEntity Artist, ArtistSocialLinkEntity Link)> SeedArtistWithLinkAsync()
    {
        return await SeedAsync<ContentDbContext, (ArtistEntity, ArtistSocialLinkEntity)>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.Create();
            ArtistSocialLinkEntity link = ArtistSocialLinkEntity.Create(
                Guid.NewGuid(),
                artist.Id,
                EnumSocialPlatform.TikTok,
                "https://tiktok.com/@someone"
            );

            ctx.Artists.Add(artist);
            ctx.ArtistSocialLinks.Add(link);

            return (artist, link);
        });
    }

    [Fact]
    public async Task RemoveArtistSocialLink_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Url(Guid.NewGuid(), EnumSocialPlatform.TikTok));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveArtistSocialLink_WhenLinkExists_RemovesIt()
    {
        (ArtistEntity artist, ArtistSocialLinkEntity link) = await SeedArtistWithLinkAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Url(artist.Id, EnumSocialPlatform.TikTok));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        (await ctx.ArtistSocialLinks.AnyAsync(l => l.Id == link.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task RemoveArtistSocialLink_Twice_SecondDeleteReturnsNotFound()
    {
        (ArtistEntity artist, _) = await SeedArtistWithLinkAsync();
        Client.AuthenticateAsAdmin();

        await Client.DeleteAsync(Url(artist.Id, EnumSocialPlatform.TikTok));
        var response = await Client.DeleteAsync(Url(artist.Id, EnumSocialPlatform.TikTok));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ArtistSocialLink"))
        );
    }

    [Fact]
    public async Task DeletingArtist_CascadesSocialLinksAway()
    {
        (ArtistEntity artist, ArtistSocialLinkEntity link) = await SeedArtistWithLinkAsync();

        await using (ContentDbContext ctx = CreateDbContext<ContentDbContext>())
        {
            ArtistEntity tracked = await ctx.Artists.SingleAsync(a => a.Id == artist.Id);
            ctx.Artists.Remove(tracked);
            await ctx.SaveChangesAsync();
        }

        await using ContentDbContext verify = CreateDbContext<ContentDbContext>();
        (await verify.ArtistSocialLinks.AnyAsync(l => l.Id == link.Id)).Should().BeFalse();
    }
}
