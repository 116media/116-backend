using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle.V1;

/// <summary>
/// Integration tests for the PublicShareArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicShareArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ArticleEntity> SeedArticleAsync()
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity article = ArticleFactory.CreatePublished(category.Id);
            ctx.Articles.Add(article);
            return article;
        });
    }

    [Fact]
    public async Task ShareArticle_AsAnonymous_ReturnsOk()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Articles.Shares(article.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ArticleShares.AnyAsync(s => s.ArticleId == article.Id && s.UserId == null)).Should().BeTrue();
    }

    [Fact]
    public async Task ShareArticle_AsVisitor_ReturnsOk()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Articles.Shares(article.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ArticleShares.AnyAsync(s => s.ArticleId == article.Id && s.UserId == TestUser.VisitorId))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ShareArticle_NonExistentArticle_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Articles.Shares(Guid.NewGuid()), null);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShareArticle_WithShareChannel_PersistsChannel()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.Shares(article.Id),
            new { shareChannel = "WebShare" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        ArticleShareEntity share = await verifyDb.ArticleShares.SingleAsync(s => s.ArticleId == article.Id);
        share.ShareChannel.Should().Be(EnumShareChannel.WebShare);
    }
}
