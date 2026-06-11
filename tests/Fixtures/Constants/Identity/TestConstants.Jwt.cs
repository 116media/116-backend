namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for JWT testing.
    /// Mirrors <c>src/BuildingBlocks/Constants/JwtClaimsConstants.cs</c> and JWT configuration.
    /// </summary>
    public static class Jwt
    {
        public const string ValidSecret = "ThisIsAVerySecureSecretKeyForTesting123!@#";
        public const string ValidIssuer = "116_test";
        public const string ValidAudience = "116_test_client";
        public const int AccessTokenExpirationMinutes = 60;
        public const int RefreshTokenExpirationDays = 30;
        public const string ValidAccessToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRlc3QgVXNlciIsImlhdCI6MTUxNjIzOTAyMn0.test_signature";
    }
}
