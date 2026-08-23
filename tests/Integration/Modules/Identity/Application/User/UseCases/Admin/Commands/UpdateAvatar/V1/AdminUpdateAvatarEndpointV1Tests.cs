using System.Net.Http.Headers;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.User.Constants;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.V1;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.V1;

/// <summary>
/// Integration tests for the AdminUpdateAvatar endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateAvatarEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/{IdentityConstants.Me}/{UserRouteConstants.Avatar}";

    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task AdminUpdateAvatar_AsSuperAdmin_WithValidSession_UpdatesAvatar()
    {
        var sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.SuperAdminId));
        });

        Client.AuthenticateAs(TestUser.SuperAdminId, "SuperAdmin", sessionId);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "avatarFile", "avatar.jpg");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateAvatarResponse body = await response.ReadAsAsync<AdminUpdateAvatarResponse>();
        body.User.Id.Should().Be(TestUser.SuperAdminId);
        body.User.Avatar.Should().NotBeNull();
        body.User.Avatar!.StorageUrl.Should().Contain("res.cloudinary.com/test-cloud");

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity? user = await verifyContext.Users.FindAsync(TestUser.SuperAdminId);
        user!.AvatarFileId.Should().NotBeNull();
    }

    [Fact]
    public async Task AdminUpdateAvatar_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "avatar.jpg");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminUpdateAvatar_WithInvalidExtension_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "avatarFile", "avatar.bmp");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "AvatarFile",
                Localized<ValidationErrorMessage>(m =>
                    m.AvatarFileInvalidExtension(string.Join(", ", FileConstants.AllowedAvatarExtensions))
                )
            )
        );
    }

    [Fact]
    public async Task AdminUpdateAvatar_WithInvalidMimeType_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "avatarFile", "document.pdf");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "AvatarFile",
                Localized<ValidationErrorMessage>(m =>
                    m.AvatarFileInvalidType(string.Join(", ", FileConstants.AllowedAvatarMimeTypes))
                )
            )
        );
    }

    [Fact]
    public async Task AdminUpdateAvatar_WithNoFilePart_ReturnsLocalizedValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("unused"), "note");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("AvatarFile", Localized<ValidationErrorMessage>(m => m.AvatarFileRequired()))
        );
    }
}
