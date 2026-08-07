using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;

/// <summary>
/// Integration tests for the AdminUpdateLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<CategoryEntity> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            return category;
        });
    }

    [Fact]
    public async Task UpdateLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateLyrics_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task UpdateLyrics_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        CategoryEntity category = await SeedCategoryAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity l = LyricsFactory.Create(category.Id);
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug(lyrics.Slug)
            .WithSongTitle("Updated Song Title")
            .WithArtistName("Updated Artist")
            .WithLyricsText("Updated lyrics text that contains enough content to satisfy validation.")
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateLyricsResponse>();
        body.Lyrics.Id.Should().Be(lyrics.Id);
        body.Lyrics.SongTitle.Should().Be(request.SongTitle);
        body.Lyrics.ArtistName.Should().Be(request.ArtistName);
        body.Lyrics.LyricsText.Should().Be(request.LyricsText);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.SongTitle.Should().Be(request.SongTitle);
        persisted.ArtistName.Should().Be(request.ArtistName);
        persisted.LyricsText.Should().Be(request.LyricsText);
    }

    [Fact]
    public async Task UpdateLyrics_AsSuperAdmin_WithNewCategory_UpdatesCategory()
    {
        CategoryEntity originalCategory = await SeedCategoryAsync();
        CategoryEntity newCategory = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            return category;
        });
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity l = LyricsFactory.Create(originalCategory.Id);
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder()
            .WithCategoryId(newCategory.Id)
            .WithSlug(lyrics.Slug)
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateLyricsResponse>();
        body.Lyrics.CategoryId.Should().Be(newCategory.Id);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.CategoryId.Should().Be(newCategory.Id);
    }

    [Fact]
    public async Task UpdateLyrics_AsSuperAdmin_WithCustomerAndOrderItem_UpdatesCommerceFields()
    {
        CategoryEntity category = await SeedCategoryAsync();
        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            CustomerEntity c = CustomerFactory.Create();
            ctx.Customers.Add(c);
            return c;
        });
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity l = LyricsFactory.Create(category.Id);
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();
        Guid orderItemId = Guid.NewGuid();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug(lyrics.Slug)
            .WithCustomerId(customer.Id)
            .WithOrderItemId(orderItemId)
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateLyricsResponse>();
        body.Lyrics.CustomerId.Should().Be(customer.Id);
        body.Lyrics.OrderItemId.Should().Be(orderItemId);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted!.CustomerId.Should().Be(customer.Id);
        persisted.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public async Task UpdateLyrics_AsSuperAdmin_WithUnchangedSlug_ReturnsOk()
    {
        CategoryEntity category = await SeedCategoryAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity l = LyricsFactory.Create(category.Id);
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug(lyrics.Slug)
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateLyrics_AsSuperAdmin_WithSlugUsedByAnotherLyrics_ReturnsConflict()
    {
        CategoryEntity category = await SeedCategoryAsync();
        LyricsEntity otherLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity l = LyricsFactory.Create(category.Id);
            ctx.Lyrics.Add(l);
            return l;
        });
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity l = LyricsFactory.Create(category.Id);
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateLyricsRequest request = new AdminUpdateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug(otherLyrics.Slug)
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}", request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsErrorMessage>(m => m.SlugAlreadyExists(otherLyrics.Slug))
        );
    }
}
