using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="PermissionEntity"/> instances in tests.
/// For test code, prefer using PermissionFactory instead of direct Builder usage.
/// </summary>
internal class PermissionBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _resource;
    private string _action;
    private string _description;
    private bool _isActive = true;
    private bool _isDeleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionBuilder"/> class with random default values.
    /// </summary>
    public PermissionBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        _resource = word[..Math.Min(TestConstants.Permission.ResourceMaxLength, word.Length)];
        _action = _faker.PickRandom("read", "create", "update", "delete", "approve");
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the permission ID.
    /// </summary>
    /// <param name="id">The permission identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the resource name.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder WithResource(string resource)
    {
        _resource = resource;
        return this;
    }

    /// <summary>
    /// Sets the action name.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder WithAction(string action)
    {
        _action = action;
        return this;
    }

    /// <summary>
    /// Sets the resource and action together.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <param name="action">The action name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder WithResourceAction(string resource, string action)
    {
        _resource = resource;
        _action = action;
        return this;
    }

    /// <summary>
    /// Sets the permission description.
    /// </summary>
    /// <param name="description">The permission description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Marks the permission as inactive.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Marks the permission as active.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder AsActive()
    {
        _isActive = true;
        return this;
    }

    /// <summary>
    /// Marks the permission as soft-deleted.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public PermissionBuilder AsDeleted()
    {
        _isDeleted = true;
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PermissionEntity"/> instance.
    /// </summary>
    /// <returns>A configured PermissionEntity instance.</returns>
    public PermissionEntity Build()
    {
        var permission = PermissionEntity.Create(_id, _resource, _action, _description);

        if (!_isActive)
        {
            permission.Deactivate();
        }

        if (_isDeleted)
        {
            permission.SoftDelete();
        }

        return permission;
    }
}
