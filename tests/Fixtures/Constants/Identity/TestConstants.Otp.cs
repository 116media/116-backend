namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for OTP entity testing.
    /// Mirrors <c>src/BuildingBlocks/Constants/UserConstants.cs</c> (OTP fields).
    /// </summary>
    public static class Otp
    {
        public const int CodeLength = 6;
        public const int MaxAttempts = 5;
        public const int ExpirationMinutes = 10;

        public const string ValidCode = "123456";
        public const string InvalidCode = "000000";
        public const string DefaultCode = "654321";
    }
}
