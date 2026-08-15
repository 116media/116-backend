using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Identity;

/// <summary>
/// Fluent builder for creating <see cref="UserEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; UserFactory only names chains three or more tests share.
/// </summary>
public class UserBuilder
{
    private const int SuffixLength = 6;

    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private string? _email;
    private string _userName;
    private string _passwordHash;
    private EnumAuthProvider _authProvider = EnumAuthProvider.Local;
    private string _providerSubjectId = $"sub-{Guid.NewGuid():N}";
    private bool _isVerified;
    private bool _isActive = true;
    private readonly List<RoleEntity> _roles = [];
    private string? _fullPhoneNumber;
    private string? _partialPhoneNumber;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserBuilder"/> class with random default values.
    /// </summary>
    public UserBuilder()
    {
        _id = Guid.NewGuid();
        _email = $"{_faker.Internet.UserName()}.{Guid.NewGuid():N}@example.test".ToLowerInvariant();

        string suffix = Guid.NewGuid().ToString("N")[..SuffixLength];
        string generatedName = _faker.Name.FirstName();
        int nameBudget = TestConstants.User.UserNameMaxLength - SuffixLength;

        _userName = $"{generatedName[..Math.Min(generatedName.Length, nameBudget)]}{suffix}";
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
    /// Clears the email address, producing the shape a social provider that
    /// shares no address leaves behind. Only meaningful together with
    /// <see cref="WithAuthProvider"/>, since local accounts always carry one.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithoutEmail()
    {
        _email = null;
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
    /// Sets the phone number fields on the user.
    /// </summary>
    /// <param name="fullPhoneNumber">The full international phone number (e.g. "+1234567890").</param>
    /// <param name="partialPhoneNumber">The partial/local phone number.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithPhoneNumber(string fullPhoneNumber, string partialPhoneNumber)
    {
        _fullPhoneNumber = fullPhoneNumber;
        _partialPhoneNumber = partialPhoneNumber;
        return this;
    }

    /// <summary>
    /// Sets the provider subject id used when building an external user.
    /// </summary>
    /// <param name="providerSubjectId">The provider's stable subject id.</param>
    /// <returns>The builder instance for chaining.</returns>
    public UserBuilder WithProviderSubjectId(string providerSubjectId)
    {
        _providerSubjectId = providerSubjectId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="UserEntity"/> instance for local authentication.
    /// </summary>
    /// <returns>A configured UserEntity instance.</returns>
    public UserEntity Build()
    {
        var errors = TestErrorsFactory.CreateUserErrors();
        UserEntity user =
            _authProvider == EnumAuthProvider.Local
                ? UserEntity.Create(_id, _email!, _userName, _passwordHash, errors)
                : UserEntity.CreateExternal(_id, _userName, _authProvider, _providerSubjectId, errors, _email);

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

        if (_fullPhoneNumber is not null && _partialPhoneNumber is not null)
        {
            user.UpdatePhoneNumber(
                countryName: null,
                countryIsoCode: null,
                countryDialCode: null,
                fullPhoneNumber: _fullPhoneNumber,
                partialPhoneNumber: _partialPhoneNumber
            );
        }

        foreach (RoleEntity role in _roles)
        {
            var userRole = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), _id, role.Id);

            typeof(UserRoleEntity).GetProperty(nameof(UserRoleEntity.Role))!.SetValue(userRole, role);
            user.AssignRole(userRole, errors);
        }

        return user;
    }
}
