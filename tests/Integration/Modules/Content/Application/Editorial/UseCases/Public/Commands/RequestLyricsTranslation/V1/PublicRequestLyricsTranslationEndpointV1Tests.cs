using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation.V1;

/// <summary>
/// Integration tests for the PublicRequestLyricsTranslation endpoint.
/// </summary>
[Collection("Database")]
public class PublicRequestLyricsTranslationEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RequestLyricsTranslation_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Lyrics.Translations(Guid.NewGuid()),
            new PublicRequestLyricsTranslationRequest("es")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLyricsTranslation_AsVisitor_WithNonExistentLyrics_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Lyrics.Translations(Guid.NewGuid()),
            new PublicRequestLyricsTranslationRequest("es")
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task RequestLyricsTranslation_FirstRequestForLanguage_CreatesAiSourcedTranslation()
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

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Lyrics.Translations(lyrics.Id),
            new PublicRequestLyricsTranslationRequest("es")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicRequestLyricsTranslationResponse>();
        body.Text.Should().Be(lyrics.LyricsText);
        body.Source.Should().Be(nameof(EnumTranslationSource.Ai));

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<LyricsTranslationEntity> persisted = await ctx
            .LyricsTranslations.Where(t => t.LyricsId == lyrics.Id && t.Language == "es")
            .ToListAsync();

        persisted.Should().ContainSingle();
        persisted[0].Source.Should().Be(EnumTranslationSource.Ai);
    }

    [Fact]
    public async Task RequestLyricsTranslation_SecondRequestForSameLanguage_DoesNotCreateSecondRow()
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

        Client.AuthenticateAsVisitor();

        await Client.PostAsJsonAsync(
            Routes.Public.Lyrics.Translations(lyrics.Id),
            new PublicRequestLyricsTranslationRequest("es")
        );

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Lyrics.Translations(lyrics.Id),
            new PublicRequestLyricsTranslationRequest("es")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        int persistedCount = await ctx
            .LyricsTranslations.Where(t => t.LyricsId == lyrics.Id && t.Language == "es")
            .CountAsync();

        persistedCount.Should().Be(1);
    }

    [Fact]
    public async Task RequestLyricsTranslation_DifferentLanguageForSameLyrics_CreatesSecondIndependentRow()
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

        Client.AuthenticateAsVisitor();

        await Client.PostAsJsonAsync(
            Routes.Public.Lyrics.Translations(lyrics.Id),
            new PublicRequestLyricsTranslationRequest("es")
        );

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Lyrics.Translations(lyrics.Id),
            new PublicRequestLyricsTranslationRequest("en")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        int persistedCount = await ctx.LyricsTranslations.Where(t => t.LyricsId == lyrics.Id).CountAsync();

        persistedCount.Should().Be(2);
    }
}
