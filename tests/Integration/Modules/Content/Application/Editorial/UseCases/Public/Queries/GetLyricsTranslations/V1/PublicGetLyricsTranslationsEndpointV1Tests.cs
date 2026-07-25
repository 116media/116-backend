using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations.V1;

/// <summary>
/// Integration tests for the PublicGetLyricsTranslations endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetLyricsTranslationsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetLyricsTranslations_AnonymousWithNonExistentLyrics_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Translations(Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Lists every translation of a lyrics page, across every requested language, without
    /// requiring authentication.
    /// </summary>
    [Fact]
    public async Task GetLyricsTranslations_AnonymousWithMultipleLanguages_ReturnsAllTranslations()
    {
        (LyricsEntity lyrics, LyricsTranslationEntity spanish, LyricsTranslationEntity english) = await SeedAsync<
            ContentDbContext,
            (LyricsEntity, LyricsTranslationEntity, LyricsTranslationEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsTranslationEntity spanish = LyricsTranslationFactory.CreateWithText(
                lyrics.Id,
                "es",
                "Texto en espanol"
            );
            LyricsTranslationEntity english = LyricsTranslationFactory.CreateWithText(
                lyrics.Id,
                "en",
                "Text in english"
            );
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsTranslations.AddRange(spanish, english);
            return (lyrics, spanish, english);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Translations(lyrics.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicGetLyricsTranslationsResponse>();

        body.Translations.Should().HaveCount(2);
        body.Translations.Should()
            .Contain(t => t.Id == spanish.Id && t.Language == "es" && t.Text == "Texto en espanol");
        body.Translations.Should()
            .Contain(t => t.Id == english.Id && t.Language == "en" && t.Text == "Text in english");
    }

    /// <summary>
    /// A lyrics page with no translations yet returns an empty list rather than an error.
    /// </summary>
    [Fact]
    public async Task GetLyricsTranslations_NoTranslationsYet_ReturnsEmptyList()
    {
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Lyrics.Translations(lyrics.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicGetLyricsTranslationsResponse>();

        body.Translations.Should().BeEmpty();
    }
}
