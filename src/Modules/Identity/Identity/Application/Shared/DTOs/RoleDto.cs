namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing role information with associated permissions.
/// </summary>
/// <param name="Id">The unique identifier of the role</param>
/// <param name="Name">Name of the role (e.g., "Admin", "Editor")</param>
/// <param name="Description">Human-readable description of the role's purpose</param>
public record RoleDto(
    Guid Id,
    string Name,
    string Description
);
