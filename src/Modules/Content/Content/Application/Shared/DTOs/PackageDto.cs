using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a bundle deal package.
/// </summary>
/// <param name="Id">The unique identifier of the package.</param>
/// <param name="Name">The display name of the package.</param>
/// <param name="Description">An optional description of what the package includes.</param>
/// <param name="FlatPriceUsd">The flat price in USD for the entire package.</param>
/// <param name="IsActive">Whether the package is currently active and available for orders.</param>
/// <param name="Slots">The content slots that define the composition of this package.</param>
public record PackageDto(
    Guid Id,
    string Name,
    string? Description,
    decimal FlatPriceUsd,
    bool IsActive,
    IReadOnlyList<PackageSlotDto> Slots
) : AuditableDto;
