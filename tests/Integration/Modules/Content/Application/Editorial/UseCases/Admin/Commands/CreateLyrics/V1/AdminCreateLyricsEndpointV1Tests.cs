using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;

/// <summary>
/// Integration tests for the AdminCreateLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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
    public async Task CreateLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        string slug = $"test-lyrics-{Guid.NewGuid().ToString("N")[..8]}";
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug(slug)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateLyricsResponse>();
        body.Lyrics.Id.Should().NotBeEmpty();
        body.Lyrics.SongTitle.Should().Be(request.SongTitle);
        body.Lyrics.ArtistName.Should().Be(request.ArtistName);
        body.Lyrics.Slug.Should().Be(request.Slug);
        body.Lyrics.CategoryId.Should().Be(category.Id);
        body.Lyrics.Language.Should().Be(request.Language);
        body.Lyrics.Status.Should().Be(EnumContentStatus.Draft);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(body.Lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.SongTitle.Should().Be(request.SongTitle);
        persisted.ArtistName.Should().Be(request.ArtistName);
        persisted.Slug.Should().Be(request.Slug);
        persisted.Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithNonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(Guid.NewGuid())
            .WithSlug("non-existent-category-lyrics")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithNonExistentVideo_ReturnsNotFound()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug($"non-existent-video-lyrics-{Guid.NewGuid().ToString("N")[..8]}")
            .WithVideoId(Guid.NewGuid())
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        (await ctx.Lyrics.FirstOrDefaultAsync(l => l.Slug == request.Slug)).Should().BeNull();
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithEmptySongTitle_ReturnsValidationError()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSongTitle(string.Empty)
            .WithSlug("empty-song-title-lyrics")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("SongTitle", Localized<LyricsErrorMessage>(m => m.SongTitleRequired()))
        );
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithEmptyLyricsText_ReturnsValidationError()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(category.Id)
            .WithLyricsText(string.Empty)
            .WithSlug("empty-lyrics-text-lyrics")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("LyricsText", Localized<LyricsErrorMessage>(m => m.LyricsTextRequired()))
        );
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithDuplicateSlug_ReturnsConflict()
    {
        LyricsEntity existingLyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(existingLyrics.CategoryId)
            .WithSlug(existingLyrics.Slug)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<LyricsErrorMessage>(m => m.SlugAlreadyExists(existingLyrics.Slug))
        );
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_WithPaidLyrics_ReturnsCreated()
    {
        Guid categoryId = Guid.Empty;
        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            CustomerEntity c = CustomerFactory.Create();
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Customers.Add(c);
            categoryId = category.Id;
            return c;
        });

        Client.AuthenticateAsSuperAdmin();
        string slug = $"paid-lyrics-{Guid.NewGuid().ToString("N")[..8]}";
        Guid orderItemId = Guid.NewGuid();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(categoryId)
            .WithSlug(slug)
            .WithCustomerId(customer.Id)
            .WithOrderItemId(orderItemId)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateLyricsResponse>();
        body.Lyrics.Id.Should().NotBeEmpty();
        body.Lyrics.CustomerId.Should().Be(customer.Id);
        body.Lyrics.OrderItemId.Should().Be(orderItemId);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(body.Lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(customer.Id);
        persisted.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public async Task CreateLyrics_AsSuperAdmin_LinkedToVideo_MarksVideoAsHavingLyrics()
    {
        Guid categoryId = Guid.Empty;
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity parentVideo = VideoFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(parentVideo);
            categoryId = category.Id;
            return parentVideo;
        });

        video.HasLyrics.Should().BeFalse();

        Client.AuthenticateAsSuperAdmin();
        AdminCreateLyricsRequest request = new AdminCreateLyricsRequestBuilder()
            .WithCategoryId(categoryId)
            .WithSlug($"video-linked-lyrics-{Guid.NewGuid().ToString("N")[..8]}")
            .WithVideoId(video.Id)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Lyrics, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateLyricsResponse>();
        body.Lyrics.VideoId.Should().Be(video.Id);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        VideoEntity? persistedVideo = await ctx.Videos.FindAsync(video.Id);
        persistedVideo.Should().NotBeNull();
        persistedVideo!.HasLyrics.Should().BeTrue();
    }
}
