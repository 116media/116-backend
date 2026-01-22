namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing role information with associated permissions.
/// </summary>
/// <param name="Id">The unique identifier of the role</param>
/// <param name="Name">Name of the role (e.g., "Admin", "Editor")</param>
/// <param name="Description">Human-readable description of the role's purpose</param>
/// <param name="IsActive">Indicates whether the role is active and can be assigned</param>
/// <param name="IsDeleted">Indicates whether the role has been soft-deleted</param>
/// <param name="DeletedAt">Date and time when the role was soft-deleted</param>
public record RoleDto(Guid Id, string Name, string Description, bool IsActive, bool IsDeleted, DateTime? DeletedAt);
