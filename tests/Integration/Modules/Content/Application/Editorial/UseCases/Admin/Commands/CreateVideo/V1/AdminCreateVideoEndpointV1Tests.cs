using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.V1;

/// <summary>
/// Integration tests for the AdminCreateVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateVideo_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var slug = $"test-video-{Guid.NewGuid().ToString("N")[..8]}";
        var request = new
        {
            CategoryId = category.Id,
            Title = "Test Video Title",
            Slug = slug,
            Description = "A test video description for integration testing.",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateVideo_AsSuperAdmin_WithNonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Test Video Title",
            Slug = "non-existent-category-video",
            Description = "A test video description.",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVideo_AsSuperAdmin_WithEmptyTitle_ReturnsValidationError()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = category.Id,
            Title = "",
            Slug = "empty-title-video",
            Description = "A test video description.",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateVideo_AsSuperAdmin_WithDuplicateSlug_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var existingVideo = VideoFactory.Create(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Videos.Add(existingVideo);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            CategoryId = category.Id,
            Title = "Duplicate Slug Video",
            Slug = existingVideo.Slug,
            Description = "A test video description.",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }
}
