using System.Text.Json;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;

/// <summary>
/// Integration tests for the PublicChangePassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicChangePasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string KnownPassword = "Test123!abc";
    private const string ChangePasswordUrl = $"{ApiRoutes.Public.Auth}/change-password";

    [Fact]
    public async Task ChangePassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { OldPassword = "Old123!abc", NewPassword = "New123!abc" };

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { OldPassword = "Old123!abc", NewPassword = "New123!abc" };

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that a Visitor user with a correct current password can successfully change their password.
    /// </summary>
    [Fact]
    public async Task ChangePassword_AsVisitor_WithCorrectPassword_ReturnsOk()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        string hashedPassword = passwordService.Hash(KnownPassword);
        var errors = TestErrorsFactory.CreateUserErrors();

        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var userId = Guid.NewGuid();
        var user = UserFactory.CreateWithId(userId, $"chg-ok-{userId:N}@test.com");
        user.MarkAsVerified();
        user.Activate();
        user.UpdatePassword(hashedPassword, errors);

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, userId);

        seedContext.Users.Add(user);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(userId, "Visitor", sessionId);

        var request = new { OldPassword = KnownPassword, NewPassword = "NewPass123!abc" };

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that providing an incorrect current password returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_ReturnsBadRequest()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        string hashedPassword = passwordService.Hash(KnownPassword);
        var errors = TestErrorsFactory.CreateUserErrors();

        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var userId = Guid.NewGuid();
        var user = UserFactory.CreateWithId(userId, $"chg-wrong-{userId:N}@test.com");
        user.MarkAsVerified();
        user.Activate();
        user.UpdatePassword(hashedPassword, errors);

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, userId);

        seedContext.Users.Add(user);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(userId, "Visitor", sessionId);

        var request = new { OldPassword = "WrongPassword123!", NewPassword = "NewPass123!abc" };

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that setting the new password to the same value as the old password returns a 409 Conflict.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WithSameAsOldPassword_ReturnsConflict()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        string hashedPassword = passwordService.Hash(KnownPassword);
        var errors = TestErrorsFactory.CreateUserErrors();

        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var userId = Guid.NewGuid();
        var user = UserFactory.CreateWithId(userId, $"chg-same-{userId:N}@test.com");
        user.MarkAsVerified();
        user.Activate();
        user.UpdatePassword(hashedPassword, errors);

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, userId);

        seedContext.Users.Add(user);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(userId, "Visitor", sessionId);

        var request = new { OldPassword = KnownPassword, NewPassword = KnownPassword };

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that changing password on an account with no password hash (social-only) returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task ChangePassword_WithSocialOnlyAccount_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var socialUser = UserFactory.CreateExternal(EnumAuthProvider.Google);
        socialUser.Activate();
        var socialUserId = socialUser.Id;

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, socialUserId);

        seedContext.Users.Add(socialUser);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(socialUserId, "Visitor", sessionId);

        var request = new { OldPassword = "AnyPass123!", NewPassword = "NewPass123!abc" };

        var response = await Client.PatchAsJsonAsync(ChangePasswordUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
