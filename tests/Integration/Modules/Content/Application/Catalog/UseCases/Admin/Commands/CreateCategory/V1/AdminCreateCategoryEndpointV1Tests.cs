using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;

/// <summary>
/// Integration tests for the AdminCreateCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<ContentTypeEntity> SeedContentTypeAsync()
    {
        return await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            return contentType;
        });
    }

    [Fact]
    public async Task CreateCategory_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        ContentTypeEntity contentType = await SeedContentTypeAsync();

        Client.AuthenticateAsSuperAdmin();
        string name = ShortName("cc");
        string slug = ShortSlug("cc");
        var request = new
        {
            Name = name,
            Slug = slug,
            Description = "Test desc",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreateCategoryResponse>();
        body.Category.Id.Should().NotBeEmpty();
        body.Category.Name.Should().Be(name);
        body.Category.Slug.Should().Be(slug);
        body.Category.ContentTypeId.Should().Be(contentType.Id);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryEntity? category = await verifyContext.Categories.FirstOrDefaultAsync(c => c.Id == body.Category.Id);
        category.Should().NotBeNull();
        category!.Name.Should().Be(name);
        category.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task CreateCategory_AsAdmin_ReturnsForbidden()
    {
        ContentTypeEntity contentType = await SeedContentTypeAsync();

        Client.AuthenticateAsAdmin();
        var request = new
        {
            Name = ShortName("fa"),
            Slug = ShortSlug("fa"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Name = ShortName("na"),
            Slug = ShortSlug("na"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ReturnsConflict()
    {
        ContentTypeEntity contentType = await SeedContentTypeAsync();
        await SeedAsync<ContentDbContext>(ctx =>
        {
            CategoryEntity existing = CategoryFactory.Create(contentType.Id, ShortName("dup"), "dup-slug-test");
            ctx.Categories.Add(existing);
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("dup2"),
            Slug = "dup-slug-test",
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<CategoryErrorMessage>(m => m.AlreadyExists("dup-slug-test"))
        );
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsValidationError()
    {
        ContentTypeEntity contentType = await SeedContentTypeAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "",
            Slug = ShortSlug("en"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Name", Localized<CategoryErrorMessage>(m => m.NameRequired()))
        );
    }

    [Fact]
    public async Task CreateCategory_WithEmptySlug_ReturnsValidationError()
    {
        ContentTypeEntity contentType = await SeedContentTypeAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("es"),
            Slug = "",
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{contentType.Id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<CategoryErrorMessage>(m => m.SlugRequired()))
        );
    }

    [Fact]
    public async Task CreateCategory_WithInvalidContentTypeId_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("ic"),
            Slug = ShortSlug("ic"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/not-a-guid", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("ContentTypeId", Localized<ContentTypeErrorMessage>(m => m.Localizer["IdInvalid"].Value))
        );
    }

    [Fact]
    public async Task CreateCategory_WithNonExistentContentType_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("ne"),
            Slug = ShortSlug("ne"),
            Description = "Test",
            IsFree = true,
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentType"))
        );
    }
}
