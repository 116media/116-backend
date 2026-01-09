namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing permission information.
/// </summary>
/// <param name="Id">The unique identifier of the permission</param>
/// <param name="Resource">The name or key of the resource (e.g., "user", "receipt", "article")</param>
/// <param name="Action">The type of action allowed on the resource (e.g., "read", "create", "approve")</param>
/// <param name="Description">Human-readable description of the permission's purpose</param>
public record PermissionDto(Guid Id, string Resource, string Action, string Description);
