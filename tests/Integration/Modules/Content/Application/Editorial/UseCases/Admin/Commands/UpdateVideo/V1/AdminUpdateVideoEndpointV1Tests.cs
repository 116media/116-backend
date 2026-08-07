using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;

/// <summary>
/// Integration tests for the AdminUpdateVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private async Task<(CategoryEntity Category, VideoEntity Video)> SeedVideoAsync(Func<Guid, VideoEntity> create)
    {
        CategoryEntity? seededCategory = null;
        VideoEntity video = await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            seededCategory = category;
            VideoEntity v = create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(v);
            return v;
        });
        return (seededCategory!, video);
    }

    [Fact]
    public async Task UpdateVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task UpdateVideo_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        (CategoryEntity category, VideoEntity video) = await SeedVideoAsync(categoryId =>
            VideoFactory.Create(categoryId)
        );
        Client.AuthenticateAsSuperAdmin();
        string slug = $"updated-video-{Guid.NewGuid():N}"[..20];
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder()
            .WithCategoryId(category.Id)
            .WithTitle("Updated Video Title")
            .WithSlug(slug)
            .WithDescription("An updated video description for integration testing.")
            .WithSocialBoost(true)
            .WithMetaTitle("Updated meta title")
            .WithMetaDescription("An updated meta description that comfortably exceeds the minimum length requirement.")
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{video.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateVideoResponse body = await response.ReadAsAsync<AdminUpdateVideoResponse>();
        body.Video.Id.Should().Be(video.Id);
        body.Video.Title.Should().Be(request.Title);
        body.Video.Slug.Should().Be(slug);
        body.Video.SocialBoost.Should().BeTrue();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await verifyContext.Videos.FindAsync(video.Id);
        persisted!.Title.Should().Be(request.Title);
        persisted.Slug.Should().Be(slug);
        persisted.Description.Should().Be(request.Description);
        persisted.SocialBoost.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateVideo_WithTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder().WithTitle(new string('V', 200)).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Title",
                Localized<VideoErrorMessage>(m => m.TitleTooLong(ContentConstants.MaxTitleLength))
            )
        );
    }

    [Fact]
    public async Task UpdateVideo_WithInvalidSlug_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder().WithSlug("INVALID SLUG!!!").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<VideoErrorMessage>(m => m.SlugInvalidFormat()))
        );
    }

    [Fact]
    public async Task UpdateVideo_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder().WithSlug(new string('a', 300)).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<VideoErrorMessage>(m => m.SlugTooLong(ContentConstants.MaxSlugLength)))
        );
    }

    [Fact]
    public async Task UpdateVideo_WithEmptyDescription_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder().WithDescription(string.Empty).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Description", Localized<VideoErrorMessage>(m => m.DescriptionRequired()))
        );
    }

    [Fact]
    public async Task UpdateVideo_WithMetaTitleTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder().WithMetaTitle("Short").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "MetaTitle",
                Localized<ArticleErrorMessage>(m => m.MetaTitleTooShort(ContentConstants.MinMetaTitleLength))
            )
        );
    }

    [Fact]
    public async Task UpdateVideo_WithMetaDescriptionTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        AdminUpdateVideoRequest request = new AdminUpdateVideoRequestBuilder().WithMetaDescription("Too short").Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "MetaDescription",
                Localized<ArticleErrorMessage>(m =>
                    m.MetaDescriptionTooShort(ContentConstants.MinMetaDescriptionLength)
                )
            )
        );
    }
}
