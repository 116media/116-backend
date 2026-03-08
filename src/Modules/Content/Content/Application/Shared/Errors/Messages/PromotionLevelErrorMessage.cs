namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>PromotionLevel</c> domain.
/// Covers conflict situations and validation failures related to promotion level operations.
/// </summary>
public static class PromotionLevelErrorMessage
{
    /// <summary>
    /// Gets an error message for when a promotion level with the given name already exists.
    /// </summary>
    /// <param name="name">The promotion level name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a promotion level with the specified name already exists.
    /// </returns>
    public static string AlreadyExists(string name)
    {
        return $"Promotion level '{name}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a promotion level is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the promotion level is already active.
    /// </returns>
    public static string AlreadyActive()
    {
        return "Promotion level is already active";
    }

    /// <summary>
    /// Gets an error message for when a promotion level is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the promotion level is already inactive.
    /// </returns>
    public static string AlreadyInactive()
    {
        return "Promotion level is already inactive";
    }

    /// <summary>
    /// Gets an error message for when a promotion level name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the promotion level name is required.
    /// </returns>
    public static string NameRequired()
    {
        return "Promotion level name is required";
    }

    /// <summary>
    /// Gets an error message for when the promotion level duration is not a positive number.
    /// </summary>
    /// <returns>
    /// An error message indicating that the duration must be greater than zero.
    /// </returns>
    public static string DurationMustBePositive()
    {
        return "Promotion level duration must be greater than zero";
    }

    /// <summary>
    /// Gets an error message for when the promotion level price is negative.
    /// </summary>
    /// <returns>
    /// An error message indicating that the price must be zero or greater.
    /// </returns>
    public static string PriceMustBeNonNegative()
    {
        return "Promotion level price must be zero or greater";
    }
}
