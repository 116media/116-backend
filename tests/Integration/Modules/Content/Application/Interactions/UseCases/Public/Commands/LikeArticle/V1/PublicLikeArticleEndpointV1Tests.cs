using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticle.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeArticle.V1;

/// <summary>
/// Integration tests for the PublicLikeArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicLikeArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task LikeArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Articles.Likes(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LikeArticle_AsVisitor_NonExistentArticle_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Articles.Likes(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task LikeArticle_AsVisitor_WithValidArticle_ReturnsOk()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Articles.Likes(article.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicLikeArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ArticleLikes.AnyAsync(l => l.ArticleId == article.Id && l.UserId == TestUser.VisitorId))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task LikeArticle_AsVisitor_AlreadyLiked_ReturnsConflict()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.AuthenticateAsVisitor();

        await Client.PostAsync(Routes.Public.Articles.Likes(article.Id), null);

        var response = await Client.PostAsync(Routes.Public.Articles.Likes(article.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArticleInteractionErrorMessage>(m => m.AlreadyLiked())
        );

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ArticleLikes.CountAsync(l => l.ArticleId == article.Id && l.UserId == TestUser.VisitorId))
            .Should()
            .Be(1);
    }
}
