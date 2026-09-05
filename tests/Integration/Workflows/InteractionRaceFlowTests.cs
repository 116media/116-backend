using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Flow covering the lost insert race Stage 6 closes: two concurrent identical likes both pass the
/// application pre-check, the database's unique index refuses the loser, and the answer must be
/// the same 409 the pre-check produces — never a 500.
/// </summary>
[Collection("Database")]
public class InteractionRaceFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task LikeArticle_Concurrently_OneWinsOneConflictsAndExactlyOneRowPersists()
    {
        // Arrange
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

        Client.AuthenticateAsVisitor();

        // Act — fire both before awaiting either, so they overlap inside the host
        Task<HttpResponseMessage> first = Client.PostAsync(Routes.Public.Articles.Likes(article.Id), null);
        Task<HttpResponseMessage> second = Client.PostAsync(Routes.Public.Articles.Likes(article.Id), null);
        HttpResponseMessage[] responses = await Task.WhenAll(first, second);

        // Assert — one accepted, one 409; the loser is a conflict whether the pre-check or the
        // unique index caught it, and nothing surfaces as a 500
        responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(1);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ArticleLikes.CountAsync(l => l.ArticleId == article.Id && l.UserId == TestUser.VisitorId))
            .Should()
            .Be(1);
    }
}
