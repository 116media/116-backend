using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Shared.Infrastructure.Interceptors;

/// <summary>
/// Verifies that the DispatchDomainEventsInterceptor dispatches and clears
/// domain events during SaveChanges.
/// </summary>
[Collection("Database")]
public class DispatchDomainEventsInterceptorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SaveChanges_EntityWithNoDomainEvents_ShouldNotThrow()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        var permission = PermissionFactory.Create("dispatch", "read");
        context.Permissions.Add(permission);

        var act = () => context.SaveChangesAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveChanges_ShouldClearDomainEventsAfterDispatch()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        role.DomainEvents.Should().BeEmpty();
    }
}
