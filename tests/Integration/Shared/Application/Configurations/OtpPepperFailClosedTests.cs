namespace _116.Integration.Tests.Shared.Application.Configurations;

/// <summary>
/// Verifies that a host started without <c>OTP_PEPPER</c> refuses to boot, rather than falling
/// back to an unkeyed hash that would leave stored codes recoverable from a dump.
/// </summary>
/// <remarks>
/// The pepperless host is built inside the test because boot validation throws during host
/// construction; a collection fixture warming the host would fail before any test ran. The
/// fixture restores the variable on dispose, so the shared hosts are unaffected.
/// </remarks>
/// <param name="db">The shared Testcontainer database the pepperless host points at.</param>
[Collection("OtpPepperless")]
public class OtpPepperFailClosedTests(OtpPepperlessPostgresFixture db)
{
    [Fact]
    public void Boot_WithoutAConfiguredPepper_RefusesToStartNamingTheVariable()
    {
        // Arrange
        using var pepperlessHost = new OtpPepperlessApiFixture(db);

        // Act
        Action boot = () => pepperlessHost.CreateClient();

        // Assert
        boot.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Invalid environment configuration*OTP_PEPPER is missing or empty.*");
    }
}
