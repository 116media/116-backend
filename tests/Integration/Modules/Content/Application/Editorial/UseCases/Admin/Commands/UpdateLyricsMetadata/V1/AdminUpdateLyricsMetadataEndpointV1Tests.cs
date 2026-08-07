using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;

/// <summary>
/// Integration tests for the AdminUpdateLyricsMetadata endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateLyricsMetadataEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Rebuilds the envelope FluentValidation puts in the ProblemDetails detail. The rules on
    /// this command are independent, so a request that breaks several of them produces one
    /// failure per property, in the order the validator declares them.
    /// </summary>
    /// <param name="failures">The expected property/message pairs, in validator order.</param>
    /// <returns>The expected detail.</returns>
    private static string ValidationDetail(params (string Property, string Message)[] failures) =>
        new ValidationException(
            failures.Select(failure => new ValidationFailure(failure.Property, failure.Message))
        ).Message;

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
            new AdminUpdateLyricsMetadataRequest(null, null, null, null, null)
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

        var request = new AdminUpdateLyricsMetadataRequest(
            Album: "Testament",
            ReleaseYear: 1995,
            Label: "Sonodisc",
            Songwriter: "Papa Wemba",
            Producer: "Viviane Arnoux"
        );

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
            new AdminUpdateLyricsMetadataRequest("Album", 1990, "Label", "Songwriter", "Producer")
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminUpdateLyricsMetadataRequest(null, 1990, null, "Songwriter", null)
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
            new AdminUpdateLyricsMetadataRequest(null, 1899, null, null, null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLyricsMetadata_WithOverLongCredits_ReturnsValidationProblem()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminUpdateLyricsMetadataRequest(
            Album: new string('a', ContentConstants.MaxAlbumNameLength + 1),
            ReleaseYear: null,
            Label: new string('l', ContentConstants.MaxLabelNameLength + 1),
            Songwriter: new string('s', ContentConstants.MaxCreditNameLength + 1),
            Producer: new string('p', ContentConstants.MaxCreditNameLength + 1)
        );

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Metadata(EditorialRouteConstants.Lyrics, lyrics.Id),
            request
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                ("Album", Localized<LyricsErrorMessage>(m => m.AlbumTooLong(ContentConstants.MaxAlbumNameLength))),
                ("Label", Localized<LyricsErrorMessage>(m => m.LabelTooLong(ContentConstants.MaxLabelNameLength))),
                (
                    "Songwriter",
                    Localized<LyricsErrorMessage>(m => m.SongwriterTooLong(ContentConstants.MaxCreditNameLength))
                ),
                (
                    "Producer",
                    Localized<LyricsErrorMessage>(m => m.ProducerTooLong(ContentConstants.MaxCreditNameLength))
                )
            )
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.Album.Should().BeNull();
        persisted.Label.Should().BeNull();
        persisted.Songwriter.Should().BeNull();
        persisted.Producer.Should().BeNull();
    }
}
