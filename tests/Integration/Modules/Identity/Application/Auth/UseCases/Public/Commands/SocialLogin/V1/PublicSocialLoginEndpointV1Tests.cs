using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;

/// <summary>
/// Integration tests for the PublicSocialLogin endpoint, with the provider verifier stubbed.
/// </summary>
[Collection("Database")]
public class PublicSocialLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private StubSocialTokenVerifier Verifier => Api.Services.GetRequiredService<StubSocialTokenVerifier>();

    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private static SocialTokenPayload Payload(string email, string subjectId, bool emailVerified = true) =>
        new(
            Email: email,
            Name: "Social User",
            ProviderSubjectId: subjectId,
            EmailVerified: emailVerified,
            PictureUrl: null
        );

    private async Task SeedVisitorRoleAsync()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"));
        await seedContext.SaveChangesAsync();
    }

    private PublicSocialLoginRequest GoogleRequest() =>
        new PublicSocialLoginRequestBuilder().WithProvider(nameof(EnumAuthProvider.Google)).Build();

    [Fact]
    public async Task SocialLogin_WithInvalidProvider_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicSocialLoginRequestBuilder().WithProvider("InvalidProvider").Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Provider", Localized<ValidationErrorMessage>(m => m.AuthProviderInvalid()))
        );
    }

    [Fact]
    public async Task SocialLogin_WithVerifiedToken_CreatesUserAndReturnsTokens()
    {
        await SeedVisitorRoleAsync();
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"social-{Guid.NewGuid():N}@test.com";
        Verifier.NextPayload = Payload(email, subjectId: $"sub-{Guid.NewGuid():N}");

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), GoogleRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicSocialLoginMobileResponse body = await response.ReadAsAsync<PublicSocialLoginMobileResponse>();
        body.User.Email.Should().Be(email);
        body.TokenType.Should().Be("Bearer");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var created = await verifyContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        created.Should().NotBeNull();
        created!.Id.Should().Be(body.User.Id);
        created.ProviderSubjectId.Should().Be(Verifier.NextPayload.ProviderSubjectId);
    }

    [Fact]
    public async Task SocialLogin_WithSameSubject_ReturnsSameUser()
    {
        await SeedVisitorRoleAsync();
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"social-{Guid.NewGuid():N}@test.com";
        var subjectId = $"sub-{Guid.NewGuid():N}";
        Verifier.NextPayload = Payload(email, subjectId);

        var first = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), GoogleRequest());
        PublicSocialLoginMobileResponse firstBody = await first.ReadAsAsync<PublicSocialLoginMobileResponse>();

        // Same subject id logs in again — must resolve to the same account.
        Verifier.NextPayload = Payload(email, subjectId);
        var second = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), GoogleRequest());
        PublicSocialLoginMobileResponse secondBody = await second.ReadAsAsync<PublicSocialLoginMobileResponse>();

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        secondBody.User.Id.Should().Be(firstBody.User.Id);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        int count = await verifyContext.Users.CountAsync(u => u.Email == email);
        count.Should().Be(1);
    }

    [Fact]
    public async Task SocialLogin_WithInvalidToken_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        // The provider could not verify the token — the verifier throws, the pipeline maps it.
        Verifier.ThrowInvalid = true;

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), GoogleRequest());

        await response.ShouldBeProblem<SocialTokenVerificationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidProviderToken())
        );
    }

    [Fact]
    public async Task SocialLogin_WithUnverifiedEmail_IsRejected()
    {
        await SeedVisitorRoleAsync();
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"social-{Guid.NewGuid():N}@test.com";
        Verifier.NextPayload = Payload(email, subjectId: $"sub-{Guid.NewGuid():N}", emailVerified: false);

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), GoogleRequest());

        await response.ShouldBeProblem<AccountNotVerifiedException>(
            HttpStatusCode.Forbidden,
            Localized<AuthorizationErrorMessage>(m => m.ProviderEmailNotVerified())
        );

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Users.AnyAsync(u => u.Email == email)).Should().BeFalse();
    }
}
