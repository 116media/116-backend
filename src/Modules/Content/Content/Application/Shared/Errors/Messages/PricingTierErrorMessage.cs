namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>PricingTier</c> domain.
/// Covers conflict situations and validation failures related to pricing tier operations.
/// </summary>
public static class PricingTierErrorMessage
{
    /// <summary>
    /// Gets an error message for when a pricing tier with the given name already exists.
    /// </summary>
    /// <param name="name">The pricing tier name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a pricing tier with the specified name already exists.
    /// </returns>
    public static string AlreadyExists(string name)
    {
        return $"Pricing tier '{name}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a pricing tier is already active.
    /// </summary>
    /// <returns>
    /// An error message indicating that the pricing tier is already active.
    /// </returns>
    public static string AlreadyActive()
    {
        return "Pricing tier is already active";
    }

    /// <summary>
    /// Gets an error message for when a pricing tier is already inactive.
    /// </summary>
    /// <returns>
    /// An error message indicating that the pricing tier is already inactive.
    /// </returns>
    public static string AlreadyInactive()
    {
        return "Pricing tier is already inactive";
    }

    /// <summary>
    /// Gets an error message for when a pricing tier name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the pricing tier name is required.
    /// </returns>
    public static string NameRequired()
    {
        return "Pricing tier name is required";
    }
}
