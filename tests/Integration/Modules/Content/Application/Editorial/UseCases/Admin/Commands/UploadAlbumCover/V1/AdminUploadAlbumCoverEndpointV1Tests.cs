using System.Net.Http.Headers;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover.V1;

/// <summary>
/// Integration tests for the AdminUploadAlbumCover endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadAlbumCoverEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private async Task<AlbumEntity> SeedAlbumAsync()
    {
        return await SeedAsync<ContentDbContext, AlbumEntity>(ctx =>
        {
            AlbumEntity album = AlbumFactory.Create();
            ctx.Albums.Add(album);
            return album;
        });
    }

    private static MultipartFormDataContent CreateCoverContent()
    {
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formContent.Add(fileContent, "file", "cover.jpg");
        return formContent;
    }

    [Fact]
    public async Task UploadAlbumCover_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(Routes.Admin.Albums.Cover(Guid.NewGuid()), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadAlbumCover_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(Routes.Admin.Albums.Cover(Guid.NewGuid()), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadAlbumCover_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(Routes.Admin.Albums.Cover(Guid.NewGuid()), formContent);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Album"))
        );
    }

    [Fact]
    public async Task UploadAlbumCover_AsAdmin_WithValidFile_ReturnsOkAndPersists()
    {
        AlbumEntity album = await SeedAlbumAsync();
        album.CoverImageFileId.Should().BeNull();

        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(Routes.Admin.Albums.Cover(album.Id), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUploadAlbumCoverResponse body = await response.ReadAsAsync<AdminUploadAlbumCoverResponse>();
        body.CoverImageUrl.Should().StartWith("https://res.cloudinary.com/test-cloud/");
        body.CoverImageStorageKey.Should().NotBeNullOrEmpty();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        AlbumEntity? persisted = await ctx.Albums.FindAsync(album.Id);
        persisted!.CoverImageFileId.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadAlbumCover_WithNoFilePart_ReturnsLocalizedValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent("unused"), "note");

        var response = await Client.PostAsync(Routes.Admin.Albums.Cover(Guid.NewGuid()), formContent);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("File", Localized<LyricsErrorMessage>(m => m.FileRequired()))
        );
    }
}
