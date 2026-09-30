namespace _116.BuildingBlocks.Application.Persistence;

/// <summary>
/// Classifies a persistence failure without exposing the database provider to callers.
/// </summary>
public interface IUniqueConstraintDetector
{
    /// <summary>
    /// Reports whether the failure is a unique-constraint violation, meaning a lost
    /// check-then-act race rather than a defect.
    /// </summary>
    /// <param name="exception">The failure raised by the persistence layer.</param>
    /// <returns><c>true</c> when the write lost a uniqueness race.</returns>
    bool IsUniqueConstraintViolation(Exception exception);
}
