using System.Text.Json;
using _116.Content.Application.Editorial.UseCases.Public.Queries.Search.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Editorial.UseCases.Public.Queries.Search.V1;

/// <summary>
/// Integration tests for the PublicSearch endpoint, covering anonymous access, scope
/// narrowing across articles, videos and shorts, per-scope counts, term validation and
/// the no-store cache directive.
/// </summary>
[Collection("Database")]
public class PublicSearchEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string Term = "bolingo";

    /// <summary>
    /// A term carried by one article alone, whose teaser is rich text longer than an excerpt,
    /// so the markup stripping and the word-boundary cut are both exercised.
    /// </summary>
    private const string RichTextTerm = "sebene";

    /// <summary>
    /// A term carried by the one article whose teaser is markup and nothing else.
    /// </summary>
    private const string EmptyTeaserTerm = "ndombolo";

    /// <summary>
    /// A teaser as the editor authors it: block tags, an entity, and more prose than an
    /// excerpt holds.
    /// </summary>
    private const string RichTextHeadline =
        "<p>Le <strong>sebene</strong> &amp; la rumba</p><p>Une plongee dans les ateliers de "
        + "Kinshasa ou les guitaristes repetent la nuit, entre les sessions de studio et les "
        + "concerts, pour tenir la ligne melodique qui porte toute la chanson jusqu'au bout.</p>";

    private CategoryEntity _category = null!;

    /// <inheritdoc />
    protected override async Task SeedAsync()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        _category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            CategoryEntity entity = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(entity);
            return entity;
        });

        await SeedAsync<ContentDbContext>(ctx =>
            ctx.Articles.AddRange(
                new ArticleBuilder(_category.Id)
                    .WithTitle("Bolingo, la chronique")
                    .WithSlug("bolingo-chronique")
                    .AsPublished()
                    .Build(),
                new ArticleBuilder(_category.Id).WithTitle("Bolingo en coulisses").WithSlug("bolingo-coulisses").Build()
            )
        );

        await SeedAsync<ContentDbContext>(ctx =>
            ctx.Articles.Add(
                new ArticleBuilder(_category.Id)
                    .WithTitle("Sebene, la chronique")
                    .WithSlug("sebene-chronique")
                    .WithBody(RichTextHeadline, "<p>Corps de l'article.</p>")
                    .AsPublished()
                    .Build()
            )
        );

        await SeedAsync<ContentDbContext>(ctx =>
            ctx.Articles.Add(
                new ArticleBuilder(_category.Id)
                    .WithTitle("Ndombolo, la chronique")
                    .WithSlug("ndombolo-chronique")
                    .WithBody("<p></p><br/>", "<p>Corps de l'article.</p>")
                    .AsPublished()
                    .Build()
            )
        );

        await SeedAsync<ContentDbContext>(ctx =>
            ctx.Videos.Add(
                new VideoBuilder(_category.Id).WithTitle("Bolingo live").WithSlug("bolingo-live").AsPublished().Build()
            )
        );

        await SeedAsync<ContentDbContext>(ctx =>
            ctx.ShortVideos.AddRange(
                new ShortVideoBuilder().WithTitle("Bolingo teaser").WithSlug("bolingo-teaser").Build(),
                new ShortVideoBuilder().WithTitle("Bolingo retire").WithSlug("bolingo-retire").AsInactive().Build()
            )
        );

        await RebuildSearchAsync();
    }

    /// <summary>
    /// Projects the seeded rows into the search documents. Seeding writes straight to the
    /// context, so the domain events that normally drive the projection never fire.
    /// </summary>
    private async Task RebuildSearchAsync()
    {
        using IServiceScope scope = Api.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISearchProjection>().RebuildAllAsync();
    }

    [Fact]
    public async Task Search_AsAnonymous_ReturnsThePublishedHitsFromBothScopes()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Query.Should().Be(Term);
        body.Scope.Should().Be(EnumSearchScope.All);
        body.Results.Items.Should()
            .HaveCount(3)
            .And.Contain(hit => hit.Slug == "bolingo-chronique")
            .And.Contain(hit => hit.Slug == "bolingo-live")
            .And.Contain(hit => hit.Slug == "bolingo-teaser");
        body.Results.Items.Should()
            .NotContain(hit => hit.Slug == "bolingo-coulisses")
            .And.NotContain(hit => hit.Slug == "bolingo-retire");
    }

    [Fact]
    public async Task Search_AsAnonymous_ReportsTheCountsForEveryScope()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Counts["articles"].Should().Be(1);
        body.Counts["videos"].Should().Be(1);
        body.Counts["shorts"].Should().Be(1);
        body.Counts["all"].Should().Be(3);
        body.Results.Count.Should().Be(3);
    }

    [Fact]
    public async Task Search_WithTheArticlesScope_ReturnsOnlyArticleHits()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&scope={EnumSearchScope.Articles}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Scope.Should().Be(EnumSearchScope.Articles);
        body.Results.Items.Should().OnlyContain(hit => hit.Type == EnumCoreContentType.Article);
        body.Results.Count.Should().Be(1);
    }

    [Fact]
    public async Task Search_WithTheVideosScope_ReturnsOnlyVideoHits()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&scope={EnumSearchScope.Videos}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Scope.Should().Be(EnumSearchScope.Videos);
        body.Results.Items.Should().OnlyContain(hit => hit.Type == EnumCoreContentType.Video);
        body.Results.Count.Should().Be(1);
    }

    [Fact]
    public async Task Search_WithTheShortsScope_ReturnsOnlyActiveShortHits()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&scope={EnumSearchScope.Shorts}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Scope.Should().Be(EnumSearchScope.Shorts);
        body.Results.Items.Should().OnlyContain(hit => hit.Type == EnumCoreContentType.Short);
        body.Results.Items.Should().ContainSingle(hit => hit.Slug == "bolingo-teaser");
        body.Results.Count.Should().Be(1);
    }

    [Fact]
    public async Task Search_ForAShortHit_ReportsNoCategory()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&scope={EnumSearchScope.Shorts}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Results.Items.Should().OnlyContain(hit => hit.CategoryId == null && hit.CategoryName == null);
    }

    [Fact]
    public async Task Search_ForAHit_ResolvesTheCategoryName()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Results.Items.Should()
            .OnlyContain(hit => hit.Type == EnumCoreContentType.Short || hit.CategoryName == _category.Name);
    }

    [Fact]
    public async Task Search_WithAPageSizeOfOne_PagesThroughTheHits()
    {
        Client.ClearAuthentication();

        var firstPage = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&pageIndex=0&pageSize=1");
        var secondPage = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&pageIndex=1&pageSize=1");

        firstPage.StatusCode.Should().Be(HttpStatusCode.OK);
        secondPage.StatusCode.Should().Be(HttpStatusCode.OK);

        var first = await firstPage.ReadAsAsync<PublicSearchResponse>();
        var second = await secondPage.ReadAsAsync<PublicSearchResponse>();

        first.Results.Items.Should().ContainSingle();
        second.Results.Items.Should().ContainSingle();
        second.Results.Items.Single().Id.Should().NotBe(first.Results.Items.Single().Id);
    }

    [Fact]
    public async Task Search_WithATermThatMatchesNothing_ReturnsNoHitsAndZeroCounts()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q=zzzznotaterm");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Results.Items.Should().BeEmpty();
        body.Counts["all"].Should().Be(0);
    }

    [Fact]
    public async Task Search_WithASingleCharacterTerm_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q=a");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithoutATerm_ReturnsBadRequest()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Search);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(30_000_000)]
    [InlineData(100_000_000)]
    [InlineData(int.MaxValue)]
    public async Task Search_WithAnOverflowingPageIndex_ReturnsAnEmptyPageRatherThanFailing(int pageIndex)
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&pageIndex={pageIndex}&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        body.Results.Items.Should().BeEmpty();
        body.Counts["all"].Should().Be(3, "the counts still report the real totals");
    }

    [Fact]
    public async Task Search_ForTheSameTermTwice_ReportsTheSameCounts()
    {
        Client.ClearAuthentication();

        var first = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}");
        var second = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term.ToUpperInvariant()}");

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstBody = await first.ReadAsAsync<PublicSearchResponse>();
        var secondBody = await second.ReadAsAsync<PublicSearchResponse>();

        secondBody.Counts.Should().BeEquivalentTo(firstBody.Counts);
    }

    [Fact]
    public async Task Search_ForAnyTerm_ShapesTheCountsAsOneKeyPerScope()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement counts = document.RootElement.GetProperty("counts");

        counts
            .EnumerateObject()
            .Select(property => property.Name)
            .Should()
            .BeEquivalentTo("all", "articles", "videos", "shorts");
        counts.GetProperty("all").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task Search_ForAnyTerm_ForbidsCachingTheResponse()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl!.NoStore.Should().BeTrue();
    }

    [Fact]
    public async Task Search_WhenTheTeaserIsRichText_ReturnsAPlainTextExcerpt()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={RichTextTerm}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        SearchHitDto hit = body.Results.Items.Should().ContainSingle().Subject;

        hit.Excerpt.Should().NotContain("<", "markup would reach the reader verbatim");
        hit.Excerpt.Should().NotContain("&amp;").And.Contain("&", "entities are decoded, not stripped");
        hit.Excerpt.Should().Contain("sebene").And.Contain("la rumba", "adjacent blocks keep a space between them");
    }

    [Fact]
    public async Task Search_WhenTheTeaserOutgrowsAnExcerpt_CutsItOnAWordBoundary()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={RichTextTerm}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        SearchHitDto hit = body.Results.Items.Should().ContainSingle().Subject;

        hit.Excerpt.Length.Should().BeLessThanOrEqualTo(200);
        hit.Excerpt.Should().NotEndWith(" ");
        RichTextHeadline.Should().Contain(hit.Excerpt.Split(' ')[^1], "the cut lands between words");
    }

    [Fact]
    public async Task Search_WhenTheTeaserIsMarkupOnly_ReturnsAnEmptyExcerpt()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Search}?q={EmptyTeaserTerm}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicSearchResponse>();
        SearchHitDto hit = body.Results.Items.Should().ContainSingle().Subject;

        hit.Excerpt.Should().BeEmpty("stripping the tags leaves nothing to show");
        hit.Title.Should().Contain("Ndombolo", "the hit is still returned, excerpt or not");
    }
}
