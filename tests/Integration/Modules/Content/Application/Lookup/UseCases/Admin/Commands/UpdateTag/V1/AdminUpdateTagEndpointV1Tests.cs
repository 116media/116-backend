using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Constants;
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
        var request = new AdminUpdateTagRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdateTagRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTag_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        TagEntity tag = await SeedAsync<ContentDbContext, TagEntity>(ctx =>
        {
            TagEntity entity = TagFactory.Create();
            ctx.Tags.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdateTagRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{tag.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateTagResponse>();
        body.Tag.Id.Should().Be(tag.Id);
        body.Tag.Name.Should().Be(request.Name);
        body.Tag.Slug.Should().Be(request.Slug);

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        TagEntity? persisted = await context.Tags.FindAsync(tag.Id);
        persisted!.Name.Should().Be(request.Name);
        persisted.Slug.Should().Be(request.Slug);
    }

    /// <summary>
    /// Verifies that updating a tag with a slug exceeding the maximum allowed length
    /// (60 characters) returns a 400 Bad Request response.
    /// </summary>
    [Fact]
    public async Task UpdateTag_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdateTagRequestBuilder()
            .WithSlug(new string('a', TestConstants.Content.Tag.SlugMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a tag with a name exceeding the maximum allowed length
    /// (50 characters) returns a 400 Bad Request response, exercising the
    /// <c>isRequired=false</c> branch of <c>ValidTagName</c> in TagValidation.
    /// </summary>
    [Fact]
    public async Task UpdateTag_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdateTagRequestBuilder()
            .WithName(new string('T', TestConstants.Content.Tag.NameMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a tag with a slug that does not match the required format
    /// (lowercase letters, numbers, and hyphens only) returns a 400 Bad Request response,
    /// exercising the slug regex branch of TagValidation.
    /// </summary>
    [Fact]
    public async Task UpdateTag_WithInvalidSlugFormat_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdateTagRequestBuilder().WithSlug("INVALID SLUG!!!").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Tags}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
