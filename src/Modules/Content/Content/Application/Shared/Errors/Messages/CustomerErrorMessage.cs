using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Customer</c> domain.
/// Covers conflict situations and validation failures related to customer operations.
/// </summary>
public class CustomerErrorMessage(IStringLocalizer<CustomerErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a customer with the given email already exists.
    /// </summary>
    /// <param name="email">The customer email that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a customer with the specified email already exists.
    /// </returns>
    public string AlreadyExists(string email)
    {
        return string.Format(localizer["AlreadyExists"], email);
    }

    /// <summary>
    /// Gets an error message for when a customer full name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the customer full name is required.
    /// </returns>
    public string FullNameRequired()
    {
        return localizer["FullNameRequired"];
    }

    /// <summary>
    /// Gets an error message for when a customer email is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the customer email is required.
    /// </returns>
    public string EmailRequired()
    {
        return localizer["EmailRequired"];
    }

    /// <summary>
    /// Gets an error message for when a customer full name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string FullNameTooLong(int max) => string.Format(localizer["FullNameTooLong"], max);

    /// <summary>
    /// Gets an error message for when a customer email format is invalid.
    /// </summary>
    public string EmailInvalidFormat() => localizer["EmailInvalidFormat"];

    /// <summary>
    /// Gets an error message for when a customer email exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string EmailTooLong(int max) => string.Format(localizer["EmailTooLong"], max);

    /// <summary>
    /// Gets an error message for when a customer phone exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string PhoneTooLong(int max) => string.Format(localizer["PhoneTooLong"], max);

    /// <summary>
    /// Gets an error message for when a customer company exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string CompanyTooLong(int max) => string.Format(localizer["CompanyTooLong"], max);

    /// <summary>
    /// Gets an error message for when customer notes exceed the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NotesTooLong(int max) => string.Format(localizer["NotesTooLong"], max);

    /// <summary>
    /// Gets an error message for when a customer ID is required.
    /// </summary>
    public string CustomerIdRequired() => localizer["CustomerIdRequired"];
}
