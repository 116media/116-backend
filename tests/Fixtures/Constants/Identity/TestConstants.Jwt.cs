namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for JWT testing. All test-owned: production reads this configuration from the
    /// environment. Must stay in sync with <c>ApiFixture.SetEnvironmentVariables</c>.
    /// </summary>
    public static class Jwt
    {
        /// <summary>
        /// Test-owned signing key. Must equal the <c>JWT_SECRET</c> the API fixture exports.
        /// </summary>
        public const string ValidSecret = "ThisIsAVerySecureSecretKeyForTesting123!@#";

        /// <summary>
        /// Test-owned issuer. Must equal the <c>JWT_ISSUER</c> the API fixture exports.
        /// </summary>
        public const string ValidIssuer = "116_test";

        /// <summary>
        /// Test-owned audience. Must equal the <c>JWT_AUDIENCE</c> the API fixture exports.
        /// </summary>
        public const string ValidAudience = "116_test_client";

        /// <summary>
        /// Test-owned access token lifetime in minutes. Must equal the
        /// <c>JWT_ACCESS_TOKEN_EXPIRATION</c> the API fixture exports, in the same unit.
        /// </summary>
        public const int AccessTokenExpirationMinutes = 60;

        /// <summary>
        /// Test-owned refresh token lifetime in days. Must equal the
        /// <c>JWT_REFRESH_TOKEN_EXPIRATION</c> the API fixture exports, in minutes.
        /// </summary>
        public const int RefreshTokenExpirationDays = 30;

        /// <summary>
        /// Test-owned security stamp seeded on every test-created user's token-state row and
        /// emitted as the <c>sstamp</c> claim by the hand-minted test tokens.
        /// </summary>
        public static readonly Guid WellKnownSecurityStamp = Guid.Parse("7f3a1c58-9b2d-4e6f-8a01-5c4d3e2b1a09");

        /// <summary>
        /// Test-owned hand-written token for tests needing a well-formed JWT string they never
        /// verify. The signature is a placeholder and does not validate.
        /// </summary>
        public const string ValidAccessToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRlc3QgVXNlciIsImlhdCI6MTUxNjIzOTAyMn0.test_signature";
    }
}
