using _116.Shared.Application.Configurations;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Configurations;

/// <summary>
/// Unit tests for <see cref="EnvVar{T}"/> and the <see cref="EnvVarValidators"/> extensions,
/// covering presence, parsing, defaults and validator evaluation.
/// </summary>
[Collection("EnvironmentVariable")]
public class EnvVarTests : IDisposable
{
    private const string TestVar = "ENVVAR_TESTS_VALUE";
    private readonly string? _original;

    public EnvVarTests()
    {
        _original = Environment.GetEnvironmentVariable(TestVar);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(TestVar, _original);
        GC.SuppressFinalize(this);
    }

    #region Required

    [Fact]
    public void Error_RequiredAndMissing_ShouldNameTheVariable()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, null);
        EnvVar<string> envVar = EnvVar.Required(TestVar);

        // Act
        string? error = envVar.Error();

        // Assert
        error.Should().Be($"{TestVar} is missing or empty.");
    }

    [Fact]
    public void Error_RequiredAndWhitespace_ShouldNameTheVariable()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "   ");
        EnvVar<string> envVar = EnvVar.Required(TestVar);

        // Act
        string? error = envVar.Error();

        // Assert
        error.Should().Be($"{TestVar} is missing or empty.");
    }

    [Fact]
    public void Error_RequiredAndPresent_ShouldBeNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "some-value");
        EnvVar<string> envVar = EnvVar.Required(TestVar);

        // Act & Assert
        envVar.Error().Should().BeNull();
        envVar.Value.Should().Be("some-value");
    }

    #endregion

    #region Optional

    [Fact]
    public void Value_OptionalAndMissing_ShouldBeNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, null);
        EnvVar<string?> envVar = EnvVar.Optional(TestVar);

        // Act & Assert
        envVar.Error().Should().BeNull();
        envVar.Value.Should().BeNull();
    }

    [Fact]
    public void Value_OptionalWithDefaultAndMissing_ShouldUseDefault()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, null);
        EnvVar<string> envVar = EnvVar.Optional(TestVar, @default: "fallback");

        // Act & Assert
        envVar.Error().Should().BeNull();
        envVar.Value.Should().Be("fallback");
    }

    [Fact]
    public void Value_ReadsTheEnvironmentLive_ShouldObserveSwappedValues()
    {
        // Arrange
        EnvVar<string?> envVar = EnvVar.Optional(TestVar);

        // Act & Assert
        Environment.SetEnvironmentVariable(TestVar, "first");
        envVar.Value.Should().Be("first");
        Environment.SetEnvironmentVariable(TestVar, "second");
        envVar.Value.Should().Be("second");
    }

    #endregion

    #region Int parsing

    [Fact]
    public void Value_IntAndMissing_ShouldUseDefault()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, null);
        EnvVar<int> envVar = EnvVar.Int(TestVar, @default: 42);

        // Act & Assert
        envVar.Error().Should().BeNull();
        envVar.Value.Should().Be(42);
    }

    [Fact]
    public void Error_IntAndUnparsable_ShouldReportParseFailure()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "not-a-number");
        EnvVar<int> envVar = EnvVar.Int(TestVar, @default: 42);

        // Act & Assert
        envVar.Error().Should().Be($"{TestVar} must be an integer.");
    }

    [Fact]
    public void Value_IntAndPresent_ShouldParse()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "1440");
        EnvVar<int> envVar = EnvVar.Int(TestVar, @default: 42);

        // Act & Assert
        envVar.Error().Should().BeNull();
        envVar.Value.Should().Be(1440);
    }

    #endregion

    #region Validators

    [Fact]
    public void Error_MinLengthViolated_ShouldReportTheRule()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "short");
        EnvVar<string> envVar = EnvVar.Required(TestVar).MinLength(length: 32);

        // Act & Assert
        envVar.Error().Should().Be($"{TestVar} must be at least 32 characters.");
    }

    [Fact]
    public void Error_MinLengthSatisfied_ShouldBeNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, new string('x', 32));
        EnvVar<string> envVar = EnvVar.Required(TestVar).MinLength(length: 32);

        // Act & Assert
        envVar.Error().Should().BeNull();
    }

    [Fact]
    public void Error_InRangeViolated_ShouldReportTheRule()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "0");
        EnvVar<int> envVar = EnvVar.Int(TestVar, @default: 60).InRange(min: 1, max: 1440);

        // Act & Assert
        envVar.Error().Should().Be($"{TestVar} must be between 1 and 1440.");
    }

    [Fact]
    public void Error_AbsoluteUrlViolated_ShouldReportTheRule()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "not a url");
        EnvVar<string> envVar = EnvVar.Required(TestVar).AbsoluteUrl();

        // Act & Assert
        envVar.Error().Should().Be($"{TestVar} must be an absolute URL.");
    }

    [Fact]
    public void Error_AbsoluteUrlListWithValidEntries_ShouldBeNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "https://app.116.com, http://localhost:3000");
        EnvVar<string> envVar = EnvVar.Required(TestVar).AbsoluteUrlList();

        // Act & Assert
        envVar.Error().Should().BeNull();
    }

    [Fact]
    public void Error_AbsoluteUrlListWithBadEntry_ShouldReportTheRule()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, "https://app.116.com, nope");
        EnvVar<string> envVar = EnvVar.Required(TestVar).AbsoluteUrlList();

        // Act & Assert
        envVar.Error().Should().Be($"{TestVar} must be an absolute URL or a comma-separated list of absolute URLs.");
    }

    [Fact]
    public void Error_OptionalAndMissingWithValidators_ShouldBeNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TestVar, null);
        EnvVar<int> envVar = EnvVar.Int(TestVar, @default: 60).InRange(min: 1, max: 1440);

        // Act & Assert
        envVar.Error().Should().BeNull();
    }

    #endregion
}
