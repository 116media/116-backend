using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;

/// <summary>
/// Integration tests for the PublicSocialLogin endpoint.
/// </summary>
[Collection("Database")]
public class PublicSocialLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task SocialLogin_WithInvalidProvider_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new PublicSocialLoginRequestBuilder()
            .WithEmail("social@test.com")
            .WithUserName("socialuser")
            .WithAvatarUrl("https://example.com/avatar.png")
            .WithProvider("InvalidProvider")
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Provider", Localized<ValidationErrorMessage>(m => m.AuthProviderInvalid()))
        );
    }

    [Fact]
    public async Task SocialLogin_WithValidProvider_CreatesUserAndReturnsTokens()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        seedContext.Roles.Add(visitorRole);
        await seedContext.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"social-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        // AvatarUrl is intentionally null: a non-null provider URL triggers a real
        // HTTP download in the file repository, which is out of scope for this test.
        var request = new PublicSocialLoginRequestBuilder()
            .WithEmail(email)
            .WithUserName(userName)
            .WithAvatarUrl(null)
            .WithProvider(nameof(EnumAuthProvider.Google))
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicSocialLoginMobileResponse body = await response.ReadAsAsync<PublicSocialLoginMobileResponse>();
        body.User.Id.Should().NotBeEmpty();
        body.User.Email.Should().Be(request.Email);
        body.TokenType.Should().Be("Bearer");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var created = await verifyContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        created.Should().NotBeNull();
        created!.Id.Should().Be(body.User.Id);
    }
}
