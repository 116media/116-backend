using _116.Shared.Application.Configurations;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Configurations;

/// <summary>
/// Unit tests for <see cref="EnvSchema.ValidateAtBoot"/>, covering the aggregate failure that
/// names every invalid variable in one error.
/// </summary>
[Collection("EnvironmentVariable")]
public class EnvSchemaTests : IDisposable
{
    private const string JwtSecretEnvVar = "JWT_SECRET";
    private const string OtpPepperEnvVar = "OTP_PEPPER";
    private readonly string? _originalJwtSecret;
    private readonly string? _originalOtpPepper;

    public EnvSchemaTests()
    {
        _originalJwtSecret = Environment.GetEnvironmentVariable(JwtSecretEnvVar);
        _originalOtpPepper = Environment.GetEnvironmentVariable(OtpPepperEnvVar);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(JwtSecretEnvVar, _originalJwtSecret);
        Environment.SetEnvironmentVariable(OtpPepperEnvVar, _originalOtpPepper);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ValidateAtBoot_WithMissingJwtSecret_ShouldThrowNamingIt()
    {
        // Arrange
        Environment.SetEnvironmentVariable(JwtSecretEnvVar, null);

        // Act
        Action validate = EnvSchema.ValidateAtBoot;

        // Assert
        validate
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Invalid environment configuration*JWT_SECRET is missing or empty.*");
    }

    [Fact]
    public void ValidateAtBoot_WithSeveralProblems_ShouldNameThemAll()
    {
        // Arrange
        Environment.SetEnvironmentVariable(JwtSecretEnvVar, "too-short");
        Environment.SetEnvironmentVariable(OtpPepperEnvVar, null);

        // Act
        Action validate = EnvSchema.ValidateAtBoot;

        // Assert
        validate
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*JWT_SECRET must be at least 32 characters.*")
            .WithMessage("*OTP_PEPPER is missing or empty.*");
    }
}
