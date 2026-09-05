using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;

/// <summary>
/// Integration tests for the PublicChangePassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicChangePasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string KnownPassword = TestAuth.ValidPassword;

    [Fact]
    public async Task ChangePassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword(TestAuth.OldPassword)
            .WithNewPassword(TestAuth.NewPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword(TestAuth.OldPassword)
            .WithNewPassword(TestAuth.NewPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

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
        user.InitializePasswordHash(hashedPassword);

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, userId);

        seedContext.Users.Add(user);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(userId, "Visitor", sessionId);

        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword(KnownPassword)
            .WithNewPassword(TestAuth.ChangedPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicChangePasswordResponse body = await response.ReadAsAsync<PublicChangePasswordResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var updated = await verifyContext.Users.FirstAsync(u => u.Id == userId);
        updated.PasswordHash.Should().NotBe(hashedPassword);
        passwordService.Verify(request.NewPassword, updated.PasswordHash).Should().BeTrue();
    }

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
        user.InitializePasswordHash(hashedPassword);

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, userId);

        seedContext.Users.Add(user);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(userId, "Visitor", sessionId);

        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword(TestAuth.IncorrectCurrentPassword)
            .WithNewPassword(TestAuth.ChangedPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.IncorrectCurrentPassword())
        );
    }

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
        user.InitializePasswordHash(hashedPassword);

        var sessionId = Guid.NewGuid();
        var session = SessionFactory.CreateWithId(sessionId, userId);

        seedContext.Users.Add(user);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(userId, "Visitor", sessionId);

        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword(KnownPassword)
            .WithNewPassword(KnownPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ValidationErrorMessage>(m => m.NewPasswordSameAsOld())
        );
    }

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

        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword(TestAuth.SocialAccountPassword)
            .WithNewPassword(TestAuth.ChangedPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.PasswordNotConfigured(EnumAuthProvider.Google))
        );
    }
}
