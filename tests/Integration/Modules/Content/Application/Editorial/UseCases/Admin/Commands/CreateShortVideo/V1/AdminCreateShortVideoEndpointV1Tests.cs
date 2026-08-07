using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminCreateShortVideo endpoint.
/// A short video is created as a JSON draft; the video file is uploaded separately.
/// </summary>
[Collection("Database")]
public class AdminCreateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private static object BuildRequest(string title, string slug, Guid? videoId = null) =>
        new
        {
            Title = title,
            Slug = slug,
            VideoId = videoId,
        };

    [Fact]
    public async Task CreateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Shorts,
            BuildRequest("Test Short", "test-short-slug")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Shorts,
            BuildRequest("Test Short", "test-short-slug")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateShortVideo_WithValidData_ReturnsCreatedAndPersistsDraft()
    {
        Client.AuthenticateAsSuperAdmin();
        string slug = $"test-short-{Guid.NewGuid():N}"[..20];

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Shorts, BuildRequest("Test Short", slug));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateShortVideoResponse body = await response.ReadAsAsync<AdminCreateShortVideoResponse>();
        body.ShortVideo.Id.Should().NotBeEmpty();
        body.ShortVideo.Title.Should().Be("Test Short");
        body.ShortVideo.Slug.Should().Be(slug);

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? persisted = await verifyContext.ShortVideos.FindAsync(body.ShortVideo.Id);
        persisted.Should().NotBeNull();
        persisted!.Slug.Should().Be(slug);
        persisted.VideoFileId.Should().BeNull();
        persisted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateShortVideo_WithEmptyTitle_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Shorts, BuildRequest(string.Empty, "valid-slug"));

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Title", Localized<ShortVideoErrorMessage>(m => m.TitleRequired()))
        );
    }

    [Fact]
    public async Task CreateShortVideo_WithEmptySlug_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Shorts, BuildRequest("Valid Title", string.Empty));

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<ShortVideoErrorMessage>(m => m.SlugRequired()))
        );
    }

    [Fact]
    public async Task CreateShortVideo_WithInvalidSlug_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Shorts,
            BuildRequest("Valid Title", "INVALID SLUG!!!")
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Slug", Localized<ShortVideoErrorMessage>(m => m.SlugInvalidFormat()))
        );
    }

    [Fact]
    public async Task CreateShortVideo_WithTitleTooLong_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Shorts,
            BuildRequest(new string('T', 300), "valid-slug")
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Title",
                Localized<ShortVideoErrorMessage>(m => m.TitleTooLong(ContentConstants.MaxShortVideoTitleLength))
            )
        );
    }

    [Fact]
    public async Task CreateShortVideo_WithSlugTooLong_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.Admin.Shorts,
            BuildRequest("Valid Title", new string('a', 300))
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Slug",
                Localized<ShortVideoErrorMessage>(m => m.SlugTooLong(ContentConstants.MaxSlugLength))
            )
        );
    }
}
