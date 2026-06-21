namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;

/// <summary>
/// Integration tests for the AdminUpdateVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{nonExistentId}", new { });

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that updating a video with a title exceeding the maximum allowed length
    /// (100 characters) returns a 400 Bad Request response from the validator, exercising
    /// the <c>isRequired=false</c> branch of <c>ValidVideoTitle</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateVideo_WithTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = new string('V', 200),
            Slug = "valid-slug",
            Description = "Some description",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a video with a slug containing spaces and special characters
    /// returns a 400 Bad Request response from the validator, exercising the slug regex
    /// validation in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateVideo_WithInvalidSlug_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "INVALID SLUG!!!",
            Description = "Some description",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a video with a slug exceeding the maximum allowed length
    /// (220 characters) returns a 400 Bad Request response from the validator, exercising
    /// the MaximumLength branch of <c>ValidVideoSlug</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateVideo_WithSlugTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = new string('a', 300),
            Description = "Some description",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a video with an empty description returns a 400 Bad Request
    /// response from the validator, exercising the <c>ValidVideoDescription</c> branch
    /// in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateVideo_WithEmptyDescription_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Description = "",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a video with a meta title shorter than the minimum allowed
    /// length (10 characters) returns a 400 Bad Request response from the validator,
    /// exercising the MinimumLength branch of <c>ValidMetaTitle</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateVideo_WithMetaTitleTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Description = "Some description",
            MetaTitle = "Short",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that updating a video with a meta description shorter than the minimum
    /// allowed length (50 characters) returns a 400 Bad Request response from the validator,
    /// exercising the MinimumLength branch of <c>ValidMetaDescription</c> in EditorialValidation.
    /// </summary>
    [Fact]
    public async Task UpdateVideo_WithMetaDescriptionTooShort_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();
        var request = new
        {
            CategoryId = Guid.NewGuid(),
            Title = "Valid Title",
            Slug = "valid-slug",
            Description = "Some description",
            MetaDescription = "Too short",
            SocialBoost = false,
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Videos}/{id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
