namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// The public projection of a permission grant: name and description only. Lifecycle state
/// stays on <see cref="PermissionDto" /> for admin.
/// </summary>
/// <param name="Id">The permission identifier.</param>
/// <param name="Resource">The resource the permission applies to.</param>
/// <param name="Action">The permitted action.</param>
/// <param name="Description">The human-readable description.</param>
public record PublicPermissionDto(Guid Id, string Resource, string Action, string Description);
