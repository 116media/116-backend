using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a B2B client (artist, music label, or brand) who commissions paid content.
/// Customers are entirely separate from platform visitor accounts (identity.users).
/// A customer account must exist before an order can be opened for them.
/// </summary>
public class CustomerEntity : Aggregate<Guid>
{
    /// <summary>
    /// Full name of the customer or contact person.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCustomerFullNameLength)]
    public string FullName { get; private set; } = null!;

    /// <summary>
    /// Email address of the customer. Used as the unique identifier — not updatable after creation.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCustomerEmailLength)]
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Optional phone number for the customer.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCustomerPhoneLength)]
    public string? Phone { get; private set; }

    /// <summary>
    /// Optional company or label name for the customer.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCustomerCompanyLength)]
    public string? Company { get; private set; }

    /// <summary>
    /// Optional internal notes about the customer (e.g., payment preferences, special instructions).
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCustomerNotesLength)]
    public string? Notes { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private CustomerEntity() { }

    /// <summary>
    /// Creates a new customer entity.
    /// </summary>
    /// <param name="id">The unique identifier for the customer.</param>
    /// <param name="fullName">The full name of the customer.</param>
    /// <param name="email">The email address of the customer.</param>
    /// <param name="phone">An optional phone number.</param>
    /// <param name="company">An optional company or label name.</param>
    /// <param name="notes">Optional internal notes.</param>
    /// <returns>A new <see cref="CustomerEntity" /> instance.</returns>
    public static CustomerEntity Create(
        Guid id,
        string fullName,
        string email,
        string? phone,
        string? company,
        string? notes
    )
    {
        if (string.IsNullOrWhiteSpace(value: fullName))
        {
            throw CustomerErrors.FullNameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: email))
        {
            throw CustomerErrors.EmailRequired();
        }

        return new CustomerEntity
        {
            Id = id,
            FullName = fullName,
            Email = email,
            Phone = phone,
            Company = company,
            Notes = notes,
        };
    }

    /// <summary>
    /// Updates the customer's contact information. Email is intentionally excluded —
    /// it is the unique identifier and cannot be changed after creation.
    /// </summary>
    /// <param name="fullName">The new full name.</param>
    /// <param name="phone">The new optional phone number.</param>
    /// <param name="company">The new optional company name.</param>
    /// <param name="notes">The new optional internal notes.</param>
    public void Update(string fullName, string? phone, string? company, string? notes)
    {
        if (string.IsNullOrWhiteSpace(value: fullName))
        {
            throw CustomerErrors.FullNameRequired();
        }

        FullName = fullName;
        Phone = phone;
        Company = company;
        Notes = notes;
    }
}
