using System.IdentityModel.Tokens.Jwt;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;

/// <summary>
/// Integration tests for the PublicLogin endpoint.
/// </summary>
[Collection("Database")]
public class PublicLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(params (string Property, string Message)[] failures) =>
        new ValidationException(failures.Select(f => new ValidationFailure(f.Property, f.Message))).Message;

    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicLoginRequestBuilder().WithCredentials(string.Empty).WithPassword(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                ("Credentials", Localized<ValidationErrorMessage>(m => m.EmailOrUsernameRequired())),
                ("Password", Localized<ValidationErrorMessage>(m => m.PasswordRequired()))
            )
        );
    }

    [Fact]
    public async Task Login_WithNonExistentCredentials_ReturnsInvalidCredentialsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new PublicLoginRequestBuilder()
            .WithCredentials("nobody@nowhere.com")
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);

        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials())
        );
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokensAndUser()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"login-ok-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicLoginMobileResponse body = await response.ReadAsAsync<PublicLoginMobileResponse>();
        body.TokenType.Should().Be("Bearer");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        body.RefreshTokenExpiresAt.Should().BeAfter(body.AccessTokenExpiresAt);
        body.User.Email.Should().Be(email);
        body.User.IsActive.Should().BeTrue();
        body.User.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WhenTheRoleCarriesPermissions_StampsThemOnTheAccessToken()
    {
        // Arrange — the Visitor role signup assigns carries two permissions
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        PermissionEntity readArticles = PermissionFactory.Create("articles", "read");
        PermissionEntity createLikes = PermissionFactory.Create("likes", "create");

        seedContext.Roles.Add(visitorRole);
        seedContext.Permissions.AddRange(readArticles, createLikes);
        seedContext.RolePermissions.Add(RolePermissionFactory.Create(visitorRole.Id, readArticles.Id));
        seedContext.RolePermissions.Add(RolePermissionFactory.Create(visitorRole.Id, createLikes.Id));
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"login-perms-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        // Assert — the minted token carries one claim per granted permission
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicLoginMobileResponse body = await response.ReadAsAsync<PublicLoginMobileResponse>();
        JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(body.AccessToken);

        string[] permissions = token
            .Claims.Where(claim => claim.Type == JwtClaimsConstants.Permissions)
            .Select(claim => claim.Value)
            .ToArray();

        permissions.Should().BeEquivalentTo("articles:read", "likes:create");
    }

    [Fact]
    public async Task Login_WithInactiveAccount_ReturnsLocked()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"inactive-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Deactivate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        await response.ShouldBeProblem<AccountInactiveException>(
            HttpStatusCode.Locked,
            Localized<AuthorizationErrorMessage>(m => m.AccountInactive(email))
        );
    }

    [Fact]
    public async Task Login_WithoutDeviceIdHeader_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"nodevice-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await updateContext.SaveChangesAsync();

        Client.DefaultRequestHeaders.Remove("X-Device-Id");

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<AuthenticationErrorMessage>(m => m.DeviceIdRequired())
        );
    }

    [Fact]
    public async Task Login_WithUnverifiedAccount_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"unverified-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);

        await using var updateContext = CreateDbContext<IdentityDbContext>();
        var user = await updateContext.Users.FirstAsync(u => u.Email == email);
        user.Activate();
        await updateContext.SaveChangesAsync();

        var loginRequest = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), loginRequest);

        await response.ShouldBeProblem<AccountNotVerifiedException>(
            HttpStatusCode.Forbidden,
            Localized<AuthorizationErrorMessage>(m => m.AccountNotVerified(email))
        );
    }

    [Fact]
    public async Task Login_WithKnownEmailAndWrongPassword_ReturnsInvalidCredentialsUnauthorized()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        UserErrors errors = TestErrorsFactory.CreateUserErrors();

        var email = $"wrong-password-{Guid.NewGuid():N}@test.com";
        var user = UserFactory.Create(email);
        user.MarkAsVerified();
        user.Activate();
        user.InitializePasswordHash(passwordService.Hash(TestAuth.ValidPassword));

        await SeedAsync<IdentityDbContext>(context => context.Users.Add(user));

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword($"{TestAuth.ValidPassword}-not-it")
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);

        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials(), LocalizedMessage.EnglishCulture)
        );

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Detail.Should().NotContain("user account");
    }

    [Fact]
    public async Task Login_WithNonExistentCredentials_ReturnsFriendlyDetailWithoutLeakingEmail()
    {
        Client.ClearAuthentication();
        const string email = "ghost-en@nowhere.com";
        var request = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Auth.Login())
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Add("Accept-Language", "en");

        var response = await Client.SendAsync(httpRequest);

        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials(), LocalizedMessage.EnglishCulture)
        );

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Detail.Should().NotContain(email);
        problem.Detail.Should().NotContain("User with");
    }

    [Fact]
    public async Task Login_WithNonExistentCredentials_InFrench_ReturnsLocalizedFriendlyDetail()
    {
        Client.ClearAuthentication();
        const string email = "fantome-fr@nowhere.com";
        var request = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, Routes.Public.Auth.Login())
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Add("Accept-Language", "fr");

        var response = await Client.SendAsync(httpRequest);

        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials())
        );

        ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
        problem.Detail.Should().NotContain(email);
    }
}
