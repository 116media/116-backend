using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a B2B customer.
/// </summary>
/// <param name="Id">The unique identifier of the customer.</param>
/// <param name="FullName">The full name of the customer or contact person.</param>
/// <param name="Email">The email address of the customer.</param>
/// <param name="Phone">The optional phone number of the customer.</param>
/// <param name="Company">The optional company or label name.</param>
/// <param name="Notes">Optional internal notes about the customer.</param>
public record CustomerDto(Guid Id, string FullName, string Email, string? Phone, string? Company, string? Notes)
    : AuditableDto;
