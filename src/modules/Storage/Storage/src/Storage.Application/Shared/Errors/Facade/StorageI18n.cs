namespace _116.Storage.Application.Shared.Errors.Facade;

/// <summary>
/// Single i18n entry point for the Storage module.
/// Inject this in every Storage handler and service instead of individual
/// <c>*Errors</c> classes.
/// </summary>
public class StorageI18n(FileErrors file)
{
    /// <summary>
    /// File domain errors and messages.
    /// </summary>
    public FileErrors File => file;
}
