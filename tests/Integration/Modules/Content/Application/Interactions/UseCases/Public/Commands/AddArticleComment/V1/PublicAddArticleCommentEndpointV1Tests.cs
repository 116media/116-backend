using _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment.V1;

/// <summary>
/// Integration tests for the PublicAddArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class PublicAddArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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
    public async Task AddArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        PublicAddArticleCommentRequest request = new PublicAddArticleCommentRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Articles.Comments(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddArticleComment_AsVisitor_NonExistentArticle_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        PublicAddArticleCommentRequest request = new PublicAddArticleCommentRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Articles.Comments(Guid.NewGuid()), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task AddArticleComment_AsVisitor_WithEmptyBody_ReturnsValidationError()
    {
        Client.AuthenticateAsVisitor();
        PublicAddArticleCommentRequest request = new PublicAddArticleCommentRequestBuilder()
            .WithBody(string.Empty)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Articles.Comments(Guid.NewGuid()), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Body", Localized<ArticleInteractionErrorMessage>(m => m.CommentBodyRequired()))
        );
    }

    [Fact]
    public async Task AddArticleComment_AsVisitor_WithValidData_ReturnsCreated()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.AuthenticateAsVisitor();
        PublicAddArticleCommentRequest request = new PublicAddArticleCommentRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Articles.Comments(article.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PublicAddArticleCommentResponse body = await response.ReadAsAsync<PublicAddArticleCommentResponse>();
        body.Comment.Id.Should().NotBeEmpty();
        body.Comment.Body.Should().Be(request.Body);
        body.Comment.UserId.Should().Be(TestUser.VisitorId);
        body.Comment.IsDeleted.Should().BeFalse();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity? stored = await verifyDb.ArticleComments.FindAsync(body.Comment.Id);
        stored.Should().NotBeNull();
        stored!.ArticleId.Should().Be(article.Id);
        stored.Body.Should().Be(request.Body);
        stored.UserId.Should().Be(TestUser.VisitorId);
    }
}
