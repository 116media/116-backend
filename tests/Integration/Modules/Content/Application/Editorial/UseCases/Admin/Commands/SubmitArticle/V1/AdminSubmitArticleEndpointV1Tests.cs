using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle.V1;

/// <summary>
/// Integration tests for the AdminSubmitArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminSubmitArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    private async Task<EnumContentStatus> GetArticleStatusAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? article = await ctx.Articles.FindAsync(id);
        return article!.Status;
    }

    [Fact]
    public async Task SubmitArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that submitting a free article that is already in PendingReview status
    /// returns a 409 Conflict problem and leaves the status unchanged.
    /// </summary>
    [Fact]
    public async Task SubmitArticle_WhenAlreadySubmitted_ReturnsConflict()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.CreatePendingReview);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Articles, article.Id),
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
        (await GetArticleStatusAsync(article.Id)).Should().Be(EnumContentStatus.PendingReview);
    }

    /// <summary>
    /// Verifies that submitting a paid article that has already left Draft (it is awaiting
    /// payment) returns a 409 Conflict via the already-submitted message and leaves the
    /// status unchanged. This exercises the paid branch that reports AlreadySubmitted, as
    /// opposed to the free branch that reports AlreadyPendingReview.
    /// </summary>
    [Fact]
    public async Task SubmitArticle_WhenPaidArticleAwaitingPayment_ReturnsConflict()
    {
        Guid orderItemId = Guid.NewGuid();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            CustomerEntity customer = CustomerFactory.Create();
            ArticleEntity paidArticle = ArticleFactory.CreatePendingPayment(category.Id, customer.Id, orderItemId);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Customers.Add(customer);
            ctx.Articles.Add(paidArticle);
            return paidArticle;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Articles, article.Id),
            null
        );

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
        (await GetArticleStatusAsync(article.Id)).Should().Be(EnumContentStatus.PendingPayment);
    }

    /// <summary>
    /// Verifies that submitting a free draft article succeeds, returns IsSuccess true,
    /// and transitions the persisted status from Draft to PendingReview.
    /// </summary>
    [Fact]
    public async Task SubmitArticle_AsSuperAdmin_FreeArticle_ReturnsOk()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.Create);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Submit(EditorialRouteConstants.Articles, article.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminSubmitArticleResponse>();
        body.IsSuccess.Should().BeTrue();
        (await GetArticleStatusAsync(article.Id)).Should().Be(EnumContentStatus.PendingReview);
    }
}
