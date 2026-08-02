using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
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

    /// <summary>
    /// Verifies that submitting a phone number another account already holds returns the
    /// <c>PhoneNumberAlreadyExists</c> 409 raised by <c>PublicUpdateProfileAuthFactory</c>. The
    /// username is left out of the request so the phone-uniqueness check is the only branch that
    /// can fail, and the caller's own profile is left unchanged because the conflict is thrown
    /// before the unit of work commits.
    /// </summary>
    [Fact]
    public async Task PublicUpdateOwnProfile_WithPhoneNumberHeldByAnotherUser_ReturnsConflict()
    {
        const string countryDialCode = "+1";
        const string partialPhoneNumber = "5550117788";
        const string fullPhoneNumber = $"{countryDialCode}{partialPhoneNumber}";

        var sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Users.Add(UserFactory.CreateWithPhoneNumber(fullPhoneNumber, partialPhoneNumber));
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
        });

        Client.AuthenticateAs(TestUser.VisitorId, "Visitor", sessionId);
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new PublicUpdateOwnProfileRequestBuilder()
            .WithUserName(null)
            .WithPartialPhoneNumber(partialPhoneNumber)
            .WithCountryDialCode(countryDialCode)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Me.Profile(), request);

        await response.ShouldBeProblem(HttpStatusCode.Conflict, $"Phone number '{fullPhoneNumber}' is already taken.");

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity? caller = await verifyContext.Users.FindAsync(TestUser.VisitorId);
        caller!.FullPhoneNumber.Should().BeNull();
    }

    /// <summary>
    /// Verifies that an address whose local part is a single character is still masked in the
    /// alert sent to the address that just lost the account. There is no leading character to
    /// keep once the whole local part is the masked part, so the notice carries only the domain.
    /// </summary>
    [Fact]
    public async Task PublicUpdateOwnProfile_ToASingleCharacterLocalPart_MasksTheWholeLocalPart()
    {
        var sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
        });

        Client.AuthenticateAs(TestUser.VisitorId, "Visitor", sessionId);

        string domain = $"short-{Guid.NewGuid():N}.example.com";
        string newEmail = $"q@{domain}";

        var request = new PublicUpdateOwnProfileRequestBuilder().WithEmail(newEmail).Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Me.Profile(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity alert = (
            await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail).ToListAsync()
        )
            .Should()
            .ContainSingle(o => o.Template == "EmailChangedAlertOld")
            .Subject;

        alert.TextBody.Should().Contain($"***@{domain}");
        alert.TextBody.Should().NotContain(newEmail);
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
