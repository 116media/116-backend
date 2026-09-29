namespace _116.Identity.Application.Shared.Errors.Facade;

/// <summary>
/// Single i18n entry point for the Identity module.
/// Inject this in every Identity validator and handler instead of individual
/// <c>*Errors</c> classes.
/// </summary>
public class IdentityI18n(UserErrors user, SessionErrors session)
{
    /// <summary>
    /// User domain errors and messages.
    /// </summary>
    public UserErrors User => user;

    /// <summary>
    /// Session domain errors and messages.
    /// </summary>
    public SessionErrors Session => session;
}
