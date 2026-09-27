using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Queries.GetAllArtists.V1;

/// <summary>
/// Integration tests for the AdminGetAllArtists endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllArtistsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllArtists_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Artists);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllArtists_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Artists);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllArtists_AsAdmin_ReturnsPaginatedArtists()
    {
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Artists.AddRange(ArtistFactory.CreateMany(3));
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Artists);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllArtistsResponse body = await response.ReadAsAsync<AdminGetAllArtistsResponse>();
        body.Artists.Items.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllArtists_WithSearchQuery_ReturnsOnlyMatchingArtists()
    {
        string uniqueName = $"UniqueArtist{Guid.NewGuid():N}"[..20];

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Artists.Add(ArtistFactory.Create(uniqueName, $"slug-{Guid.NewGuid():N}"));
            ctx.Artists.Add(ArtistFactory.Create());
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Artists}?search={uniqueName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllArtistsResponse body = await response.ReadAsAsync<AdminGetAllArtistsResponse>();
        body.Artists.Items.Should().ContainSingle(a => a.Name == uniqueName);
    }
}
