using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;

/// <summary>
/// Command for updating an existing customer's contact information.
/// Email is excluded — it is the unique identifier and cannot be changed.
/// </summary>
/// <param name="Id">The unique identifier of the customer to update.</param>
/// <param name="FullName">The new full name of the customer.</param>
/// <param name="Phone">The new optional phone number.</param>
/// <param name="Company">The new optional company or label name.</param>
/// <param name="Notes">The new optional internal notes.</param>
public record UpdateCustomerCommand(Guid Id, string FullName, string? Phone, string? Company, string? Notes)
    : ICommand<UpdateCustomerResult>;

/// <summary>
/// Result of the <see cref="UpdateCustomerCommand" /> containing the updated customer details.
/// </summary>
/// <param name="Customer">The updated customer information.</param>
public record UpdateCustomerResult(CustomerDto Customer);
