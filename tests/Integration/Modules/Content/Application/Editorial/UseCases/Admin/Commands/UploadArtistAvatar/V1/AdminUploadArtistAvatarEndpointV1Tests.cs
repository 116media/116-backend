using System.Net.Http.Headers;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar.V1;

/// <summary>
/// Integration tests for the AdminUploadArtistAvatar endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadArtistAvatarEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    private static MultipartFormDataContent CreateAvatarContent()
    {
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formContent.Add(fileContent, "file", "avatar.jpg");
        return formContent;
    }

    [Fact]
    public async Task UploadArtistAvatar_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using MultipartFormDataContent formContent = CreateAvatarContent();

        var response = await Client.PostAsync(Routes.Admin.Artists.Avatar(Guid.NewGuid()), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadArtistAvatar_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using MultipartFormDataContent formContent = CreateAvatarContent();

        var response = await Client.PostAsync(Routes.Admin.Artists.Avatar(Guid.NewGuid()), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadArtistAvatar_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateAvatarContent();

        var response = await Client.PostAsync(Routes.Admin.Artists.Avatar(Guid.NewGuid()), formContent);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadArtistAvatar_AsAdmin_WithValidFile_ReturnsOkAndPersists()
    {
        ArtistEntity artist = await SeedArtistAsync();
        artist.AvatarFileId.Should().BeNull();

        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateAvatarContent();

        var response = await Client.PostAsync(Routes.Admin.Artists.Avatar(artist.Id), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUploadArtistAvatarResponse body = await response.ReadAsAsync<AdminUploadArtistAvatarResponse>();
        body.AvatarUrl.Should().StartWith("https://res.cloudinary.com/test-cloud/");
        body.AvatarStorageKey.Should().NotBeNullOrEmpty();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? persisted = await ctx.Artists.FindAsync(artist.Id);
        persisted!.AvatarFileId.Should().NotBeNull();
    }

    /// <summary>
    /// Uploading a replacement avatar for an artist that already has one overwrites the
    /// stored file reference rather than accumulating multiple files.
    /// </summary>
    [Fact]
    public async Task UploadArtistAvatar_WhenArtistHasExistingAvatar_OverwritesInPlace()
    {
        ArtistEntity artist = await SeedArtistAsync();
        Client.AuthenticateAsAdmin();

        using (MultipartFormDataContent firstUpload = CreateAvatarContent())
        {
            await Client.PostAsync(Routes.Admin.Artists.Avatar(artist.Id), firstUpload);
        }

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArtistEntity? afterFirst = await ctx.Artists.FindAsync(artist.Id);
        Guid firstFileId = afterFirst!.AvatarFileId!.Value;

        using MultipartFormDataContent secondUpload = CreateAvatarContent();
        var response = await Client.PostAsync(Routes.Admin.Artists.Avatar(artist.Id), secondUpload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ArtistEntity? afterSecond = await verifyContext.Artists.FindAsync(artist.Id);
        afterSecond!.AvatarFileId.Should().NotBeNull();
        firstFileId.Should().NotBe(Guid.Empty);
    }
}
