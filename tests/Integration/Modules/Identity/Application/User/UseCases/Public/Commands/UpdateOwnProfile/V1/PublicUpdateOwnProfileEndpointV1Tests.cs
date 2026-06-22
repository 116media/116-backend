using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;

/// <summary>
/// Integration tests for the PublicUpdateOwnProfile endpoint.
/// </summary>
[Collection("Database")]
public class PublicUpdateOwnProfileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublicUpdateOwnProfile_AsVisitor_WithValidSession_UpdatesProfile()
    {
        var sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
        });

        Client.AuthenticateAs(TestUser.VisitorId, "Visitor", sessionId);

        // Country fields only persist alongside a phone-number update, so the builder's
        // valid default carries the full phone + country set to exercise the country update path.
        var request = new PublicUpdateOwnProfileRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Me.Profile(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicUpdateOwnProfileResponse body = await response.ReadAsAsync<PublicUpdateOwnProfileResponse>();
        body.User.Id.Should().Be(TestUser.VisitorId);
        body.User.UserName.Should().Be(request.UserName);
        body.User.CountryName.Should().Be(request.CountryName);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity? user = await verifyContext.Users.FindAsync(TestUser.VisitorId);
        user!.UserName.Should().Be(request.UserName);
        user.CountryName.Should().Be(request.CountryName);
        user.CountryDialCode.Should().Be(request.CountryDialCode);
    }

    [Fact]
    public async Task PublicUpdateOwnProfile_AsVisitor_WithoutValidSession_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new PublicUpdateOwnProfileRequest(
            Email: null,
            UserName: "updated123",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        var response = await Client.PatchAsJsonAsync(Routes.Public.Me.Profile(), request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }
}
