namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a single content slot within a package.
/// </summary>
/// <param name="Id">The unique identifier of the slot.</param>
/// <param name="CategoryId">The identifier of the fixed category, or null for an open slot.</param>
/// <param name="CategoryName">The display name of the fixed category, or null for an open slot.</param>
/// <param name="IsRequired">Whether this slot must be fulfilled for the package to be complete.</param>
/// <param name="Quantity">The number of content pieces required for this slot.</param>
public record PackageSlotDto(Guid Id, Guid? CategoryId, string? CategoryName, bool IsRequired, int Quantity);
