using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType.V1;

/// <summary>
/// Integration tests for the AdminCreateContentType endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateContentTypeEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateContentType_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminCreateContentTypeRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateContentType_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new AdminCreateContentTypeRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateContentType_AsSuperAdmin_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreateContentTypeRequestBuilder().WithName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateContentType_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreateContentTypeRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreateContentTypeResponse>();
        body.ContentType.Id.Should().NotBeEmpty();
        body.ContentType.Name.Should().Be(request.Name);
        body.ContentType.IsActive.Should().BeTrue();

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        ContentTypeEntity? persisted = await context.ContentTypes.FindAsync(body.ContentType.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be(request.Name);
    }

    /// <summary>
    /// Verifies that creating a content type with a name that already exists
    /// returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task CreateContentType_WithDuplicateName_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity existing = ContentTypeFactory.Create("DuplicateType");
            ctx.ContentTypes.Add(existing);
            return existing;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreateContentTypeRequestBuilder().WithName("DuplicateType").Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.ContentTypes, request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }
}
