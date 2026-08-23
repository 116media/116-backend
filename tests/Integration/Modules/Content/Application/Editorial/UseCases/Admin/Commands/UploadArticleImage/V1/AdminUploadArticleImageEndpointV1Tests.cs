using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage.V1;

/// <summary>
/// Integration tests for the AdminUploadArticleImage endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadArticleImageEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task UploadArticleImage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Images(EditorialRouteConstants.Articles, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadArticleImage_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Images(EditorialRouteConstants.Articles, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadArticleImage_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Images(EditorialRouteConstants.Articles, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadArticleImage_AsSuperAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Editorial.Images(EditorialRouteConstants.Articles, Guid.NewGuid())}"
                + $"?imageType={EnumArticleImageType.Cover}",
            formContent
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task UploadArticleImage_WithInvalidGuid_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(
            $"{ApiRoutes.Admin.Articles}/not-a-guid/{EditorialRouteConstants.Images}"
                + $"?imageType={EnumArticleImageType.Cover}",
            formContent
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("ArticleId", Localized<ArticleErrorMessage>(m => m.Localizer["IdInvalid"].Value))
        );
    }

    [Fact]
    public async Task UploadArticleImage_WithNoFilePart_ReturnsLocalizedValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent("unused"), "note");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Editorial.Images(EditorialRouteConstants.Articles, id)}"
                + $"?imageType={EnumArticleImageType.Cover}",
            formContent
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("File", Localized<ArticleErrorMessage>(m => m.FileRequired()))
        );
    }

    [Fact]
    public async Task UploadArticleImage_AsSuperAdmin_WithCoverImage_ReturnsCreated()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity a = ArticleFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(a);
            return a;
        });

        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        formContent.Add(fileContent, "file", "cover.jpg");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Editorial.Images(EditorialRouteConstants.Articles, article.Id)}"
                + $"?imageType={EnumArticleImageType.Cover}",
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminUploadArticleImageResponse>();
        body.Image.Id.Should().NotBeEmpty();
        body.Image.ImageType.Should().Be(EnumArticleImageType.Cover);
        body.Image.Url.Should().Contain("res.cloudinary.com/test-cloud");

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleImageEntity? persisted = await ctx.ArticleImages.FindAsync(body.Image.Id);
        persisted.Should().NotBeNull();
        persisted!.ArticleId.Should().Be(article.Id);
        persisted.ImageType.Should().Be(EnumArticleImageType.Cover);
        persisted.Url.Should().Contain("res.cloudinary.com/test-cloud");
    }

    [Fact]
    public async Task UploadArticleImage_WithEmptyGuid_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        formContent.Add(fileContent, "file", "test.jpg");

        var response = await Client.PostAsync(
            $"{Routes.Admin.Editorial.Images(EditorialRouteConstants.Articles, Guid.Empty)}"
                + $"?imageType={EnumArticleImageType.Cover}",
            formContent
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }
}
