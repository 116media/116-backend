using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain;
using _116.Identity.Application.Shared.Errors;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a permission that defines an action allowed on a specific resource.
/// </summary>
/// <remarks>
/// Permissions are typically associated with roles through <see cref="RolePermissionEntity"/>.
/// </remarks>
public class PermissionEntity : Aggregate<Guid>
{
    /// <summary>
    /// The name or key of the resource (e.g., "user", "receipt", "article").
    /// </summary>
    [MaxLength(PermissionConstants.MaxPermissionResourceLength)]
    public string Resource { get; private set; } = null!;

    /// <summary>
    /// The type of action allowed on the resource (e.g., "read", "create", "approve").
    /// </summary>
    [MaxLength(PermissionConstants.MaxPermissionActionLength)]
    public string Action { get; private set; } = null!;

    /// <summary>
    /// Human-readable description of the permission's purpose or scope.
    /// </summary>
    [MaxLength(PermissionConstants.MaxPermissionDescriptionLength)]
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Navigation property:
    /// Collection of role-permission associations linked to this permission.
    /// </summary>
    public ICollection<RolePermissionEntity> RolePermissions { get; private set; } = new List<RolePermissionEntity>();

    /// <summary>
    /// Creates a new permission entity.
    /// </summary>
    /// <param name="id">The unique identifier of the permission.</param>
    /// <param name="resource">The resource name (e.g., "user", "file", "receipt").</param>
    /// <param name="action">The action name (e.g., "read", "create", "delete").</param>
    /// <param name="description">The description of the permission's purpose.</param>
    /// <returns>A new <see cref="PermissionEntity"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are null, empty, or exceed maximum length.</exception>
    public static PermissionEntity Create(Guid id, string resource, string action, string description)
    {
        BadRequestException? error = (resource, action, description) switch
        {
            var (r, _, _) when string.IsNullOrWhiteSpace(r) => UserErrors.PermissionResourceRequired(),
            var (_, a, _) when string.IsNullOrWhiteSpace(a) => UserErrors.PermissionActionRequired(),
            var (_, _, d) when string.IsNullOrWhiteSpace(d) => UserErrors.PermissionDescriptionRequired(),
            _ => null
        };

        if (error is not null)
            throw error;

        return new PermissionEntity
        {
            Id = id,
            Resource = resource,
            Action = action,
            Description = description
        };
    }
}
