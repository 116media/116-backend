using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Commands.Roles;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdatePermissionCommand"/> instances in tests.
/// </summary>
public class UpdatePermissionCommandBuilder
{
    private readonly Faker _faker = new();

    private string? _action;
    private string? _resource;
    private Guid _permissionId;
    private string? _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePermissionCommandBuilder"/> class with random default values.
    /// </summary>
    public UpdatePermissionCommandBuilder()
    {
        _permissionId = Guid.NewGuid();
        string resourceWord = _faker.Lorem.Word();
        _resource = resourceWord[..Math.Min(TestConstants.Permission.ResourceMaxLength, resourceWord.Length)];
        _action = _faker.PickRandom("read", "create", "update", "delete");
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the permission ID.
    /// </summary>
    /// <param name="permissionId">The permission identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithPermissionId(Guid permissionId)
    {
        _permissionId = permissionId;
        return this;
    }

    /// <summary>
    /// Sets the resource name.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithResource(string? resource)
    {
        _resource = resource;
        return this;
    }

    /// <summary>
    /// Sets the action name.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithAction(string? action)
    {
        _action = action;
        return this;
    }

    /// <summary>
    /// Sets the permission description.
    /// </summary>
    /// <param name="description">The permission description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Clears the resource (null).
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithoutResource()
    {
        _resource = null;
        return this;
    }

    /// <summary>
    /// Clears the action (null).
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithoutAction()
    {
        _action = null;
        return this;
    }

    /// <summary>
    /// Clears the description (null).
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithoutDescription()
    {
        _description = null;
        return this;
    }

    /// <summary>
    /// Sets valid test values for the command.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdatePermissionCommandBuilder WithValidData()
    {
        _resource = TestConstants.Permission.ValidResource;
        _action = TestConstants.Permission.ValidAction;
        _description = TestConstants.Permission.ValidDescription;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdatePermissionCommand"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdatePermissionCommand instance.</returns>
    public AdminUpdatePermissionCommand Build()
    {
        return new AdminUpdatePermissionCommand(_permissionId, _resource, _action, _description);
    }
}
