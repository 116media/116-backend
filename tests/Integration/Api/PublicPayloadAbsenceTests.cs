using System.Text.Json;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Api;

/// <summary>
/// Raw-JSON absence assertions over public payloads. Deserializing into the server's own DTO
/// types hides extra fields, which is how the audit and staff-data leak had green tests — so
/// these tests walk the raw document and fail on any forbidden property, at any depth.
/// </summary>
public class PublicPayloadAbsenceTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Properties no public payload may carry: the audit trail, staff identifiers, commercial
    /// linkage and editorial state.
    /// </summary>
    private static readonly string[] ForbiddenProperties =
    [
        "createdBy",
        "updatedBy",
        "createdAt",
        "updatedAt",
        "authorId",
        "customerId",
        "customerName",
        "orderItemId",
        "rejectionReason",
        "email",
    ];

    /// <summary>
    /// Walks every object in the document and asserts no forbidden property appears.
    /// </summary>
    /// <param name="element">The element to walk.</param>
    /// <param name="path">The property path, for the failure message.</param>
    /// <param name="allow">Properties allowed at this payload's top level (e.g. the own email).</param>
    private static void AssertNoForbiddenProperties(JsonElement element, string path, string[]? allow = null)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    bool allowed = allow is not null && path == "$" && allow.Contains(property.Name);
                    if (!allowed)
                    {
                        ForbiddenProperties
                            .Should()
                            .NotContain(property.Name, $"public payloads must not carry {path}.{property.Name}");
                    }

                    AssertNoForbiddenProperties(property.Value, $"{path}.{property.Name}", allow);
                }
                break;
            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    AssertNoForbiddenProperties(item, $"{path}[]", allow);
                }
                break;
        }
    }

    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    [Fact]
    public async Task ArticleBySlug_AsAnonymous_CarriesNoAuditCommercialOrStaffData()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();
        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertNoForbiddenProperties(body.RootElement, "$");
    }

    [Fact]
    public async Task PublishedArticles_AsAnonymous_CarryNoAuditCommercialOrStaffData()
    {
        Guid categoryId = await SeedCategoryAsync();
        await SeedAsync<ContentDbContext>(ctx => ctx.Articles.Add(ArticleFactory.CreatePublished(categoryId)));

        Client.ClearAuthentication();
        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}?pageIndex=0&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertNoForbiddenProperties(body.RootElement, "$");
    }

    [Fact]
    public async Task ContentTypes_AsVisitor_CarryNoAuditData()
    {
        await SeedAsync<ContentDbContext>(ctx => ctx.ContentTypes.Add(ContentTypeFactory.Create()));

        Client.AuthenticateAsVisitor();
        var response = await Client.GetAsync(ApiRoutes.Public.ContentTypes);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertNoForbiddenProperties(body.RootElement, "$");
    }

    [Fact]
    public async Task OwnProfile_AsVisitor_CarriesNoAuditData()
    {
        Client.AuthenticateAsVisitor();
        var response = await Client.GetAsync(Routes.Public.Me.Profile());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement user = body.RootElement.GetProperty("user");
        AssertNoForbiddenProperties(user, "$", allow: ["email"]);
    }

    [Fact]
    public async Task OwnSessions_AsVisitor_CarryNoAuditData()
    {
        Client.AuthenticateAsVisitor();
        var response = await Client.GetAsync($"{ApiRoutes.BaseUrl}/{ApiRoutes.ApiVersion}/public/me/sessions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertNoForbiddenProperties(body.RootElement, "$");
    }

    [Fact]
    public async Task OwnRoles_AsVisitor_CarryNoAuditOrLifecycleData()
    {
        Client.AuthenticateAsVisitor();
        var response = await Client.GetAsync(Routes.Public.Me.Roles());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertNoForbiddenProperties(body.RootElement, "$");
        body.RootElement.GetRawText().Should().NotContain("isDeleted").And.NotContain("deletedAt");
    }
}
