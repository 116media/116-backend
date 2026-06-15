using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;

/// <summary>
/// Integration tests for the AdminUpdateTag endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateTagEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateTag_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Updated", Slug = "updated" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated", Slug = "updated" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create();
        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Updated Tag", Slug = "updated-tag" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{tag.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that updating a tag with a slug exceeding the maximum allowed length
    /// (60 characters) returns a 400 Bad Request or 422 Unprocessable Entity response.
    /// </summary>
    [Fact]
    public async Task UpdateTag_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new { Name = "Valid Name", Slug = new string('a', 200) };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a tag with a name exceeding the maximum allowed length
    /// (50 characters) returns a 400 Bad Request or 422 Unprocessable Entity response,
    /// exercising the <c>isRequired=false</c> branch of <c>ValidTagName</c> in TagValidation.
    /// </summary>
    [Fact]
    public async Task UpdateTag_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new { Name = new string('T', 200), Slug = "valid-slug" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a tag with a slug that does not match the required format
    /// (lowercase letters, numbers, and hyphens only) returns a 400 Bad Request or
    /// 422 Unprocessable Entity response, exercising the slug regex branch of TagValidation.
    /// </summary>
    [Fact]
    public async Task UpdateTag_WithInvalidSlugFormat_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new { Name = "Valid Name", Slug = "INVALID SLUG!!!" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
