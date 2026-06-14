using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;

/// <summary>
/// Integration tests for the PublicSocialLogin endpoint.
/// </summary>
[Collection("Database")]
public class PublicSocialLoginEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SocialLogin_WithInvalidProvider_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "social@test.com",
            UserName = "socialuser",
            AvatarUrl = "https://example.com/avatar.png",
            Provider = "InvalidProvider",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/social-login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
