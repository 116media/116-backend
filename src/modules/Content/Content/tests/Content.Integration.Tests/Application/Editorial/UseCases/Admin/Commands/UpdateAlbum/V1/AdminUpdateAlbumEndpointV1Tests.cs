using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum.V1;

/// <summary>
/// Integration tests for the AdminUpdateAlbum endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateAlbumEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<AlbumEntity> SeedAlbumAsync()
    {
        return await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            AlbumEntity album = AlbumFactory.Create();
            ctx.Albums.Add(album);
            return album;
        });
    }

    [Fact]
    public async Task UpdateAlbum_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Albums}/{Guid.NewGuid()}",
            new AdminUpdateAlbumRequest("Name", null, null, EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAlbum_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Albums}/{Guid.NewGuid()}",
            new AdminUpdateAlbumRequest("Name", null, null, EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAlbum_AsSuperAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Albums}/{Guid.NewGuid()}",
            new AdminUpdateAlbumRequest("Name", null, null, EnumReleaseType.Album)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Album"))
        );
    }

    [Fact]
    public async Task UpdateAlbum_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Albums}/{album.Id}",
            new AdminUpdateAlbumRequest("Updated Name", 2001, "Updated Label", EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateAlbumResponse body = await response.ReadAsAsync<AdminUpdateAlbumResponse>();
        body.Album.Name.Should().Be("Updated Name");
        body.Album.ReleaseYear.Should().Be(2001);
        body.Album.Label.Should().Be("Updated Label");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        AlbumEntity? persisted = await ctx.Albums.FindAsync(album.Id);
        persisted!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAlbum_ShouldPreserveExistingCoverImageFileId()
    {
        AlbumEntity album = await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            AlbumEntity a = AlbumFactory.CreateWithCoverImageFileId(Guid.NewGuid());
            ctx.Albums.Add(a);
            return a;
        });
        Guid originalCoverImageFileId = album.CoverImageFileId!.Value;

        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Albums}/{album.Id}",
            new AdminUpdateAlbumRequest("Updated Name", null, null, EnumReleaseType.Album)
        );

        await using ContentDbContext ctx2 = CreateDbContext<ContentDbContext>();
        AlbumEntity? persisted = await ctx2.Albums.FindAsync(album.Id);
        persisted!.CoverImageFileId.Should().Be(originalCoverImageFileId);
    }

    [Fact]
    public async Task UpdateAlbum_WithEmptyName_ReturnsValidationProblem()
    {
        AlbumEntity album = await SeedAlbumAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Admin.Albums}/{album.Id}",
            new AdminUpdateAlbumRequest(string.Empty, null, null, EnumReleaseType.Album)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
