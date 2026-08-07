using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment.V1;

/// <summary>
/// Integration tests for the AdminDeleteArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string CommentUrl(Guid articleId, Guid commentId) =>
        $"{ApiRoutes.Admin.Articles}/{articleId}/{InteractionsRouteConstants.Comments}/{commentId}";

    private async Task<(ArticleEntity Article, ArticleCommentEntity Comment)> SeedArticleWithCommentAsync(Guid authorId)
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity created = ArticleFactory.CreatePublished(category.Id);
            ctx.Articles.Add(created);
            return created;
        });

        ArticleCommentEntity comment = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity created = ArticleCommentFactory.Create(article.Id, authorId);
            ctx.ArticleComments.Add(created);
            return created;
        });

        return (article, comment);
    }

    [Fact]
    public async Task DeleteArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(CommentUrl(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteArticleComment_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(CommentUrl(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteArticleComment_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(CommentUrl(Guid.NewGuid(), Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ArticleComment"))
        );
    }

    [Fact]
    public async Task DeleteArticleComment_AsSuperAdmin_WithExistingComment_SoftDeletesComment()
    {
        (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync(TestUser.VisitorId);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(CommentUrl(article.Id, comment.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AdminDeleteArticleCommentResponse body = await response.ReadAsAsync<AdminDeleteArticleCommentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity? stored = await verifyDb.ArticleComments.FindAsync(comment.Id);
        stored!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteArticleComment_WithCommentBelongingToAnotherArticle_ReturnsNotFound()
    {
        (ArticleEntity _, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync(TestUser.VisitorId);
        (ArticleEntity otherArticle, ArticleCommentEntity _) = await SeedArticleWithCommentAsync(TestUser.VisitorId);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(CommentUrl(otherArticle.Id, comment.Id));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ArticleComment"))
        );

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity? persisted = await verifyDb.ArticleComments.FindAsync(comment.Id);
        persisted!.IsDeleted.Should().BeFalse("a comment under another article must not be deleted");
    }
}
