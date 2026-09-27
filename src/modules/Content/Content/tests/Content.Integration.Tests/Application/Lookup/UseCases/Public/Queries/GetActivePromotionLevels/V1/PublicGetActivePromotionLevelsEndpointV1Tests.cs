using _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;

/// <summary>
/// Integration tests for the PublicGetActivePromotionLevels endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetActivePromotionLevelsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetActivePromotionLevels_AsAnonymous_ReturnsActivePromotionLevels()
    {
        PromotionLevelEntity active = await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity entity = PromotionLevelFactory.Create();
            ctx.PromotionLevels.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.PromotionLevels);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetActivePromotionLevelsResponse>();
        body.PromotionLevels.Should().Contain(p => p.Id == active.Id);
        body.PromotionLevels.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetActivePromotionLevels_AsVisitor_ExcludesInactivePromotionLevels()
    {
        PromotionLevelEntity inactive = await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity entity = PromotionLevelFactory.CreateInactive();
            ctx.PromotionLevels.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.PromotionLevels);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetActivePromotionLevelsResponse>();
        body.PromotionLevels.Should().NotContain(p => p.Id == inactive.Id);
        body.PromotionLevels.Should().OnlyContain(p => p.IsActive);
    }
}
