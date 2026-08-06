using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.V1;

/// <summary>
/// Integration tests for the AdminUpdateCategory endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateCategoryEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            return category;
        });
    }

    [Fact]
    public async Task UpdateCategory_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        string name = ShortName("un");
        string slug = ShortSlug("un");
        var request = new
        {
            Name = name,
            Slug = slug,
            Description = "Updated",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateCategoryResponse>();
        body.Category.Id.Should().Be(category.Id);
        body.Category.Name.Should().Be(name);
        body.Category.Slug.Should().Be(slug);
        body.Category.Description.Should().Be("Updated");

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryEntity? updated = await verifyContext.Categories.FindAsync(category.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(name);
        updated.Slug.Should().Be(slug);
        updated.Description.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateCategory_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("nf"),
            Slug = ShortSlug("nf"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task UpdateCategory_AsAdmin_ReturnsForbidden()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsAdmin();
        var request = new
        {
            Name = ShortName("uf"),
            Slug = ShortSlug("uf"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategory_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Name = ShortName("ua"),
            Slug = ShortSlug("ua"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategory_WithEmptyName_ReturnsValidationError()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = "",
            Slug = ShortSlug("ev"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{category.Id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Name", Localized<CategoryErrorMessage>(m => m.NameRequired()))
        );
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("iv"),
            Slug = ShortSlug("iv"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/not-a-guid", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Id", Localized<CategoryErrorMessage>(m => m.Localizer["IdInvalid"].Value))
        );
    }

    [Fact]
    public async Task UpdateCategory_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new
        {
            Name = new string('A', 300),
            Slug = ShortSlug("tl"),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Name",
                Localized<CategoryErrorMessage>(m => m.NameTooLong(ContentConstants.MaxCategoryNameLength))
            )
        );
    }

    [Fact]
    public async Task UpdateCategory_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new
        {
            Name = ShortName("sl"),
            Slug = new string('a', 200),
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Slug",
                Localized<CategoryErrorMessage>(m => m.SlugTooLong(ContentConstants.MaxCategorySlugLength))
            )
        );
    }

    [Fact]
    public async Task UpdateCategory_WithDescriptionTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new
        {
            Name = ShortName("dl"),
            Slug = ShortSlug("dl"),
            Description = new string('D', 500),
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Description",
                Localized<CategoryErrorMessage>(m =>
                    m.DescriptionTooLong(ContentConstants.MaxCategoryDescriptionLength)
                )
            )
        );
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidSlugFormat_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new
        {
            Name = ShortName("sf"),
            Slug = "INVALID SLUG!!!",
            Description = "Test",
            IsGossip = false,
            IsExclusive = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<CategoryErrorMessage>(m => m.SlugInvalidFormat()))
        );
    }

    [Fact]
    public async Task UpdateCategory_SetExclusive_ClearsPreviouslyExclusiveCategory()
    {
        Guid previousExclusiveId = Guid.Empty;
        CategoryEntity target = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(contentType);
            CategoryEntity previousExclusive = CategoryFactory.Create(contentType.Id, isExclusive: true);
            CategoryEntity candidate = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(previousExclusive);
            ctx.Categories.Add(candidate);
            previousExclusiveId = previousExclusive.Id;
            return candidate;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            Name = ShortName("ex"),
            Slug = ShortSlug("ex"),
            Description = "Now exclusive",
            IsGossip = false,
            IsExclusive = true,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Categories}/{target.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateCategoryResponse>();
        body.Category.IsExclusive.Should().BeTrue();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryEntity? previous = await verifyContext.Categories.FindAsync(previousExclusiveId);
        previous!.IsExclusive.Should().BeFalse();
        CategoryEntity? updated = await verifyContext.Categories.FindAsync(target.Id);
        updated!.IsExclusive.Should().BeTrue();
    }
}
