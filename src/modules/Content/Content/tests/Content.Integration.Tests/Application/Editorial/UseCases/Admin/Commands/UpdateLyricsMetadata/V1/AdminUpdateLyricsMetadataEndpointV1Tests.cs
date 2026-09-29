using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;

/// <summary>
/// Integration tests for the AdminUpdateLyricsMetadata endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateLyricsMetadataEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<LyricsEntity> SeedLyricsAsync()
    {
        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    [Fact]
    public async Task UpdateLyricsMetadata_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateLyricsMetadata_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateLyricsMetadata_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminUpdateLyricsMetadataRequestBuilder().Build()
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task UpdateLyricsMetadata_AsSuperAdmin_WithAllFields_PersistsMetadata()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminUpdateLyricsMetadataRequestBuilder()
            .WithAlbum("Testament")
            .WithReleaseYear(1995)
            .WithLabel("Sonodisc")
            .WithSongwriter("Papa Wemba")
            .WithProducer("Viviane Arnoux")
            .Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateLyricsMetadataResponse>();
        body.Lyrics.Album.Should().Be("Testament");
        body.Lyrics.ReleaseYear.Should().Be(1995);
        body.Lyrics.Label.Should().Be("Sonodisc");
        body.Lyrics.Songwriter.Should().Be("Papa Wemba");
        body.Lyrics.Producer.Should().Be("Viviane Arnoux");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.Album.Should().Be("Testament");
        persisted.ReleaseYear.Should().Be(1995);
        persisted.Label.Should().Be("Sonodisc");
        persisted.Songwriter.Should().Be("Papa Wemba");
        persisted.Producer.Should().Be("Viviane Arnoux");
    }

    [Fact]
    public async Task UpdateLyricsMetadata_WithSomeFieldsNulled_ClearsOnlyThoseFields()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminUpdateLyricsMetadataRequestBuilder()
                .WithAlbum("Album")
                .WithReleaseYear(1990)
                .WithLabel("Label")
                .WithSongwriter("Songwriter")
                .WithProducer("Producer")
                .Build()
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminUpdateLyricsMetadataRequestBuilder().WithReleaseYear(1990).WithSongwriter("Songwriter").Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.Album.Should().BeNull();
        persisted.ReleaseYear.Should().Be(1990);
        persisted.Label.Should().BeNull();
        persisted.Songwriter.Should().Be("Songwriter");
        persisted.Producer.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLyricsMetadata_WithReleaseYearOutOfBounds_ReturnsValidationProblem()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminUpdateLyricsMetadataRequestBuilder().WithReleaseYear(1899).Build()
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLyricsMetadata_WithOverLongCredits_ReturnsValidationProblem()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminUpdateLyricsMetadataRequestBuilder()
            .WithAlbum(new string('a', ContentConstants.MaxAlbumNameLength + 1))
            .WithLabel(new string('l', ContentConstants.MaxLabelNameLength + 1))
            .WithSongwriter(new string('s', ContentConstants.MaxCreditNameLength + 1))
            .WithProducer(new string('p', ContentConstants.MaxCreditNameLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            request
        );

        await response.ShouldBeValidationProblem(
            ("Album", Localized<LyricsErrorMessage>(m => m.AlbumTooLong(ContentConstants.MaxAlbumNameLength))),
            ("Label", Localized<LyricsErrorMessage>(m => m.LabelTooLong(ContentConstants.MaxLabelNameLength))),
            (
                "Songwriter",
                Localized<LyricsErrorMessage>(m => m.SongwriterTooLong(ContentConstants.MaxCreditNameLength))
            ),
            ("Producer", Localized<LyricsErrorMessage>(m => m.ProducerTooLong(ContentConstants.MaxCreditNameLength)))
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.Album.Should().BeNull();
        persisted.Label.Should().BeNull();
        persisted.Songwriter.Should().BeNull();
        persisted.Producer.Should().BeNull();
    }
}
