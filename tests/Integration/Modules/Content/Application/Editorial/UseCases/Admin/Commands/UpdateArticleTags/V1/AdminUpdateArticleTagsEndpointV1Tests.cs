using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;

/// <summary>
/// Integration tests for the AdminUpdateArticleTags endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateArticleTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateArticleTags_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Articles, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateArticleTags_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Articles, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArticleTags_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        AdminUpdateArticleTagsRequest request = new AdminUpdateArticleTagsRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Articles, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task UpdateArticle_WithTags_ReplacesExistingTags()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            TagEntity tag1 = TagFactory.Create("OldTag", "old-tag");
            TagEntity tag2 = TagFactory.Create("NewTag", "new-tag");
            ArticleEntity a = ArticleFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Tags.AddRange(tag1, tag2);
            ctx.Articles.Add(a);
            return a;
        });

        Client.AuthenticateAsSuperAdmin();

        AdminUpdateArticleTagsRequest assignRequest = new AdminUpdateArticleTagsRequestBuilder()
            .WithTagNames(new[] { "OldTag" })
            .Build();
        await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Articles, article.Id),
            assignRequest
        );

        AdminUpdateArticleTagsRequest replaceRequest = new AdminUpdateArticleTagsRequestBuilder()
            .WithTagNames(new[] { "NewTag" })
            .Build();
        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Editorial.Tags(EditorialRouteConstants.Articles, article.Id),
            replaceRequest
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateArticleTagsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        List<string> tagNames = await ctx
            .ArticleTags.Where(at => at.ArticleId == article.Id)
            .Join(ctx.Tags, at => at.TagId, t => t.Id, (at, t) => t.Name)
            .ToListAsync();

        tagNames.Should().ContainSingle().Which.Should().Be("NewTag");
    }
}
