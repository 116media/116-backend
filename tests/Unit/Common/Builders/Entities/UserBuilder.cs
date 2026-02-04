using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Constants;
using Bogus;

namespace _116.Unit.Tests.Common.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="UserEntity"/> instances in tests.
/// </summary>
public class UserBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _email;
    private string _userName;
    private string _passwordHash;
    private EnumAuthProvider _authProvider = EnumAuthProvider.Local;
    private bool _isVerified;
    private bool _isActive = true;
    private readonly List<RoleEntity> _roles = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UserBuilder"/> class with random default values.
    /// </summary>
    public UserBuilder()
    {
        _id = Guid.NewGuid();
        _email = _faker.Internet.Email().ToLowerInvariant();
        // Generate a username that fits within the max length (20 chars)
        string generatedName = $"{_faker.Name.FirstName()}{_faker.Random.Number(100, 999)}";
        _userName =
            generatedName.Length > TestConstants.User.UserNameMaxLength
                ? generatedName[..TestConstants.User.UserNameMaxLength]
                : generatedName;
        _passwordHash = TestConstants.User.DefaultPasswordHash;
    }

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the user email.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithEmail(string email)
    {
        _email = email.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Sets the username.
    /// </summary>
    /// <param name="userName">The username.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    /// <summary>
    /// Sets the password hash.
    /// </summary>
    /// <param name="passwordHash">The password hash.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    /// <summary>
    /// Sets the authentication provider.
    /// </summary>
    /// <param name="authProvider">The authentication provider.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithAuthProvider(EnumAuthProvider authProvider)
    {
        _authProvider = authProvider;
        return this;
    }

    /// <summary>
    /// Marks the user as verified.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder AsVerified()
    {
        _isVerified = true;
        return this;
    }

    /// <summary>
    /// Marks the user as unverified.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder AsUnverified()
    {
        _isVerified = false;
        return this;
    }

    /// <summary>
    /// Marks the user as active.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder AsActive()
    {
        _isActive = true;
        return this;
    }

    /// <summary>
    /// Marks the user as inactive.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Adds a role to the user.
    /// </summary>
    /// <param name="role">The role to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithRole(RoleEntity role)
    {
        _roles.Add(role);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="UserEntity"/> instance for local authentication.
    /// </summary>
    /// <returns>A configured UserEntity instance.</returns>
    public UserEntity Build()
    {
        UserEntity user;

        if (_authProvider == EnumAuthProvider.Local)
        {
            user = UserEntity.Create(_id, _email, _userName, _passwordHash);
        }
        else
        {
            user = UserEntity.CreateExternal(_id, _userName, _authProvider, _email);
        }

        if (_isVerified)
        {
            user.MarkAsVerified();
        }

        if (_isActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        foreach (RoleEntity role in _roles)
        {
            UserRoleEntity userRole = UserRoleEntity.Create(Guid.NewGuid(), _id, role.Id);
            // Set the Role navigation property via reflection for testing
            typeof(UserRoleEntity).GetProperty(nameof(UserRoleEntity.Role))!.SetValue(userRole, role);
            user.AssignRole(userRole);
        }

        return user;
    }
}
