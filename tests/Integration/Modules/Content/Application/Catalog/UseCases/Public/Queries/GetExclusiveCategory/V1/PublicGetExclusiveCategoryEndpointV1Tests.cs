using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.V1;

/// <summary>
/// Integration tests for the PublicGetExclusiveCategory endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetExclusiveCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublicGetExclusiveCategory_AsAnonymous_WhenNoExclusive_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Categories}/exclusive?pageIndex=0&pageSize=10");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<CategoryErrorMessage>(m => m.NoExclusiveCategoryFound())
        );
    }

    [Fact]
    public async Task PublicGetExclusiveCategory_AsAnonymous_WithSeededExclusive_ReturnsOk()
    {
        CategoryEntity exclusiveCategory = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            exclusiveCategory = CategoryFactory.Create(contentType.Id, isExclusive: true);
            ctx.Categories.Add(exclusiveCategory);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Categories}/exclusive?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<PublicGetExclusiveCategoryResponse>();
        body.Category.Id.Should().Be(exclusiveCategory.Id);
        body.Category.IsExclusive.Should().BeTrue();
        body.Videos.Should().NotBeNull();
    }
}
