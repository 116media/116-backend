using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Flows covering what a social login does to the local row: an account predating provider-subject
/// tracking adopts the subject id on its next login, and the display name the provider supplies is
/// held to the same username rules as any other input, since it reaches the domain unvalidated.
/// </summary>
[Collection("Database")]
public class ExternalAccountLinkingFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    private StubSocialTokenVerifier Verifier => Api.Services.GetRequiredService<StubSocialTokenVerifier>();

    /// <summary>
    /// Seeds a verified, active Google account and clears its stored provider subject id, leaving
    /// the row in the shape accounts created before subject-id tracking still have.
    /// </summary>
    /// <returns>The seeded account.</returns>
    private async Task<UserEntity> SeedUnlinkedExternalAccountAsync()
    {
        UserEntity user = UserFactory.CreateExternal(EnumAuthProvider.Google);
        user.MarkAsVerified();
        user.Activate();

        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"));
            context.Users.Add(user);
        });

        await using IdentityDbContext unlinkContext = CreateDbContext<IdentityDbContext>();
        await unlinkContext
            .Users.Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.ProviderSubjectId, (string?)null));

        return user;
    }

    [Fact]
    public async Task SocialLogin_WithAnAccountThatHasNoSubjectId_AdoptsTheOneOnTheToken()
    {
        // Arrange
        UserEntity user = await SeedUnlinkedExternalAccountAsync();
        var subjectId = $"sub-{Guid.NewGuid():N}";

        Verifier.NextPayload = new SocialTokenPayload(
            ProviderSubjectId: subjectId,
            Email: user.Email!,
            EmailVerified: true,
            Name: "Social User",
            PictureUrl: null
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Remove("X-Device-Id");
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var request = new PublicSocialLoginRequestBuilder().WithProvider(nameof(EnumAuthProvider.Google)).Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity persisted = await verifyContext.Users.FirstAsync(u => u.Id == user.Id);
        persisted.ProviderSubjectId.Should().Be(subjectId);
    }

    [Fact]
    public async Task SocialLogin_WithADisplayNameLongerThanAUsername_IsRefused()
    {
        // Arrange — the provider's display name is adopted as the username, so it has to satisfy
        // the same length rule
        UserEntity user = await SeedUnlinkedExternalAccountAsync();
        var overLongName = new string('n', UserConstants.MaxUserNameLength + 1);

        Verifier.NextPayload = new SocialTokenPayload(
            ProviderSubjectId: $"sub-{Guid.NewGuid():N}",
            Email: user.Email!,
            EmailVerified: true,
            Name: overLongName,
            PictureUrl: null
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Remove("X-Device-Id");
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new PublicSocialLoginRequestBuilder().WithProvider(nameof(EnumAuthProvider.Google)).Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), request);

        // Assert
        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(
                m => m.InvalidUsernameFormat(overLongName),
                LocalizedMessage.EnglishCulture
            )
        );
    }

    [Fact]
    public async Task SocialLogin_ForANewAccountWithABlankDisplayName_IsRefused()
    {
        // Arrange — a blank name is not null, so it survives the fallback to the email address
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"))
        );

        Verifier.NextPayload = new SocialTokenPayload(
            ProviderSubjectId: $"sub-{Guid.NewGuid():N}",
            Email: $"blank-name-{Guid.NewGuid():N}@test.com",
            EmailVerified: true,
            Name: string.Empty,
            PictureUrl: null
        );

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Remove("X-Device-Id");
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var request = new PublicSocialLoginRequestBuilder().WithProvider(nameof(EnumAuthProvider.Google)).Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(Routes.Public.Auth.SocialLogin(), request);

        // Assert
        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(
                m => m.InvalidUsernameFormat(string.Empty),
                LocalizedMessage.EnglishCulture
            )
        );
    }
}
