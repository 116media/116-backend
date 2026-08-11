using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Identity;

/// <summary>
/// Fluent builder for creating <see cref="RoleEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; RoleFactory only names chains three or more tests share.
/// </summary>
public class RoleBuilder
{
    private readonly Faker _faker = TestFaker.Create();
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
        string prefix = word.Length > 4 ? word[..4] : word;
        string unique = $"{prefix}{Guid.NewGuid():N}";
        _name = unique[..Math.Min(TestConstants.Role.NameMaxLength, unique.Length)];
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
        var role = RoleEntity.Create(_id, _name, _description, TestErrorsFactory.CreateUserErrors());

        if (!_isActive)
        {
            role.Deactivate();
        }

        if (_isDeleted)
        {
            role.SoftDelete();
        }

        foreach (
            RolePermissionEntity rolePermission in _permissions.Select(permission =>
                RolePermissionFactory.CreateWithPermission(_id, permission)
            )
        )
        {
            role.RolePermissions.Add(rolePermission);
        }

        return role;
    }
}
