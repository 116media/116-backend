using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Command for creating a new B2B customer.
/// </summary>
/// <param name="FullName">The full name of the customer or contact person.</param>
/// <param name="Email">The email address of the customer (used as the unique identifier).</param>
/// <param name="Phone">An optional phone number.</param>
/// <param name="Company">An optional company or label name.</param>
/// <param name="Notes">Optional internal notes about the customer.</param>
public record AdminCreateCustomerCommand(string FullName, string Email, string? Phone, string? Company, string? Notes)
    : ICommand<AdminCreateCustomerResult>;

/// <summary>
/// Result of the <see cref="AdminCreateCustomerCommand" /> containing the created customer details.
/// </summary>
/// <param name="Customer">The created customer information.</param>
public record AdminCreateCustomerResult(CustomerDto Customer);
