using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;

/// <summary>
/// Integration tests for the PublicSignUp endpoint.
/// </summary>
[Collection("Database")]
public class PublicSignUpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignUp_WithValidData_ReturnsCreated()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        context.Roles.Add(visitorRole);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        var request = new PublicSignUpRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        PublicSignUpMobileResponse body = await response.ReadAsAsync<PublicSignUpMobileResponse>();
        body.User.Id.Should().NotBeEmpty();
        body.User.Email.Should().Be(request.Email);
        body.User.UserName.Should().Be(request.UserName);
        body.TokenType.Should().Be("Bearer");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.VerificationRequired.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var created = await verifyContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        created.Should().NotBeNull();
        created!.Id.Should().Be(body.User.Id);
        created.UserName.Should().Be(request.UserName);
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_ReturnsConflict()
    {
        Client.ClearAuthentication();
        var request = new PublicSignUpRequestBuilder().WithEmail(TestUser.SuperAdminEmail).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that a fresh email paired with a taken username returns the
    /// <c>UsernameAlreadyExists</c> 409. The email uniqueness check in
    /// <c>AuthRepository.EnsureUserDoesNotExistAsync</c> runs first, so a novel email is required
    /// for the username branch to be the one that fires.
    /// </summary>
    [Fact]
    public async Task SignUp_WithDuplicateUserName_ReturnsConflict()
    {
        var takenUserName = $"taken{Guid.NewGuid():N}"[..12];
        await SeedAsync<IdentityDbContext>(context =>
            context.Users.Add(UserFactory.Create($"holder-{Guid.NewGuid():N}@test.com", takenUserName))
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");
        var request = new PublicSignUpRequestBuilder()
            .WithEmail($"fresh-{Guid.NewGuid():N}@test.com")
            .WithUserName(takenUserName)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict, $"Username '{takenUserName}' is already taken.");

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Users.CountAsync(u => u.UserName == takenUserName)).Should().Be(1);
    }

    [Fact]
    public async Task SignUp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicSignUpRequestBuilder().WithEmail(string.Empty).WithUserName("validuser").Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SignUp_WithWeakPassword_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicSignUpRequestBuilder().WithPassword("abc").Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SignUp_WithShortUsername_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicSignUpRequestBuilder().WithUserName("ab").Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignUp(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
