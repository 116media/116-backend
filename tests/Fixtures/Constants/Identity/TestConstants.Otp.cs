using _116.BuildingBlocks.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for OTP entity testing. The numeric limits alias
    /// <see cref="UserConstants" /> rather than copying it.
    /// </summary>
    public static class Otp
    {
        /// <summary>
        /// Test-owned OTP pepper. Must equal the <c>OTP_PEPPER</c> the API fixture exports, or
        /// codes seeded by the builder will not verify against the running host.
        /// </summary>
        public const string Pepper = "TestOtpPepperValueForIntegrationTests123!";

        /// <summary>
        /// The production OTP code length.
        /// </summary>
        public const int CodeLength = UserConstants.OtpCodeLength;

        /// <summary>
        /// The production brute-force lockout threshold, reached when <c>AttemptCount</c>
        /// equals this value.
        /// </summary>
        public const int MaxAttempts = UserConstants.MaxOtpAttempts;

        /// <summary>
        /// The production OTP validity window in minutes.
        /// </summary>
        public const int ExpirationMinutes = UserConstants.OtpExpirationMinutes;

        /// <summary>
        /// The production cap on codes issued per purpose inside the resend window.
        /// </summary>
        public const int MaxResendsPerWindow = UserConstants.MaxOtpResendsPerWindow;

        /// <summary>
        /// The production window over which resends are counted, in minutes.
        /// </summary>
        public const int ResendWindowMinutes = UserConstants.OtpResendWindowMinutes;

        /// <summary>
        /// A well-formed code used wherever a test needs a code that parses.
        /// Test-owned: production generates codes rather than declaring them.
        /// </summary>
        public const string ValidCode = "123456";

        /// <summary>
        /// A well-formed code that is never the one under test.
        /// Test-owned for the same reason as <see cref="ValidCode" />.
        /// </summary>
        public const string InvalidCode = "000000";

        /// <summary>
        /// The code the OTP service mock returns from <c>GenerateOtpCode()</c>.
        /// Test-owned: production generates codes rather than declaring them.
        /// </summary>
        public const string DefaultCode = "654321";

        /// <summary>
        /// The stored hash the OTP hasher mock returns, carrying the production scheme prefix.
        /// Test-owned: production derives hashes rather than declaring them.
        /// </summary>
        public const string DefaultCodeHash = "h1:VGVzdE90cEhhc2hlZENvZGVWYWx1ZUZvclVuaXRz";
    }
}
