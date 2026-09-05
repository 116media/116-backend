using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Flows driving credentials whose stored hash cannot be read back — a truncated column, a partial
/// write, a row carried over from a scheme the application no longer emits. Every one of them must
/// read as a plain authentication failure rather than surfacing a decoding error to the caller.
/// </summary>
[Collection("Database")]
public class CorruptedCredentialHashFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Seeds a verified, active account whose password hash is written by the real service, then
    /// overwrites that hash in the database with the supplied value.
    /// </summary>
    /// <param name="storedHash">The corrupt value to leave in the password column.</param>
    /// <returns>The email of the seeded account.</returns>
    private async Task<string> SeedAccountWithStoredPasswordHashAsync(string storedHash)
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        UserErrors errors = TestErrorsFactory.CreateUserErrors();

        var email = $"corrupt-hash-{Guid.NewGuid():N}@test.com";
        UserEntity user = UserFactory.Create(email);
        user.MarkAsVerified();
        user.Activate();
        user.InitializePasswordHash(passwordService.Hash(TestAuth.ValidPassword));

        await SeedAsync<IdentityDbContext>(context => context.Users.Add(user));

        await using IdentityDbContext corruptContext = CreateDbContext<IdentityDbContext>();
        await corruptContext
            .Users.Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.PasswordHash, storedHash));

        return email;
    }

    /// <summary>
    /// Seeds an active account with an outstanding OTP, then overwrites the stored code hash.
    /// </summary>
    /// <param name="storedHash">The corrupt value to leave in the code-hash column.</param>
    /// <returns>The email of the seeded account.</returns>
    private async Task<string> SeedAccountWithStoredCodeHashAsync(string storedHash)
    {
        var email = $"corrupt-otp-{Guid.NewGuid():N}@test.com";
        UserEntity user = UserFactory.Create(email);
        user.Activate();

        OtpEntity otp = OtpFactory.Create(user.Id, Otp.ValidCode, EnumOtpPurpose.EmailVerification);

        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Users.Add(user);
            context.Otps.Add(otp);
        });

        await using IdentityDbContext corruptContext = CreateDbContext<IdentityDbContext>();
        await corruptContext
            .Otps.Where(o => o.Id == otp.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.CodeHash, storedHash));

        return email;
    }

    /// <summary>
    /// Attempts a login with the account's real password.
    /// </summary>
    /// <param name="email">The account to authenticate as.</param>
    /// <returns>The login response.</returns>
    private async Task<HttpResponseMessage> LoginAsync(string email)
    {
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Remove("X-Device-Id");
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var request = new PublicLoginRequestBuilder()
            .WithCredentials(email)
            .WithPassword(TestAuth.ValidPassword)
            .Build();

        return await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);
    }

    /// <summary>
    /// Attempts to verify the account's real OTP code.
    /// </summary>
    /// <param name="email">The account to verify.</param>
    /// <returns>The verification response.</returns>
    private async Task<HttpResponseMessage> VerifyOtpAsync(string email)
    {
        Client.ClearAuthentication();

        var request = new PublicVerifyOtpRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithPurpose(nameof(EnumOtpPurpose.EmailVerification))
            .Build();

        return await Client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), request);
    }

    [Fact]
    public async Task Login_WithAStoredPasswordHashOfTheWrongLength_IsRefused()
    {
        // Arrange — the prefix and base64 are well formed, but the payload is too short to split
        // into a salt and a hash
        string email = await SeedAccountWithStoredPasswordHashAsync($"v2:{Convert.ToBase64String(new byte[10])}");

        // Act
        HttpResponseMessage response = await LoginAsync(email);

        // Assert
        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials())
        );
    }

    [Fact]
    public async Task Login_WithAStoredPasswordHashThatIsNotBase64_IsRefused()
    {
        // Arrange
        string email = await SeedAccountWithStoredPasswordHashAsync("v2:this-is-not-base64!!");

        // Act
        HttpResponseMessage response = await LoginAsync(email);

        // Assert — the decoding failure is absorbed, not surfaced as a server error
        await response.ShouldBeProblem<AuthenticationException>(
            HttpStatusCode.Unauthorized,
            Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials())
        );
    }

    [Fact]
    public async Task VerifyOtp_WithAStoredCodeHashFromAnUnknownScheme_IsRefused()
    {
        // Arrange — a row predating the keyed OTP scheme still carries the password prefix
        string email = await SeedAccountWithStoredCodeHashAsync($"v1:{Convert.ToBase64String(new byte[48])}");

        // Act
        HttpResponseMessage response = await VerifyOtpAsync(email);

        // Assert
        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.InvalidOtpCode())
        );
    }

    [Fact]
    public async Task VerifyOtp_WithAStoredCodeHashThatIsNotBase64_IsRefused()
    {
        // Arrange
        string email = await SeedAccountWithStoredCodeHashAsync("h1:this-is-not-base64!!");

        // Act
        HttpResponseMessage response = await VerifyOtpAsync(email);

        // Assert
        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.InvalidOtpCode())
        );
    }
}
