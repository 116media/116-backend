namespace _116.Integration.Tests.Modules.Mailer;

/// <summary>
/// Verifies which email adapter a real host registers for a given <c>EMAIL_PROVIDER</c>, and that a
/// misconfigured provider stops the application at boot rather than at the first send.
/// </summary>
/// <param name="db">The dedicated Testcontainer database and Resend-configured application host.</param>
[Collection("Resend")]
public class MailerModuleTests(ResendPostgresFixture db) : IDisposable
{
    private readonly ResendPostgresFixture _db = db;
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AResendDeployment_BootsAndServesRequests()
    {
        using HttpResponseMessage response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void AResendDeploymentWithoutItsApiKey_FailsAtBoot()
    {
        using var fixture = new KeylessResendApiFixture(_db);

        Action act = () => _ = fixture.Services;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("RESEND_API_KEY is required when EMAIL_PROVIDER is 'resend'.");
    }

    [Fact]
    public void ADeploymentNamingAnUnsupportedProvider_FailsAtBoot()
    {
        using var fixture = new UnknownProviderApiFixture(_db);

        Action act = () => _ = fixture.Services;

        act.Should().Throw<InvalidOperationException>().WithMessage("Unknown EMAIL_PROVIDER 'carrier-pigeon'.");
    }
}
