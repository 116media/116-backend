using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Factories;

/// <summary>
/// Factory for quickly creating <see cref="UserEntity"/> instances in tests.
/// </summary>
public static class UserFactory
{
    /// <summary>
    /// Creates a user with default random values.
    /// </summary>
    /// <returns>A new UserEntity with random values.</returns>
    public static UserEntity Create() => new UserBuilder().Build();

    /// <summary>
    /// Creates a user with a specific email.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>A new UserEntity with the specified email.</returns>
    public static UserEntity Create(string email) => new UserBuilder().WithEmail(email).Build();

    /// <summary>
    /// Creates a user with a specific email and username.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="userName">The user's username.</param>
    /// <returns>A new UserEntity with the specified values.</returns>
    public static UserEntity Create(string email, string userName) =>
        new UserBuilder().WithEmail(email).WithUserName(userName).Build();

    /// <summary>
    /// Creates a user with a specific ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>A new UserEntity with the specified ID.</returns>
    public static UserEntity CreateWithId(Guid id) => new UserBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a user with a specific ID and email.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="email">The user's email address.</param>
    /// <returns>A new UserEntity with the specified values.</returns>
    public static UserEntity CreateWithId(Guid id, string email) =>
        new UserBuilder().WithId(id).WithEmail(email).Build();

    /// <summary>
    /// Creates a verified and active user.
    /// </summary>
    /// <returns>A new verified and active UserEntity.</returns>
    public static UserEntity CreateVerifiedActive() => new UserBuilder().AsVerified().AsActive().Build();

    /// <summary>
    /// Creates an unverified user.
    /// </summary>
    /// <returns>A new unverified UserEntity.</returns>
    public static UserEntity CreateUnverified() => new UserBuilder().AsUnverified().Build();

    /// <summary>
    /// Creates an inactive user.
    /// </summary>
    /// <returns>A new inactive UserEntity.</returns>
    public static UserEntity CreateInactive() => new UserBuilder().AsInactive().Build();

    /// <summary>
    /// Creates a user with a specific role.
    /// </summary>
    /// <param name="role">The role to assign.</param>
    /// <returns>A new UserEntity with the specified role.</returns>
    public static UserEntity CreateWithRole(RoleEntity role) =>
        new UserBuilder().AsVerified().AsActive().WithRole(role).Build();

    /// <summary>
    /// Creates an external auth user (e.g., Google, Facebook).
    /// </summary>
    /// <param name="authProvider">The authentication provider.</param>
    /// <returns>A new external auth UserEntity.</returns>
    public static UserEntity CreateExternal(EnumAuthProvider authProvider) =>
        new UserBuilder().WithAuthProvider(authProvider).AsVerified().Build();

    /// <summary>
    /// Creates a list of users with the specified count.
    /// </summary>
    /// <param name="count">The number of users to create.</param>
    /// <returns>A list of UserEntity instances.</returns>
    public static List<UserEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>
    /// Creates a SuperAdmin user with the SuperAdmin role.
    /// </summary>
    /// <returns>A UserEntity with SuperAdmin role.</returns>
    public static UserEntity CreateSuperAdmin()
    {
        RoleEntity superAdminRole = RoleFactory.CreateSuperAdmin();
        return new UserBuilder()
            .WithEmail(TestConstants.User.SuperAdminEmail)
            .AsVerified()
            .AsActive()
            .WithRole(superAdminRole)
            .Build();
    }

    /// <summary>
    /// Creates an Admin user with the Admin role.
    /// </summary>
    /// <returns>A UserEntity with Admin role.</returns>
    public static UserEntity CreateAdmin()
    {
        RoleEntity adminRole = RoleFactory.CreateAdmin();
        return new UserBuilder()
            .WithEmail(TestConstants.User.AdminEmail)
            .AsVerified()
            .AsActive()
            .WithRole(adminRole)
            .Build();
    }

    /// <summary>
    /// Creates a Visitor user with the Visitor role.
    /// </summary>
    /// <returns>A UserEntity with Visitor role.</returns>
    public static UserEntity CreateVisitor()
    {
        RoleEntity visitorRole = RoleFactory.CreateVisitor();
        return new UserBuilder()
            .WithEmail(TestConstants.User.VisitorEmail)
            .AsVerified()
            .AsActive()
            .WithRole(visitorRole)
            .Build();
    }
}
