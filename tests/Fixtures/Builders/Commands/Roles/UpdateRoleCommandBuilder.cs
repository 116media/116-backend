using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Commands.Roles;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateRoleCommand"/> instances in tests.
/// </summary>
public class UpdateRoleCommandBuilder
{
    private readonly Faker _faker = new();

    private Guid _roleId;
    private string? _name;
    private string? _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandBuilder"/> class with random default values.
    /// </summary>
    public UpdateRoleCommandBuilder()
    {
        _roleId = Guid.NewGuid();
        string nameWord = _faker.Lorem.Word();
        _name = nameWord[..Math.Min(TestConstants.Role.NameMaxLength, nameWord.Length)];
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the role ID.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithRoleId(Guid roleId)
    {
        _roleId = roleId;
        return this;
    }

    /// <summary>
    /// Sets the role name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the role description.
    /// </summary>
    /// <param name="description">The role description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Clears the name (null).
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithoutName()
    {
        _name = null;
        return this;
    }

    /// <summary>
    /// Clears the description (null).
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithoutDescription()
    {
        _description = null;
        return this;
    }

    /// <summary>
    /// Sets valid test values for the command.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithValidData()
    {
        _name = TestConstants.Role.ValidName;
        _description = TestConstants.Role.ValidDescription;
        return this;
    }

    /// <summary>
    /// Sets an empty name to trigger validation error.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithEmptyName()
    {
        _name = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets a name that exceeds the maximum length.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UpdateRoleCommandBuilder WithNameExceedingMaxLength()
    {
        _name = new string('a', TestConstants.Role.NameMaxLength + 1);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateRoleCommand"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateRoleCommand instance.</returns>
    public AdminUpdateRoleCommand Build()
    {
        return new AdminUpdateRoleCommand(_roleId, _name, _description);
    }
}
