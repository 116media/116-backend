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
    }
}
