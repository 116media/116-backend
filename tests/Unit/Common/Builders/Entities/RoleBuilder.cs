using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Constants;
using Bogus;

namespace _116.Unit.Tests.Common.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="RoleEntity"/> instances in tests.
/// </summary>
public class RoleBuilder
{
    private readonly Faker _faker = new();
    private readonly List<PermissionEntity> _permissions = [];

    private Guid _id;
    private string _name;
    private string _description;
    private bool _isActive = true;
    private bool _isDeleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleBuilder"/> class with random default values.
    /// </summary>
    public RoleBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        _name = word[..Math.Min(TestConstants.Role.NameMaxLength, word.Length)];
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the role ID.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the role name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the role description.
    /// </summary>
    /// <param name="description">The role description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Marks the role as inactive.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Marks the role as active.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder AsActive()
    {
        _isActive = true;
        return this;
    }

    /// <summary>
    /// Marks the role as soft-deleted.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder AsDeleted()
    {
        _isDeleted = true;
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Adds a permission to the role.
    /// </summary>
    /// <param name="permission">The permission to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder WithPermission(PermissionEntity permission)
    {
        _permissions.Add(permission);
        return this;
    }

    /// <summary>
    /// Adds multiple permissions to the role.
    /// </summary>
    /// <param name="permissions">The permissions to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RoleBuilder WithPermissions(IEnumerable<PermissionEntity> permissions)
    {
        _permissions.AddRange(permissions);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="RoleEntity"/> instance.
    /// </summary>
    /// <returns>A configured RoleEntity instance.</returns>
    public RoleEntity Build()
    {
        RoleEntity role = RoleEntity.Create(_id, _name, _description);

        if (!_isActive)
        {
            role.Deactivate();
        }

        if (_isDeleted)
        {
            role.SoftDelete();
        }

        foreach (PermissionEntity permission in _permissions)
        {
            RolePermissionEntity rolePermission = new RolePermissionBuilder()
                .WithRoleId(_id)
                .WithPermission(permission)
                .Build();
            role.RolePermissions.Add(rolePermission);
        }

        return role;
    }
}
