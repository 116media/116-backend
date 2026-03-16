namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Customer</c> domain.
/// Covers conflict situations and validation failures related to customer operations.
/// </summary>
public static class CustomerErrorMessage
{
    /// <summary>
    /// Gets an error message for when a customer with the given email already exists.
    /// </summary>
    /// <param name="email">The customer email that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a customer with the specified email already exists.
    /// </returns>
    public static string AlreadyExists(string email)
    {
        return $"Customer with email '{email}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a customer full name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the customer full name is required.
    /// </returns>
    public static string FullNameRequired()
    {
        return "Customer full name is required";
    }

    /// <summary>
    /// Gets an error message for when a customer email is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the customer email is required.
    /// </returns>
    public static string EmailRequired()
    {
        return "Customer email is required";
    }
}
