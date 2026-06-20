using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Services;

/// <summary>
/// Integration tests for <see cref="IUserLookupService" /> verifying user name
/// and author info lookups against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class UserLookupServiceTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetUserNameByIdAsync_ExistingUser_ReturnsUserName()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = Resolve<IUserLookupService>();

        var result = await service.GetUserNameByIdAsync(user.Id);

        result.Should().Be(user.UserName);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_NonExistentUser_ReturnsNull()
    {
        var service = Resolve<IUserLookupService>();

        var result = await service.GetUserNameByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_ExistingUserWithRole_ReturnsAuthorInfoWithRole()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        var role = RoleFactory.Create();
        context.Users.Add(user);
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var userRole = UserRoleFactory.Create(user.Id, role.Id);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        var service = Resolve<IUserLookupService>();

        var result = await service.GetAuthorInfoByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.UserName.Should().Be(user.UserName);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(role.Name);
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_ExistingUserWithoutRole_ReturnsAuthorInfoWithNullRole()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = Resolve<IUserLookupService>();

        var result = await service.GetAuthorInfoByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.UserName.Should().Be(user.UserName);
        result.Role.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_NonExistentUser_ReturnsNull()
    {
        var service = Resolve<IUserLookupService>();

        var result = await service.GetAuthorInfoByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_UserWithMultipleRoles_ReturnsFirstRole()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        var role1 = RoleFactory.Create();
        var role2 = RoleFactory.Create();
        context.Users.Add(user);
        context.Roles.AddRange(role1, role2);
        await context.SaveChangesAsync();

        context.UserRoles.Add(UserRoleFactory.Create(user.Id, role1.Id));
        context.UserRoles.Add(UserRoleFactory.Create(user.Id, role2.Id));
        await context.SaveChangesAsync();

        var service = Resolve<IUserLookupService>();

        var result = await service.GetAuthorInfoByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Role.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_UserWithAvatarFileId_ReturnsAvatarFileId()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        var avatarId = Guid.NewGuid();
        user.UpdateAvatar(avatarId, EnumAvatarSource.Manual);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = Resolve<IUserLookupService>();

        var result = await service.GetAuthorInfoByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.AvatarFileId.Should().Be(avatarId);
    }
}
