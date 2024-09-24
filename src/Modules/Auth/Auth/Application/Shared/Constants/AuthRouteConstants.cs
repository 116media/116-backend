using _116.Auth.Domain.Constants;

namespace _116.Auth.Application.Shared.Constants;

/// <summary>
/// Contains route path constants for authentication-related API endpoints.
/// Provides centralized string constants for URL segments used in authentication
/// routes to ensure consistency across public and admin authentication flows.
/// </summary>
public static class AuthRouteConstants
{
    /// <summary>
    /// Route segment for user-related endpoints.
    /// Used in routes like /api/v1/public/auth/users.
    /// </summary>
    public const string Users = "users";

    /// <summary>
    /// Route segment for user profile endpoints.
    /// Used in routes like /api/v1/public/auth/profile.
    /// </summary>
    public const string Profile = "profile";

    /// <summary>
    /// Route segment for password change endpoints.
    /// Used when authenticated users want to change their existing password.
    /// Example: /api/v1/public/auth/change-password.
    /// </summary>
    public const string ChangePassword = "change-password";

    /// <summary>
    /// Route segment for password recovery initiation endpoints.
    /// Used when users request a password reset via email.
    /// Example: /api/v1/public/auth/forgot-password.
    /// </summary>
    public const string ForgotPassword = "forgot-password";

    /// <summary>
    /// Route segment for password reset completion endpoints.
    /// Used when users complete the password reset process with OTP verification.
    /// Example: /api/v1/public/auth/reset-password.
    /// </summary>
    public const string ResetPassword = "reset-password";

    /// <summary>
    /// Route segment for user authentication endpoints.
    /// Used for email/password login flow.
    /// Example: /api/v1/public/auth/login or /api/v1/admin/auth/login.
    /// </summary>
    public const string Login = "login";

    /// <summary>
    /// Route segment for user registration endpoints.
    /// Used for creating new user accounts.
    /// Example: /api/v1/public/auth/signup.
    /// </summary>
    public const string SignUp = "signup";

    /// <summary>
    /// Route segment for OTP resend endpoints.
    /// Used when users need to request a new one-time password.
    /// Example: /api/v1/public/auth/resend-otp.
    /// </summary>
    public const string ResendOtp = "resend-otp";

    /// <summary>
    /// Route segment for OTP verification endpoints.
    /// Used to verify one-time passwords for account activation or password reset.
    /// Example: /api/v1/public/auth/verify-otp.
    /// </summary>
    public const string VerifyOtp = "verify-otp";

    /// <summary>
    /// Route segment for user sign-out endpoints.
    /// Used to terminate user sessions and invalidate tokens.
    /// Example: /api/v1/public/auth/sign-out.
    /// </summary>
    public const string SignOut = "sign-out";

    /// <summary>
    /// Route segment for social authentication endpoints.
    /// Used for OAuth-based login with providers like Google, Facebook, etc.
    /// Example: /api/v1/public/auth/social-login.
    /// </summary>
    public const string SocialLogin = "social-login";

    /// <summary>
    /// Route segment for user avatar management endpoints.
    /// Used for uploading and updating user profile images.
    /// Example: /api/v1/public/auth/avatar.
    /// </summary>
    public const string Avatar = "avatar";

    /// <summary>
    /// The base endpoint path for all authentication routes.
    /// Dynamically generated from the Auth module name in lowercase (e.g., "auth").
    /// Combined with public/admin prefixes: /api/v1/public/auth or /api/v1/admin/auth.
    /// </summary>
    public static readonly string Endpoint = AuthConstants.ModuleName.ToLowerInvariant();
}
