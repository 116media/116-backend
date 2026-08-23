using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;

/// <summary>
/// Integration tests for the PublicSetPassword endpoint.
/// </summary>
[Collection("Database")]
public class PublicSetPasswordEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task SetPassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetPassword_ForOAuthUser_ReturnsOk()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();

        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var oauthUser = UserFactory.CreateExternal(EnumAuthProvider.Google);

        seedContext.Users.Add(oauthUser);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(oauthUser.Id, "Visitor");

        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicSetPasswordResponse body = await response.ReadAsAsync<PublicSetPasswordResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var updated = await verifyContext.Users.FirstAsync(u => u.Id == oauthUser.Id);
        updated.PasswordHash.Should().NotBeNullOrWhiteSpace();
        passwordService.Verify(TestAuth.ValidPassword, updated.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task SetPassword_ForLocalUser_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.PasswordOnlyForExternalAuth())
        );
    }

    [Fact]
    public async Task SetPassword_ForOAuthUserWithoutAnEmailAddress_ReturnsBadRequest()
    {
        UserEntity oauthUser = await SeedAsync<IdentityDbContext, UserEntity>(context =>
        {
            UserEntity created = UserFactory.CreateExternalWithoutEmail(EnumAuthProvider.Facebook);
            context.Users.Add(created);
            return created;
        });

        Client.AuthenticateAs(oauthUser.Id, "Visitor");
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new PublicSetPasswordRequestBuilder().WithPassword(TestAuth.ValidPassword).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.EmailRequiredToSetPassword(), LocalizedMessage.EnglishCulture)
        );

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity untouched = await verifyContext.Users.FirstAsync(u => u.Id == oauthUser.Id);
        untouched.PasswordHash.Should().BeNull();
        untouched.AuthProvider.Should().Be(EnumAuthProvider.Facebook);
    }

    [Fact]
    public async Task SetPassword_WithEmptyPassword_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var request = new PublicSetPasswordRequestBuilder().WithPassword(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SetPassword(), request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Password", Localized<ValidationErrorMessage>(m => m.PasswordRequired()))
        );
    }
}
