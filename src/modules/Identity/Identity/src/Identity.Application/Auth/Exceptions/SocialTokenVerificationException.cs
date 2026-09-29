namespace _116.Identity.Application.Auth.Exceptions;

/// <summary>
/// Raised by a social-token verifier adapter when a provider token fails verification (bad signature,
/// expired, wrong audience or app). Carries no message: its strategy handler resolves the localized
/// detail, so the infrastructure layer never touches i18n.
/// </summary>
public class SocialTokenVerificationException : Exception;
