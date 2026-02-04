using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Mothers;

/// <summary>
/// Provides predefined <see cref="UserEntity"/> scenarios for testing.
/// </summary>
public static class UserMother
{
    /// <summary>
    /// A SuperAdmin user with full system access.
    /// </summary>
    public static UserEntity SuperAdmin()
    {
        RoleEntity role = RoleMother.SuperAdmin();
        return new UserBuilder()
            .WithEmail(TestConstants.User.SuperAdminEmail)
            .WithUserName("superadmin")
            .AsVerified()
            .AsActive()
            .WithRole(role)
            .Build();
    }

    /// <summary>
    /// A SuperAdmin user with a specific ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    public static UserEntity SuperAdmin(Guid id)
    {
        RoleEntity role = RoleMother.SuperAdmin();
        return new UserBuilder()
            .WithId(id)
            .WithEmail(TestConstants.User.SuperAdminEmail)
            .WithUserName("superadmin")
            .AsVerified()
            .AsActive()
            .WithRole(role)
            .Build();
    }

    /// <summary>
    /// An Admin user with administrative access.
    /// </summary>
    public static UserEntity Admin()
    {
        RoleEntity role = RoleMother.Admin();
        return new UserBuilder()
            .WithEmail(TestConstants.User.AdminEmail)
            .WithUserName("admin")
            .AsVerified()
            .AsActive()
            .WithRole(role)
            .Build();
    }

    /// <summary>
    /// An Admin user with a specific ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    public static UserEntity Admin(Guid id)
    {
        RoleEntity role = RoleMother.Admin();
        return new UserBuilder()
            .WithId(id)
            .WithEmail(TestConstants.User.AdminEmail)
            .WithUserName("admin")
            .AsVerified()
            .AsActive()
            .WithRole(role)
            .Build();
    }

    /// <summary>
    /// A Visitor user with standard access.
    /// </summary>
    public static UserEntity Visitor()
    {
        RoleEntity role = RoleMother.Visitor();
        return new UserBuilder()
            .WithEmail(TestConstants.User.VisitorEmail)
            .WithUserName("visitor")
            .AsVerified()
            .AsActive()
            .WithRole(role)
            .Build();
    }

    /// <summary>
    /// A Visitor user with a specific ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    public static UserEntity Visitor(Guid id)
    {
        RoleEntity role = RoleMother.Visitor();
        return new UserBuilder()
            .WithId(id)
            .WithEmail(TestConstants.User.VisitorEmail)
            .WithUserName("visitor")
            .AsVerified()
            .AsActive()
            .WithRole(role)
            .Build();
    }

    /// <summary>
    /// An unverified user (email not confirmed).
    /// </summary>
    public static UserEntity Unverified() =>
        new UserBuilder()
            .WithEmail("unverified@test.com")
            .WithUserName("unverifieduser")
            .AsUnverified()
            .AsActive()
            .Build();

    /// <summary>
    /// An inactive user (account disabled).
    /// </summary>
    public static UserEntity Inactive() =>
        new UserBuilder().WithEmail("inactive@test.com").WithUserName("inactiveuser").AsVerified().AsInactive().Build();

    /// <summary>
    /// An unverified and inactive user.
    /// </summary>
    public static UserEntity UnverifiedInactive() =>
        new UserBuilder()
            .WithEmail("unverified.inactive@test.com")
            .WithUserName("unverifiedinactive")
            .AsUnverified()
            .AsInactive()
            .Build();

    /// <summary>
    /// A user authenticated via Google.
    /// </summary>
    public static UserEntity GoogleUser() =>
        new UserBuilder()
            .WithEmail("google.user@gmail.com")
            .WithUserName("googleuser")
            .WithAuthProvider(EnumAuthProvider.Google)
            .AsVerified()
            .AsActive()
            .Build();

    /// <summary>
    /// A user authenticated via Facebook.
    /// </summary>
    public static UserEntity FacebookUser() =>
        new UserBuilder()
            .WithEmail("facebook.user@example.com")
            .WithUserName("facebookuser")
            .WithAuthProvider(EnumAuthProvider.Facebook)
            .AsVerified()
            .AsActive()
            .Build();

    /// <summary>
    /// A user with valid test data.
    /// </summary>
    public static UserEntity Valid() =>
        new UserBuilder()
            .WithEmail(TestConstants.User.ValidEmail)
            .WithUserName(TestConstants.User.ValidUserName)
            .AsVerified()
            .AsActive()
            .Build();

    /// <summary>
    /// A random user for general testing.
    /// </summary>
    public static UserEntity Random() => new UserBuilder().Build();

    /// <summary>
    /// A verified and active user without any roles.
    /// </summary>
    public static UserEntity VerifiedActiveNoRoles() =>
        new UserBuilder().WithEmail("noroles@test.com").WithUserName("norolesuser").AsVerified().AsActive().Build();
}
