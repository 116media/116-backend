namespace _116.Identity.Application.Shared.Authorizations.Contracts;

/// <summary>
/// Distinguishes a store that was unreachable from a store that answered. Authorization uses
/// this to decide whether a failed lookup is an outage or a defect.
/// </summary>
public interface ITransientFaultDetector
{
    /// <summary>
    /// Reports whether the failure means the datastore could not be reached.
    /// </summary>
    /// <param name="exception">The failure raised while reading.</param>
    /// <returns><c>true</c> when the datastore was unreachable.</returns>
    bool IsUnreachable(Exception exception);
}
