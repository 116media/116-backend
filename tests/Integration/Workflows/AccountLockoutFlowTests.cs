using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end flows for the per-account brute-force counters: repeated wrong passwords lock the
/// account, and the lock outlives a correct password until it expires.
/// </summary>
[Collection("Database")]
public class AccountLockoutFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Registers a verified, active account through the real signup endpoint so its password hash
    /// is written by the application rather than a fixture.
    /// </summary>
    /// <returns>The email the account was created with.</returns>
    private async Task<string> RegisterVerifiedAccountAsync()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"))
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Remove("X-Device-Id");
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"lockout-{Guid.NewGuid():N}@test.com";
        var signupRequest = new PublicSignUpRequestBuilder()
            .WithEmail(email)
            .WithUserName($"u{Guid.NewGuid():N}"[..10])
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        HttpResponseMessage signUp = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), signupRequest);
        signUp.StatusCode.Should().Be(HttpStatusCode.Created);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        UserEntity user = await context.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await context.SaveChangesAsync();

        return email;
    }

    /// <summary>
    /// Attempts a login and returns the response, leaving status assertions to the caller.
    /// </summary>
    /// <param name="email">The account to authenticate as.</param>
    /// <param name="password">The password to present.</param>
    /// <returns>The login response.</returns>
    private async Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        var request = new PublicLoginRequestBuilder().WithCredentials(email).WithPassword(password).Build();
        return await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);
    }

    [Fact]
    public async Task Login_AfterTheAttemptCap_LocksTheAccountEvenForTheCorrectPassword()
    {
        // Arrange
        string email = await RegisterVerifiedAccountAsync();

        // Act — exhaust the allowance with wrong passwords
        for (int attempt = 0; attempt < UserConstants.MaxLoginAttempts; attempt++)
        {
            HttpResponseMessage failed = await LoginAsync(email, "WrongPassword123!x");
            failed.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Assert — the correct password is now refused too, which is the point of the lock
        HttpResponseMessage locked = await LoginAsync(email, TestAuth.ValidPassword);
        locked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        UserEntity user = await context.Users.FirstAsync(u => u.Email == email);
        user.FailedLoginAttempts.Should().BeGreaterThanOrEqualTo(UserConstants.MaxLoginAttempts);
        user.LockedUntil.Should().NotBeNull();
        user.LockedUntil.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithTheCorrectPassword_ClearsTheFailureCounter()
    {
        // Arrange — stay one attempt below the cap
        string email = await RegisterVerifiedAccountAsync();

        for (int attempt = 0; attempt < UserConstants.MaxLoginAttempts - 1; attempt++)
        {
            HttpResponseMessage failed = await LoginAsync(email, "WrongPassword123!x");
            failed.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Act
        HttpResponseMessage success = await LoginAsync(email, TestAuth.ValidPassword);

        // Assert
        success.StatusCode.Should().Be(HttpStatusCode.OK);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        UserEntity user = await context.Users.FirstAsync(u => u.Email == email);
        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task Login_WithAnUnknownAccount_AnswersExactlyLikeAWrongPassword()
    {
        // Arrange
        string email = await RegisterVerifiedAccountAsync();

        // Act
        HttpResponseMessage unknown = await LoginAsync($"absent-{Guid.NewGuid():N}@test.com", TestAuth.ValidPassword);
        HttpResponseMessage wrongPassword = await LoginAsync(email, "WrongPassword123!x");

        // Assert
        unknown.StatusCode.Should().Be(wrongPassword.StatusCode);

        ProblemDetails unknownProblem = await unknown.ReadAsAsync<ProblemDetails>();
        ProblemDetails wrongPasswordProblem = await wrongPassword.ReadAsAsync<ProblemDetails>();

        unknownProblem.Status.Should().Be(wrongPasswordProblem.Status);
        unknownProblem.Title.Should().Be(wrongPasswordProblem.Title);
        unknownProblem.Detail.Should().Be(wrongPasswordProblem.Detail);
    }
}
