using System.Reflection;
using _116.Identity.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="UserRoleEntity"/> instances in tests.
/// For complex junction table scenarios.
/// </summary>
internal class UserRoleBuilder
{
    private Guid _id;
    private Guid _userId;
    private Guid _roleId;
    private RoleEntity? _role;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleBuilder"/> class with random default values.
    /// </summary>
    public UserRoleBuilder()
    {
        _id = Guid.NewGuid();
        _userId = Guid.NewGuid();
        _roleId = Guid.NewGuid();
    }

    /// <summary>
    /// Sets the user-role association ID.
    /// </summary>
    /// <param name="id">The association identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserRoleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserRoleBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the role ID.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserRoleBuilder WithRoleId(Guid roleId)
    {
        _roleId = roleId;
        return this;
    }

    /// <summary>
    /// Sets both user and role IDs.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserRoleBuilder ForUserAndRole(Guid userId, Guid roleId)
    {
        _userId = userId;
        _roleId = roleId;
        return this;
    }

    /// <summary>
    /// Sets the role entity for navigation property testing.
    /// </summary>
    /// <param name="role">The role entity.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserRoleBuilder WithRole(RoleEntity role)
    {
        _role = role;
        _roleId = role.Id;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="UserRoleEntity"/> instance.
    /// </summary>
    /// <returns>A configured UserRoleEntity instance.</returns>
    public UserRoleEntity Build()
    {
        var userRole = UserRoleEntity.Create(_id, _userId, _roleId);

        if (_role is not null)
        {
            SetNavigationProperty(userRole, "Role", _role);
        }

        return userRole;
    }

    /// <summary>
    /// Sets a navigation property using reflection for testing purposes.
    /// </summary>
    private static void SetNavigationProperty<T>(UserRoleEntity entity, string propertyName, T value)
    {
        PropertyInfo? property = typeof(UserRoleEntity).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance
        );
        property?.SetValue(entity, value);
    }
}
