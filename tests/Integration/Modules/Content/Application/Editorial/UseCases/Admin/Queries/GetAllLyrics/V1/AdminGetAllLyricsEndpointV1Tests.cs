using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics.V1;

/// <summary>
/// Integration tests for the AdminGetAllLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    [Fact]
    public async Task GetAllLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Lyrics);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Lyrics);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllLyrics_AsAdmin_IsAllowed()
    {
        Guid categoryId = await SeedCategoryAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Lyrics);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllLyricsResponse body = await response.ReadAsAsync<AdminGetAllLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == lyrics.Id);
        body.Lyrics.Count.Should().BeGreaterThanOrEqualTo(1);
        body.Lyrics.PageIndex.Should().Be(0);
        body.Lyrics.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllLyrics_WithSearch_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Lyrics}?search=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllLyricsResponse body = await response.ReadAsAsync<AdminGetAllLyricsResponse>();
        body.Lyrics.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the search query parameter filters lyrics by song title,
    /// returning only lyrics whose song title matches the search term.
    /// </summary>
    [Fact]
    public async Task GetAllLyrics_WithSearchQuery_ReturnsFilteredResults()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity matchingLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId, "UniqueSearchTerm Song", "Test Artist");
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity otherLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId, "Completely Different Song", "Other Artist");
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Lyrics}?search=UniqueSearchTerm");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllLyricsResponse body = await response.ReadAsAsync<AdminGetAllLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == matchingLyrics.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == otherLyrics.Id);
        body.Lyrics.Items.Should().OnlyContain(item => item.SongTitle.Contains("UniqueSearchTerm"));
    }

    /// <summary>
    /// Verifies that the status query parameter filters lyrics by editorial workflow status,
    /// returning only lyrics whose status matches the requested filter.
    /// </summary>
    [Fact]
    public async Task GetAllLyrics_WithStatusFilter_ReturnsOnlyMatchingLyrics()
    {
        Guid categoryId = await SeedCategoryAsync();

        LyricsEntity published = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.CreatePublished(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity draft = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Lyrics}?status={EnumContentStatus.Published}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllLyricsResponse body = await response.ReadAsAsync<AdminGetAllLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == published.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == draft.Id);
        body.Lyrics.Items.Should().OnlyContain(item => item.Status == EnumContentStatus.Published);
    }

    /// <summary>
    /// Verifies that the categoryId query parameter filters lyrics down to the given category.
    /// </summary>
    [Fact]
    public async Task GetAllLyrics_WithCategoryFilter_ReturnsOnlyMatchingLyrics()
    {
        Guid category1Id = await SeedCategoryAsync();
        Guid category2Id = await SeedCategoryAsync();

        LyricsEntity lyricsInCategory1 = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(category1Id);
            ctx.Lyrics.Add(entity);
            return entity;
        });
        LyricsEntity lyricsInCategory2 = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(category2Id);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Lyrics}?categoryId={category1Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetAllLyricsResponse body = await response.ReadAsAsync<AdminGetAllLyricsResponse>();
        body.Lyrics.Items.Should().Contain(item => item.Id == lyricsInCategory1.Id);
        body.Lyrics.Items.Should().NotContain(item => item.Id == lyricsInCategory2.Id);
        body.Lyrics.Items.Should().OnlyContain(item => item.CategoryId == category1Id);
    }
}
