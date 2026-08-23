using _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment.V1;

/// <summary>
/// Integration tests for the PublicDeleteArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class PublicDeleteArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    private async Task<ArticleCommentEntity> SeedCommentAsync(Guid articleId, Guid authorId)
    {
        return await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity comment = ArticleCommentFactory.Create(articleId, authorId);
            ctx.ArticleComments.Add(comment);
            return comment;
        });
    }

    [Fact]
    public async Task DeleteArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Comment(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteArticleComment_AsVisitor_NonExistentComment_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Comment(Guid.NewGuid(), Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ArticleComment"))
        );
    }

    [Fact]
    public async Task DeleteArticleComment_AsVisitor_NotOwner_ReturnsBadRequest()
    {
        ArticleEntity article = await SeedArticleAsync();
        ArticleCommentEntity comment = await SeedCommentAsync(article.Id, TestUser.SuperAdminId);
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Comment(article.Id, comment.Id));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ArticleInteractionErrorMessage>(m => m.NotCommentOwner())
        );
    }

    [Fact]
    public async Task DeleteArticleComment_AsOwner_SoftDeletesComment()
    {
        ArticleEntity article = await SeedArticleAsync();
        ArticleCommentEntity comment = await SeedCommentAsync(article.Id, TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Comment(article.Id, comment.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicDeleteArticleCommentResponse body = await response.ReadAsAsync<PublicDeleteArticleCommentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity? stored = await verifyDb.ArticleComments.FindAsync(comment.Id);
        stored!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteArticleComment_AsOwner_WithCommentBelongingToAnotherArticle_ReturnsNotFound()
    {
        ArticleEntity article = await SeedArticleAsync();
        ArticleEntity otherArticle = await SeedArticleAsync();
        ArticleCommentEntity comment = await SeedCommentAsync(article.Id, TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Comment(otherArticle.Id, comment.Id));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ArticleComment"))
        );

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity? persisted = await verifyDb.ArticleComments.FindAsync(comment.Id);
        persisted!.IsDeleted.Should().BeFalse("a comment under another article must not be deleted");
    }
}
