using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="CustomerEntity" />. Its state lives in <c>Entities/CustomerEntity.cs</c>.
/// </summary>
public partial class CustomerEntity
{
    /// <summary>
    /// Updates the customer's contact information.
    /// </summary>
    /// <param name="fullName">The new full name.</param>
    /// <param name="email">The new email address.</param>
    /// <param name="phone">The new optional phone number.</param>
    /// <param name="company">The new optional company name.</param>
    /// <param name="notes">The new optional internal notes.</param>
    public void Update(string fullName, string email, string? phone, string? company, string? notes)
    {
        if (string.IsNullOrWhiteSpace(value: fullName))
        {
            throw new ContentRuleException(ContentRuleCodes.CustomerFullNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: email))
        {
            throw new ContentRuleException(ContentRuleCodes.CustomerEmailRequired);
        }

        FullName = fullName;
        Email = email;
        Phone = phone;
        Company = company;
        Notes = notes;
    }
}
