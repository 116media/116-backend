namespace _116.Core.Application.Shared.Errors.Facade;

/// <summary>
/// Single i18n entry point for the Core module.
/// Inject this in every Core handler and service instead of individual
/// <c>*Errors</c> classes.
/// </summary>
public class CoreI18n(FileErrors file)
{
    /// <summary>
    /// File domain errors and messages.
    /// </summary>
    public FileErrors File => file;
}
