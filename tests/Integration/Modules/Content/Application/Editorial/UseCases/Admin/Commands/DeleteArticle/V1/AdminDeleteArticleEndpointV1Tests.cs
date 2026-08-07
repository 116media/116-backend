using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle.V1;

/// <summary>
/// Integration tests for the AdminDeleteArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ArticleEntity> SeedArticleAsync(Func<Guid, ArticleEntity> create)
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity article = create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(article);
            return article;
        });
    }

    [Fact]
    public async Task DeleteArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task DeleteArticle_WhenDraft_RemovesArticle()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.Create);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{article.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminDeleteArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? persisted = await ctx.Articles.FindAsync(article.Id);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteArticle_WhenPublished_ReturnsBadRequest()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.CreatePublished);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Articles}/{article.Id}");

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ArticleErrorMessage>(m => m.CannotDeletePublishedArticle())
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? persisted = await ctx.Articles.FindAsync(article.Id);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(EnumContentStatus.Published);
    }
}
