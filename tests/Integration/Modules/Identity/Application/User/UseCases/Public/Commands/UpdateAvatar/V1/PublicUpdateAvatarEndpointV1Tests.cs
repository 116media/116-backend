using System.Net.Http.Headers;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.V1;

/// <summary>
/// Integration tests for the PublicUpdateAvatar endpoint.
/// </summary>
[Collection("Database")]
public class PublicUpdateAvatarEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task UpdateAvatar_AsVisitor_WithValidSession_UpdatesAvatar()
    {
        var sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
        });

        Client.AuthenticateAs(TestUser.VisitorId, "Visitor", sessionId);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "avatarFile", "avatar.jpg");

        var response = await Client.PatchAsync(Routes.Public.Me.Avatar(), content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicUpdateAvatarResponse body = await response.ReadAsAsync<PublicUpdateAvatarResponse>();
        body.User.Id.Should().Be(TestUser.VisitorId);
        body.User.Avatar.Should().NotBeNull();
        body.User.Avatar!.StorageUrl.Should().Contain("res.cloudinary.com/test-cloud");

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity? user = await verifyContext.Users.FindAsync(TestUser.VisitorId);
        user!.AvatarFileId.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAvatar_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "avatar.jpg");

        var response = await Client.PatchAsync(Routes.Public.Me.Avatar(), content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAvatar_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "avatar.jpg");

        var response = await Client.PatchAsync(Routes.Public.Me.Avatar(), content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAvatar_WithInvalidFileFormat_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0x4D, 0x5A }), "avatarFile", "malicious.exe");

        var response = await Client.PatchAsync(Routes.Public.Me.Avatar(), content);

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
    public async Task UpdateAvatar_WithNoFilePart_ReturnsLocalizedValidationProblem()
    {
        Client.AuthenticateAsVisitor();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("unused"), "note");

        var response = await Client.PatchAsync(Routes.Public.Me.Avatar(), content);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("AvatarFile", Localized<ValidationErrorMessage>(m => m.AvatarFileRequired()))
        );
    }
}
