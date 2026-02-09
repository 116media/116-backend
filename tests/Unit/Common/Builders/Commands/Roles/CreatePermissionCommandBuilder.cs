using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;
using _116.Unit.Tests.Common.Constants;
using Bogus;

namespace _116.Unit.Tests.Common.Builders.Commands.Roles;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreatePermissionCommand"/> instances in tests.
/// </summary>
public class CreatePermissionCommandBuilder
{
    private readonly Faker _faker = new();

    private string _resource;
    private string _action;
    private string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePermissionCommandBuilder"/> class with random default values.
    /// </summary>
    public CreatePermissionCommandBuilder()
    {
        string resourceWord = _faker.Lorem.Word();
        _resource = resourceWord[..Math.Min(TestConstants.Permission.ResourceMaxLength, resourceWord.Length)];
        _action = _faker.PickRandom("read", "create", "update", "delete");
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the resource name.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithResource(string resource)
    {
        _resource = resource;
        return this;
    }

    /// <summary>
    /// Sets the action name.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithAction(string action)
    {
        _action = action;
        return this;
    }

    /// <summary>
    /// Sets the permission description.
    /// </summary>
    /// <param name="description">The permission description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets valid test values for the command.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithValidData()
    {
        _resource = TestConstants.Permission.ValidResource;
        _action = TestConstants.Permission.ValidAction;
        _description = TestConstants.Permission.ValidDescription;
        return this;
    }

    /// <summary>
    /// Sets an empty resource to trigger validation error.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithEmptyResource()
    {
        _resource = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets an empty action to trigger validation error.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithEmptyAction()
    {
        _action = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets an empty description to trigger validation error.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithEmptyDescription()
    {
        _description = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets a resource that exceeds the maximum length.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithResourceExceedingMaxLength()
    {
        _resource = new string('a', TestConstants.Permission.ResourceMaxLength + 1);
        return this;
    }

    /// <summary>
    /// Sets an action that exceeds the maximum length.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreatePermissionCommandBuilder WithActionExceedingMaxLength()
    {
        _action = new string('a', TestConstants.Permission.ActionMaxLength + 1);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreatePermissionCommand"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreatePermissionCommand instance.</returns>
    public AdminCreatePermissionCommand Build()
    {
        return new AdminCreatePermissionCommand(_resource, _action, _description);
    }
}
