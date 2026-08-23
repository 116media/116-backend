using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.V1;
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

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.V1;

/// <summary>
/// Integration tests for the AdminCreateVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
        CategoryEntity category = await SeedCategoryAsync();
        Client.AuthenticateAsSuperAdmin();
        string slug = $"test-video-{Guid.NewGuid():N}"[..18];
        AdminCreateVideoRequest request = new AdminCreateVideoRequestBuilder()
            .WithCategoryId(category.Id)
            .WithSlug(slug)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateVideoResponse body = await response.ReadAsAsync<AdminCreateVideoResponse>();
        body.Video.Id.Should().NotBeEmpty();
        body.Video.Title.Should().Be(request.Title);
        body.Video.Slug.Should().Be(slug);
        body.Video.CategoryId.Should().Be(category.Id);
        body.Video.Status.Should().Be(EnumContentStatus.Draft);

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await verifyContext.Videos.FindAsync(body.Video.Id);
        persisted.Should().NotBeNull();
        persisted!.Slug.Should().Be(slug);
        persisted.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task CreateVideo_AsSuperAdmin_WithNonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateVideoRequest request = new AdminCreateVideoRequestBuilder()
            .WithCategoryId(Guid.NewGuid())
            .WithSlug("non-existent-category-video")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task CreateVideo_AsSuperAdmin_WithEmptyTitle_ReturnsValidationError()
    {
        CategoryEntity category = await SeedCategoryAsync();
        Client.AuthenticateAsSuperAdmin();
        AdminCreateVideoRequest request = new AdminCreateVideoRequestBuilder()
            .WithCategoryId(category.Id)
            .WithTitle(string.Empty)
            .WithSlug("empty-title-video")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Title", Localized<VideoErrorMessage>(m => m.TitleRequired()))
        );
    }

    [Fact]
    public async Task CreateVideo_AsSuperAdmin_WithDuplicateSlug_ReturnsConflict()
    {
        VideoEntity existingVideo = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = VideoFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);
            return video;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateVideoRequest request = new AdminCreateVideoRequestBuilder()
            .WithCategoryId(existingVideo.CategoryId)
            .WithSlug(existingVideo.Slug)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Videos, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<VideoErrorMessage>(m => m.SlugAlreadyExists(existingVideo.Slug))
        );
    }
}
