using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Commands.Roles;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateRoleCommand"/> instances in tests.
/// </summary>
public class CreateRoleCommandBuilder
{
    private readonly Faker _faker = new();

    private string _name;
    private string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateRoleCommandBuilder"/> class with random default values.
    /// </summary>
    public CreateRoleCommandBuilder()
    {
        string nameWord = _faker.Lorem.Word();
        _name = nameWord[..Math.Min(TestConstants.Role.NameMaxLength, nameWord.Length)];
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the role name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public CreateRoleCommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the role description.
    /// </summary>
    /// <param name="description">The role description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public CreateRoleCommandBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets valid test values for the command.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreateRoleCommandBuilder WithValidData()
    {
        _name = TestConstants.Role.ValidName;
        _description = TestConstants.Role.ValidDescription;
        return this;
    }

    /// <summary>
    /// Sets an empty name to trigger validation error.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreateRoleCommandBuilder WithEmptyName()
    {
        _name = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets an empty description to trigger validation error.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreateRoleCommandBuilder WithEmptyDescription()
    {
        _description = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets a name that exceeds the maximum length.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreateRoleCommandBuilder WithNameExceedingMaxLength()
    {
        _name = new string('a', TestConstants.Role.NameMaxLength + 1);
        return this;
    }

    /// <summary>
    /// Sets a description that exceeds the maximum length.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public CreateRoleCommandBuilder WithDescriptionExceedingMaxLength()
    {
        _description = new string('a', TestConstants.Role.DescriptionMaxLength + 1);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateRoleCommand"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateRoleCommand instance.</returns>
    public AdminCreateRoleCommand Build()
    {
        return new AdminCreateRoleCommand(_name, _description);
    }
}
