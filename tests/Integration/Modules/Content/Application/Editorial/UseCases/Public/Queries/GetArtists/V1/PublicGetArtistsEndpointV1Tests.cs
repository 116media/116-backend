using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtists.V1;

/// <summary>
/// Integration tests for the PublicGetArtists directory endpoint. Every test seeds through
/// the real DbContext and reads through real HTTP, so the content predicate, the folded
/// ordering and the per-card count are all exercised as one SQL statement.
/// </summary>
[Collection("Database")]
public class PublicGetArtistsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    /// <summary>
    /// Resolves FluentValidation's own built-in message for a rule that carries no
    /// <c>WithMessage</c> override, through the same static language manager the in-process
    /// test host resolved it with, under the culture a header-less request selects.
    /// </summary>
    /// <param name="validatorName">The FluentValidation validator name keying the message.</param>
    /// <param name="arguments">The placeholder values to substitute into the template.</param>
    /// <returns>The built-in message the endpoint produced.</returns>
    private static string BuiltInRuleMessage(
        string validatorName,
        params (string Placeholder, string Value)[] arguments
    )
    {
        using var cultureScope = new CultureScope(LocalizedMessage.DefaultCulture);

        string template = ValidatorOptions.Global.LanguageManager.GetString(validatorName);

        return arguments.Aggregate(
            template,
            (message, argument) =>
                message.Replace($"{{{argument.Placeholder}}}", argument.Value, StringComparison.Ordinal)
        );
    }

    /// <summary>
    /// Seeds an artist with one published lyrics page so it clears the content predicate.
    /// </summary>
    private async Task<ArtistEntity> SeedListedArtistAsync(string name)
    {
        return await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.Create(name, $"slug-{Guid.NewGuid():N}");
            LyricsEntity lyrics = LyricsFactory.CreatePublishedForArtist(category.Id, artist.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(lyrics);

            return artist;
        });
    }

    [Fact]
    public async Task GetArtists_WithNoContent_ExcludesTheArtistEntirely()
    {
        // An artist row with a bio but nothing published is a stub, not a destination.
        ArtistEntity stub = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.Create($"Stub {Guid.NewGuid():N}", $"stub-{Guid.NewGuid():N}");
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?pageSize=100"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistsResponse body = await response.ReadAsAsync<PublicGetArtistsResponse>();
        body.Artists.Items.Should().NotContain(a => a.Slug == stub.Slug);
    }

    [Fact]
    public async Task GetArtists_WithPublishedContent_ListsTheArtistWithItsCount()
    {
        ArtistEntity artist = await SeedListedArtistAsync($"Listed {Guid.NewGuid():N}");

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?pageSize=100"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistsResponse body = await response.ReadAsAsync<PublicGetArtistsResponse>();
        var card = body.Artists.Items.SingleOrDefault(a => a.Slug == artist.Slug);
        card.Should().NotBeNull();
        card!.ContentCount.Should().Be(1);
        card.Name.Should().Be(artist.Name);
    }

    [Fact]
    public async Task GetArtists_ResponseBody_CarriesNoArtistIds()
    {
        await SeedListedArtistAsync($"NoIds {Guid.NewGuid():N}");

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?pageSize=100"));

        string raw = await response.Content.ReadAsStringAsync();
        raw.Should().NotContainEquivalentOf("\"id\":");
        raw.Should().NotContainEquivalentOf("\"userId\":");
    }

    [Fact]
    public async Task GetArtists_WithAccentedName_SortsBucketsAndSearchesAccentInsensitively()
    {
        string unique = Guid.NewGuid().ToString("N")[..8];
        ArtistEntity accented = await SeedListedArtistAsync($"Élodie {unique}");

        Client.ClearAuthentication();

        // Buckets under E, not under a separate accented bucket.
        var letterResponse = await Client.GetAsync(Routes.Public.Artists.Directory("?letter=E&pageSize=100"));
        PublicGetArtistsResponse letterBody = await letterResponse.ReadAsAsync<PublicGetArtistsResponse>();
        letterBody.Artists.Items.Should().Contain(a => a.Slug == accented.Slug);
        letterBody.AvailableLetters.Should().Contain("E");

        // Matches an unaccented search.
        var searchResponse = await Client.GetAsync(
            Routes.Public.Artists.Directory($"?search=elodie {unique}&pageSize=100")
        );
        PublicGetArtistsResponse searchBody = await searchResponse.ReadAsAsync<PublicGetArtistsResponse>();
        searchBody.Artists.Items.Should().Contain(a => a.Slug == accented.Slug);
    }

    [Fact]
    public async Task GetArtists_SearchMatchesNameButNeverBio()
    {
        string marker = Guid.NewGuid().ToString("N")[..10];

        // An artist whose BIO carries the marker but whose name does not.
        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.Create(
                $"BioOnly {Guid.NewGuid():N}",
                $"slug-{Guid.NewGuid():N}",
                $"Biography mentioning bio-{marker}"
            );
            LyricsEntity lyrics = LyricsFactory.CreatePublishedForArtist(category.Id, artist.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Lyrics.Add(lyrics);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory($"?search={marker}&pageSize=100"));

        PublicGetArtistsResponse body = await response.ReadAsAsync<PublicGetArtistsResponse>();
        body.Artists.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("a")]
    [InlineData("1")]
    public async Task GetArtists_WithInvalidLetter_ReturnsBadRequest(string letter)
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory($"?letter={letter}"));

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Letter", BuiltInRuleMessage("PredicateValidator", ("PropertyName", "Letter")))
        );
    }

    [Fact]
    public async Task GetArtists_WithSingleCharacterSearch_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?search=f"));

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Search",
                BuiltInRuleMessage(
                    "MinimumLengthValidator",
                    ("PropertyName", "Search"),
                    ("MinLength", ContentConstants.MinArtistSearchLength.ToString()),
                    ("TotalLength", "1")
                )
            )
        );
    }

    [Fact]
    public async Task GetArtists_WithLetterAndSearchTogether_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Directory("?letter=F&search=fally"));

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(string.Empty, Localized<ArtistErrorMessage>(m => m.LetterAndSearchExclusive()))
        );
    }
}
