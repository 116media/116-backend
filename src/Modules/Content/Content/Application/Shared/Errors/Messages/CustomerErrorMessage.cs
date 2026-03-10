namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Customer</c> domain.
/// Covers conflict situations and validation failures related to customer operations.
/// </summary>
public static class CustomerErrorMessage
{
    /// <summary>Gets an error message for when a customer with the given email already exists.</summary>
    public static string AlreadyExists(string email) => $"Customer with email '{email}' already exists";

    /// <summary>Gets an error message for when a customer full name is required but not provided.</summary>
    public static string FullNameRequired() => "Customer full name is required";

    /// <summary>Gets an error message for when a customer email is required but not provided.</summary>
    public static string EmailRequired() => "Customer email is required";
}
