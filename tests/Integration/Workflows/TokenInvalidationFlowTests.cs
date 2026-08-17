using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end flows for the token-invalidation layer: session denylisting, token-version bumps,
/// security-stamp rotation, and the hardened refresh checks.
/// </summary>
[Collection("Database")]
public class TokenInvalidationFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    private const string RefreshTokenUrl =
        $"{ApiRoutes.Public.Base}/{SessionRouteConstants.Endpoint}/{SessionRouteConstants.RefreshToken}";

    /// <summary>
    /// Hashes a raw refresh token the way the application stores it.
    /// </summary>
    /// <param name="rawToken">The raw refresh token.</param>
    /// <returns>The Base64-encoded SHA-256 hash.</returns>
    private static string HashToken(string rawToken)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }

    [Fact]
    public async Task SignOut_OverRealHttp_RejectsTheStillUnexpiredAccessToken()
    {
        // Arrange — a session whose id rides the access token's ref claim
        var rawRefreshToken = $"invalidation-signout-{Guid.NewGuid():N}";
        var session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.CreateWithRefreshTokenHash(
                TestUser.VisitorId,
                HashToken(rawRefreshToken)
            );
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAs(TestUser.VisitorId, "Visitor", session.Id);

        HttpResponseMessage before = await Client.GetAsync(Routes.Public.Me.Roles());
        before.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act — sign out the session, then replay the same still-unexpired access token
        var request = new PublicSignOutRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        HttpResponseMessage signOut = await Client.PostAsJsonAsync(Routes.Public.Auth.SignOut(), request);
        signOut.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage after = await Client.GetAsync(Routes.Public.Me.Roles());

        // Assert — the denylist rejects the token within the request, not at its natural expiry
        after.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PermissionRevocation_OverRealHttp_InvalidatesTheTokenAndRefreshRecovers()
    {
        // Arrange — the visitor holds a role with a permission, and owns a live session
        var rawRefreshToken = $"invalidation-tver-{Guid.NewGuid():N}";
        var (role, permission) = await SeedAsync<IdentityDbContext, (RoleEntity, PermissionEntity)>(ctx =>
        {
            RoleEntity createdRole = RoleFactory.Create();
            PermissionEntity createdPermission = PermissionFactory.Create();
            ctx.Roles.Add(createdRole);
            ctx.Permissions.Add(createdPermission);
            ctx.RolePermissions.Add(RolePermissionFactory.Create(createdRole.Id, createdPermission.Id));
            ctx.UserRoles.Add(UserRoleFactory.Create(TestUser.VisitorId, createdRole.Id));
            ctx.Sessions.Add(SessionFactory.CreateWithRefreshTokenHash(TestUser.VisitorId, HashToken(rawRefreshToken)));
            return (createdRole, createdPermission);
        });

        Client.AuthenticateAsVisitor();
        HttpResponseMessage before = await Client.GetAsync(Routes.Public.Me.Roles());
        before.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act — an admin revokes the permission from the role, bumping every member's version
        Client.AuthenticateAsSuperAdmin();
        HttpResponseMessage removal = await Client.DeleteAsync(Routes.Admin.Roles.Permission(role.Id, permission.Id));
        removal.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.AuthenticateAsVisitor();
        HttpResponseMessage stale = await Client.GetAsync(Routes.Public.Me.Roles());

        // Assert — the outstanding token is stale, and the row records the bump
        stale.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await using (IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>())
        {
            UserTokenStateEntity tokenState = await identityContext.UserTokenStates.SingleAsync(s =>
                s.Id == TestUser.VisitorId
            );
            tokenState.TokenVersion.Should().Be(1);
        }

        // Assert — the session is intact, so a silent refresh mints a working token again
        Client.ClearAuthentication();
        var refreshRequest = new PublicRefreshTokenRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        HttpResponseMessage refresh = await Client.PostAsJsonAsync(RefreshTokenUrl, refreshRequest);
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicRefreshTokenMobileResponse refreshed = await refresh.ReadAsAsync<PublicRefreshTokenMobileResponse>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);

        HttpResponseMessage recovered = await Client.GetAsync(Routes.Public.Me.Roles());
        recovered.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PasswordReset_OverRealHttp_RejectsTheTokenAndTheRefresh()
    {
        // Arrange — a verified active user with a live session and a usable reset code
        var userId = Guid.NewGuid();
        string email = $"invalidation-sstamp-{userId:N}@test.com";
        var rawRefreshToken = $"invalidation-sstamp-{Guid.NewGuid():N}";

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            var user = UserFactory.CreateWithId(userId, email);
            user.MarkAsVerified();
            user.Activate();

            ctx.Users.Add(user);
            ctx.Otps.Add(OtpFactory.CreateUsed(userId, Otp.ValidCode, EnumOtpPurpose.PasswordReset));
            ctx.Sessions.Add(SessionFactory.CreateWithRefreshTokenHash(userId, HashToken(rawRefreshToken)));
        });

        // Act — reset the password, which rotates the stamp and revokes the sessions
        Client.ClearAuthentication();
        var resetRequest = new PublicResetPasswordRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithNewPassword(TestAuth.ResetNewPassword)
            .Build();
        HttpResponseMessage reset = await Client.PostAsJsonAsync(Routes.Public.Auth.ResetPassword(), resetRequest);
        reset.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — the outstanding token (minted against the old stamp) is rejected
        Client.AuthenticateAs(userId, "Visitor");
        HttpResponseMessage stale = await Client.GetAsync(Routes.Public.Me.Roles());
        stale.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Assert — the refresh is rejected too: the reset means a forced re-login
        Client.ClearAuthentication();
        var refreshRequest = new PublicRefreshTokenRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        HttpResponseMessage refresh = await Client.PostAsJsonAsync(RefreshTokenUrl, refreshRequest);
        await refresh.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        UserTokenStateEntity tokenState = await identityContext.UserTokenStates.SingleAsync(s => s.Id == userId);
        tokenState.SecurityStamp.Should().NotBe(Jwt.WellKnownSecurityStamp);
    }

    [Fact]
    public async Task Request_WithATokenMissingTheSecurityMarkers_IsRejected()
    {
        // Arrange — a correctly signed token minted before invalidation markers shipped
        Client.AuthenticateWithoutSecurityMarkers(TestUser.VisitorId, "Visitor");

        // Act
        HttpResponseMessage response = await Client.GetAsync(Routes.Public.Me.Roles());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WithAForeignSecurityStamp_IsRejected()
    {
        // Arrange — a correctly signed token whose stamp does not match the user's current one
        Client.AuthenticateWithSecurityMarkers(TestUser.VisitorId, "Visitor", securityStamp: Guid.NewGuid());

        // Act
        HttpResponseMessage response = await Client.GetAsync(Routes.Public.Me.Roles());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Interaction_WithAnInactiveOrUnverifiedAccountClaim_IsForbidden()
    {
        // Arrange & Act — authorization runs before the handler, so no article needs to exist
        Client.AuthenticateWithAccountFlags(TestUser.VisitorId, "Visitor", isActive: false, isVerified: true);
        HttpResponseMessage inactive = await Client.PostAsync(Routes.Public.Articles.Likes(Guid.NewGuid()), null);

        Client.AuthenticateWithAccountFlags(TestUser.VisitorId, "Visitor", isActive: true, isVerified: false);
        HttpResponseMessage unverified = await Client.PostAsync(Routes.Public.Articles.Likes(Guid.NewGuid()), null);

        // Assert
        inactive.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        unverified.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RefreshToken_ForADeactivatedAccount_IsRefusedAndRevokesTheSession()
    {
        // Arrange — a deactivated account still holding a live refresh token
        var userId = Guid.NewGuid();
        var rawRefreshToken = $"invalidation-deactivated-{Guid.NewGuid():N}";

        var session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            var user = UserFactory.CreateWithId(userId, $"invalidation-deactivated-{userId:N}@test.com");
            user.MarkAsVerified();
            user.Deactivate();
            ctx.Users.Add(user);

            SessionEntity entity = SessionFactory.CreateWithRefreshTokenHash(userId, HashToken(rawRefreshToken));
            ctx.Sessions.Add(entity);
            return entity;
        });

        // Act
        Client.ClearAuthentication();
        var request = new PublicRefreshTokenRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        HttpResponseMessage response = await Client.PostAsJsonAsync(RefreshTokenUrl, request);

        // Assert — refused, and the credential dies with the attempt
        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity revoked = await identityContext.Sessions.SingleAsync(s => s.Id == session.Id);
        revoked.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshToken_PastTheAbsoluteLifetime_IsRefusedAndRevokesTheSession()
    {
        // Arrange — a session whose sliding expiry is fine but whose absolute ceiling has passed
        var rawRefreshToken = $"invalidation-absolute-{Guid.NewGuid():N}";

        var session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = new SessionBuilder()
                .WithUserId(TestUser.VisitorId)
                .WithRefreshTokenHash(HashToken(rawRefreshToken))
                .WithAbsoluteExpiresAt(DateTime.UtcNow.AddMinutes(-5))
                .Build();
            ctx.Sessions.Add(entity);
            return entity;
        });

        // Act
        Client.ClearAuthentication();
        var request = new PublicRefreshTokenRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        HttpResponseMessage response = await Client.PostAsJsonAsync(RefreshTokenUrl, request);

        // Assert — the session cannot be slid past its ceiling and is closed for good
        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity revoked = await identityContext.Sessions.SingleAsync(s => s.Id == session.Id);
        revoked.IsRevoked.Should().BeTrue();
    }
}
