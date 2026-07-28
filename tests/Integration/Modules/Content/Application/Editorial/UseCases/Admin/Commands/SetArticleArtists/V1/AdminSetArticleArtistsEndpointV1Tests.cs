using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists.V1;

/// <summary>
/// Integration tests for the AdminSetArticleArtists endpoint.
/// </summary>
[Collection("Database")]
public class AdminSetArticleArtistsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string Url(Guid articleId) =>
        Routes.Admin.Editorial.Artists(EditorialRouteConstants.Articles, articleId);

    private async Task<(
        ArticleEntity Article,
        ArtistEntity ArtistA,
        ArtistEntity ArtistB
    )> SeedArticleWithArtistsAsync()
    {
        return await SeedAsync<ContentDbContext, (ArticleEntity, ArtistEntity, ArtistEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity article = ArticleFactory.CreatePublished(category.Id);
            ArtistEntity artistA = ArtistFactory.Create();
            ArtistEntity artistB = ArtistFactory.Create();

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(article);
            ctx.Artists.AddRange(artistA, artistB);

            return (article, artistA, artistB);
        });
    }

    [Fact]
    public async Task SetArticleArtists_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Url(Guid.NewGuid()),
            new AdminSetArticleArtistsRequest([Guid.NewGuid()])
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetArticleArtists_WithUnknownArticle_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(Url(Guid.NewGuid()), new AdminSetArticleArtistsRequest([]));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetArticleArtists_WithUnknownArtistId_ReturnsNotFound()
    {
        (ArticleEntity article, _, _) = await SeedArticleWithArtistsAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(article.Id),
            new AdminSetArticleArtistsRequest([Guid.NewGuid()])
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tagging with two artists then re-tagging with one must leave exactly one junction
    /// row — a set-replace, not an accumulate.
    /// </summary>
    [Fact]
    public async Task SetArticleArtists_Retagging_ReplacesTheSetExactly()
    {
        (ArticleEntity article, ArtistEntity artistA, ArtistEntity artistB) = await SeedArticleWithArtistsAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(Url(article.Id), new AdminSetArticleArtistsRequest([artistA.Id, artistB.Id]));
        var response = await Client.PutAsJsonAsync(Url(article.Id), new AdminSetArticleArtistsRequest([artistB.Id]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AdminSetArticleArtistsResponse body = await response.ReadAsAsync<AdminSetArticleArtistsResponse>();
        body.ArtistIds.Should().Equal(artistB.Id);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<ArticleArtistEntity> rows = await ctx.ArticleArtists.Where(aa => aa.ArticleId == article.Id).ToListAsync();
        rows.Should().HaveCount(1);
        rows[0].ArtistId.Should().Be(artistB.Id);
    }

    [Fact]
    public async Task SetArticleArtists_WithEmptyList_UntagsEverything()
    {
        (ArticleEntity article, ArtistEntity artistA, _) = await SeedArticleWithArtistsAsync();
        Client.AuthenticateAsAdmin();

        await Client.PutAsJsonAsync(Url(article.Id), new AdminSetArticleArtistsRequest([artistA.Id]));
        var response = await Client.PutAsJsonAsync(Url(article.Id), new AdminSetArticleArtistsRequest([]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        (await ctx.ArticleArtists.AnyAsync(aa => aa.ArticleId == article.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task SetArticleArtists_WithDuplicateIds_ReturnsBadRequest()
    {
        (ArticleEntity article, ArtistEntity artistA, _) = await SeedArticleWithArtistsAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Url(article.Id),
            new AdminSetArticleArtistsRequest([artistA.Id, artistA.Id])
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Deleting the article cascades its junction rows away.
    /// </summary>
    [Fact]
    public async Task DeletingArticle_CascadesJunctionRowsAway()
    {
        (ArticleEntity article, ArtistEntity artistA, _) = await SeedArticleWithArtistsAsync();
        Client.AuthenticateAsAdmin();
        await Client.PutAsJsonAsync(Url(article.Id), new AdminSetArticleArtistsRequest([artistA.Id]));

        await using (ContentDbContext ctx = CreateDbContext<ContentDbContext>())
        {
            ArticleEntity tracked = await ctx.Articles.SingleAsync(a => a.Id == article.Id);
            ctx.Articles.Remove(tracked);
            await ctx.SaveChangesAsync();
        }

        await using ContentDbContext verify = CreateDbContext<ContentDbContext>();
        (await verify.ArticleArtists.AnyAsync(aa => aa.ArticleId == article.Id)).Should().BeFalse();
    }
}
